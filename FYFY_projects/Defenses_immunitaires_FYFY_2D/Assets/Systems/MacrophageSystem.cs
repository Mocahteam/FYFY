using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

// Ce système gère la phagocytose des macrophage
public class MacrophageSystem : FSystem {
	private Family f_macrophages = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(Triggered2D), typeof(Macrophage)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach(GameObject macrophage_go in f_macrophages) {
			int phagocytoseDamages = macrophage_go.GetComponent<Macrophage>()._phagocytoseDamages;

			foreach(GameObject target in macrophage_go.GetComponent<Triggered2D>().Targets) {
				// Ne pas traiter les cibles en train de mourrir
				if(target.GetComponent<Death>() != null)
					continue;
				// destruction des bactéries, déchets, toxines et virus agglutiné
				if (target.tag == "Bactery" || target.tag == "Waste" || target.tag == "Toxin" || target.tag == "StuckVirus") {
					GameObjectManager.addComponent<Death>(target);
					// Si c'est un virus agglutiné, faire mourrir aussi tous ces enfants
					if (target.tag == "StuckVirus")
					{
						foreach (Transform child in target.transform)
						{
							GameObjectManager.setGameObjectParent(child.gameObject, null, true);
							GameObjectManager.addComponent<Death>(child.gameObject);
						}
					}
					// appliquer les dégats de phagocytose
					GameObjectManager.addComponent<Damage>(macrophage_go, new { damages = phagocytoseDamages });
				}
			}
		}
	}
}
