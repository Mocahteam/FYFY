using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class DoorManager : FSystem {
	private Family doors = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Door), typeof(ComponentMonitoring)));

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Only one GO is under pointer
			GameObject door_GO = doors.First();
			if (door_GO != null) {
				// Check if hero is near to the door (only hero can produce this component thanks to Unity Physics layers)
				Triggered3D triggered = door_GO.GetComponent<Triggered3D> ();
				if (triggered != null) {
					Door door = door_GO.GetComponent<Door> ();
					ComponentMonitoring monitor = door_GO.GetComponent<ComponentMonitoring> ();
					if (door.isOpen)
						MonitoringManager.trace (monitor, "turnOff", MonitoringManager.Source.PLAYER);
					else
						MonitoringManager.trace (monitor, "turnOn", MonitoringManager.Source.PLAYER);
					// Check if the door is constrained
					if (door.constraint == null || door.constraint.activeInHierarchy) {
						Animator anim = door_GO.GetComponentInParent<Animator> ();
						door.isOpen = !door.isOpen;
						anim.SetBool ("isOpen", door.isOpen);
					}
				}
			}
		}
	}
}