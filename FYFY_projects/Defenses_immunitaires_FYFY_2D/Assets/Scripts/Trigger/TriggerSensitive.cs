using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Ecrire dans la doc : composant ajoute et supprime automatiquement. NE PAS Y TOUCHER MANUELLEMENT SINON ON PEUT PAS GARANTIR LES COMPORTEMENTS!!!!!!!!!
// OnTrigger apres le fixedUpdate (apres internal physics update)
namespace FYFY_plugins.Trigger {
	public abstract class TriggerSensitive : MonoBehaviour {
		internal List<GameObject> _others;
		internal ReadOnlyCollection<GameObject> _othersReadOnly;

		protected bool _triggeredAdded;

		protected void Awake() {
			_others = new List<GameObject>();
			_othersReadOnly = new ReadOnlyCollection<GameObject>(_others);

			_triggeredAdded = false;
		}
	}
}