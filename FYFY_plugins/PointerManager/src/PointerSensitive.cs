using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FYFY;
using System.Collections.Generic;

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
			_cpt = 1; // force counter to 1 and ask to remove pointerOver if one exists
			removePointerOver();
		}
		
		// WARNING bug identified and not resolved: If we move mouse pointer over a game object with a PointerSensitive and this.enabled is set to false and finally to true, no PointerOver will be added to the game object even if mouse pointer is still over the game object.
		//
		// Why?
		// In Unity all component could be "disabled/enabled" by two ways :
		//  - this.gameObject.SetActive(true/false) => it enables/disables the game object and not this component but it will call anyway OnEnable/OnDisable of this component
		//  - this.enabled = true/false => it enables/disables this component and not the game object and it will call of course OnEnable/OnDisable of this component
		// When we use this.gameObject.SetActive, all On...Enter/Exit functions will be call. This is very cool for us because PointerOver will be properly set.
		// But if we use this.enabled, no one On..Enter/Exit functions will be call. In this case even if the mouse pointer is over the game object when enabling is set, no PointerOver will be added...
		// The problem is not for disabling step, indeed in both cases we want to remove existing PointerOver. The problem occurs on enabling step, if enabling is due to this.gameObject.SetActive nothing is required but if enabling is due to this.enabled we would emulate On...Enter functions (with Raycast for instance). But because we don't know which case fired OnEnable function, we don't know if we have to call Raycast.
		
		private void OnDestroy(){
			_cpt = 1; // force counter to 1 to force PointerOver removing if one exists
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
			if (_cpt > 0)
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