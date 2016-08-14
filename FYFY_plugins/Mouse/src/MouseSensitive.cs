using UnityEngine;

namespace FYFY_plugins.Mouse {
	/// <summary>
	/// 	Component allowing <c>GameObject</c> to be sensitive to the mouse.
	/// 	Add automatically a <see cref="FYFY_plugins.Mouse.MouseOver">component</see> when the mouse points the <c>GameObject</c>.
	/// </summary>
	/// <remarks>
	/// 	<para>
	///			This component works only on objects that aren't on Ignore Raycast layer 
	///			and which have a collider marked as Trigger (with <c>Physics.queriesHitTriggers</c> true).
	///		</para>
	/// </remarks>

	// Overlay of the Unity Mouse System.
	// It's a tricky component with non FYFY conventional design.
	// It uses Unity logic inside so it can't be considered like a real FYFY component.
	[DisallowMultipleComponent]
	public class MouseSensitive : MonoBehaviour {
		// Called when the mouse enters the GameObject.
		// Callback automatically called by Unity.
		private void OnMouseEnter() {
			FYFY.GameObjectManager.addComponent<MouseOver>(this.gameObject);
		}

		// Called when the mouse is not any longer over the GameObject.
		// Callback automatically called by Unity.
		private void OnMouseExit() {
			FYFY.GameObjectManager.removeComponent<MouseOver>(this.gameObject);
		}
	}
}