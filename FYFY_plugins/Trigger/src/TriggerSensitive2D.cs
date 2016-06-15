using UnityEngine;
using FYFY;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	public class TriggerSensitive2D : TriggerSensitive {
		internal override void detriggered() {
			if (_triggered == true) {
				// Exemple : 2 GO avec TriggerSensitive qui se touchent sont supprimés à la même frame => Présence de 2 Destroy dans la pile.
				// Lors de la gestion du premier, on rentre dans le onDestroy de son GTT => désinscription de ce GTT du TS du second GO => Si TS vide => Ajout dans la pile de la suppression de T du second GO (*)
				// Lors de la gestion du second , on fait la même chose
				// ICI les deux GO sont supprimé
				// lorsqu'on arrive sur la demande de suppression de T (voir *) => la référence vers le GO est nulle => CRASH !!!
				// => SOLUTION : Eviter de notifier une suppression de Triggered2D d'un GO lorsque dans la pile ce GO va être supprimé
				foreach(FYFY.IGameObjectManagerAction action in FYFY.GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(DestroyGameObject)) {
						if(((DestroyGameObject)action)._gameObject == this.gameObject)
							return;
					}
				}

				// Ici,  rien dans la pile ne nous empêche d'ajouter à la pile la demande de suppression du trigger
				GameObjectManager.removeComponent<Triggered2D>(this.gameObject);
				_triggered = false;
			} else {
				// throw error
			}
		}

		private void OnTriggerEnter2D(Collider2D other) {
			GameObject target = other.gameObject;

			if (_targets.ContainsKey(target) == false) {
				GhostTriggeredTarget gtt = target.AddComponent<GhostTriggeredTarget>(); // on a besoin de ce composant tt de suite donc pas de FYFY -> en plus on veut pas que le cp soit traite par le sytem
				gtt._triggerSensitiveSource = this;
				_targets.Add(target, gtt);

				if (_triggered == false) {
					GameObjectManager.addComponent<Triggered2D>(this.gameObject);
					_triggered = true;
				}
			} else {
				// throw error
			}
		}

		internal void OnTriggerExit2D(Collider2D other) { // not fired when gameObject has been destroyed
			GameObject target = other.gameObject;

			GhostTriggeredTarget gtt;
			if(_targets.TryGetValue(target, out gtt) == true) {
				Object.Destroy(gtt); // pareil que dans le add
				_targets.Remove(target);

				if(_targets.Count == 0) {
					GameObjectManager.removeComponent<Triggered2D>(this.gameObject);
					_triggered = false;
				}
			} else {
				// throw error
			}
		}
	}
}