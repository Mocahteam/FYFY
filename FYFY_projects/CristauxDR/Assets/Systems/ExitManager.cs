using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class ExitManager : FSystem {
	private Family exit_F = FamilyManager.getFamily(new AllOfComponents(typeof(TriggerSensitive3D), typeof(Exit), typeof(ComponentMonitoring)));
	private Family endScreen_F = FamilyManager.getFamily(new AnyOfTags("EndScreen"));
	private GameObject exit_GO = null;
	private GameObject endScreen_GO = null;

	public ExitManager(){
		if (Application.isPlaying) {
			// Get the exit (only one)
			exit_GO = exit_F.First ();
			if (exit_GO == null)
				Debug.Log ("ExitManager: Warning!!! no exit in this scene on start.");
			// Get the end screen (only one)
			endScreen_GO = endScreen_F.First ();
			if (endScreen_GO == null)
				Debug.Log ("ExitManager: Warning!!! no end screen in this scene on start.");
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0) && exit_GO.GetComponent<PointerOver>() != null) {
			ComponentMonitoring monitor = exit_GO.GetComponent<ComponentMonitoring> ();
			// Check if hero is near to exit (only hero can produce this component thanks to Unity Physics layers)
			Triggered3D triggered = exit_GO.GetComponent<Triggered3D> ();
			if (triggered != null) {
				monitor.trace("perform", MonitoringManager.Source.PLAYER);
				GameObjectManager.setGameObjectState (endScreen_GO, true);
				foreach (FSystem sys in FSystemManager.fixedUpdateSystems())
					sys.Pause = true;
				foreach (FSystem sys in FSystemManager.lateUpdateSystems())
					sys.Pause = true;
				foreach (FSystem sys in FSystemManager.updateSystems())
					sys.Pause = true;
			}
		}
	}
}