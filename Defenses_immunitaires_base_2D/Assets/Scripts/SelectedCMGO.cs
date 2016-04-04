using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SelectedCMGO : MonoBehaviour {
	public static List<SelectedCMGO> selectedComponents = new List<SelectedCMGO>();

	private Material material;

	void Awake () {
		selectedComponents.Add (this);

		material = this.gameObject.GetComponent<Renderer>().material;
		material.color = new Vector4 (material.color.r, material.color.g, material.color.b, 0.75f);
	}

	void OnDestroy(){
		selectedComponents.Remove (this);

		material.color = new Vector4 (material.color.r, material.color.g, material.color.b, 1f);
	}

	public static void unselectAllSelectedCMGO(){
		for (int i = 0; i < selectedComponents.Count; ++i)
			Destroy(selectedComponents[i]);
	}
}
