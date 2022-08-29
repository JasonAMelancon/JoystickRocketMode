using System;
using System.IO;
using UnityEngine;

namespace JasonAMelancon_KerbalPlugins
{
    /// <summary>Alerts the player that the x and z axes have been swapped by the accompanying TARGET script.</summary>
    /// <remarks>
    /// This Kerbal Space Program plugin interfaces with Thrustmaster TARGET, and is only usable in concert
    /// with it. TARGET is a powerful scripting system that can modify input from supported Thrustmaster devices, 
    /// including changing which axes of a supported joystick control a particular axis of the virtual joystick 
    /// driver created by TARGET. 
    /// 
    /// When used with the TARGET script supplied along with this KSP plugin, this plugin solves the following
    /// problem.
    /// 
    /// The TARGET script, which was written before this plugin, can swap the x and z axes of the joystick, so
    /// that the x joystick axis controls the z virtual driver axis, and vice versa. This turns out to be 
    /// advantageous for KSP which allows flying both airplane-like and rocket-like craft. 
    /// 
    /// The orientation axes of these two types of craft differ in two respects: one, relative to the gravity 
    /// vector, and two, when viewed from a camera oriented so that the ground is down, relative to the 
    /// orientation of the joystick. For both craft types, the joystick points up, opposite to the gravity 
    /// vector, but the orientation of the craft differs by ninety degrees relative to this vector, at least on 
    /// takeoff.
    /// 
    /// Since a rocket begins flight directly against the pull of gravity, the horizontal axes — the ones that 
    /// are perpendicular to the gravity vector — are pitch and yaw, while roll is parallel to gravity. This is 
    /// usually not so in an airplane, where pitch and roll are perpendicular, while yaw is parallel. So, unlike 
    /// the axes of an airplane, the pitch and yaw axes of a rocket have the same orientational relationship to 
    /// the gravity and thrust vectors. Neither is privileged over the other with respect to gravity or thrust. 
    /// This symmetry between pitch and yaw in a rocket is mirrored in the symmetry between the x and y axes of a 
    /// joystick. Furthermore, the shape of a rocket is similar to the shape of a stick, and when the two are
    /// superimposed in the imagination, with y controlling pitch and x controlling yaw (rather than roll, as in 
    /// an airplane), and with the camera pointed horizontally toward the (vertical) rocket such that the camera 
    /// direction is the same as the down direction in the pilot's field of view, the player has a controller that 
    /// can be treated and handled as though it were the rocket itself. Any motion of the joystick controls the 
    /// rocket in ways that, under normal circumstances, produce the same motion in the rocket: An x axis joystick
    /// motion produces a sideways rotation, a y motion produces an up or down rotation, and a z twist produces a 
    /// twisting rotation, all in the same direction under this camera view.
    /// 
    /// This is all done by the TARGET script. This plugin simply informs the player of which "mode" the joystick
    /// is currently in. Experience shows that beginning flight without any indication that the stick is in a mode
    /// intended for a different type of craft can produce disastrous mishaps. Therefore, any time the player 
    /// takes control of a craft (including by unpausing the game), a message, which is output by the TARGET script 
    /// to a special text file, is displayed informing the player of the current joystick mode. And when the mode 
    /// is changed mid-flight, the same message is displayed, confirming the change.
    /// </remarks>
    [KSPAddon(KSPAddon.Startup.Flight, once: true)]
    public class AxisModeMessage : MonoBehaviour
    {
        /// <summary>Flag accessible from both the Unity thread and the FileSystemWatcher thread.</summary>
        /// <remarks>
        /// Unity methods are not threadsafe; i.e., they can't be called from threads that aren't Unity threads.
        /// Before, when I tried calling ScreenMessages.PostScreenMessage() from OnNewMessage_Handler(), it would
        /// cause the game to crash immediately. Now, the PostScreenMessage() is only called from Unity methods
        /// or events, namely the GameEvents.onFlightReady handler and Update(). The FileSystemWatcher thread and 
        /// its event handler OnNewMessage_Handler() will only be responsible for setting this flag, while Update() 
        /// will check it and (indirectly) call PostScreenMessage().
        /// </remarks>
        private bool NewMessageIsReady { get; set; }
        private readonly object lockObj = new object();
        private bool paused = false;

        /// <summary>The best way we have of detecting the action by TARGET.</summary>
        private FileSystemWatcher messageListener = null;
        private readonly string thisClass;

        public AxisModeMessage()
        {
            GameObject.DontDestroyOnLoad(this); 
            messageListener = new FileSystemWatcher();
            thisClass = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
        }

        private static readonly string msgFilename = "axismode.txt";
        private static string msgPath;
        private string fullFilename; // store filename, since we could read it repeatedly

