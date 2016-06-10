using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public abstract class Triggered : MonoBehaviour {
		internal Dictionary<GameObject, GhostTriggeredTarget> _targets; // pointer to TriggerSensitive._targets

		public GameObject[] Targets { get { return _targets.Keys.ToArray(); } }
	}
}