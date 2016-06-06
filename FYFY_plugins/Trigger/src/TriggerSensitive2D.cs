using UnityEngine;
using FYFY;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	public class TriggerSensitive2D : TriggerSensitive {
		private void OnTriggerEnter2D(Collider2D other) {
			if(base._triggeredAdded == false) { // pour gerer les multiples triggers !!
				FYFY.GameObjectManager.addComponent<Triggered2D> (this.gameObject);
				base._triggeredAdded = true;
			}
			
			base._others.Add(other.gameObject);
		}

		private void OnTriggerExit2D(Collider2D other) { // not fired when gameObject has been destroyed
			base._others.Remove(other.gameObject);

			if(_others.Count == 0) { // pour gerer les multiples triggers !! (on ne supprime le composant que quand le dernier sort)
				FYFY.GameObjectManager.removeComponent<Triggered2D> (this.gameObject);
				base._triggeredAdded = false;
			}
		}
	}
}