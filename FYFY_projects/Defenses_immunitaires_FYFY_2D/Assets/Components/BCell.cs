using UnityEngine;

public class BCell : MonoBehaviour {
	public enum TYPE { NAIVE, BACTERIAL, VIRAL };

	public TYPE _type;
	public float _bacteryRecognitionTime = 1f;
	public float _virusRecognitionTime = 1f;
	public float _bacteryRecognitionProgress = 0f;
	public float _virusRecognitionProgress = 0f;

	public GameObject recognitionZone;
	public GameObject actionZone;
	public RectTransform recognitionBar;
}
