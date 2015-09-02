=====================================
SixenseVR SDK Unity Integration 0.2.2
=====================================

The SixenseVR SDK provides a set of editor tools and prefabs that make it quick and easy to build a fully body-tracked avatar with physical interaction.

=================
Known Issues
=================
 * We recommend using the Oculus Unity SDK version 0.4.3 for optimal performance, as newer versions have been observed to cause increased latency with our tracking.
 * We recommend using Unity 4.5.5f1, other versions have not yet been fully tested.

=================
Avatar Setup
=================
Begin by selecting "GameObject->Create Other->SixenseVR Avatar..." This will open a new window where you can configure your avatar’s appearance and rig it for tracking.
 * Select a player model.  You will see a list of all models that are configured to a "Humanoid" rig type.
 * Click the buttons to select the HMD, left and right controllers.  Use the Unity Editor’s translate and rotate tools to position those tracking devices as if they were being worn and held by the avatar.  Providing an accurate visual match will ensure accurate avatar movements inside of virtual reality.
 * Click "Done" and your avatar will be ready to use.  If the Oculus SDK is installed in your project, the OVR camera rig will automatically be set up for you.
 * Hit "Play" to test the tracking behavior of your avatar.

=================
Prop Configuration
=================
In order to pick up an object inside the virtual world, that object only needs to have a rigid body component and collider configured.  Squeezing the trigger will pick up the object.  More complex interaction can be implemented by adding a Grab Point to the object, by first selecting the object and then "GameObject->Create Other->SixenseVR Grab Point Child".
 * The Grab Point appears as a model of a STEM Controller, use the Unity Editor translate and rotate tools to position it such that it lines up with the object in a way that indicates a natural grip.  If the object is a weapon or tool, for example, the controller should be overlaid such that its grip is aligned with the grip of the object.  This will ensure a good match between what you see inside the virtual environment and what you feel in your hand.
 * If your object should be held differently in the left or right hand, add a second Grab Point, and set the handedness property of both to indicate which hand may use each grab point.  Any number of additional Grab Points may be added as desired if an object can be held multiple ways.
 * Enter the distance and angle at which the Grab Point will activate relative to your hand.  Large values will allow you to pick up the object no matter where you grab it, small values require you to line up your hand carefully in the correct position and orientation.
 * If "Drop On Release" is not checked, squeezing the trigger will cause the grab point to be “locked” into your hand, and the object will not be dropped until you press the Bumper button located above the trigger.  Otherwise the object will be dropped as soon as the trigger is released.
 * Unity Scripts can use "GetComponentInChildren<SixenseVR.GrabPoint>()" to get a reference to a grab point.  This component has information about user input from the relevant controller through the "Input" property, allowing your script to react to button presses and joystick movement.  While the object is not held, all input methods will return false.
 * Make sure that any held props are on a layer that does not collide with the Avatar's capsule collider, otherwise objects in your hand could block your movement.

=================
Stationary Mode
=================
It is possible to configure your avatar such that instead of using thumbsticks to walk around, the avatar will move relative to a fixed point in space, only when the player physically moves.  This can greatly improve user comfort, but restricts mobility, and requires the user to set up a Safe Play Zone and Floor Distance in the SixenseVR Configuration Utility.  Enable this mode by selecting the Avatar object and unchecking "Joystick Locomotion".
 * You will see a yellow cylinder shape around the avatar, this represents the area inside the user's configured safe zone.  The player will only be able to move and interact within this area.
 * Adjust the "Minimum Safe Area Radius" to be as small as possible while still encompassing your interactive objects.  If the user has configured a safe area smaller than this minimum radius, joystick locomotion will be forced on, as the user does not have enough physical space in which to safely play your game.
 * The center of the safe area cylinder is determined by the initial placement of the avatar in your scene.  You may also specify a custom transform, letting you control the position of the tracking volume.  This can be useful for moving platforms or other cases where the player's position must be changed over time.

=================
Fixed Base Mode
=================
In some cases, you may want control over the absolute position of the player in your tracking space.  Fixed base mode allows you to define an exact mapping between physical positions and virtual positions.  Enable this mode by selecting GameObject->Create Other->SixenseVR Fixed Base.
 * You will see a model of a STEM Base placed in your scene.  The position of this model will correspond with the actual position of the base, and all tracking will occur relative to it.
 * Joystick locomotion will be turned off automatically, if the user has configured a safe area.
