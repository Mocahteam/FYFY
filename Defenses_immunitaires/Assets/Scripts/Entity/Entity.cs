using UnityEngine;
using System.Collections.Generic;

namespace UECS {
	internal class Entity {
		internal readonly GameObject _gameObject;
		internal readonly HashSet<uint> _componentTypeIds;

		internal Entity(GameObject go, HashSet<uint> componentTypeIds){
			_gameObject = go;
			_componentTypeIds = componentTypeIds;
		}
	}
}