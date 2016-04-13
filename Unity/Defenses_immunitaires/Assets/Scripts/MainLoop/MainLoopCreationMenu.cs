﻿using UnityEngine;
using UnityEditor;

public static class MainLoopCreationMenu {
	[MenuItem("ECS/Create Main Loop %m")]
	private static void createMainLoop() {
		string name = "Main_Loop";
		GameObject mainLoop = GameObject.Find(name);
		if(mainLoop == null)
			mainLoop = new GameObject(name);
		
		mainLoop.AddComponent<MainLoop>();
	}

	[MenuItem("ECS/Create Main Loop %m", true)]
	private static bool activeMenu() {
		return Object.FindObjectOfType<MainLoop>() == null && !Application.isPlaying;
	}
}
