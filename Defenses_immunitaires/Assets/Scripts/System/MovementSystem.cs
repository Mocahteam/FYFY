using UnityEngine;
using FYFY;

public class MovementSystem : FSystem {
	private Family family;

	public MovementSystem(){
		family = FamilyManager.getFamily(
			new AllOfTypes(typeof(TargetPositionComponent), typeof(MeshFilter)), 
			new GameObjectStateMatcher(GameObjectStateMatcher.STATE.ACTIVE)
		);
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame) {
	}

	protected override void onProcess(int currentFrame){
		foreach (GameObject go in family) {
			TargetPositionComponent tpc = go.GetComponent<TargetPositionComponent> ();
			Vector3 position = go.transform.position;

			if (position.x != tpc.x || position.y != tpc.y)
				go.transform.position = Vector3.MoveTowards (position, new Vector3 (tpc.x, tpc.y), Time.deltaTime);
		}
	}
}