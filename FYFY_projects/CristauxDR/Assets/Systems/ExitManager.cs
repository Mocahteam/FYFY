using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using monitoring;

public class ExitManager : FSystem {
	private Family exit_F = FamilyManager.getFamily(new AllOfComponents(typeof(TriggerSensitive3D), typeof(Exit), typeof(ComponentMonitoring)));
	private Family hero_F = FamilyManager.getFamily(new AllOfComponents(typeof(Animator), typeof(Rigidbody), typeof(Controllable)));
	private Family endScreen_F = FamilyManager.getFamily(new AnyOfTags("EndScreen"));
	private Family iceMonitor_F = FamilyManager.getFamily(new AnyOfTags("IceCubeMonitor"));
	private GameObject exit_GO = null;
	private GameObject hero_GO = null;
	private GameObject endScreen_GO = null;
	private GameObject iceMonitor_GO = null;

	public ExitManager(){
		// Get the exit (only one)
		exit_GO = exit_F.First();
		if (exit_GO == null)
			Debug.Log("ExitManager: Warning!!! no exit in this scene on start.");
		// Get the hero (only one)
		hero_GO = hero_F.First();
		if (hero_GO == null)
			Debug.Log("ExitManager: Warning!!! no hero in this scene on start.");
		// Get the end screen (only one)
		endScreen_GO = endScreen_F.First();
		if (endScreen_GO == null)
			Debug.Log("ExitManager: Warning!!! no end screen in this scene on start.");
		// Get the iceMonitor (only one)
		iceMonitor_GO = iceMonitor_F.First();
		if (iceMonitor_GO == null)
			Debug.Log("ExitManager: Warning!!! no ice monitor in this scene on start.");
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0) && exit_GO.GetComponent<PointerOver>() != null) {
			ComponentMonitoring monitor = exit_GO.GetComponent<ComponentMonitoring> ();
			Triggered3D triggered = exit_GO.GetComponent<Triggered3D> ();
			bool heroFound = false;
			foreach (GameObject target in triggered.Targets) {
				if (target == hero_GO) {
					heroFound = true;
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
			if (!heroFound)
				monitor.trace("perform", MonitoringManager.Source.PLAYER, true);
		}
	}
}