using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;

public class DoorManager : FSystem {
	private Family doors = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Door)));

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			foreach (GameObject go in doors) {
				Door door = go.GetComponent<Door> ();
				if (door.constraint == null || door.constraint.activeInHierarchy) {
					Animator anim = go.GetComponentInParent<Animator> ();
					Triggered3D triggered = go.GetComponent<Triggered3D> ();
					foreach (GameObject target in triggered.Targets) {
						if (target.name == "HeroSprite") {
							door.isOpen = !door.isOpen;
							anim.SetBool ("isOpen", door.isOpen);
						}
					}
				}
			}
		}
	}
}