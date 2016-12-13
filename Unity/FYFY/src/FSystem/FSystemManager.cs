using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FYFY_Inspector")] // ugly

namespace FYFY {
	/// <summary>
	/// 	Manager of <see cref="FYFY.FSystem"/>.
	/// </summary>
	public static class FSystemManager {
		internal static readonly List<FSystem> _fixedUpdateSystems = new List<FSystem>(); // automatically managed in MainLoop
		internal static readonly List<FSystem> _updateSystems      = new List<FSystem>(); // automatically managed in MainLoop
		internal static readonly List<FSystem> _lateUpdateSystems  = new List<FSystem>(); // automatically managed in MainLoop

		/// <summary>
		/// 	Get enumerator over systems which are executed in the fixed update block.
		/// </summary>
		public static IEnumerable<FSystem> fixedUpdateSystems() {
			return _fixedUpdateSystems;
		}

		/// <summary>
		/// 	Get enumerator over systems which are executed in the update block.
		/// </summary>
		public static IEnumerable<FSystem> updateSystems() {
			return _updateSystems;
		}

		/// <summary>
		/// 	Get enumerator over systems which are executed in the late update block.
		/// </summary>
		public static IEnumerable<FSystem> lateUpdateSystems() {
			return _lateUpdateSystems;
		}
	}
}