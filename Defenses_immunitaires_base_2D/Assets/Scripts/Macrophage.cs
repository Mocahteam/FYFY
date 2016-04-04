using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class Macrophage : MonoBehaviour {
	public static List<Macrophage> macrophageComponents = new List<Macrophage>();

	private int phagocytoseDamages = 10;
	private Health health;

	void Awake(){
		macrophageComponents.Add (this);

		health = this.gameObject.GetComponent<Health> ();
	}

	void OnDestroy(){
		macrophageComponents.Remove (this);
	}

	void OnTriggerEnter2D(Collider2D other) {
		GameObject odd = other.gameObject;

		if(odd.tag == "Waste" || odd.tag == "Toxin" || odd.tag == "StuckVirus"){
			Destroy (odd);

			health.hit(phagocytoseDamages);
		} else if (odd.tag == "Bactery") {
			Health oddHealth = odd.GetComponent<Health> ();
			oddHealth.hit (oddHealth.currentHealth);

			health.hit(phagocytoseDamages);
		}
	}
}

// Component c = other.GetComponent<MonoBehaviour> ();
// c.getType() == Toxin