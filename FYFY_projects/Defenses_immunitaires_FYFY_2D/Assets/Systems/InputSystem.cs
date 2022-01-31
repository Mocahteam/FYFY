using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

// Ce système se charge de la sélection des unitées contrôlables par le joueur
public class InputSystem : FSystem {
	public KeyCode _selectionButton = KeyCode.Mouse0; 
	public KeyCode _actionButton = KeyCode.Mouse1;
	public KeyCode _multipleSelectionKey = KeyCode.LeftControl;

	private Family _mouseOverAndSelected = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(PointerOver), typeof(Selected)),
		new NoneOfComponents(typeof(Death))
	);
	private Family _mouseOverAndNotSelected = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(PointerOver)),
		new NoneOfComponents(typeof(Death), typeof(Selected))
	);
	private Family _selected = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY),
		new AllOfComponents(typeof(Selected), typeof(Target)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onProcess(int currentFrame) {
		if (Input.GetKeyDown(_actionButton)) {
			int acc = 0;
			// parcours des objets sélectionnés
			foreach (GameObject gameObject in _selected) {
				Vector3 worldPoint = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 5.4f));

				if (acc++ > 0) {
					// pour eviter la superposition en cas de sélection multiples
					worldPoint.x += Random.Range (-0.25f, 0.25f);
					worldPoint.y += Random.Range (-0.25f, 0.25f);
				}
				// défintion de son objectif de déplacement
				gameObject.GetComponent<Target> ()._target = worldPoint;
			}
		} else if(Input.GetKey(_multipleSelectionKey)) {
			if(Input.GetKeyDown(_selectionButton))
			{
				// sélectionner les objets non sélectionnés
				foreach (GameObject gameObject in _mouseOverAndNotSelected)
					GameObjectManager.addComponent<Selected>(gameObject);
				// désélectionner les objets sélectionnés
				foreach (GameObject gameObject in _mouseOverAndSelected)
					GameObjectManager.removeComponent<Selected>(gameObject);
			}
		} else if(Input.GetKeyDown(_selectionButton) && _mouseOverAndSelected.Count == 0 && _mouseOverAndNotSelected.Count == 0) {
			// click dans le vide => on déselectionne tout le monde
			foreach(GameObject gameObject in _selected)
				GameObjectManager.removeComponent<Selected>(gameObject);
		} else if(Input.GetKeyDown(_selectionButton) && _mouseOverAndNotSelected.Count != 0) {
			// click sur une cellule non sélectionnée => déselectionner toutes les cellules sélectionnées
			foreach (GameObject gameObject in _selected)
				GameObjectManager.removeComponent<Selected> (gameObject);
			// sélectionner la nouvelle cellule
			foreach (GameObject gameObject in _mouseOverAndNotSelected)
				GameObjectManager.addComponent<Selected> (gameObject);
		}
	}
}