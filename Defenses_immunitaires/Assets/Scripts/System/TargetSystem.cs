using UnityEngine;
using System.Collections.Generic;
using FYFY;

public class TargetSystem : FSystem {
	private Family family;

	public TargetSystem() {
		family = FamilyManager.getFamily(new AllOfTypes(typeof(TargetPositionComponent)), new GameObjectStateMatcher(GameObjectStateMatcher.STATE.ACTIVE));

		GameObject go = GameObjectManager.createPrimitive(PrimitiveType.Cube);
		go.name = "test";
		GameObjectManager.addComponent<TargetPositionComponent>(go);
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame) {
	}

	protected override void onProcess(int currentFrame){
//		foreach(GameObject go in family.entries()) {
//			Debug.Log ("ENTERED " + go.name);
//		}
//
//		foreach(int id in family.exits()) {
//			Debug.Log ("EXITED " + id);
//		}

		foreach (GameObject go in family) {
			TargetPositionComponent tpc = go.GetComponent<TargetPositionComponent>();
			Vector3 position = go.transform.position;

			if (position.x == tpc.x || position.y == tpc.y) {
				GameObjectManager.removeComponent<TargetPositionComponent>(go);
				GameObjectManager.addComponent<TargetPositionComponent>(go, new { x = Random.Range (-3, 3), y = Random.Range(-3, 3) });
			}
		}
	}
}