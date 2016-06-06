using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
using System.Collections.Generic;

public class StuckVirusSystem : FSystem {
	private Family _stuckVirus = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AnyOfTags("StuckVirus"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		HashSet<GameObject> stuckVirusGroups = new HashSet<GameObject>();

		foreach(GameObject gameObject in _stuckVirus) {
			if(stuckVirusGroups.Contains(gameObject))
				continue;

			Triggered2D t2d = gameObject.GetComponent<Triggered2D>();
			foreach(GameObject other in t2d.Others) {
				if(other.tag != "StuckVirus" || stuckVirusGroups.Contains(other))
					continue;

				stuckVirusGroups.Add(gameObject);
				stuckVirusGroups.Add(other);

				GameObjectManager.setGameObjectParent(other, gameObject, true);
				GameObjectManager.setGameObjectTag(gameObject, "StuckVirusGroup");
				GameObjectManager.setGameObjectLayer(gameObject, LayerMask.NameToLayer("StuckVirusGroup"));
				GameObjectManager.setGameObjectTag(other, "StuckVirusGroup");
				GameObjectManager.setGameObjectLayer(other, LayerMask.NameToLayer("StuckVirusGroup"));
				GameObjectManager.removeComponent<Target>(other);
				GameObjectManager.removeComponent<Speed>(other);
				break;
			}
		}
	}
}
