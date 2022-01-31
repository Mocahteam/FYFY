using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class haloManager : FSystem {
	// only hero can produce generation of Triggered3D component thanks to Unity Physics layers
	public Family closeTo = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfComponents(typeof(SpriteRenderer), typeof(MeshRenderer)), new AnyOfTags("Interactable"));
	public Family farFrom = FamilyManager.getFamily(new NoneOfComponents(typeof(Triggered3D)), new AnyOfComponents(typeof(SpriteRenderer), typeof(MeshRenderer)), new AnyOfTags("Interactable"));

	protected override void onStart (){
		closeTo.addEntryCallback (playerCloseTo);
		farFrom.addEntryCallback(playerFarFrom);
	}

	private void playerCloseTo (GameObject go){
		Renderer rend = go.GetComponent<Renderer> ();
		rend.material.color = new Color(1f, 0.9f, 0f);
	}

	private void playerFarFrom (GameObject go)
	{
		Renderer rend = go.GetComponent<Renderer> ();
		rend.material.color = Color.white;
	}
}