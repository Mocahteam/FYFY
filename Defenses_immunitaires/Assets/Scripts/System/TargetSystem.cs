using UnityEngine;

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