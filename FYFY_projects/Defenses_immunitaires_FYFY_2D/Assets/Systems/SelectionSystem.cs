using FYFY;
using UnityEngine;

// Ce système met à jour l'apparence d'une cellule lorsqu'elle est (dé)sélectionnée
public class SelectionSystem : FSystem {
	private Family f_livingSelected = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
			new AllOfComponents(typeof(Selected), typeof(SpriteRenderer)),
			new NoneOfComponents(typeof(Death))
		);
	private Family f_livingUnselected = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
			new AllOfComponents(typeof(SpriteRenderer)),
			new NoneOfComponents(typeof(Death), typeof(Selected))
		);

	public SelectionSystem() {
		// Ajout d'une callback lorsqu'une cellule est sélectionnée
		f_livingSelected.addEntryCallback(onNewCellSelected);
		// Ajout d'une callback lorsqu'une cellule est dé-sélectionnée
		f_livingUnselected.addEntryCallback(onNewCellUnselected);
	}

	private void onNewCellSelected(GameObject go)
    {
		Material material = go.GetComponent<Renderer>().material;
		material.color = new Vector4(
			material.color.r,
			material.color.g,
			material.color.b,
			0.75f
		);
	}

	private void onNewCellUnselected(GameObject go)
	{
		Material material = go.GetComponent<Renderer>().material;
		material.color = new Vector4(
			material.color.r,
			material.color.g,
			material.color.b,
			1f
		);
	}
}