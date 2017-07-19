using UnityEngine;
using UnityEngine.EventSystems;

namespace FYFY_plugins.PointerManager {
	/// <summary>
	/// 	Component allowing <c>GameObject</c> to be sensitive to the pointer.
	/// 	Add automatically a <see cref="FYFY_plugins.PointerManager.PointerOver">component</see> when the pointer points the <c>GameObject</c>.
	/// </summary>
	/// <remarks>
	/// 	<para>
	///			This component works only on objects that aren't on Ignore Raycast layer 
	///			and which have a collider marked as Trigger (with <c>Physics.queriesHitTriggers</c> true).
	///		</para>
	/// </remarks>

	// Overlay of the Unity Pointer System.
	// It's a tricky component with non FYFY conventional design.
	// It uses Unity logic inside so it can't be considered like a real FYFY component.
	[DisallowMultipleComponent]
	public class PointerSensitive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		
		///////////////////////////////////////////////////
		// OnPointerEnter/Exit is useful for UI elements //
		///////////////////////////////////////////////////
		/// <summary>
		///		Called when the pointer enters the GameObject.
		/// 	Callback automatically called by Unity.
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData) {
			FYFY.GameObjectManager.addComponent<PointerOver>(this.gameObject);
		}
		/// <summary>
		/// 	Called when the pointer is not any longer over the GameObject.
		/// 	Callback automatically called by Unity.
		/// </summary>
		public void OnPointerExit(PointerEventData eventData) {
			FYFY.GameObjectManager.removeComponent<PointerOver>(this.gameObject);
		}
		
		//////////////////////////////////////////////////////////
		// OnMouseEnter/Exit is useful with colliders and GUI's //
		//////////////////////////////////////////////////////////
		// Called when the mouse enters the GameObject.
		// Callback automatically called by Unity.
		private void OnMouseEnter() {
			FYFY.GameObjectManager.addComponent<PointerOver>(this.gameObject);
		}
		// Called when the mouse is not any longer over the GameObject.
		// Callback automatically called by Unity.
		private void OnMouseExit() {
			FYFY.GameObjectManager.removeComponent<PointerOver>(this.gameObject);
		}
	}
}