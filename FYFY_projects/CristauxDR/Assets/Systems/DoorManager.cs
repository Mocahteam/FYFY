using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class DoorManager : FSystem {
	private Family doors = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Door), typeof(ComponentMonitoring)));

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
				ComponentMonitoring monitor = door_GO.GetComponent<ComponentMonitoring> ();
				bool heroFound = false;
				// Check if the door is constrained
				if (door.constraint == null || door.constraint.activeInHierarchy) {
					Animator anim = door_GO.GetComponentInParent<Animator> ();
					// Check if hero is near to the door (only hero can produce this component thanks to Unity Physics layers)
					Triggered3D triggered = door_GO.GetComponent<Triggered3D> ();
					if (triggered != null) {
						heroFound = true;
						door.isOpen = !door.isOpen;
						if (door.isOpen)
							monitor.trace ("turnOn", MonitoringManager.Source.PLAYER, false, "l0");
						else
							monitor.trace ("turnOff", MonitoringManager.Source.PLAYER);
						anim.SetBool ("isOpen", door.isOpen);
					}
				}
				if (!(door.constraint == null || door.constraint.activeInHierarchy) || !heroFound) {
					if (door.isOpen)
						monitor.trace ("turnOff", MonitoringManager.Source.PLAYER, true);
					else
						monitor.trace ("turnOn", MonitoringManager.Source.PLAYER, true);
				}
			}
		}
	}
}