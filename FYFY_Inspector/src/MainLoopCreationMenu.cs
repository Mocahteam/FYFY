﻿using UnityEngine;
using UnityEditor;

namespace FYFY_Inspector {
	/// <summary>
	/// 	Class automatically loaded by Unity Editor to create the FYFY menu.
	/// </summary>
	public static class MainLoopCreationMenu {
		[MenuItem("Tools/FYFY/Create Main Loop %m")]
		private static void createMainLoop() {
			string name = "Main_Loop";
			GameObject mainLoop = GameObject.Find(name);
			if(mainLoop == null) {
				mainLoop = new GameObject(name);
				mainLoop.isStatic = true;
			}
			
			mainLoop.AddComponent<FYFY.MainLoop>();
			if (!mainLoop.GetComponent<MainLoopEditorScanner>())
				mainLoop.AddComponent<MainLoopEditorScanner>();
			Undo.RegisterCreatedObjectUndo(mainLoop, "Create MainLoop");
		}

		[MenuItem("Tools/FYFY/Create Main Loop %m", true)]
		private static bool activeMenu() {
			return Object.FindObjectOfType<FYFY.MainLoop>() == null && !Application.isPlaying;
		}
	}
}