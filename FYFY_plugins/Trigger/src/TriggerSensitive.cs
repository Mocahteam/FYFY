using UnityEngine;
using System.Collections.Generic;

// Ecrire dans la doc : composant ajoute et supprime automatiquement. NE PAS Y TOUCHER MANUELLEMENT SINON ON PEUT PAS GARANTIR LES COMPORTEMENTS!!!!!!!!!
// OnTrigger apres le fixedUpdate (apres internal physics update)
namespace FYFY_plugins.Trigger {
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