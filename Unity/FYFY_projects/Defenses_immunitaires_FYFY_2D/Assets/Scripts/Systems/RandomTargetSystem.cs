using UnityEngine;
using FYFY;

public class RandomTargetSystem : FSystem {
	private Family _gameObjects = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AnyOfTags("Bactery", "Waste", "Toxin", "Virus", "StuckVirus"),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject gameObject in _gameObjects) {
			Target target = gameObject.GetComponent<Target>();

			if(gameObject.transform.position == target._target) {
				target._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range(-2.5f, 2.5f));
			}
		}
	}
}
