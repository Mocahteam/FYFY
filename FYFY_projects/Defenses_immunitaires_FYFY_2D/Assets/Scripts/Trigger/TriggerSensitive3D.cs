using UnityEngine;
using FYFY;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	public class TriggerSensitive3D : TriggerSensitive {
		private void OnTriggerEnter3D(Collider other) {
			if(base._triggeredAdded == false) { // pour gerer les multiples triggers !!
				FYFY.GameObjectManager.addComponent<Triggered3D>(this.gameObject);
				base._triggeredAdded = false;
			}

			base._others.Add(other.gameObject);		}

		private void OnTriggerExit3D(Collider other) { // not fired when gameObject has been destroyed
			base._others.Remove(other.gameObject);

			if(_others.Count == 0) { // pour gerer les multiples triggers !! (on ne supprime le composant que quand le dernier sort)
				FYFY.GameObjectManager.removeComponent<Triggered3D>(this.gameObject);
				base._triggeredAdded = false;
			}
		}
	}
}