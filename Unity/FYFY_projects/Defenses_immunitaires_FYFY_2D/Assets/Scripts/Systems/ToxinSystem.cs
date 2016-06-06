using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
using System.Collections.Generic;

public class ToxinSystem : FSystem {
	private Family _toxins = FamilyManager.getFamily(
		new AnyOfTags("Toxin"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame){}

	protected override void onResume(int currentFrame){}

	protected override void onProcess(int currentFrame) {
		HashSet<GameObject> dead = new HashSet<GameObject>();

		foreach (GameObject toxin in _toxins) {
			Triggered2D t2D = toxin.GetComponent<Triggered2D>();
			float damages = toxin.GetComponent<Toxin>()._damages;

			foreach(GameObject other in t2D.Others) {
				if((other.tag == "StructureCell" || other.tag == "BCell" || other.tag == "TCell") && (other.GetComponent<Death>() == null && dead.Contains(other) == false)) {
					if(Health.hit(other, damages))
						dead.Add(other);
					
					GameObjectManager.addComponent<Death>(toxin);
					break;
				}
			}
		}
	}
}
