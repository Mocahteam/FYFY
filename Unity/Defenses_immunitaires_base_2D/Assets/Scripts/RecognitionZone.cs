using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RecognitionZone : MonoBehaviour {
	private BCell bcellComponent;
	private RectTransform typeBarTransform;
	private Image typeBarImage;

	void Awake(){
		GameObject typeBar = this.gameObject.transform.parent.FindChild ("TypeCanvas").FindChild ("TypeBar").gameObject;
		typeBarTransform = (RectTransform) typeBar.transform;
		typeBarImage = typeBar.GetComponent<Image> ();
		bcellComponent = this.gameObject.transform.GetComponentInParent<BCell> ();
	}

	void OnTriggerStay2D(Collider2D other){
		GameObject odd = other.gameObject;

		if (odd.tag == "Virus") {
			bcellComponent.virusRecognitionProgress += Time.deltaTime;
			if (bcellComponent.virusRecognitionProgress >= bcellComponent.recognitionTime) {
				bcellComponent.type = BCell.TYPE.VIRAL;
				bcellComponent.recognitionZone.SetActive (false);
				bcellComponent.actionZone.SetActive (true);
			}
			updateTypeBar (BCell.TYPE.VIRAL, bcellComponent.virusRecognitionProgress, bcellComponent.recognitionTime);
		} else if (odd.tag == "Bactery") {
			bcellComponent.bacteryRecognitionProgress += Time.deltaTime;
			if (bcellComponent.bacteryRecognitionProgress >= bcellComponent.recognitionTime) {
				bcellComponent.type = BCell.TYPE.BACTERIAL;
				bcellComponent.recognitionZone.SetActive (false);
				bcellComponent.actionZone.SetActive (true);
			}
			updateTypeBar (BCell.TYPE.BACTERIAL, bcellComponent.bacteryRecognitionProgress, bcellComponent.recognitionTime);
		}
	}

	void updateTypeBar(BCell.TYPE type, float progress, float maximumProgress) {
		float width = (progress > maximumProgress) ? 1f : progress / maximumProgress;
		float height = typeBarTransform.sizeDelta.y;

		typeBarTransform.sizeDelta = new Vector2 (width, height);
		typeBarImage.color = (type == BCell.TYPE.BACTERIAL) ? Color.red : Color.green;
	}
}
