using UnityEngine;
using FYFY;

// Ce système détermine la prochaine cible à atteindre pour les agents qui se déplacent seuls
public class RandomTargetSystem : FSystem {
	private Family f_movingAgents = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AnyOfTags("Bactery", "Waste", "Toxin", "Virus", "StuckVirus"),
		new AllOfComponents(typeof(Target)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach (GameObject agent in f_movingAgents) {
			Target target = agent.GetComponent<Target>();

			if(agent.transform.position == target._target)
				target._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range(-2.5f, 2.5f));
		}
	}
}
