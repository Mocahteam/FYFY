using UnityEngine;

public class NonControllableMovingGO : MovingGO {
	protected override void Update(){
		base.Update ();

		if (transform.position == target)
			target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range(-2.5f, 2.5f));
	}
}