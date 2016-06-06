using UnityEngine;
using FYFY;

public class FacingSystem : FSystem {
	private Family _gameObjects = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(Facing)),
		new AllOfComponents(typeof(Target)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject gameObject in _gameObjects) {
			Vector3 position = gameObject.transform.position;
			Vector3 target = gameObject.GetComponent<Target>()._target;

			if(position == target)
				continue;

			Vector3 headDirection = gameObject.transform.right;
			Vector3 targetDirection = target - position;

			float desiredAngle = Vector3.Angle(headDirection, targetDirection);
			if(desiredAngle > 1f){
				if (Vector3.Cross (headDirection, targetDirection).z < 0f)
					desiredAngle = -desiredAngle;

				float effectiveAngle = desiredAngle * Time.deltaTime * gameObject.GetComponent<Facing>()._rotationSpeed;
				if(System.Math.Abs (desiredAngle) < System.Math.Abs (effectiveAngle))
					effectiveAngle = desiredAngle;

				gameObject.transform.RotateAround(position, gameObject.transform.forward, effectiveAngle);
			}
		}
	}
}
