using UnityEngine;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	Component specifying that the <c>GameObject</c> is in collision with at least one other <c>GameObject</c>. This component is managed by <see cref="FYFY_plugins.CollisionManager.CollisionSensitive2D" />, you do not have to add, update or remove it manually.
	/// </summary>
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