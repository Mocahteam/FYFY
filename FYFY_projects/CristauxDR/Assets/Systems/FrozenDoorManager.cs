using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class FrozenDoorManager : FSystem {
	private Family doors = FamilyManager.getFamily(new AllOfComponents(typeof(TriggerSensitive3D), typeof(PointerOver), typeof(Door), typeof(ComponentMonitoring)));
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(Boiler)));

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Only one GO is under pointer
			GameObject door_GO = doors.First();
			if (door_GO != null) {
				// Check if hero is near to exit (only hero can produce this component thanks to Unity Physics layers)
				Triggered3D triggered = door_GO.GetComponent<Triggered3D> ();
				if (triggered != null) {
					Door door = door_GO.GetComponent<Door> ();
					ComponentMonitoring monitor = door_GO.GetComponent<ComponentMonitoring> ();
					if (door.isOpen) {
						MonitoringManager.trace (monitor, "turnOff", MonitoringManager.Source.PLAYER);
					} else {
						MonitoringManager.trace (monitor, "turnOn", MonitoringManager.Source.PLAYER);
					}
					// Check constraints on the door
					if (door.constraint == null || door.constraint.activeInHierarchy) {
						Animator anim = door_GO.GetComponentInParent<Animator> ();
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