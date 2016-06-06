using UnityEngine;
using FYFY;

public class MovingSystem : FSystem {
	private Family _gameObjects = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(Target), typeof(Speed)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject gameObject in _gameObjects) {
			Vector3 position = gameObject.transform.position;
			Vector3 target = gameObject.GetComponent<Target>()._target;
			float speed = gameObject.GetComponent<Speed>()._speed;

			if (position != target) {
				gameObject.transform.position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
			}
		}
	}
}
