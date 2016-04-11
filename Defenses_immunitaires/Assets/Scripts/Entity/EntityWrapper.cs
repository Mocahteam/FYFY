using UnityEngine;
using System.Collections.Generic;

namespace UECS {
	internal class EntityWrapper {
		internal readonly GameObject _gameObject;
		internal readonly HashSet<uint> _componentTypeIds;

		internal EntityWrapper(GameObject go, HashSet<uint> componentTypeIds){
			_gameObject = go;
			_componentTypeIds = componentTypeIds;
		}
	}
}