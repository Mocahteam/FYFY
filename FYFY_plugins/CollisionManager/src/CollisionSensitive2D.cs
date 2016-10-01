using UnityEngine;
using System.Collections.Generic;
using FYFY;

namespace FYFY_plugins.CollisionManager {
	[DisallowMultipleComponent]
	public class CollisionSensitive2D : MonoBehaviour {
		internal Dictionary<GameObject, Collision2D> _collisions = new Dictionary<GameObject, Collision2D>();
		internal Dictionary<GameObject, CollisionSensitive2DTarget> _components = new Dictionary<GameObject, CollisionSensitive2DTarget>();

		internal bool _destroying = false;
		internal bool _inCollision = false;

		private void OnCollisionEnter2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			CollisionSensitive2DTarget cst = target.gameObject.AddComponent<CollisionSensitive2DTarget>();
			cst._source = this;

			_collisions.Add(target, coll);
			_components.Add(target, cst);

			if(_inCollision == false) {
				GameObjectManager.addComponent<InCollision2D>(this.gameObject); // add at next preprocess so immediatly after ! (on aurait pu lajouter direct mais on doit signaler a FYFY lajout pr les familles)
				_inCollision = true;
			}
		}

		private void OnCollisionStay2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			_collisions[target] = coll;
		}

		private void OnCollisionExit2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			CollisionSensitive2DTarget cst = _components[target];

			Component.Destroy(cst); // side effect in cst.OnDestroy
		}

		private void OnDestroy() {
			_destroying = true;

			foreach(CollisionSensitive2DTarget cst in _components.Values) {
				Object.Destroy(cst);
			}
		}
	}
}