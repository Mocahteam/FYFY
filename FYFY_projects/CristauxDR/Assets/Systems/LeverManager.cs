using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using monitoring;

public class LeverManager : FSystem {
	private Family interactLevers = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Lever), typeof(SpriteRenderer)));
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
			foreach (GameObject go in interactLevers) {
				Lever lever = go.GetComponent<Lever> ();
				Triggered3D triggered = go.GetComponent<Triggered3D> ();
				bool heroFound = false;
				foreach (GameObject target in triggered.Targets) {
					if (target.name == "HeroSprite") {
						heroFound = true;
						lever.isOn = !lever.isOn;
						SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
						if (lever.isOn) {
							sr.sprite = lever.on;
							fm.trace("turnOn", TraceHandler.Source.PLAYER);
						} else {
							sr.sprite = lever.off;
							fm.trace("turnOff", TraceHandler.Source.PLAYER);
						}
					}
				}
				if (!heroFound) {
					if (lever.isOn)
						fm.trace("turnOff", TraceHandler.Source.PLAYER, true);
					else
						fm.trace("turnOn", TraceHandler.Source.PLAYER, true);
				}
			}
		}
	}
}