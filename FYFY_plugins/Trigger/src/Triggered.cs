using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FYFY_plugins.Trigger {
	/// <summary>
	/// 	Component specifying that the <c>GameObject</c>(the source) is in contact with at least one other <c>GameObject</c>(the target).
	/// </summary>
	public abstract class Triggered : MonoBehaviour {
		//* <summary>
		//* 	Pointer to <see cref="FYFY_plugins.Trigger.TriggerSensitive._targets">dictionary</see>.
		//* </summary>
		internal Dictionary<GameObject, GhostTriggeredTarget> _targets;

		/// <summary>
		/// 	Gets all the <c>GameObjects</c> in contact with the <c>GameObject</c>.
		/// </summary>
		public GameObject[] Targets { get { return _targets.Keys.ToArray(); } }
	}
}