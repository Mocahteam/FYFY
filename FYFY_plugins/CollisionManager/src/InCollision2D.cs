using UnityEngine;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	Component specifying that the GameObject is in contact with at least one other GameObject.
	/// </summary>
	/// <remarks>
	/// 	<para>! AUTOMATICALLY ADDED, UPDATED OR REMOVED !</para>
	/// 	<para>! DO NOT TOUCH MANUALLY OTHERWISE WE CAN'T GUARANTEE THE COMPORTMENT !</para>
	/// </remarks>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class InCollision2D : MonoBehaviour {
		private CollisionSensitive2D _collisionSensitive;

		private void Awake() {
			_collisionSensitive = this.gameObject.GetComponent<CollisionSensitive2D>();
		}

		/// <summary>
		/// 	Gets all the GameObjects in contact with this GameObject.
		/// </summary>
		public GameObject[] Targets {
			get {
				return _collisionSensitive.getTargets();
			}
		}

		/// <summary>
		/// 	Gets all the collisions informations involving this GameObject.
		/// </summary>
		public Collision2D[] Collisions {
			get {
				return _collisionSensitive.getCollisions();
			}
		}
	}
}