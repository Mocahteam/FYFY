using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
	
public class BCellSystem : FSystem {
	private Family _bcells;

	public BCellSystem() {
		_bcells = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
			new AllOfComponents(typeof(BCell)),
			new NoneOfComponents(typeof(Death))
		);
	
		foreach(GameObject bcell in _bcells) {
			BCell.TYPE type = bcell.GetComponent<BCell>()._type;

			GameObject recognitionZone = bcell.transform.FindChild("RecognitionZone").gameObject;
			GameObject actionZone = bcell.transform.FindChild("ActionZone").gameObject;

			if (type == BCell.TYPE.NAIVE) {
				recognitionZone.SetActive(true); // je m'en fiche de maj la car aucune famille sur ca -> donc je nutilise pas la surcouche !!
				actionZone.SetActive(false);
			} else {
				recognitionZone.SetActive(false); // je m'en fiche de maj la car aucune famille sur ca -> donc je nutilise pas la surcouche !!
				actionZone.SetActive(true);
			}
		}
	}

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach(GameObject bcell in _bcells) {
			BCell bcellC = bcell.GetComponent<BCell>();
			GameObject recognitionZone = bcell.transform.FindChild("RecognitionZone").gameObject;
			GameObject actionZone = bcell.transform.FindChild("ActionZone").gameObject;

			if((recognitionZone.activeInHierarchy && recognitionZone.activeSelf) == true) {
				Triggered2D rzTriggered = recognitionZone.GetComponent<Triggered2D>();

				if(rzTriggered != null) {
					foreach (GameObject other in rzTriggered.Targets) {
						if (other.tag == "Bactery") {
							bcellC._bacteryRecognitionProgress += Time.deltaTime;

							if (bcellC._bacteryRecognitionProgress >= bcellC._bacteryRecognitionTime) {
								bcellC._type = BCell.TYPE.BACTERIAL;
								recognitionZone.SetActive(false); // je m'en fiche de maj la car aucune famille sur ca -> donc je nutilise pas la surcouche !!
								actionZone.SetActive(true);
								updateTypeBar(bcell, BCell.TYPE.BACTERIAL, 1f, 1f);
							} else {
								this.updateTypeBar(bcell, BCell.TYPE.BACTERIAL, bcellC._bacteryRecognitionProgress, bcellC._bacteryRecognitionTime);
							}
						} else if (other.tag == "Virus") {
							bcellC._virusRecognitionProgress += Time.deltaTime;

							if(bcellC._virusRecognitionProgress >= bcellC._virusRecognitionTime) {
								bcellC._type = BCell.TYPE.VIRAL;
								recognitionZone.SetActive(false); // je m'en fiche de maj la car aucune famille sur ca -> donc je nutilise pas la surcouche !!
								actionZone.SetActive(true);
								updateTypeBar(bcell, BCell.TYPE.VIRAL, 1f, 1f);
							} else {
								this.updateTypeBar(bcell, BCell.TYPE.VIRAL, bcellC._virusRecognitionProgress, bcellC._virusRecognitionTime);
							}
						}
					}
				}
			} else if (((actionZone.activeInHierarchy && actionZone.activeSelf) == true)) {
				Triggered2D azTriggered = actionZone.GetComponent<Triggered2D>();

				if(azTriggered != null) {
					foreach (GameObject other in azTriggered.Targets) {
						if (bcellC._type == BCell.TYPE.BACTERIAL && other.tag == "Bactery") {
							Speed speedC = other.GetComponent<Speed>();
							speedC._speed = speedC._stuckSpeed;
						} else if (bcellC._type == BCell.TYPE.VIRAL && other.tag == "Virus") {
							Speed speedC = other.GetComponent<Speed>();
							speedC._speed = speedC._stuckSpeed;
							GameObjectManager.removeComponent<Virus>(other);
							GameObjectManager.setGameObjectTag(other, "StuckVirus");
							GameObjectManager.setGameObjectLayer(other, LayerMask.NameToLayer("StuckVirus"));
						}
					}
				}
			}
		}
	}

	private void updateTypeBar(GameObject bcell, BCell.TYPE type, float progress, float maximumProgress) {
		RectTransform typeBarTransform = (RectTransform) bcell.transform.FindChild("TypeCanvas").FindChild("TypeBar");
		UnityEngine.UI.Image typeBarImage = typeBarTransform.gameObject.GetComponent<UnityEngine.UI.Image>();

		float width = progress / maximumProgress;
		float height = typeBarTransform.sizeDelta.y;

		typeBarTransform.sizeDelta = new Vector2(width, height);
		typeBarImage.color = (type == BCell.TYPE.BACTERIAL) ? Color.red : Color.green;
	}
}

