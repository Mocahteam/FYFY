using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirusProperties {
	public int damages = 2;
	public float productionTime = 10f;
	public float infectionChance = 0.25f; // [0, 1]
}

[DisallowMultipleComponent]
public class Virus : MonoBehaviour {
	public VirusProperties properties = new VirusProperties();

	void OnTriggerEnter2D(Collider2D other) {
		GameObject odd = other.gameObject;
		bool tagIsValid = (odd.tag == "StructureCell" || odd.tag == "Macrophage" || odd.tag == "BCell" || odd.tag == "TCell" || odd.tag == "Bactery");

		if (tagIsValid && odd.GetComponent<Infected> () == null && Random.value < properties.infectionChance) {
			Infected oddInfectedC = odd.AddComponent<Infected> ();
			oddInfectedC.virusProperties = properties;

			Destroy (this.gameObject);
		}
	}
}
