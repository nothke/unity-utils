A set of single-script Unity utilities I created or collected over the many years of gamedev. You can freely drop any of the individual files in your project and they should work out of the box. The repo can also be used as a Unity package. If you wish to use it like that, just add the git link to your package manager. Many of these scripts could be previously found on my [gist](https://gist.github.com/nothke).

Most scripts contain description on how to use them in the header, unless the usage is self evident.

### Includes:
* GizmosX.cs - Extensions for drawing additional gizmos in OnDrawGizmos() function, like circles, cubes (with rotation!), labels, arcs, heightfields etc.. Editor only.
* SingletonScriptableObject.cs - A self referencing ScriptableObject, useful for global game settings and such.
* Closest.cs - Extension for finding a closest instance for an array of components. E.g. `Clock closestClock = listOfClocks.GetClosest(player.transform.position);`
* RTUtils.cs - RenderTexture utilities for direct drawing sprites, converting to Texture2D.
* ProceduralReverb.cs - Calculates reverb parameters by raycasting into the world around the player (naive and not really physically accurate). First used in "Bruturb".
* ObjectPreviewer.cs - Directly draws a GameObject for preview without any object cloning. Used for example when you want to preview the "build" location in RTS games.
* BoundsUtils.cs - Additional utils for bounds like finding bounds of an entire GameObject in world or local space, collider bounds, transforming bounds, etc.
* LabelDrawer.cs - Draws labels anywhere in 3D space as long as it's called.
* BezierUtility.cs - Contains both pure functions and a struct (for ease of use), for drawing, finding tangents and closest points for cubic bezier curves. Originally derived from BezierUtility found in Unity's URP package.
* ArrayRandomExtension.cs - Extension for getting a random element from a list or array with `array.GetRandom()`

### Other utils you might like not in this repo:
* [FlyCam](https://github.com/nothke/FlyCam) - Simple WASD flyable camera with zooming and changing speed on scroll.
* [NAudio](https://github.com/nothke/NAudio) - Single line audio playing utils.
* [NDraw](https://github.com/nothke/NDraw) - Runtime [debug] line drawing. similar to Unity's Gizmos, but runtime.
* [ProtoGUI](https://github.com/nothke/ProtoGUI) - Runtime window drawer with a few additional tricks that looks a bit better than Unity's standard GUI.