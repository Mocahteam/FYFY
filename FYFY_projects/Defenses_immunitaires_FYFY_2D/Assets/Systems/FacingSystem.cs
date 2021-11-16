using UnityEngine;
using FYFY;

// Ce système se charge d'orienter les bactéries vers la position qu'elles cherchent à atteindre
public class FacingSystem : FSystem {
	private Family f_facing = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(Facing)),
		new AllOfComponents(typeof(Target)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach (GameObject facing_go in f_facing) {
			Vector3 position = facing_go.transform.position;
			Vector3 target = facing_go.GetComponent<Target>()._target;

			if(position == target)
				continue;

			Vector3 headDirection = facing_go.transform.right;
			Vector3 targetDirection = target - position;

			float desiredAngle = Vector3.Angle(headDirection, targetDirection);
			if(desiredAngle > 1f){
				if (Vector3.Cross (headDirection, targetDirection).z < 0f)
					desiredAngle = -desiredAngle;

				float effectiveAngle = desiredAngle * Time.deltaTime * facing_go.GetComponent<Facing>()._rotationSpeed;
				if(System.Math.Abs (desiredAngle) < System.Math.Abs (effectiveAngle))
					effectiveAngle = desiredAngle;

				facing_go.transform.RotateAround(position, facing_go.transform.forward, effectiveAngle);
			}
		}
	}
}
