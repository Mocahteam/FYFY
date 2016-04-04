using UnityEngine;

[DisallowMultipleComponent]
public class ActionZone : MonoBehaviour {
	private BCell bcellComponent;

	void Awake(){
		bcellComponent = this.gameObject.transform.GetComponentInParent<BCell> ();
	}

	void OnTriggerEnter2D(Collider2D other){
		GameObject odd = other.gameObject;
		MovingGO mgoc = odd.GetComponent<MovingGO> ();

		if(bcellComponent.type == BCell.TYPE.BACTERIAL && odd.tag == "Bactery" && mgoc.speed != mgoc.stickSpeed){
			mgoc.speed = mgoc.stickSpeed;
		} else if(bcellComponent.type == BCell.TYPE.VIRAL && odd.tag == "Virus"){
			Destroy (odd.GetComponent<Virus> ());
			mgoc.speed = mgoc.stickSpeed;
			odd.AddComponent<StuckVirus> ();
			odd.tag = "StuckVirus";
		}
	}
}