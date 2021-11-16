using FYFY;
using UnityEngine;

// Les bactéries produisent des toxines à intervale régulier. Ce système se charge de générer des toxines pour chaque bactérie
public class BacterySystem : FSystem {
	private Family f_bacteries = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(Bactery)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach(GameObject bactery in f_bacteries) {
			Bactery bacteryComponent = bactery.GetComponent<Bactery>();
			// prise en compte du temps passé
			bacteryComponent._reloadProgress += Time.deltaTime;

			if (bacteryComponent._reloadProgress >= bacteryComponent._reloadTime) {
				bacteryComponent._reloadProgress = 0;
				// génération d'une toxine
				GameObject toxin = Object.Instantiate (bacteryComponent._toxinPrefab);
				// positionnement de la toxine sous la bactérie
				toxin.transform.position = bactery.transform.position;
				// définition d'une position à atteindre pour faire dériver la toxine
				toxin.GetComponent<Target>()._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range (-2.5f, 2.5f));
				// définition des domages de la toxine en fonction de la bactérie
				toxin.GetComponent<Toxin>()._damages = bacteryComponent._toxinDamages;

				GameObjectManager.bind (toxin);
			}
		}
	}
}
