using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
using System.Collections.Generic;

public class MacrophageSystem : FSystem {
	private Family _gameObjects = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AnyOfTags("Macrophage"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		HashSet<GameObject> dead = new HashSet<GameObject>();

		foreach(GameObject gameObject in _gameObjects) {
			int phagocytoseDamages = gameObject.GetComponent<Macrophage>()._phagocytoseDamages;

			foreach(GameObject other in gameObject.GetComponent<Triggered2D>().Targets) { // attention car Triggered2D contient TOUS les GO avec qui tu es en contact (y compris les death etc)
				if(other.GetComponent<Death>() != null || dead.Contains(other) == true)
					continue;
				
				if (other.tag == "Bactery") {
					if(Health.hit(other, float.PositiveInfinity))
						dead.Add (other);
					
					Health.hit(gameObject, phagocytoseDamages);
				} else if (other.tag == "Waste" || other.tag == "Toxin" || other.tag == "StuckVirus") {
					GameObjectManager.addComponent<Death>(other);
					dead.Add(other);

					Health.hit(gameObject, phagocytoseDamages);
				} else if (other.tag == "StuckVirusGroup") {
					GameObjectManager.addComponent<Death>(other);
					dead.Add(other);

					if (other.transform.parent != null) {
						GameObject parent = other.transform.parent.gameObject;
						GameObjectManager.setGameObjectParent(other, null, true);
						GameObjectManager.addComponent<Death>(parent);
						dead.Add(parent);
					} else {
						GameObject child = other.transform.GetChild(0).gameObject;
						GameObjectManager.setGameObjectParent(child, null, true);
						GameObjectManager.addComponent<Death>(child);
						dead.Add(child);
					}

					Health.hit(gameObject, phagocytoseDamages);
				}
			}
		}
	}
}
