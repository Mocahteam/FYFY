using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using FYFY_plugins.Monitoring;

public class InventoryManager : FSystem {
	private Family inInventoryObjectsFocused = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ComponentMonitoring)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY), new AnyOfLayers(5)); // Layer 5 == UI
	private Family highlightable = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Image)), new AnyOfLayers(5)); // Layer 5 == UI
	private Family itemSelected = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentSelection), typeof(ComponentMonitoring)));
	private Dictionary<int, GameObject> goId2GO = new Dictionary<int, GameObject> ();

	public InventoryManager () {
		highlightable.addEntryCallback (onMouseEnter);
		highlightable.addExitCallback (onMouseExit);
		itemSelected.addEntryCallback (onNewItemSelected);
		itemSelected.addExitCallback (onOldItemSelected);
	}

	void onMouseEnter(GameObject addingGO){
		Image img = addingGO.GetComponent<Image> ();
		img.color = new Color(1f, 0.9f, 0f);
		if (!goId2GO.ContainsKey(addingGO.GetInstanceID ()))
			goId2GO.Add (addingGO.GetInstanceID (), addingGO);
	}

	void onMouseExit(int removingGOid){
		if (goId2GO.ContainsKey(removingGOid) && goId2GO [removingGOid] != null) {
			Image img = goId2GO [removingGOid].GetComponent<Image> ();
			img.color = Color.white;
		} else
			goId2GO.Remove (removingGOid);
	}

	void onNewItemSelected(GameObject newItem){
		GameObject child = newItem.transform.parent.GetChild (1).gameObject; // get second child
		if (!child.activeInHierarchy)
			child.SetActive (true);
		if (!goId2GO.ContainsKey(newItem.GetInstanceID ()))
			goId2GO.Add (newItem.GetInstanceID (), newItem);
	}

	void onOldItemSelected (int oldItemId){
		if (goId2GO.ContainsKey(oldItemId) && goId2GO [oldItemId] != null) {
			GameObject child = goId2GO [oldItemId].transform.parent.GetChild (1).gameObject; // get second child
			if (child.activeInHierarchy)
				child.SetActive (false);
		} else
			goId2GO.Remove (oldItemId);

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