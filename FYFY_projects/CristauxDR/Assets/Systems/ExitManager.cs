using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class ExitManager : FSystem {
	public GameObject exit_GO;
	public GameObject endScreen_GO;

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0) && exit_GO.GetComponent<PointerOver>() != null) {
			ComponentMonitoring monitor = exit_GO.GetComponent<ComponentMonitoring> ();
			// Check if hero is near to exit (only hero can produce this component thanks to Unity Physics layers)
			Triggered3D triggered = exit_GO.GetComponent<Triggered3D> ();
			if (triggered != null) {
				MonitoringManager.trace(monitor, "perform", MonitoringManager.Source.PLAYER);
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