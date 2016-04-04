using UnityEngine;

[DisallowMultipleComponent]
public class StuckVirus : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other){
		if (this.enabled == false || other.gameObject.tag != "StuckVirus")
			return;
		
		GameObject odd = other.gameObject;
		StuckVirus oddSvc = odd.GetComponent<StuckVirus> ();
		if (oddSvc.enabled == true) { // creation dun groupe - je suis le maitre, lui devient mon enfant
			this.enabled = false;
			oddSvc.enabled = false;
			odd.GetComponent<MovingGO> ().enabled = false;
			odd.transform.parent = this.gameObject.transform;
		}
	}

	void OnDestroy(){
		if (this.transform.parent != null)
			Destroy (this.transform.parent.gameObject);
	}
}