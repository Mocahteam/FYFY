﻿using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;

public class BoilerManager : FSystem {
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Boiler), typeof(Animator)));

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
			foreach (GameObject go in boilers) {
				Boiler boiler = go.GetComponent<Boiler> ();
				if (boiler.isOn || boiler.constraint == null || boiler.constraint.activeInHierarchy) {
					Animator anim = go.GetComponent<Animator> ();
					Triggered3D triggered = go.GetComponent<Triggered3D> ();
					foreach (GameObject target in triggered.Targets) {
						if (target.name == "HeroSprite") {
							boiler.isOn = !boiler.isOn;
							anim.SetBool ("isOn", boiler.isOn);
						}
					}
				}
			}
		}
	}
}