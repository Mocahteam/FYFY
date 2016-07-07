using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	internal class GameObjectWrapper {
		internal GameObject _gameObject;
		internal HashSet<uint> _componentTypeIds; // list of ids of all the gameobject's components, avoid to always parsing with GameObject.GetComponent which is costly 

		internal GameObjectWrapper(GameObject gameObject, HashSet<uint> componentTypeIds) {
			_gameObject = gameObject;
			_componentTypeIds = componentTypeIds;
		}
	}
}