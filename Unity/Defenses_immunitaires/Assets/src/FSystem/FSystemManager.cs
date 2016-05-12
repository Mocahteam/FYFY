using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	public static class FSystemManager {
		internal static readonly List<FSystem> _systems = new List<FSystem>();

		public static int Count { get { return _systems.Count; } }

		public static IEnumerable<FSystem> systems() {
			return _systems;
		}
	}
}