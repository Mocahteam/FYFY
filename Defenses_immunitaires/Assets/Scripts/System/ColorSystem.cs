using UnityEngine;
using FYFY;

namespace COLOR {
	public class ColorSystem : FSystem {
		Family _family;

		public ColorSystem() {
			_family = FamilyManager.getFamily(new AllOfTypes(typeof(MousePressedOn)));
		}

		protected override void onPause(int currentFrame) {
		}

		protected override void onResume(int currentFrame) {
		}

		protected override void onProcess(int currentFrame) {
			foreach (GameObject gameObject in _family) {
				Material material = gameObject.GetComponent<Renderer>().material;
				material.color = (material.color == Color.black) ? Color.red : Color.black;
			}
		}
	}
}