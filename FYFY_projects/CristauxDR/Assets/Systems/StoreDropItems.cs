using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class StoreDropItems : FSystem {
	private Family inGameObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Takable), typeof(ComponentMonitoring)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family inGameFarObjects = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Takable), typeof(ComponentMonitoring)), new NoneOfComponents(typeof(Triggered3D)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family dropButton = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Image)), new AnyOfTags("DropButton"), new AnyOfLayers(5)); // Layer 5 == UI
	private Family hero = FamilyManager.getFamily(new AllOfComponents(typeof(Animator), typeof(Rigidbody), typeof(Controllable)));

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Only one GO could be under the pointer

			// Check if player clics on a GameObject near to the hero
			GameObject go = inGameObjects.First ();
			if (go != null) {
				Takable item = go.GetComponent<Takable> ();
				ComponentMonitoring cm = go.GetComponent<ComponentMonitoring> ();
				// move item to inventory
				GameObjectManager.setGameObjectState (item.linkedWith, true);
				GameObjectManager.setGameObjectState (go, false);
				// trace this action
				cm.trace("store", MonitoringManager.Source.PLAYER);
			}

			// Check if player clics on a GO far from the hero
			go = inGameFarObjects.First();
			if (go != null) {
				ComponentMonitoring cm = go.GetComponent<ComponentMonitoring> ();
				// trace this action
				cm.trace("store", MonitoringManager.Source.PLAYER, true);
			}
			
			// Check if palyer clics on a dropButton
			go = dropButton.First();
			if (go != null) {
				// get root gameobject and remove selection
				GameObject root = go.transform.parent.parent.gameObject;
				GameObjectManager.removeComponent<CurrentSelection> (root);
				// get brother game object of drop button (first child)
				GameObject brother = root.transform.GetChild(0).gameObject;
				Takable item = brother.GetComponent<Takable> ();
				GameObjectManager.setGameObjectState (item.linkedWith, true);
				GameObjectManager.setGameObjectState (brother, false);
				Transform itemPos = item.linkedWith.GetComponent<Transform> ();
				GameObject theHero = hero.First ();
				if (theHero != null) {
					Transform heroPos = theHero.GetComponent<Transform> ();
					itemPos.position = new Vector3 (heroPos.position.x, itemPos.position.y, heroPos.position.z);
				}
				// Try to access Monitor component of linked GO
				ComponentMonitoring cm = item.linkedWith.GetComponent<ComponentMonitoring>();
				if (cm != null)
					cm.trace("drop", MonitoringManager.Source.PLAYER);
			}
		}
	}
}