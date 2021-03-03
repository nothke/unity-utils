A set of single-script Unity utilities I created or collected over the many years of gamedev. You can freely drop any of the individual files in your project and they should work out of the box. The repo can also be used as a Unity package. If you wish to use it like that, just add the git link to your pakcage manager.

Most scripts contain description on how to use them in the header, unless the usage is self evident.

### Includes:
* GizmosX.cs - Extensions for drawing additional gizmos in OnDrawGizmos() function, like circles, cubes (with rotation!), labels, etc.. Editor only.
* SingletonScriptableObject.cs - A self referencing ScriptableObject, useful for global game settings and such.
* Closest.cs - Extension for arrays or lists of components for finding closest GameObjects. E.g. `Clock closestClock = listOfClocks.GetClosest(player.transform.position);`
* RTUtils.cs - RenderTexture utilities for direct drawing sprites, converting to Texture2D;
* ProceduralReverb.cs - Calculates reverb parameters by raycasting into the world around the player. First used in "Bruturb"