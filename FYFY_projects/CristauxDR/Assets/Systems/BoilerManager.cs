using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using monitoring;

public class BoilerManager : FSystem {
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Boiler), typeof(Animator), typeof(ComponentMonitoring)));

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
				ComponentMonitoring cm = go.GetComponent<ComponentMonitoring> (); 
				Boiler boiler = go.GetComponent<Boiler> ();
				bool heroFound = false;
				if (boiler.isOn || boiler.constraint == null || boiler.constraint.activeInHierarchy) {
					Animator anim = go.GetComponent<Animator> ();
					Triggered3D triggered = go.GetComponent<Triggered3D> ();
					foreach (GameObject target in triggered.Targets) {
						if (target.name == "HeroSprite") {
							heroFound = true;
							boiler.isOn = !boiler.isOn;
							anim.SetBool ("isOn", boiler.isOn);
							if (boiler.isOn)
								cm.trace("turnOn", TraceHandler.Source.PLAYER);
							else
								cm.trace("turnOff", TraceHandler.Source.PLAYER);
						}
					}
				}
				if (!(boiler.isOn || boiler.constraint == null || boiler.constraint.activeInHierarchy) || !heroFound) {
					if (boiler.isOn)
						cm.trace ("turnOn", TraceHandler.Source.PLAYER, true);
					else
						cm.trace("turnOff", TraceHandler.Source.PLAYER, true);
				}
			}
		}
	}
}