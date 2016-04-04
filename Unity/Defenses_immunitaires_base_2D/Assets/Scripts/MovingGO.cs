using UnityEngine;

[DisallowMultipleComponent]
public abstract class MovingGO : MonoBehaviour {
	public float speed = 1f;
	public float stickSpeed = 1f;
	[HideInInspector]
	public Vector3 target;

	protected virtual void Awake() {
		target = this.gameObject.transform.position;
	}

	protected virtual void Update () {
		float step = speed * Time.deltaTime;

		if (this.gameObject.transform.position != target)
			this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, target, step);
	}
}