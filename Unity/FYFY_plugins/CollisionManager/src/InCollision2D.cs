using UnityEngine;
using System.Linq;

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
	[RequireComponent(typeof(CollisionSensitive2D))]
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
				return _collisionSensitive._collisions.Keys.ToArray();
			}
		}

		/// <summary>
		/// 	Gets all the collisions informations involving this GameObject.
		/// </summary>
		public Collision2D[] Collisions {
			get {
				return _collisionSensitive._collisions.Values.ToArray();
			}
		}
	}
}