# JoystickRocketMode
Thrustmaster TARGET script and KSP plugin to configure joystick axes for more convenient control of planes and rockets.

This repository includes two essentially separate programs: A Thrustmaster TARGET script to swap the x and z
axes of a supported Thrustmaster joystick back and forth, and a Kerbal Space Program plugin that alerts the player to
how the joystick is currently set up.

This accomplishes much the same thing that https://github.com/Apokee/PlaneMode does, except that PlaneMode
swaps keyboard controls, while this swaps joystick axes. As such, this is not based on PlaneMode, which I discovered
after completing work on the plugin. (I first created a TARGET script that swapped x and z axes for use in KSP in 2019.)

NOTE: This software is not usable by itself. It requires owning two things: The Kerbal Space Program computer game,
and a Thrustmaster joystick that comes with TARGET driver software for programmable changes to the joystick driver.
The Kerbal Space Program plugin and the Thrustmaster TARGET script, both contained in this repository, only work in
concert with each other. (Actually, the TARGET script works fine without the plugin, but why would you want to?)

TARGET is a powerful scripting system that can modify input from supported Thrustmaster devices, 
including changing which axes of a supported joystick control a particular axis of the virtual joystick 
driver created by TARGET. 

## Instructions

NOTE: In my experience, KSP is not compatible with TARGET out of the box. There are steps that you can take to correct this, however. See https://forum.kerbalspaceprogram.com/index.php?/topic/187297-i-got-thrustmaster-target-t16000m-to-work-with-ksp/.

Once this is done, download the two files in the "binaries" direcctory. Put the .tmc file wherever your TARGET scripts normally go, e.g. C:\Program Files (x86)\Thrustmaster\TARGET\scripts. Put the .dll into a new folder inside the GameData folder in your KSP install directory.

## Justification

In short, players often want to use joystick x to control a rocket's yaw instead of roll, as with a plane.
The included TARGET script does this. The included plugin alerts the player to the current mode. 

To explain in more depth, when used in concert, these two pieces of software solve the following problem.

The TARGET script can swap the x and z axes of the joystick, so
that the x joystick axis controls the z virtual driver axis, and vice versa. This turns out to be 
advantageous for KSP which allows flying both airplane-like and rocket-like craft. 

The orientation axes of these two types of craft differ in two respects: one, relative to the gravity 
vector, and two, when viewed from a camera oriented so that the ground is down, relative to the 
orientation of the joystick. For both craft types, the joystick points up, opposite to the gravity 
vector, but the orientation of the craft differs by ninety degrees relative to this vector, at least on 
takeoff.

Since a rocket begins flight directly against the pull of gravity, the horizontal axes — the ones that 
are perpendicular to the gravity vector — are pitch and yaw, while roll is parallel to gravity. This is 
usually not so in an airplane, where pitch and roll are perpendicular, while yaw is parallel. So, unlike 
the axes of an airplane, the pitch and yaw axes of a rocket have the same orientational relationship to 
the gravity and thrust vectors. Neither is privileged over the other with respect to gravity or thrust. 
This symmetry between pitch and yaw in a rocket is mirrored in the symmetry between the x and y axes of a 
joystick. 

Furthermore, the shape of a rocket is similar to the shape of a stick, and when the two are
superimposed in the imagination, with y controlling pitch and x controlling yaw (rather than roll, as in 
an airplane), and with the camera pointed horizontally toward the (vertical) rocket such that the camera 
direction is the same as the down direction in the pilot's field of view, the player has a controller that 
can be treated and handled as though it were the rocket itself. Any motion of the joystick controls the 
rocket in ways that, under normal circumstances, produce the same motion in the rocket: An x axis joystick
motion produces a sideways rotation, a y motion produces an up or down rotation, and a z twist produces a 
twisting rotation, all in the same direction under this camera view.

This is all done by the TARGET script. The plugin simply informs the player of which "mode" the joystick
is currently in. TARGET can't normally communicate with a running application, such as a game. Experience,
however, shows that beginning flight without any indication that the stick is in a mode intended for a
different type of craft can produce disastrous mishaps. Therefore, any time the player takes control of 
a craft (including by unpausing the game), the plugin makes sure that a message, which is output by the 
TARGET script to a special text file, is displayed informing the player of the current joystick mode. And
when the mode is changed mid-flight, the same message is displayed, confirming the change.

## License

You are free to use this software for personal, non-commercial use. Derivative works require the written permission 
of the author upon which it is based, must retain the same license as the original, and must be similarly 
non-commercial in nature. Commercial benefit is allowed only by inaction (for example, Github hosting), 
or by actions that do not require specific awareness of this software or its repository 
(for example, a takeover of Github by another company). The original author and the authors of all subsequent
derivatives must be identified in the accompanying notice or documentation.
