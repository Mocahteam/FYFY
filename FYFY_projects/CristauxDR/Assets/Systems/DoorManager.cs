using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using monitoring;

public class DoorManager : FSystem {
	private Family doors = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Door), typeof(ComponentMonitoring)));

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
				ComponentMonitoring monitor = go.GetComponent<ComponentMonitoring> ();
				bool heroFound = false;
				if (door.constraint == null || door.constraint.activeInHierarchy) {
					Animator anim = go.GetComponentInParent<Animator> ();
					Triggered3D triggered = go.GetComponent<Triggered3D> ();
					foreach (GameObject target in triggered.Targets) {
						if (target.name == "HeroSprite") {
							heroFound = true;
							door.isOpen = !door.isOpen;
							if (door.isOpen)
								monitor.trace("turnOn", TraceHandler.Source.PLAYER, false, "l0");
							else
								monitor.trace("trunOff", TraceHandler.Source.PLAYER);
							anim.SetBool ("isOpen", door.isOpen);
						}
					}
				}
				if (!(door.constraint == null || door.constraint.activeInHierarchy) || !heroFound) {
					if (door.isOpen)
						monitor.trace("turnOff", TraceHandler.Source.PLAYER, true);
					else
						monitor.trace("turnOn", TraceHandler.Source.PLAYER, true);
				}
			}
		}
	}
}