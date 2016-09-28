using UnityEngine;
using System.Linq;

namespace FYFY_plugins.CollisionManager {
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(CollisionSensitive2D))]
	public class InCollision2D : MonoBehaviour {
		private CollisionSensitive2D _collisionSensitive;

		private void Awake() {
			_collisionSensitive = this.gameObject.GetComponent<CollisionSensitive2D>();
		}

		public GameObject[] Targets {
			get {
				return _collisionSensitive._collisions.Keys.ToArray();
			}
		}

		public Collision2D[] Collisions {
			get {
				return _collisionSensitive._collisions.Values.ToArray();
			}
		}
	}
}