using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using System.Collections.Generic;

public class haloManager : FSystem {
	// only hero can produce generation of Triggered3D component thanks to Unity Physics layers
	public Family halos = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfComponents(typeof(SpriteRenderer), typeof(MeshRenderer)), new AnyOfTags("Interactable"));
	private Dictionary<int, GameObject> id2GO = new Dictionary<int, GameObject>();

	public haloManager (){
		halos.addEntryCallback (playerCloseTo);
		halos.addExitCallback (playerFarFrom);
	}

	private void playerCloseTo (GameObject go){
		Renderer rend = go.GetComponent<Renderer> ();
		rend.material.color = new Color(1f, 0.9f, 0f);
		id2GO.Add (go.GetInstanceID(), go);
	}

	private void playerFarFrom (int goId){
		GameObject go;
		if (id2GO.TryGetValue(goId, out go)){
			if (go != null) {
				Renderer rend = go.GetComponent<Renderer> ();
				rend.material.color = Color.white;
				id2GO.Remove(goId);
			}
		}
	}
}