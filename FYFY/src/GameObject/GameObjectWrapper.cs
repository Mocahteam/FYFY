using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	internal class GameObjectWrapper {
		internal GameObject _gameObject;
		internal HashSet<string> _componentTypeNames; // list of ids of all the gameobject's components, avoid to always parsing with GameObject.GetComponent which is costly 

		internal GameObjectWrapper(GameObject gameObject, HashSet<string> componentTypeNames) {
			_gameObject = gameObject;
			_componentTypeNames = componentTypeNames;
		}
	}
}