        public void Awake()
        {
            print($"[{thisClass}] Awake() executing");
            // get the full filename with path of the message file 
            string dll = System.Reflection.Assembly.GetExecutingAssembly().Location;
            msgPath = Path.GetDirectoryName(dll);
            fullFilename = Path.Combine(msgPath, msgFilename);
        }

        public void Start()
        {
            print($"[{thisClass}] Start() executing");
            // register event listeners
            print($"[{thisClass}] Initializing flight mode listener");
            GameEvents.onFlightReady.Add(OnFlightReady_Handler);
            print($"[{thisClass}] Initializing pause listener");
            GameEvents.onGamePause.Add(OnPause_Handler);
            print($"[{thisClass}] Initializing unpause listener");
            GameEvents.onGameUnpause.Add(OnUnpause_Handler);
            print($"[{thisClass}] Initializing message listener"); 
            messageListener.Path = msgPath;
            messageListener.Filter = msgFilename;
            messageListener.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            messageListener.Changed += OnNewMessage_Handler;
            messageListener.Error += OnNewMessageError_Handler;
            messageListener.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Every heartbeat, check the flag to see if we have a new message ready, set by the FileSystemWatcher thread
        /// </summary>
        public void Update()
        {
            bool readyFlag = false;
            lock (lockObj)
            {
                if (NewMessageIsReady)
                {
                    NewMessageIsReady = false;
                    readyFlag = true;
                }
            }
            if (readyFlag)
            {
                ShowMessage();
            }
        }

        public void OnDisable()
        {
            // un-register event listeners
            GameEvents.onFlightReady.Remove(OnFlightReady_Handler);
            GameEvents.onGamePause.Remove(OnPause_Handler);
            GameEvents.onGameUnpause.Remove(OnUnpause_Handler);
            messageListener.Changed -= OnNewMessage_Handler;
            messageListener.Error -= OnNewMessageError_Handler;
            messageListener.Dispose();
            messageListener = null;
        }

        private void OnFlightReady_Handler()
        {
            print($"[{thisClass}] OnFlightReady_Handler executing");
            ShowMessage();
        }

        private void OnNewMessage_Handler(object _, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            print($"[{thisClass}] OnNewMessage_Handler executing");

            lock (lockObj) // NewMessageIsReady is accessible from two threads, so protect it
            {
                NewMessageIsReady = true;
            }
        }

        private void OnNewMessageError_Handler(object _, ErrorEventArgs e)
        {
            LogEx(e.GetException());
        }

        private string msgText;

        private void OnUnpause_Handler()
        {
            paused = false;
            ShowMessage(); // always show the current mode when unpausing the game
        }

        private void OnPause_Handler()
        {
            paused = true;
        }

        /// <summary>
        /// Show the joystick axis mode state message.
        /// </summary>
        private void ShowMessage()
        {
            // don't show anything unless we're in a flight scene and not paused
            if ( ! HighLogic.LoadedSceneIsFlight || paused) return;

            msgText = GetMessage(fullFilename);
            if (string.IsNullOrWhiteSpace(msgText))
            {
                print($"[{thisClass}] Empty message file");
                return;
            }

            ScreenMessages.PostScreenMessage(
                msgText,
                duration: 5.0f,
                ScreenMessageStyle.LOWER_CENTER
            );
            print($"[{thisClass}] Axes message: \"{msgText}\"");
        }

        /// <summary>
        /// Get the joystick axis mode state message from its file.
        /// </summary>
        /// <param name="fullFilename">The full path and filename of the message file.</param>
        /// <returns>The message in the message file.</returns>
        private string GetMessage(string fullFilename)
        {
            if ( ! File.Exists(fullFilename))
            {
                // create file, but also complain
                print($"[{thisClass}] File {fullFilename} did not exist; creating...");
                try
                {
                    File.Create(fullFilename);
                }
                catch (Exception ex)
                {
                    LogEx(ex);
                    return null;
                }
            }
            string line = null;
            try
            {
                // read the first line of the file, which should be the whole message
                StreamReader file = new StreamReader(fullFilename);
                line = file.ReadLine().Trim();
                file.Close();
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }
            return line;
        }

        /// <summary>
        /// Log an exception to logs in the game directory and [homedir]\AppData\Local\Temp\SQUAD\Kerbal Space Program\Crashes.
        /// </summary>
        /// <param name="ex"></param>
        private void LogEx(Exception ex)
        {
            print($"[{thisClass}] {ex.GetType()}: {ex.Message}");
            ex = ex.InnerException;
            while (ex != null)
            {
                print($"caused by {ex.GetType()}: {ex.Message}");
                ex = ex.InnerException;
            }
        }
    }
}
