using UnityEngine;

[DisallowMultipleComponent]
public class TCell : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other) {
		GameObject odd = other.gameObject;
		Infected oddInfectedC = odd.GetComponent<Infected> ();

		if(oddInfectedC != null) {
			oddInfectedC.virusNumberToCreate = 0;
			oddInfectedC.virusProperties = null;

			Health oddHealth = odd.GetComponent<Health> ();
			oddHealth.hit (oddHealth.currentHealth);
		}
	}
}
