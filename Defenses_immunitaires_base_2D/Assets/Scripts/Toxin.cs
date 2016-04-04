using UnityEngine;

[DisallowMultipleComponent]
public class Toxin : MonoBehaviour {
	[HideInInspector]
	public int damages;

	void OnTriggerEnter2D(Collider2D other) {
		GameObject odd = other.gameObject;

		if (odd.tag == "StructureCell" || odd.tag == "BCell" || odd.tag == "TCell") {
			Health oddHealth = odd.GetComponent<Health> ();
			oddHealth.hit (damages);

			Destroy (this.gameObject);
		}
	}
}