using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Ecrire dans la doc : composant ajoute et supprime automatiquement. NE PAS Y TOUCHER MANUELLEMENT SINON ON PEUT PAS GARANTIR LES COMPORTEMENTS!!!!!!!!!
// OnTrigger apres le fixedUpdate (apres internal physics update)
namespace FYFY_plugins.Trigger {
	public abstract class TriggerSensitive : MonoBehaviour {
		protected int _gameObjectId;

		internal List<GameObject> _others;
		internal ReadOnlyCollection<GameObject> _othersReadOnly;

		protected void Awake() {
			_gameObjectId = this.gameObject.GetInstanceID();
			_others = new List<GameObject>();
			_othersReadOnly = new ReadOnlyCollection<GameObject>(_others);
		}
	}
}