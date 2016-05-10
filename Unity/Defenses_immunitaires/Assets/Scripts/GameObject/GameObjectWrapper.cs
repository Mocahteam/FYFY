using UnityEngine;
using System.Collections.Generic;

internal class GameObjectWrapper {
	internal GameObject _gameObject;
	internal HashSet<uint> _componentTypeIds;

	internal GameObjectWrapper(GameObject gameObject, HashSet<uint> componentTypeIds) {
		_gameObject = gameObject;
		_componentTypeIds = componentTypeIds;
	}
}

// Attention car en choppant un composant, ils peuvent chopper le gameobject et le modifer (donc ils cassent l'entité !!) 
// pareil, il ne faut pas faire de destroy(go) ou destroy(c) !!