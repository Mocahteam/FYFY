using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	internal class GameObjectWrapper {
		internal GameObject _gameObject;
		internal HashSet<uint> _componentTypeIds;

		internal GameObjectWrapper(GameObject gameObject, HashSet<uint> componentTypeIds) {
			_gameObject = gameObject;
			_componentTypeIds = componentTypeIds;
		}
	}
}