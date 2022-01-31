using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using FYFY_plugins.Monitoring;

public class InventoryManager : FSystem {
	private Family inInventoryObjectsFocused = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ComponentMonitoring)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY), new AnyOfLayers(5)); // Layer 5 == UI
	private Family highlighted = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Image)), new AnyOfLayers(5)); // Layer 5 == UI
	private Family notHighlighted = FamilyManager.getFamily(new NoneOfComponents(typeof(PointerOver)), new AllOfComponents(typeof(Image)), new AnyOfLayers(5)); // Layer 5 == UI
	private Family itemSelected = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentSelection), typeof(ComponentMonitoring)));
	private Family itemNotSelected = FamilyManager.getFamily(new NoneOfComponents(typeof(CurrentSelection)), new AllOfComponents(typeof(ComponentMonitoring)));

	protected override void onStart() {
		highlighted.addEntryCallback (onMouseEnter);
		notHighlighted.addEntryCallback(onMouseExit);
		itemSelected.addEntryCallback (onNewItemSelected);
		itemNotSelected.addEntryCallback(onOldItemSelected);
	}

	void onMouseEnter(GameObject addingGO){
		Image img = addingGO.GetComponent<Image> ();
		img.color = new Color(1f, 0.9f, 0f);
	}

	void onMouseExit(GameObject removingGO){
		Image img = removingGO.GetComponent<Image> ();
		img.color = Color.white;
	}

	void onNewItemSelected(GameObject newItem){
		GameObject child = newItem.transform.parent.GetChild (1).gameObject; // get second child
		if (!child.activeInHierarchy)
			GameObjectManager.setGameObjectState(child, true);
	}

	void onOldItemSelected (GameObject oldItem){
		GameObject child = oldItem.transform.parent.GetChild (1).gameObject; // get second child
		if (child.activeInHierarchy)
			GameObjectManager.setGameObjectState(child, false);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetMouseButtonDown (0)) {
			// Only one GO is under pointer
			GameObject focused_GO = inInventoryObjectsFocused.First();
			if (focused_GO != null){
				bool alreadySelected = false;
				// Reset selected items, only one selection is possible
				GameObject item = itemSelected.First();
				if (item != null){
					GameObjectManager.removeComponent<CurrentSelection> (item);
					// Get item monitor
					ComponentMonitoring cmItem = item.GetComponent<ComponentMonitoring>();
					if (item.GetInstanceID() == focused_GO.GetInstanceID()) {
						alreadySelected = true;
						// already selected => player deselect the item
						MonitoringManager.trace (cmItem, "turnOff", MonitoringManager.Source.PLAYER);
					} else {
						// player select another item  => System action to storeback current item
						MonitoringManager.trace (cmItem, "turnOff", MonitoringManager.Source.SYSTEM);
					}
				}
				if (!alreadySelected) {
					// Select current go
					GameObjectManager.addComponent<CurrentSelection> (focused_GO);
					// Get its monitor
					ComponentMonitoring cm = focused_GO.GetComponent<ComponentMonitoring>();
					// player select a new item
					MonitoringManager.trace(cm, "turnOn", MonitoringManager.Source.PLAYER);
				}
			}
		}
	}
}