using UnityEngine;

[AddComponentMenu("")] // hide in Component list
public class MousePressedOn : MonoBehaviour {
}

public class MouseSystem : UECS.System {
	Family _family;

	public MouseSystem() { // 3D OR 2D
		_family = FamilyManager.getFamily(new AllOfTypes(typeof(AbleToCatchMouseEvents)), new GameObjectStateMatcher(GameObjectStateMatcher.STATE.ACTIVE));
	}

	public override void process(int currentFrame) {
		Vector3 mousePosition = Input.mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(mousePosition);
		RaycastHit hit;

		if(Physics.Raycast(ray, out hit) == true){
			GameObject gameObject = hit.transform.gameObject;
			int gameObjectId = gameObject.GetInstanceID();

			if(_family._gameObjectIds.Contains(gameObjectId) == false)
				return;

//			switch
//			gameObject.AddComponent<>();

			// how to deal with family ? update now or later ?

			//			add to list

			// get all raycasted object ? -> ordonné du plus proche au plus loin (comment traverse le rayon)
		}
	}
}