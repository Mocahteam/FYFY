using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class BoilerManager : FSystem {
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Boiler), typeof(Animator), typeof(ComponentMonitoring)));

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Only one GO could be under pointer
			GameObject boiler_go = boilers.First();
			if (boiler_go != null) {
				// Check if the hero is near to the boiler (only hero can produce this component thanks to Unity Physics layers)
				Triggered3D triggered = boiler_go.GetComponent<Triggered3D> ();
				if (triggered != null) {
					ComponentMonitoring cm = boiler_go.GetComponent<ComponentMonitoring> (); 
					Boiler boiler = boiler_go.GetComponent<Boiler> ();
					if (boiler.isOn)
						MonitoringManager.trace (cm, "turnOff", MonitoringManager.Source.PLAYER);
					else
						MonitoringManager.trace (cm, "turnOn", MonitoringManager.Source.PLAYER);
					// Check if boiler is on OR boiler is not constrained OR the constraint is ok
					if (boiler.isOn || boiler.constraint == null || boiler.constraint.activeInHierarchy) {
						Animator anim = boiler_go.GetComponent<Animator> ();
						boiler.isOn = !boiler.isOn;
						anim.SetBool ("isOn", boiler.isOn);
					}
				}
			}
		}
	}
}