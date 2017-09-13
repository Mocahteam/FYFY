using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using monitoring;

public class StoreDropItems : FSystem {
	private Family inGameObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Takable), typeof(ComponentMonitoring)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family dropButton = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Image)), new AnyOfTags("DropButton"), new AnyOfLayers(5)); // Layer 5 == UI
	private Family hero = FamilyManager.getFamily(new AllOfComponents(typeof(Animator), typeof(Rigidbody), typeof(Controllable)));

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Parse all active GO takable in game
			foreach (GameObject go in inGameObjects) {
				Takable item = go.GetComponent<Takable> ();
				Triggered3D triggered = go.GetComponent<Triggered3D> ();
				ComponentMonitoring cm = go.GetComponent<ComponentMonitoring> ();
				// check if collision occurs with hero
				bool heroFound = false;
				foreach (GameObject target in triggered.Targets) {
					if (target.name == "HeroSprite") {
						heroFound = true;
						// move item to inventory
						GameObjectManager.setGameObjectState (item.linkedWith, true);
						GameObjectManager.setGameObjectState (go, false);
						cm.trace("store", TraceHandler.Source.PLAYER);
					}
				}
				if (!heroFound)
					cm.trace("store", TraceHandler.Source.PLAYER, true);
			}

			// Parse all active GO dropable
			foreach (GameObject go in dropButton) {
				// get root gameobject and remove selection
				GameObject root = go.transform.parent.parent.gameObject;
				GameObjectManager.removeComponent<CurrentSelection> (root);
				// get brother game object of drop button (first child)
				GameObject brother = root.transform.GetChild(0).gameObject;
				Takable item = brother.GetComponent<Takable> ();
				GameObjectManager.setGameObjectState (item.linkedWith, true);
				GameObjectManager.setGameObjectState (brother, false);
				Transform itemPos = item.linkedWith.GetComponent<Transform> ();
				foreach (GameObject theHero in hero) {
					Transform heroPos = theHero.GetComponent<Transform> ();
					itemPos.position = new Vector3 (heroPos.position.x, itemPos.position.y, heroPos.position.z);
				}
				// Try to access Monitor component of linked GO
				ComponentMonitoring cm = item.linkedWith.GetComponent<ComponentMonitoring>();
				if (cm != null)
					cm.trace("drop", TraceHandler.Source.PLAYER);
			}
		}
	}
}