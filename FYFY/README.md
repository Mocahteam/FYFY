FYFY
====

This part of the project includes source code that enables you to manage Systems, Families and so on.

Build requirements
------------------

- Unity3D 5.3.4
- Microsoft .Net Framework 4.0

Build library
-------------

Once the requirements have been installed, use the following command in order
to compile the library:

	<path-to-msbuild>\MSBuild.exe /property:buildmode=Standalone FYFY.csproj
	<path-to-msbuild>\MSBuild.exe /property:buildmode=Editor FYFY.csproj

If Unity3D is not installed in a standard location, you have to define the
`UnityEnginePath` and `UnityEditorPath` properties inside the `FYFY.csproj`
file.

Note that common path to MSBuild is:
	
	<path-to-windows>\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

Usage
-----

To use the library inside an Unity project, you just have to drop it in the
`Assets` folder of your project.

Template
--------

To ease the creation of System scripts and Component scripts inside the Unity
editor, you have to put the content of template directory inside the `Unity
templates directory` and then `restart` Unity3D. It allows you to create System
or Component script by right clicking inside the Project view as you would have
done to create C# Script.

Note that common path to Unity templates directory is:

	<path-to-program>\Unity\Editor\Data\Resources\ScriptTemplates

Directories list
----------------

- src: project sources files
- template: template files to include in Unity