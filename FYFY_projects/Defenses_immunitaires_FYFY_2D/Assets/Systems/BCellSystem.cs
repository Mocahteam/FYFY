using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
	
// En contact avec un agent pathogène, le BCell augmente ça connaissance de cet agent. S'il est suffisament de temps en contact avec un agent pathogène il se spécialise (BCell viral ou BCell bactérien) pour produire des anticorps qui attaquent cet agent s'il est dans sa zone d'action
public class BCellSystem : FSystem {
	// zones de reconnaissance des BCells
	private Family f_recognitionZones = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY, PropertyMatcher.PROPERTY.HAS_PARENT),
			new AllOfComponents(typeof(Triggered2D)),
			new AnyOfTags("RecognitionZone")
		);
	// zones d'action des BCells
	private Family f_actionZones = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY, PropertyMatcher.PROPERTY.HAS_PARENT),
			new AllOfComponents(typeof(Triggered2D)),
			new AnyOfTags("ActionZone")
		);

	protected override void onProcess(int currentFrame) {
		// gestion des zones de reconnaissances
		foreach(GameObject recognitionZone_go in f_recognitionZones)
        {
			Triggered2D rzTriggered = recognitionZone_go.GetComponent<Triggered2D>();
			BCell bCell = recognitionZone_go.GetComponentInParent<BCell>();
			// parcours des objets dans la zone de détection
			foreach (GameObject other in rzTriggered.Targets)
			{
				if (other.tag == "Bactery")
				{
					// progression de la détection
					bCell._bacteryRecognitionProgress += Time.deltaTime;
					updateTypeBar(bCell, BCell.TYPE.BACTERIAL, bCell._bacteryRecognitionProgress, bCell._bacteryRecognitionTime);

					if (bCell._bacteryRecognitionProgress >= bCell._bacteryRecognitionTime)
						// transformation de la BCell en BCell bactérien
						specializeBCell(bCell, BCell.TYPE.BACTERIAL);
				}
				else if (other.tag == "Virus")
				{
					// progression de la détection
					bCell._virusRecognitionProgress += Time.deltaTime;
					updateTypeBar(bCell, BCell.TYPE.VIRAL, bCell._virusRecognitionProgress, bCell._virusRecognitionTime);

					if (bCell._virusRecognitionProgress >= bCell._virusRecognitionTime)
						// transformation de la BCell en BCell viral
						specializeBCell(bCell, BCell.TYPE.VIRAL);
				}
			}
        }

		// gestion des zones d'action
		foreach (GameObject actionZone_go in f_actionZones)
		{
			Triggered2D azTriggered = actionZone_go.GetComponent<Triggered2D>();
			BCell bCell = actionZone_go.GetComponentInParent<BCell>();
			// parcours des objets dans la zone d'action
			foreach (GameObject other in azTriggered.Targets)
			{
				if (bCell._type == BCell.TYPE.BACTERIAL && other.tag == "Bactery")
				{
					// Si c'est une bactérie, la ralentir
					Speed speedC = other.GetComponent<Speed>();
					speedC._speed = speedC._stuckSpeed;
				}
				else if (bCell._type == BCell.TYPE.VIRAL && other.tag == "Virus")
				{
					// Si c'est un virus, l'agglutiner
					Speed speedC = other.GetComponent<Speed>();
					speedC._speed = speedC._stuckSpeed;
					GameObjectManager.removeComponent<Virus>(other);
					GameObjectManager.setGameObjectTag(other, "StuckVirus");
					GameObjectManager.setGameObjectLayer(other, LayerMask.NameToLayer("StuckVirus"));
				}
			}
		}
	}

	private void specializeBCell(BCell bCell, BCell.TYPE newType)
	{
		bCell._type = newType;
		GameObjectManager.setGameObjectState(bCell.recognitionZone, false);
		GameObjectManager.setGameObjectState(bCell.actionZone, true);
		updateTypeBar(bCell, newType, 1f, 1f);
	}

	private void updateTypeBar(BCell bCell, BCell.TYPE contactType,  float progress, float maximumProgress) {
		float width = progress / maximumProgress;
		float height = bCell.recognitionBar.sizeDelta.y;
		bCell.recognitionBar.sizeDelta = new Vector2(width, height);

		UnityEngine.UI.Image typeBarImage = bCell.recognitionBar.gameObject.GetComponent<UnityEngine.UI.Image>();
		typeBarImage.color = (contactType == BCell.TYPE.BACTERIAL) ? Color.yellow : Color.green;
	}
}

