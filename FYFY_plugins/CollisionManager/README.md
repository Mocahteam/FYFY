CollisionManager
================

This plugin provides components to manage Unity collision in a consistent manner with ECS formalism.

Add a **CollisionSensitive** to a GameObject and each time a collision occurs, a component **InCollision** will be automatically attached to the GameObject. Then, you can use these data to define your families and detect in collision game objects.

The **InCollision** component will be automatically added only if physics engine of Unity detect collision, be sure your component contains appropriate **Collider** and/or **RigidBody**.

Build requirements
------------------

- FYFY.dll

Build
-----

Once the requirements have been installed, use the following command in order
to compile the library:

	<path-to-msbuild>\MSBuild.exe CollisionManager.csproj

If Unity3D is not installed in a standard location, you have to define the
`UnityEnginePath` and `UnityEditorPath` properties inside the `CollisionManager.csproj`
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
