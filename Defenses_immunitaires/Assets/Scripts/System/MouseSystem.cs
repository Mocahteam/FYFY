using UnityEngine;
using FYFY;

[AddComponentMenu("")]
public class MousePressedOn : MonoBehaviour {
}

public class MouseSystem : FSystem {
	Family _pointableFamily;
	Family _pointedFamily;

	public MouseSystem() {
		_pointableFamily = FamilyManager.getFamily(new AllOfTypes(typeof(AbleToCatchMouseEvents)), new GameObjectStateMatcher(GameObjectStateMatcher.STATE.ACTIVE));
		_pointedFamily   = FamilyManager.getFamily(new AllOfTypes(typeof(MousePressedOn)));
	}

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame) {
	}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject gameObject in _pointedFamily)
			GameObjectManager.removeComponent<MousePressedOn>(gameObject);

		Vector3 mousePosition = Input.mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(mousePosition);
		RaycastHit hit;

		if(Physics.Raycast(ray, out hit) == true) {
			GameObject gameObject = hit.transform.gameObject;
			int gameObjectId = gameObject.GetInstanceID();

			if(_pointableFamily.contains(gameObjectId) == false)
				return;
			
			if (Input.GetMouseButtonDown(0))
				GameObjectManager.addComponent<MousePressedOn>(gameObject);
		}
	}
}

// 3D OR 2D
// get all raycasted object ? -> ordonné du plus proche au plus loin (comment traverse le rayon)