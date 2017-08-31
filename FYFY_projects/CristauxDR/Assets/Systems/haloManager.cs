using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class haloManager : FSystem {
	public Family halos = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfComponents(typeof(SpriteRenderer), typeof(MeshRenderer)), new AnyOfTags("Interactable"));

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		foreach (GameObject go in halos) {
			Renderer rend = go.GetComponent<Renderer> ();

			Triggered3D triggered = go.GetComponent<Triggered3D> ();
			bool heroFound = false;
			foreach (GameObject target in triggered.Targets) {
				if (target.name == "HeroSprite") {
					heroFound = true;
					rend.material.color = new Color(1f, 0.9f, 0f);
				}
			}
			if (!heroFound) {
				rend.material.color = Color.white;
			}
		}
	}
}