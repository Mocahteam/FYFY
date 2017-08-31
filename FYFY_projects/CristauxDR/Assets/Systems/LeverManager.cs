using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;

public class LeverManager : FSystem {
	private Family levers = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Lever), typeof(SpriteRenderer)));

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
			foreach (GameObject go in levers) {
				Lever lever = go.GetComponent<Lever> ();
				Triggered3D triggered = go.GetComponent<Triggered3D> ();
				foreach (GameObject target in triggered.Targets) {
					if (target.name == "HeroSprite") {
						lever.isOn = !lever.isOn;
						SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
						if (lever.isOn) {
							sr.sprite = lever.on;
						} else {
							sr.sprite = lever.off;
						}
					}
				}
			}
		}
	}
}