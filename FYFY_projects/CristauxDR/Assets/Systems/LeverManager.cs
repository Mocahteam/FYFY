using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using FYFY_plugins.Monitoring;

public class LeverManager : FSystem {
	private Family interactLevers = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Lever), typeof(SpriteRenderer)));
	private Family leverMonitor_F = FamilyManager.getFamily(new AllOfComponents(typeof(FamilyMonitoring)));

	private GameObject leverMonitor_GO = null;
	private FamilyMonitoring fm = null;

	public LeverManager(){
		leverMonitor_GO = leverMonitor_F.First ();
		if (leverMonitor_GO != null)
			fm = leverMonitor_GO.GetComponent<FamilyMonitoring> ();
	}

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
			GameObject leverFocused_GO = interactLevers.First();
			if (leverFocused_GO != null){
				Lever lever = leverFocused_GO.GetComponent<Lever> ();
				// Check if the hero is near to the boiler (only hero can produce this component thanks to Unity Physics layers)
				Triggered3D triggered = leverFocused_GO.GetComponent<Triggered3D> ();
				if (triggered != null){
					lever.isOn = !lever.isOn;
					SpriteRenderer sr = leverFocused_GO.GetComponent<SpriteRenderer> ();
					if (lever.isOn) {
						sr.sprite = lever.on;
						fm.trace("turnOn", MonitoringManager.Source.PLAYER);
					} else {
						sr.sprite = lever.off;
						fm.trace("turnOff", MonitoringManager.Source.PLAYER);
					}
				} else {
					if (lever.isOn)
						fm.trace("turnOff", MonitoringManager.Source.PLAYER, true);
					else
						fm.trace("turnOn", MonitoringManager.Source.PLAYER, true);
				}
			}
		}
	}
}