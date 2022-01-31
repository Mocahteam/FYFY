using UnityEngine;
using FYFY;

// Ce système gère la production de virus dans les cellules infectées, les dommages causés par le virus et l'apparence de la cellule en fonction de son état (infecté ou non)
public class InfectionSystem : FSystem {
	private Family f_infected = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
			new AllOfComponents(typeof(Infected), typeof(Health), typeof(SpriteRenderer)),
			new NoneOfComponents(typeof(Death))
		);

	protected override void onStart() {
		// gestion des cellules défà infectées à l'initialisation
		foreach (GameObject infected_go in f_infected)
			setInfected(infected_go);

		// ajout d'une callback pour chaque nouvelle cellule infectée
		f_infected.addEntryCallback(setInfected);
	}

	private void setInfected(GameObject newInfected)
    {
		Health health = newInfected.GetComponent<Health>();
		SpriteRenderer sr = newInfected.GetComponent<SpriteRenderer>();
		sr.sprite = health._infectedSprite;
	}

	protected override void onProcess(int currentFrame) {
		// ajouter des dégats à chaque cellule infectée
		foreach (GameObject infected_go in f_infected) {
			Infected infected = infected_go.GetComponent<Infected>();
			// produire de nouveaux dommages sur la cellule
			GameObjectManager.addComponent<Damage>(infected_go, new { damages = infected._virusProperties._damages * Time.deltaTime });
			// faire progresser la progression de la production des virus
			infected._virusProductionProgress += Time.deltaTime;
			if (infected._virusProductionProgress >= infected._virusProperties._productionTime) {
				++infected._virusNumberToCreate;
				infected._virusProductionProgress -= infected._virusProperties._productionTime;
			}
		}
	}
}