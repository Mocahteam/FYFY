PointerManager
==============

This plugin provides components to manage Pointer in a consistent manner with ECS formalism.

Add a **PointerSensitive** to a GameObject and each time a pointer enter on the game object, a component **PointerOver** will be automatically attached to the GameObject. Then, you can use these data to define your families and detect focused game objects.

The **PointerOver** component will be automatically added only if physics engine of Unity detect collision, be sure your component contains appropriate **GUIElement** or **Collider**.

Build requirements
------------------

- FYFY.dll

Build
-----

Once the requirements have been installed, use the following command in order
to compile the library:

	<path-to-msbuild>\MSBuild.exe PointerManager.csproj

If Unity3D is not installed in a standard location, you have to define the
`UnityEnginePath` and `UnityEditorPath` properties inside the `PointerManager.csproj`
file. Similarly, you have to define the `FYFYPath` property if FYFY is not
build in its common directory.

Note that common path to MSBuild is:
	
	<path-to-windows>\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

Usage
-----

To use the library inside an Unity project, you just have to drop it and 
`FYFY.dll` in the `Assets` folder of your project.

Directories list
----------------

- src: project sources files
