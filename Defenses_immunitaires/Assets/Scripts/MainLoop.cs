using UnityEngine;

public class MovementSystem : UECS.System {
	private Family family = FamilyManager.getFamily(new AllOfTypes(typeof(TargetPositionComponent)));

	protected override void onProcess(){
		foreach (GameObject go in family) {
			TargetPositionComponent tpc = go.GetComponent<TargetPositionComponent> ();
			Vector3 position = go.transform.position;

			if (position.x != tpc.x || position.y != tpc.y)
				go.transform.position = Vector3.MoveTowards (position, new Vector3 (tpc.x, tpc.y), Time.deltaTime);
		}
	}
}

public class TargetSystem : UECS.System {
	private Family family = FamilyManager.getFamily(new AllOfTypes(typeof(TargetPositionComponent)));

	protected override void onProcess(){
		foreach (GameObject go in family) {
			TargetPositionComponent tpc = go.GetComponent<TargetPositionComponent> ();
			Vector3 position = go.transform.position;

			if (position.x == tpc.x || position.y == tpc.y) {
				UECS.EntityManager.removeComponent<TargetPositionComponent>(go);
				UECS.EntityManager.addComponent<TargetPositionComponent> (go, new System.Collections.Generic.Dictionary<string, object> () {
					{ "x", Random.Range(-3, 3) },
					{ "y", Random.Range(-3, 3) }
				});
			}
		}
	}
}

public class MainLoop : MonoBehaviour {
	private void Awake() {
		UECS.SystemManager.setSystem(new MovementSystem());
		UECS.SystemManager.setSystem(new TargetSystem());
	}

	private void Start(){
		UECS.EntityManager.parseScene();
	}

	private void FixedUpdate(){
		int count = UECS.EntityManager._delayedActions.Count;
		while(count-- > 0)
			UECS.EntityManager._delayedActions.Dequeue().perform();
		
		for (int i = 0; i < UECS.SystemManager._systems.Count; ++i)
			UECS.SystemManager._systems [i].process();
	}
}

// PROBLEME CONSTRUCTOR UNITY MULTIPLE APPEL ?? regarder serialisation etc