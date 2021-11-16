using UnityEngine;
using FYFY;

// Contrôle l'animation de la mort et la production des déchets et éventuellement de virus
public class DeathSystem : FSystem {
	private Family f_death = FamilyManager.getFamily(new AllOfComponents(typeof(Death), typeof(SpriteRenderer)));

	protected override void onProcess(int currentFrame) {
		foreach (GameObject death_go in f_death) {
			Death death = death_go.GetComponent<Death>();
			// Faire progresser l'animation
			death._progress += Time.deltaTime;
			if (death._progress < death._duration) {
				Material material = death_go.GetComponent<Renderer>().material;
				Color color = material.color;
				float a = 1f - (death._progress / death._duration);
				material.color = new Color(color.r, color.g, color.b, a);
			} else {
				// animation terminée
				// génération des déchets
				for (int i = 0; i < death._wastesNumber; ++i) {
					GameObject waste = Object.Instantiate(death._wastePrefab);
					waste.transform.position = death_go.transform.position;
					// définition d'une position cible pour les faire dériver
					waste.GetComponent<Target>()._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range (-2.5f, 2.5f));
					waste.transform.Rotate(new Vector3(0f, 0f, Random.Range(-180f, 180f)));
					GameObjectManager.bind (waste);
				}
				// génération des virus
				for (int i = 0; i < death._virusNumber; ++i) {
					GameObject virus = Object.Instantiate(death._virusProperties._virusPrefab);
					virus.transform.position = death_go.transform.position;
					// définition d'une position cible pour les faire dériver
					virus.GetComponent<Target>()._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range (-2.5f, 2.5f));
					virus.GetComponent<Virus>()._properties = death._virusProperties;
					GameObjectManager.bind (virus);
				}

				GameObjectManager.unbind(death_go);
				Object.Destroy (death_go);
			}
		}
	}
}
