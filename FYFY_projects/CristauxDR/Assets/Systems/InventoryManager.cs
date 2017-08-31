using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;

public class InventoryManager : FSystem {
	private Family inInventoryObjects = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(Takable)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED), new AnyOfLayers(5)); // Layer 5 == UI
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
			// Parse all active GO takable in inventory
			foreach (GameObject go in inInventoryObjects) {
				bool alreadySelected = false;
				// Reset all selected items
				foreach (GameObject item in itemSelected) {
					GameObjectManager.removeComponent<CurrentSelection> (item);
					if (item == go.transform.parent.gameObject)
						alreadySelected = true;
				}
				if (!alreadySelected)
					// Select current go
					GameObjectManager.addComponent<CurrentSelection>(go.transform.parent.gameObject);
			}
		}
	}
}