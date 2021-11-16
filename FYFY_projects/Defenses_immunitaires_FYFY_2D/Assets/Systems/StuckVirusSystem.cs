using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using System.Collections.Generic;

// Ce système agglutine les virus entre eux
public class StuckVirusSystem : FSystem {
	private Family f_stuckVirus = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AnyOfTags("StuckVirus"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		// Si deux virus agglutinés se croisent il ne faut pas retraiter celui qui a été mis en fils
		HashSet<GameObject> alreadySeen = new HashSet<GameObject>();

		foreach(GameObject stuckVirus_go in f_stuckVirus) {
			if(alreadySeen.Contains(stuckVirus_go))
				continue;

			alreadySeen.Add(stuckVirus_go);

			// parcours des agent en contact avec ce virus
			Triggered2D t2d = stuckVirus_go.GetComponent<Triggered2D>();
			foreach(GameObject target in t2d.Targets) {
				// On ne traite que les virus et les virus agglutinés
				if((target.tag != "StuckVirus" && target.tag != "Virus") || alreadySeen.Contains(target))
					continue;

				alreadySeen.Add(target);
				// positionner cet agent à proximité du virus maître
				target.transform.position = new Vector3(stuckVirus_go.transform.position.x + Random.Range(-0.1f, 0.1f), stuckVirus_go.transform.position.y + Random.Range(-0.1f, 0.1f), stuckVirus_go.transform.position.z);
				// mettre cet agent comme fils du virus maître
				GameObjectManager.setGameObjectParent(target, stuckVirus_go, true);
				// désactiver cet agent
				GameObjectManager.setGameObjectTag(target, "Untagged");
				GameObjectManager.setGameObjectLayer(target, LayerMask.NameToLayer("Default"));
				GameObjectManager.removeComponent<Target>(target);
				GameObjectManager.removeComponent<Speed>(target);
				GameObjectManager.removeComponent<TriggerSensitive2D>(target);
				GameObjectManager.removeComponent<PolygonCollider2D>(target);
				GameObjectManager.removeComponent<Rigidbody2D>(target);
				if (target.tag == "Virus")
					GameObjectManager.removeComponent<Virus>(target);
				else
					// si le virus était déjà un stuckVirus, associer ses enfants au nouveau maître
					foreach (Transform child in target.transform)
						GameObjectManager.setGameObjectParent(child.gameObject, stuckVirus_go, true);
				break;
			}
		}
	}
}
