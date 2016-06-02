using UnityEngine;

namespace FYFY_plugins.Mouse {
	[DisallowMultipleComponent]
	public class MouseSensitive : MonoBehaviour {
		private void OnMouseEnter() {
			FYFY.GameObjectManager.addComponent<MouseOver>(this.gameObject);
		}
		private void OnMouseExit() {
			FYFY.GameObjectManager.removeComponent<MouseOver>(this.gameObject);
		}
	}
}