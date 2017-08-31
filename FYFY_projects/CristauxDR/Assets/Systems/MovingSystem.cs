using UnityEngine;
using FYFY;

public class MovingSystem : FSystem {

	private Family hero = FamilyManager.getFamily(new AllOfComponents(typeof(Animator), typeof(Rigidbody), typeof(Controllable)));

	public static bool enabled = true;

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
		foreach (GameObject go in hero) {
			float hori = 0;
			float vert = 0;
			/*if (Input.GetMouseButton (0) && MovingSystem.enabled) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast (ray, out hitInfo)){
					float distance = Vector3.Distance (hitInfo.point, go.transform.position);
					hori = (hitInfo.point.x - go.transform.position.x)/distance;
					vert = (hitInfo.point.z - go.transform.position.z)/distance;
				}
				
			} else {*/
				hori = Input.GetAxis ("Horizontal");
				vert = Input.GetAxis ("Vertical");
			//}

			float horiAbs = Mathf.Abs (hori);
			float vertAbs = Mathf.Abs (vert);

			Rigidbody rb = go.GetComponent<Rigidbody> ();
			Controllable control = go.GetComponent<Controllable> ();
			Animator anim = go.GetComponent<Animator> ();

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
}