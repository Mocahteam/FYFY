using UnityEngine;

public class ControllableMovingGO : MovingGO {
	void OnMouseDown(){ // LEFT CLICK
		if (!Input.GetKey(KeyCode.LeftControl))
			SelectedCMGO.unselectAllSelectedCMGO();

		if (this.gameObject.GetComponent<SelectedCMGO>() == null)
			this.gameObject.AddComponent<SelectedCMGO>();
	}
}