using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;

namespace FYFY_plugins.PointerManager {
	/// <summary>
	/// 	Component allowing <c>GameObject</c> to be sensitive to the pointer.
	/// 	Add automatically a <see cref="FYFY_plugins.PointerManager.PointerOver">component</see> when the pointer points the <c>GameObject</c>.
	/// </summary>

	// Overlay of the Unity Pointer System.
	// It's a tricky component with non FYFY conventional design.
	// It uses Unity logic inside so it can't be considered like a real FYFY component.
	[DisallowMultipleComponent]
	public class PointerSensitive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		
		private int _cpt = 0;
		
		private void OnDisable(){
			removePointerOver();
		}
		
		private void OnDestroy(){
			removePointerOver();
		}
		
		///////////////////////////////////////////////////
		// OnPointerEnter/Exit is useful for UI elements //
		///////////////////////////////////////////////////
		/// <summary>
		///		Called when the pointer enters the GameObject.
		/// 	Callback automatically called by Unity.
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData) {
			if (this.isActiveAndEnabled)
				addPointerOver();
		}
		/// <summary>
		/// 	Called when the pointer is not any longer over the GameObject.
		/// 	Callback automatically called by Unity.
		/// </summary>
		public void OnPointerExit(PointerEventData eventData) {
			if (this.isActiveAndEnabled)
				removePointerOver();
		}
		
		///////////////////////////////////////////////////////
		// OnMouseEnter/Exit is useful with collider and GUI //
		///////////////////////////////////////////////////////
		// Called when the mouse enters the GameObject.
		// Callback automatically called by Unity.
		private void OnMouseEnter() {
			if (this.isActiveAndEnabled)
				addPointerOver();
		}
		// Called when the mouse is not any longer over the GameObject.
		// Callback automatically called by Unity.
		private void OnMouseExit() {
			if (this.isActiveAndEnabled)
				removePointerOver();
		}
		
		private void removePointerOver(){
			// Check this GameObject contains a PointerOver
			if (_cpt == 1 && this.gameObject.GetComponent<PointerOver>() != null){
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to remove PointerOver in order to keep families synchronized.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove PointerOver component thanks to classic Unity function.
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (GameObjectManager.containUnbindActionFor(parents)){
					// We find an unbind action, then we remove PointerOver component with classic Unity function
					PointerOver component = GetComponent<PointerOver>();
					Object.Destroy(component);
				} else {
					// We don't find an unbind action then we remove PointerOver component with FYFY
					// This action will be added and treated in the current preprocess operation.
					// See MainLoop.preprocess for details.
					GameObjectManager.removeComponent<PointerOver>(this.gameObject, true);
				}
			}
			_cpt--;
		}
		
		private void addPointerOver(){
			// Check this GameObject doesn't contain a PointerOver
			if (_cpt == 0 && this.gameObject.GetComponent<PointerOver>() == null){
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to add PointerOver in order to keep families synchronized.
				// If so, we don't add this action because it will be queued after unbind and will not be able to proceed (unknown game object).
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (!GameObjectManager.containUnbindActionFor(parents)){
					// We don't find an unbind action, then we can add PointerOver component with classic Unity function
					FYFY.GameObjectManager.addComponent<PointerOver>(this.gameObject);
				}
			}
			_cpt++;
		}
	}
}