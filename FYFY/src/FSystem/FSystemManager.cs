using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	public static class FSystemManager {
		internal static readonly List<FSystem> _fixedUpdateSystems = new List<FSystem>();
		internal static readonly List<FSystem> _updateSystems      = new List<FSystem>();
		internal static readonly List<FSystem> _lateUpdateSystems  = new List<FSystem>();

		public static IEnumerable<FSystem> fixedUpdateSystems() {
			return _fixedUpdateSystems;
		}

		public static IEnumerable<FSystem> updateSystems() {
			return _updateSystems;
		}

		public static IEnumerable<FSystem> lateUpdateSystems() {
			return _lateUpdateSystems;
		}
	}
}