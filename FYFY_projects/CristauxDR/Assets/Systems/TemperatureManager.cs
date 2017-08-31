using UnityEngine;
using FYFY;
using System.Collections.Generic;

public class TemperatureManager : FSystem {
	public Family temperature = FamilyManager.getFamily(new AllOfComponents(typeof(Temperature)));
	public Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(Boiler)));
	public Family levers = FamilyManager.getFamily(new AllOfComponents(typeof(Lever)));
	public Family iceCubes = FamilyManager.getFamily(new AnyOfTags("IceCubeScaller"));
	public Family iceCollider = FamilyManager.getFamily(new AnyOfTags("IceCubeCollider"));
	public Family puddle = FamilyManager.getFamily(new AnyOfTags("Puddle"));

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// refresh display
		// Get the temperature (only one)
		IEnumerator<GameObject> tempEnum = temperature.GetEnumerator();
		if (tempEnum.MoveNext()){
			GameObject temp_GO = tempEnum.Current;
			Temperature temp = temp_GO.GetComponent<Temperature> ();
			// Update temperature
			// Get the boiler (only one)
			IEnumerator<GameObject> boilerEnum = boilers.GetEnumerator();
			if (boilerEnum.MoveNext ()) {
				Boiler boiler = boilerEnum.Current.GetComponent<Boiler> ();
				// Get the puddle (only one)
				IEnumerator<GameObject> puddleEnum = puddle.GetEnumerator ();
				if (puddleEnum.MoveNext ()) {
					GameObject puddle_GO = puddleEnum.Current;
					// Get ice cube collider (onlu once)
					IEnumerator<GameObject> iceColliderEnum = iceCollider.GetEnumerator ();
					if (iceColliderEnum.MoveNext ()) {
						GameObject iceCollider_GO = iceColliderEnum.Current;
						// Count number of lever on
						int nbLeverOn = 0;
						foreach (GameObject lever_GO in levers) {
							Lever lever = lever_GO.GetComponent<Lever> ();
							if (lever.isOn)
								nbLeverOn++;
						}
						// compute target temperature
						float epsilon = 2 * Time.fixedDeltaTime;
						int step = 0;
						if (boiler.isOn && temp.current + epsilon < boiler.defaultTemperature + (nbLeverOn * 2))
							step = 1;
						else if (!boiler.isOn || temp.current - epsilon > boiler.defaultTemperature + (nbLeverOn * 2))
							step = -1;
						
						if (step != 0) {
							// check state changing
							float newTemp = temp.current + epsilon * step;
							if (temp.current <= 0 && newTemp > 0) {
								// melting
								// Check if animation is over
								bool endOfAnim = false;
								foreach (GameObject iceCube in iceCubes) {
									float curentScale = iceCube.transform.localScale.x;
									float newScale = curentScale - epsilon / 20;
									if (newScale <= 0) {
										iceCube.transform.localScale = new Vector3 (0, 0, 0);
										iceCollider_GO.GetComponent<BoxCollider> ().enabled = false;
										endOfAnim = true;
									} else {
										iceCube.transform.localScale = new Vector3 (newScale, newScale, newScale);
										puddle_GO.transform.localScale = new Vector3 (puddle_GO.transform.localScale.x, puddle_GO.transform.localScale.y, 0.46f - (0.46f * newScale / 0.25f));
									}
								}
								if (endOfAnim)
									temp.current = newTemp;
								else
									temp.current = 0;
							} else if (temp.current >= 0 && newTemp < 0) {
								// solidification
								iceCollider_GO.GetComponent<BoxCollider> ().enabled = true;
								// Check if animation is over
								bool endOfAnim = false;
								foreach (GameObject iceCube in iceCubes) {
									float curentScale = iceCube.transform.localScale.x;
									float newScale = curentScale + epsilon / 20;
									if (newScale >= 0.25) {
										iceCube.transform.localScale = new Vector3 (0.25f, 0.25f, 0.25f);
										endOfAnim = true;
									} else {
										iceCube.transform.localScale = new Vector3 (newScale, newScale, newScale);
										puddle_GO.transform.localScale = new Vector3 (puddle_GO.transform.localScale.x, puddle_GO.transform.localScale.y, 0.46f - (0.46f * newScale / 0.25f));
									}
								}
								if (endOfAnim)
									temp.current = newTemp;
								else
									temp.current = 0;
							} else
								temp.current = newTemp;
						} else
							temp.current = boiler.defaultTemperature + (nbLeverOn * 2);
					} else
						Debug.Log ("TemperatureManager: Warning!!! no ice collider found.");
				} else
					Debug.Log ("TemperatureManager: Warning!!! no puddle found.");
			} else
				Debug.Log ("TemperatureManager: Warning!!! no boiler found.");
			if (temp.current > 120)
				temp.current = 120;
			if (temp.current < -50)
				temp.current = -50;
			temp_GO.transform.localScale = new Vector3 (1f, (temp.current + 50) * 1.0353f, 1f); // 1.0353f is the magic constant to align temperature value with asset graduation
		}
	}
}