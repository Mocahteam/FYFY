using UnityEngine;

namespace FYFY {
    /// <summary>
    /// Notify Fyfy for each Unity callback usefull to update families
    /// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class FyfyBridge : MonoBehaviour {
		private void OnEnable(){
			GameObjectManager.refresh(this.gameObject);
		}
		
		private void OnDisable() {
			GameObjectManager.refresh(this.gameObject);
		}
	}
}