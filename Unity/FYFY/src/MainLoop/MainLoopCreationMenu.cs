using UnityEngine;
using UnityEditor;

namespace FYFY {
	public static class MainLoopCreationMenu {
		[MenuItem("FYFY/Create Main Loop %m")]
		private static void createMainLoop() {
			string name = "Main_Loop";
			GameObject mainLoop = GameObject.Find(name);
			if(mainLoop == null) {
				mainLoop = new GameObject(name);
				mainLoop.isStatic = true;
			}
			
			mainLoop.AddComponent<MainLoop>();
		}

		[MenuItem("FYFY/Create Main Loop %m", true)]
		private static bool activeMenu() {
			return Object.FindObjectOfType<MainLoop>() == null && !Application.isPlaying;
		}
	}
}