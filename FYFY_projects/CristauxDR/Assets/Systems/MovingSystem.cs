using UnityEngine;
using FYFY;

public class MovingSystem : FSystem {

	private Family hero = FamilyManager.getFamily(new AllOfComponents(typeof(Animator), typeof(Rigidbody), typeof(Controllable)));
	private Rigidbody rb = null;
	private Controllable control = null;
	private Animator anim = null;

	public MovingSystem (){
		// Get hero
		GameObject hero_GO = hero.First();
		if (hero_GO == null)
			Debug.Log ("MovingSystem: Warning!!! no hero in this scene on start.");
		else {
			rb = hero_GO.GetComponent<Rigidbody> ();
			control = hero_GO.GetComponent<Controllable> ();
			anim = hero_GO.GetComponent<Animator> ();
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		float hori = Input.GetAxis ("Horizontal");
		float vert = Input.GetAxis ("Vertical");
		float horiAbs = Mathf.Abs (hori);
		float vertAbs = Mathf.Abs (vert);

		rb.velocity = new Vector3 (hori * control.maxSpeed, rb.velocity.y, vert * control.maxSpeed);
		if (vertAbs < 0.1 && horiAbs < 0.1) {
			if (control.currentAnim != "stop") {
				control.currentAnim = "stop";
				anim.SetTrigger ("stop");
			}
			
		} else {
			if (vertAbs > horiAbs) {
				if (vert < 0) {
					if (control.currentAnim != "goDown") {
						control.currentAnim = "goDown";
						anim.SetTrigger ("goDown");
					}
				} else {
					if (control.currentAnim != "goTop") {
						control.currentAnim = "goTop";
						anim.SetTrigger ("goTop");
					}
						
				}
			} else {
				if (hori < 0) {
					if (control.currentAnim != "goLeft") {
						control.currentAnim = "goLeft";
						anim.SetTrigger ("goLeft");
					}
				} else {
					if (control.currentAnim != "goRight") {
						control.currentAnim = "goRight";
						anim.SetTrigger ("goRight");
					}
				}
			}
		}
	}
}