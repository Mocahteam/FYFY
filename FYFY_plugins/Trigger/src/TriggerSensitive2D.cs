using UnityEngine;
using FYFY;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	public class TriggerSensitive2D : TriggerSensitive {
		internal override void detriggered() {
			if (_triggered == true) {
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