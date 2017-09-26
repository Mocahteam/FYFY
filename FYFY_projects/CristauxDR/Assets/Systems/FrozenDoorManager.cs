using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;

public class FrozenDoorManager : FSystem {
	private Family doors = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Door)));
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(Boiler)));

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
			// Only one GO is under pointer
			GameObject door_GO = doors.First();
			if (door_GO != null) {
				Door door = door_GO.GetComponent<Door> ();
				// Check constraints on the door
				if (door.constraint == null || door.constraint.activeInHierarchy) {
					Animator anim = door_GO.GetComponentInParent<Animator> ();
					// Check if hero is near to exit (only hero can produce this component thanks to Unity Physics layers)
					Triggered3D triggered = door_GO.GetComponent<Triggered3D> ();
					if (triggered != null) {
						// Check if boiler is on
						GameObject boiler_GO = boilers.First();
						Boiler boilerC = boiler_GO.GetComponent<Boiler> ();
						if (boilerC.isOn) {
							door.isOpen = !door.isOpen;
							anim.SetBool ("isOpen", door.isOpen);
						}
					}
				}
			}
		}
	}
}