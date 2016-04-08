using UnityEngine;
using System.Collections.Generic;

// SystemManager -> SystemsQueue

namespace UECS {
	public static class SystemManager {
		internal static List<UECS.System> _systems = new List<UECS.System>(); // internal

		public static void setSystem(UECS.System system){
			_systems.Add(system);
		}
	}
}

// priority ? Order ? Change order ?

// GameObject hiddenMainLoop = new GameObject("Hidden_Main_Loop");
// hiddenMainLoop.hideFlags = HideFlags.HideInHierarchy;
// hiddenMainLoop.AddComponent<> ();
