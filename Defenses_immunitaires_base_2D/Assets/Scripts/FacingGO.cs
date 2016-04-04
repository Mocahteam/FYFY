using UnityEngine;

[DisallowMultipleComponent]
public class FacingGO : MonoBehaviour {
	public float rotationSpeed = 5f; // in degree/s

	private MovingGO mgoc;

	void Awake(){
		mgoc = this.gameObject.GetComponent<MovingGO> ();
	}

	void Update () {
		Vector3 targetDirection = mgoc.target - this.gameObject.transform.position;
		Vector3 headDirection = this.gameObject.transform.right;

		float desiredAngle = Vector3.Angle(headDirection, targetDirection);

		if (desiredAngle > 1f) {
			if (Vector3.Cross (headDirection, targetDirection).z < 0f)
				desiredAngle = -desiredAngle;

			float effectiveAngle = desiredAngle * Time.deltaTime * rotationSpeed;
			if(System.Math.Abs(desiredAngle) < System.Math.Abs(effectiveAngle))
				effectiveAngle = desiredAngle;
			
			this.gameObject.transform.RotateAround (transform.position, transform.forward, effectiveAngle);
		}
	}
}

