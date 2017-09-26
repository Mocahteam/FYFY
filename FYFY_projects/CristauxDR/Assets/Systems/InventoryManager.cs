using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using FYFY_plugins.Monitoring;

public class InventoryManager : FSystem {
	private Family inInventoryObjectsFocused = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Takable)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED), new AnyOfLayers(5)); // Layer 5 == UI
	private Family highlightable = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Image)), new AnyOfLayers(5)); // Layer 5 == UI
	private Family itemSelected = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentSelection)), new AllOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD));
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
		GameObject child = newItem.transform.GetChild (1).gameObject; // get second child
		if (!child.activeInHierarchy)
			child.SetActive (true);
		if (!goId2GO.ContainsKey(newItem.GetInstanceID ()))
			goId2GO.Add (newItem.GetInstanceID (), newItem);
	}

	void onOldItemSelected (int oldItemId){
		if (goId2GO.ContainsKey(oldItemId) && goId2GO [oldItemId] != null) {
			GameObject child = goId2GO [oldItemId].transform.GetChild (1).gameObject; // get second child
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
				// Get ingame linked GO
				GameObject inGame = focused_GO.GetComponent<Takable>().linkedWith;
				// Get its monitor
				ComponentMonitoring cm = null;
				if (inGame != null)
					cm = inGame.GetComponent<ComponentMonitoring>();

				bool alreadySelected = false;
				// Reset selected items, only one selection is possible
				GameObject item = itemSelected.First();
				if (item != null){
					GameObjectManager.removeComponent<CurrentSelection> (item);
					if (item == focused_GO.transform.parent.gameObject) {
						alreadySelected = true;
						if (cm != null)
							// already selected => player deselect the item
							cm.trace ("storeBack", MonitoringManager.Source.PLAYER);
					} else {
						// Get item monitor
						ComponentMonitoring cmItem = item.transform.GetChild(0).GetComponent<Takable>().linkedWith.GetComponent<ComponentMonitoring>();
						if (cmItem != null)
							// player select another item  => System action to storeback current item
							cmItem.trace ("storeBack", MonitoringManager.Source.SYSTEM);
					}
				}
				if (!alreadySelected) {
					// Select current go
					GameObjectManager.addComponent<CurrentSelection> (focused_GO.transform.parent.gameObject);
					if (cm != null)
						// player select a new item
						cm.trace("get", MonitoringManager.Source.PLAYER);
				}
			}
		}
	}
}