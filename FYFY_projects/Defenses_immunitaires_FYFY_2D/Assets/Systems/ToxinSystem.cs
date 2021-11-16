using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

// Ce système gère les dommages causés par les toxines
public class ToxinSystem : FSystem {
	private Family _toxins = FamilyManager.getFamily(
		new AnyOfTags("Toxin"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		foreach (GameObject toxin in _toxins) {
			Triggered2D t2D = toxin.GetComponent<Triggered2D>();
			float damages = toxin.GetComponent<Toxin>()._damages;

			foreach(GameObject other in t2D.Targets) {
				if((other.tag == "StructureCell" || other.tag == "BCell" || other.tag == "TCell") && other.GetComponent<Death>() == null) {
					GameObjectManager.addComponent<Damage>(other, new { damages = damages });
					GameObjectManager.addComponent<Death>(toxin);
					break;
				}
			}
		}
	}
}
