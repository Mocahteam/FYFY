using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;

public class TemperatureManager : FSystem {
	private Family levers = FamilyManager.getFamily(new AllOfComponents(typeof(Lever)));
	private Family iceCubes = FamilyManager.getFamily(new AnyOfTags("IceCubeScaller"));

	public Temperature temp = null;
	public Boiler boiler = null;
	public GameObject iceCollider_GO = null;
	public ComponentMonitoring iceMonitor = null;
	public GameObject puddle_GO = null;

	private bool inTransition = false;

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// refresh display

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
				if (!inTransition) {
					MonitoringManager.trace (iceMonitor, "meltingStart", MonitoringManager.Source.SYSTEM);
					inTransition = true;
				}
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
				if (endOfAnim) {
					temp.current = newTemp;
					MonitoringManager.trace(iceMonitor, "meltingEnd", MonitoringManager.Source.SYSTEM);
					inTransition = false;
				}
				else
					temp.current = 0;
			} else if (temp.current >= 0 && newTemp < 0) {
				// solidification
				if (!inTransition) {	
					if (!boiler.isOn)
						MonitoringManager.trace(iceMonitor, "solidifyingStart", MonitoringManager.Source.SYSTEM, true, "l9");
					else
						MonitoringManager.trace(iceMonitor, "solidifyingStart", MonitoringManager.Source.SYSTEM, true, "l10");
					inTransition = true;
				}
					
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
				if (endOfAnim) {
					if (!boiler.isOn)
						MonitoringManager.trace(iceMonitor, "solidifyingEnd", MonitoringManager.Source.SYSTEM, true, "l7");
					else
						MonitoringManager.trace(iceMonitor, "solidifyingEnd", MonitoringManager.Source.SYSTEM, true, "l8");
					temp.current = newTemp;
					inTransition = false;
				}
				else
					temp.current = 0;
			} else
				temp.current = newTemp;
								
			if (temp.current > 120)
				temp.current = 120;
			if (temp.current < -50)
				temp.current = -50;
			temp.gameObject.transform.localScale = new Vector3 (1f, (temp.current + 50) / 170 * 2.165f, 1f); // 1.0353f is the magic constant to align temperature value with asset graduation
		}
	}
}