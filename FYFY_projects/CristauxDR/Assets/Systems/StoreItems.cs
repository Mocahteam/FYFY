using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;

public class StoreItems : FSystem {
	private Family inGameObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(PointerOver), typeof(Takable), typeof(ComponentMonitoring)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

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
				MonitoringManager.trace(cm, "perform", MonitoringManager.Source.PLAYER);
			}
		}
	}
}