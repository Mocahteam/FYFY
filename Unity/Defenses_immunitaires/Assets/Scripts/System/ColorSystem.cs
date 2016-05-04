using UnityEngine;

public class ColorSystem : UECS.System {
	Family _family;

	public ColorSystem() {
		_family = FamilyManager.getFamily(new AllOfTypes(typeof(MousePressedOn)));
	}

	public override void process(int currentFrame) {
		foreach (GameObject gameObject in _family) {
			Material material = gameObject.GetComponent<Renderer>().material;
			material.color = (material.color == Color.black) ? Color.red : Color.black;
		}
	}
}