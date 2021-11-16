using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

// Ce système gère l'infection d'une cellule par un virus
public class VirusSystem : FSystem {
	private Family _virus = FamilyManager.getFamily(
		new AnyOfLayers(LayerMask.NameToLayer("Virus")),
		new AllOfComponents(typeof(Triggered2D), typeof(Virus)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach(GameObject virus in _virus) {
			Triggered2D t2D = virus.GetComponent<Triggered2D>();
			VirusProperties virusProperties = virus.GetComponent<Virus>()._properties;

			foreach(GameObject other in t2D.Targets) {
				bool dead = other.GetComponent<Death>() != null;
				bool infected = other.GetComponent<Infected>() != null;
				bool tagIsValid = (other.tag == "StructureCell" || other.tag == "BCell" || other.tag == "TCell" || other.tag == "Macrophage" || other.tag == "Bactery");

				if(!dead && !infected && tagIsValid && Random.value < virusProperties._infectionChance) {
					// Infecter la cible
					GameObjectManager.addComponent<Infected>(other, new { _virusProperties = virusProperties });
					// Détruire le virus
					GameObjectManager.unbind(virus);
					Object.Destroy (virus);
					break;
				}
			}
		}
	}
}
