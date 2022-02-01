using UnityEngine;

namespace FYFY_plugins.TriggerManager {
	/// <summary>
	/// 	Component specifying that the <c>GameObject</c> is in contact with at least one other <c>GameObject</c>. This component is managed by <see cref="FYFY_plugins.TriggerManager.TriggerSensitive2D" />, you do not have to add, update or remove it manually.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class Triggered2D : MonoBehaviour {
		private TriggerSensitive2D _triggerSensitive;

		private void Awake() {
			_triggerSensitive = this.gameObject.GetComponent<TriggerSensitive2D>();
		}

		/// <summary>
		/// 	Gets all the GameObjects in contact with this GameObject.
		/// </summary>
		public GameObject[] Targets {
			get {
				return _triggerSensitive.getTargets();
			}
		}
	}
}