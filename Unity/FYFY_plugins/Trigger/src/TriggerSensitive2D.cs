using UnityEngine;
using FYFY;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	public class TriggerSensitive2D : TriggerSensitive {
		private static Family _triggered2D = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered2D)));

		private void OnTriggerEnter2D(Collider2D other) {
			base._others.Add(other.gameObject);

			if(_triggered2D.contains(base._gameObjectId) == false) // pour gerer les multiples triggers !!
				FYFY.GameObjectManager.addComponent<Triggered2D>(this.gameObject);
		}

		private void OnTriggerExit2D(Collider2D other) { // not fired when gameObject has been destroyed
			base._others.Remove(other.gameObject);

			if(_others.Count == 0) // pour gerer les multiples triggers !!
				FYFY.GameObjectManager.removeComponent<Triggered2D>(this.gameObject);
		}
	}
}