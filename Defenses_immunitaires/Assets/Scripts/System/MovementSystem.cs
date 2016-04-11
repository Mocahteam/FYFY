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