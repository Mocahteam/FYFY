using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

// Ce système gère les TCell qui tuent les cellules infectées au contact
public class TCellSystem : FSystem {
	private Family _tcells = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AnyOfTags("TCell"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {

		foreach(GameObject tcell in _tcells) {
			Triggered2D t2d = tcell.GetComponent<Triggered2D>();
			foreach(GameObject other in t2d.Targets) {
				// si la cellule touchée est en train de mourrir, on ne fait rien
				if(other.GetComponent<Death>() != null)
					continue;
				// vérifier si la cellule est infectée
				Infected infected = other.GetComponent<Infected>();
				if( infected != null) {
					// on la tue en faisant un max de dégâts
					infected._virusNumberToCreate = 0; // ne pas générer de virus
					GameObjectManager.addComponent<Damage>(other, new {damages = float.PositiveInfinity });
				}
			}
		}
	}
}
