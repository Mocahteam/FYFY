using UnityEngine;
using FYFY;

// Ce système déplace les agents vers leur cible
public class MovingSystem : FSystem {
	private Family f_movingAgents = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(Target), typeof(Speed)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach (GameObject agent in f_movingAgents) {
			Vector3 position = agent.transform.position;
			Vector3 target = agent.GetComponent<Target>()._target;
			float speed = agent.GetComponent<Speed>()._speed;

			if (position != target)
				agent.transform.position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
		}
	}
}
