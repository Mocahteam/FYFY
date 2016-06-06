using UnityEngine;
using FYFY;
using FYFY_plugins.Mouse;

public class InputSystem : FSystem {
	private KeyCode _selectionButton = KeyCode.Mouse0;
	private KeyCode _actionButton = KeyCode.Mouse1;
	private KeyCode _multipleSelectionKey = KeyCode.LeftControl;

	private Family _mouseOver = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(MouseOver)),
		new NoneOfComponents(typeof(Death))
	);
	private Family _selected = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(Selected)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		if (Input.GetKeyDown(_actionButton) == true) {
			int acc = 0;

			foreach (GameObject gameObject in _selected) {
				Vector3 worldPoint = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 5.4f));

				if (acc++ > 0) {
					worldPoint.x += Random.Range (-0.25f, 0.25f); // pour eviter la superposition 
					worldPoint.y += Random.Range (-0.25f, 0.25f);
				}

				gameObject.GetComponent<Target> ()._target = worldPoint;
			}
		} else if(Input.GetKey(_multipleSelectionKey) == true) {
			if(Input.GetKeyDown(_selectionButton) && _mouseOver.Count != 0) {
				foreach (GameObject gameObject in _mouseOver) {
					if (_selected.contains (gameObject.GetInstanceID ()) == false) {
						GameObjectManager.addComponent<Selected> (gameObject);
					}
				}
			}
		} else if(Input.GetKeyDown(_selectionButton) && _mouseOver.Count == 0) {
			foreach(GameObject gameObject in _selected) {
				GameObjectManager.removeComponent<Selected>(gameObject);
			}
		} else if(Input.GetKeyDown(_selectionButton) && _mouseOver.Count != 0) {
			foreach (GameObject gameObject in _selected) {
				GameObjectManager.removeComponent<Selected> (gameObject);
			}
			
			foreach (GameObject gameObject in _mouseOver) {
				if (_selected.contains (gameObject.GetInstanceID ()) == false) {
					GameObjectManager.addComponent<Selected> (gameObject);
				}
			}
		}
	}
}