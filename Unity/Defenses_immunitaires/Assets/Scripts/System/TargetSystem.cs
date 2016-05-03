using UnityEngine;
using System.Collections.Generic;

public class TargetSystem : UECS.System {
	private Family family;

	public void entered(GameObject[] gos) {
		Debug.Log ("ENTERED ");
		foreach (GameObject go in gos)
			Debug.Log (go.name);
	}
//
//	public void exited(int[] gos) {
//		Debug.Log ("EXITED ");
//		foreach (int go in gos)
//			Debug.Log(go);
//	}

	public TargetSystem() {
		family = FamilyManager.getFamily(new AllOfTypes(typeof(TargetPositionComponent)), new GameObjectStateMatcher(GameObjectStateMatcher.STATE.ACTIVE));
		family.addGameObjectsEnteredCallback(entered);
//		family.addGameObjectsExitedCallback(exited);

//		TEST FAILED
		GameObject go = EntityManager.createPrimitive(PrimitiveType.Cube);
		go.name = "test";
		EntityManager.addComponent<TargetPositionComponent>(go);
	}

	public override void process(int currentFrame){
		foreach (GameObject go in family) {
			TargetPositionComponent tpc = go.GetComponent<TargetPositionComponent>();
			Vector3 position = go.transform.position;

			if (position.x == tpc.x || position.y == tpc.y) {
				EntityManager.removeComponent<TargetPositionComponent>(go);
				//EntityManager.addComponent<TargetPositionComponent>(go, new { x = Random.Range (-3, 3), y = Random.Range(-3, 3) });
			}
		}
	}
}