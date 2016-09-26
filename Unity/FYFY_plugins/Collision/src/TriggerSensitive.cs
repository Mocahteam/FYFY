using UnityEngine;
using System.Collections.Generic;

namespace FYFY_plugins.Trigger {
	/// <summary>
	/// 	Component allowing <c>GameObject</c> to know when it is in contact with another <c>GameObject</c> by managing
	/// 	automatically a <see cref="FYFY_plugins.Trigger.Triggered">component</see>.
	/// </summary>
	public abstract class TriggerSensitive : MonoBehaviour {
		internal Dictionary<GameObject, GhostTriggeredTarget> _targets = new Dictionary<GameObject, GhostTriggeredTarget>(); // contains target GameObject and the corresponding target's GhostTT
		internal bool _triggered = false;

		// gere le cas ou:
		// - on supprime un TS
		// - puis les GTT associes
		// - et donc eviter de resupprimer le TS source de ces GTT qui correspond au TS supprime auparavant
		internal bool _destroying = false;

		internal abstract void detriggered();

		private void OnDestroy() {
			_destroying = true;
			
			foreach(GhostTriggeredTarget gtt in _targets.Values) {
				Object.Destroy(gtt);
			}
		}
	}
}