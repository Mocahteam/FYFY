using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class LeverManager : FSystem {
	private Family levers = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Lever), typeof(SpriteRenderer)));
	private Family leverMonitor_F = FamilyManager.getFamily(new AllOfComponents(typeof(FamilyMonitoring)));

	private GameObject leverMonitor_GO;
	private FamilyMonitoring fm = null;

	protected override void onStart(){
		leverMonitor_GO = leverMonitor_F.First ();
		if (leverMonitor_GO != null)
			fm = leverMonitor_GO.GetComponent<FamilyMonitoring> ();
		else
			Debug.Log ("LeverManager: Warning!!! no monitor for levers in this scene on start.");
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Only one GO is under pointer
			GameObject leverFocused_GO = levers.First();
			if (leverFocused_GO != null){
				Lever lever = leverFocused_GO.GetComponent<Lever> ();
				// Check if the hero is near to the boiler (only hero can produce this component thanks to Unity Physics layers)
				Triggered3D triggered = leverFocused_GO.GetComponent<Triggered3D> ();
				if (triggered != null){
					lever.isOn = !lever.isOn;
					SpriteRenderer sr = leverFocused_GO.GetComponent<SpriteRenderer> ();
					if (lever.isOn) {
						sr.sprite = lever.on;
						MonitoringManager.trace(fm, "turnOn", MonitoringManager.Source.PLAYER);
					} else {
						sr.sprite = lever.off;
						MonitoringManager.trace(fm, "turnOff", MonitoringManager.Source.PLAYER);
					}
				}
			}
		}
	}
}