using UnityEngine;
using FYFY;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	public class TriggerSensitive3D : TriggerSensitive {
		private static Family _triggered3D = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)));

		private void OnTriggerEnter3D(Collider other) {
			base._others.Add(other.gameObject);

			if(_triggered3D.contains(base._gameObjectId) == false) // pour gerer les multiples triggers !!
				FYFY.GameObjectManager.addComponent<Triggered3D>(this.gameObject);
		}

		private void OnTriggerExit3D(Collider other) { // not fired when gameObject has been destroyed
			base._others.Remove(other.gameObject);

			if(_others.Count == 0) // pour gerer les multiples triggers !!
				FYFY.GameObjectManager.removeComponent<Triggered3D>(this.gameObject);
		}
	}
}