A set of single-script Unity utilities I created or collected over the many years of gamedev. You can freely drop any of the individual files in your project and they should work out of the box. The repo can also be used as a Unity package. If you wish to use it like that, just add the git link to your package manager. Many of these scripts could be previously found on my [gist](https://gist.github.com/nothke).

Most scripts contain description on how to use them in the header, unless the usage is self evident.

### Includes:
* [GizmosX.cs](Runtime/GizmosX.cs) - Extensions for drawing additional gizmos in OnDrawGizmos() function, like circles, cubes (with rotation!), labels, arcs, heightfields etc.. Editor only.
* [SingletonScriptableObject.cs](Runtime/SingletonScriptableObject.cs) - A self referencing ScriptableObject, useful for global game settings and such.
* [Closest.cs](Runtime/Closest.cs) - Extension for finding a closest instance for an array of components. E.g. `Clock closestClock = listOfClocks.GetClosest(player.transform.position);`
* [RTUtils.cs](Runtime/RTUtils.cs) - RenderTexture utilities for direct drawing meshes, texts and sprites and converting to Texture2D. See [full docs here](Documentation~/RTUtils.md).
* [ProceduralReverb.cs](Runtime/ProceduralReverb.cs) - Calculates reverb parameters by raycasting into the world around the player (naive and not really physically accurate). First used in "Bruturb".
* [ObjectPreviewer.cs](Runtime/ObjectPreviewer.cs) - Directly draws a GameObject for preview without any object cloning. Used for example when you want to preview the "build" location in RTS games.
* [BoundsUtils.cs](Runtime/BoundsUtils.cs) - Additional utils for bounds like finding bounds of an entire GameObject in world or local space, collider bounds, transforming bounds, etc.
* [LabelDrawer.cs](Runtime/LabelDrawer.cs) - Draws labels anywhere in 3D space as long as it's called.
* [BezierUtility.cs](Runtime/BezierUtility.cs) - Contains both pure functions and a struct (for ease of use), for drawing, finding tangents and closest points for cubic bezier curves. Originally derived from BezierUtility found in Unity's URP package.
* [ArrayRandomExtension.cs](Runtime/ArrayRandomExtension.cs) - Extension for getting a random element from a list or array with `array.GetRandom()`
* [NamedArrayAttribute.cs](Runtime/NamedArrayAttribute.cs) - Replaces array element labels with enum names in the inspector.
* [TwinSorter.cs](Runtime/TwinSorter.cs) - Sorts a "target" list by values in the "sorter" list. Useful when having a cache of precomputed values by which you want to sort another list and avoid delegates or Linq.
* [RingBuffer.cs](Runtime/RingBuffer.cs) - A simple fixed-sized contiguous double-sided indexable ring buffer (circular queue).
* [AssetDatabaseUtils.cs](Runtime/AssetDatabaseUtils.cs) - Provides a few useful functions to help with AssetDatabase usage, like creating an asset with the full folder hierarchy, etc.
* [ADSREnvelope.cs](Runtime/ADSREnvelope.cs) - Attack-Decay-Sustain-Release envelope helper struct. Can also keep time state, just pass a signal and deltaTime to Update() and it will give you a float. Also comes with a custom property drawer.
* [Interpolator.cs](Runtime/Interpolator.cs) - A smooth transition between two states wrapper struct. Also includes an InertialInterpolator that transits in a physical with accelerating and braking and smooth interruption. E.g. for closing/opening massive doors.
* [CableMaker.cs](Runtime/CableMaker.cs) - Builds LineRenderer-based hanging cables/wires using catenary formula. Also has a static function for filling up a list of points so you can use it with any other rendering system.
* [PID.cs](Runtime/PID.cs) - Struct-based [PID controllers](https://en.wikipedia.org/wiki/PID_controller) for Float, Vector2 and Vector3.

### Other utils you might like not in this repo:
* [FlyCam](https://github.com/nothke/FlyCam) - Simple WASD flyable camera with zooming and changing speed on scroll.
* [NAudio](https://github.com/nothke/NAudio) - Single line audio playing utils.
* [NDraw](https://github.com/nothke/NDraw) - Runtime [debug] line drawing. similar to Unity's Gizmos, but runtime.
* [ProtoGUI](https://github.com/nothke/ProtoGUI) - Runtime window drawer with a few additional tricks that looks a bit better than Unity's standard GUI.

### License:

All scripts use MIT license. Primarily to avoid liability. License info is embedded in each script as they are meant to be used separately.