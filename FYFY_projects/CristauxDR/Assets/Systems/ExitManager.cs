using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;

public class ExitManager : FSystem {
	private Family exit_F = FamilyManager.getFamily(new AllOfComponents(typeof(TriggerSensitive3D), typeof(Exit)));
	private Family hero_F = FamilyManager.getFamily(new AllOfComponents(typeof(Animator), typeof(Rigidbody), typeof(Controllable)));
	private Family endScreen_F = FamilyManager.getFamily(new AnyOfTags("EndScreen"));
	private GameObject exit_GO = null;
	private GameObject hero_GO = null;
	private GameObject endScreen_GO = null;

	public ExitManager(){
		// Get the exit (only one)
		IEnumerator<GameObject> goEnum = exit_F.GetEnumerator();
		if (goEnum.MoveNext())
			exit_GO = goEnum.Current;
		else
			Debug.Log("ExitManager: Warning!!! no exit in this scene on start.");
		// Get the hero (only one)
		goEnum = hero_F.GetEnumerator();
		if (goEnum.MoveNext())
			hero_GO = goEnum.Current;
		else
			Debug.Log("ExitManager: Warning!!! no hero in this scene on start.");
		// Get the end screen (only one)
		goEnum = endScreen_F.GetEnumerator();
		if (goEnum.MoveNext())
			endScreen_GO = goEnum.Current;
		else
			Debug.Log("ExitManager: Warning!!! no end screen in this scene on start.");
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0) && exit_GO.GetComponent<PointerOver>() != null) {
			Triggered3D triggered = exit_GO.GetComponent<Triggered3D> ();
			foreach (GameObject target in triggered.Targets) {
				if (target == hero_GO) {
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
}