using FYFY;
using UnityEngine;

public class BacterySystem : FSystem {
	private Family _bacteries = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AnyOfTags("Bactery"),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach(GameObject bactery in _bacteries) {
			Bactery bacteryComponent = bactery.GetComponent<Bactery>();

			bacteryComponent._reloadProgress += Time.deltaTime;
			if (bacteryComponent._reloadProgress >= bacteryComponent._reloadTime) {
				bacteryComponent._reloadProgress = 0;

				GameObject toxin = GameObjectManager.instantiatePrefab("Toxin");
				toxin.transform.position = bactery.transform.position;
				toxin.GetComponent<Target>()._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range (-2.5f, 2.5f));
				toxin.GetComponent<Toxin>()._damages = bacteryComponent._toxinDamages;
			}
		}
	}
}
