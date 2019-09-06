using UnityEngine;

namespace FYFY {
    /// <summary>
    /// Notify Fyfy for each Unity callback usefull to update families
    /// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class FyfyBridge : MonoBehaviour {
		private void Start () {
			this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
		}
		
		private void OnEnable(){
			GameObjectManager.refresh(this.gameObject);
		}
		
		private void OnDisable() {
			GameObjectManager.refresh(this.gameObject);
		}
	}
}