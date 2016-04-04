using UnityEngine;

[DisallowMultipleComponent]
public class InputManager : MonoBehaviour {
	private KeyCode actionButton = KeyCode.Mouse1;
	private KeyCode resetSelectionKey = KeyCode.R;

	void Update () {
		if(Input.GetKeyDown(resetSelectionKey))
			SelectedCMGO.unselectAllSelectedCMGO();
		else if (Input.GetKeyDown (actionButton)) {
			Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5.4f));

			if (worldPoint.x < -3.5f)
				worldPoint.x = -3.5f;
			else if (worldPoint.x > 3.5f)
				worldPoint.x = 3.5f;

			if (worldPoint.y < -2.5f)
				worldPoint.y = -2.5f;
			else if (worldPoint.y > 2.5f)
				worldPoint.y = 2.5f;

			for (int i = 0; i < SelectedCMGO.selectedComponents.Count; ++i) {
				ControllableMovingGO cmgoc = SelectedCMGO.selectedComponents[i].gameObject.GetComponent<ControllableMovingGO> ();

				if (i > 0) { // décaller légèrement la cible si plusieurs pour éviter la superposition
					worldPoint.x += Random.Range(-0.25f, 0.25f);
					worldPoint.y += Random.Range(-0.25f, 0.25f);
				}

				cmgoc.target = worldPoint;
			}
		}
	}
}