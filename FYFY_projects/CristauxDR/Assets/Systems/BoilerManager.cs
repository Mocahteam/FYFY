using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class BoilerManager : FSystem {
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Boiler), typeof(Animator), typeof(ComponentMonitoring)));

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
			// Only one GO could be under pointer
			GameObject boiler_go = boilers.First();
			if (boiler_go != null) {
				ComponentMonitoring cm = boiler_go.GetComponent<ComponentMonitoring> (); 
				Boiler boiler = boiler_go.GetComponent<Boiler> ();
				bool heroFound = false;
				// Check if boiler is on OR boiler is not constrained OR the constraint is ok
				if (boiler.isOn || boiler.constraint == null || boiler.constraint.activeInHierarchy) {
					Animator anim = boiler_go.GetComponent<Animator> ();
					// Check if the hero is near to the boiler (only hero can produce this component thanks to Unity Physics layers)
					Triggered3D triggered = boiler_go.GetComponent<Triggered3D> ();
					if (triggered != null) {
						heroFound = true;
						boiler.isOn = !boiler.isOn;
						anim.SetBool ("isOn", boiler.isOn);
						if (boiler.isOn)
							cm.trace ("turnOn", MonitoringManager.Source.PLAYER);
						else
							cm.trace ("turnOff", MonitoringManager.Source.PLAYER);
					}
				}
				if (!(boiler.isOn || boiler.constraint == null || boiler.constraint.activeInHierarchy) || !heroFound) {
					if (boiler.isOn)
						cm.trace ("turnOn", MonitoringManager.Source.PLAYER, true);
					else
						cm.trace ("turnOff", MonitoringManager.Source.PLAYER, true);
				}
			}
		}
	}
}