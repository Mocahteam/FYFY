using UnityEngine;
using FYFY;
using System.Collections.Generic;
using FYFY_plugins.Monitoring;

public class TemperatureManager : FSystem {
	private Family temperature = FamilyManager.getFamily(new AllOfComponents(typeof(Temperature)));
	private Family boilers = FamilyManager.getFamily(new AllOfComponents(typeof(Boiler)));
	private Family levers = FamilyManager.getFamily(new AllOfComponents(typeof(Lever)));
	private Family iceCubes = FamilyManager.getFamily(new AnyOfTags("IceCubeScaller"));
	private Family iceCollider = FamilyManager.getFamily(new AnyOfTags("IceCubeCollider"));
	private Family iceMonitor_F = FamilyManager.getFamily(new AnyOfTags("IceCubeMonitor"));
	private Family puddle = FamilyManager.getFamily(new AnyOfTags("Puddle"));

	private GameObject temp_GO = null;
	private Temperature temp = null;
	private GameObject boiler_GO = null;
	private Boiler boiler = null;
	private GameObject iceCollider_GO = null;
	private GameObject iceMonitor_GO = null;
	private ComponentMonitoring iceMonitor = null;
	private GameObject puddle_GO = null;

	private bool inTransition = false;


	public TemperatureManager (){
		if (Application.isPlaying) {
			temp_GO = temperature.First ();
			if (temp_GO != null)
				temp = temp_GO.GetComponent<Temperature> ();
			else
				Debug.Log ("TemperatureManager: Warning!!! no temperature in this scene on start.");
			boiler_GO = boilers.First ();
			if (boiler_GO != null)
				boiler = boiler_GO.GetComponent<Boiler> ();
			else
				Debug.Log ("TemperatureManager: Warning!!! no boiler in this scene on start.");
			iceCollider_GO = iceCollider.First ();
			if (iceCollider_GO == null)
				Debug.Log ("TemperatureManager: Warning!!! no iceCollider in this scene on start.");
			iceMonitor_GO = iceMonitor_F.First ();
			if (iceMonitor_GO != null)
				iceMonitor = iceMonitor_GO.GetComponent<ComponentMonitoring> ();
			else
				Debug.Log ("TemperatureManager: Warning!!! no iceMonitor in this scene on start.");
			puddle_GO = puddle.First ();
			if (puddle_GO == null)
				Debug.Log ("TemperatureManager: Warning!!! no puddle in this scene on start.");
		}
	}

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
					iceMonitor.trace ("meltingStart", MonitoringManager.Source.SYSTEM);
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
					iceMonitor.trace("meltingEnd", MonitoringManager.Source.SYSTEM);
					inTransition = false;
				}
				else
					temp.current = 0;
			} else if (temp.current >= 0 && newTemp < 0) {
				// solidification
				if (!inTransition) {	
					if (!boiler.isOn)
						iceMonitor.trace("solidifyingStart", MonitoringManager.Source.SYSTEM, true, "l9");
					else
						iceMonitor.trace("solidifyingStart", MonitoringManager.Source.SYSTEM, true, "l10");
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
						iceMonitor.trace("solidifyingEnd", MonitoringManager.Source.SYSTEM, true, "l7");
					else
						iceMonitor.trace("solidifyingEnd", MonitoringManager.Source.SYSTEM, true, "l8");
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
			temp_GO.transform.localScale = new Vector3 (1f, (temp.current + 50) * 1.0353f, 1f); // 1.0353f is the magic constant to align temperature value with asset graduation
		}
	}
}