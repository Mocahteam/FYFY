using UnityEngine;

[DisallowMultipleComponent]
public class BCell : MonoBehaviour {
	public enum TYPE {NAIVE, BACTERIAL, VIRAL};

	public TYPE type;
	public float recognitionTime = 1;

	public float bacteryRecognitionProgress = 0f, virusRecognitionProgress = 0f;
	[HideInInspector]
	public GameObject actionZone, recognitionZone;
	
	void Awake(){
		actionZone = this.gameObject.transform.FindChild("ActionZone").gameObject;
		recognitionZone = this.gameObject.transform.FindChild("RecognitionZone").gameObject;

		if (type == TYPE.NAIVE) {
			recognitionZone.SetActive (true);
			actionZone.SetActive (false);
		} else {
			actionZone.SetActive (true);
			recognitionZone.SetActive (false);
		}
	}
}
