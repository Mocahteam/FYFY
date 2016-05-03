﻿using UnityEngine;
using System.Collections.Generic;

internal class CreateGameObjectWrapper : IEntityManagerAction {
	private readonly GameObject _gameObject;
	private readonly HashSet<uint> _componentTypeIds;

	internal CreateGameObjectWrapper(GameObject gameObject) {
		if (gameObject == null)
			throw new MissingReferenceException();

		_gameObject = gameObject;
		_componentTypeIds = new HashSet<uint>();

		foreach(Component c in gameObject.GetComponents<Component>()) {
			global::System.Type type = c.GetType();
			uint typeId = TypeManager.getTypeId(type);
			_componentTypeIds.Add(typeId);
		}
	}

	// pour gagner en perf sur les entites vide, evite lappel de getComponents (tas juste un Transform)!
	internal CreateGameObjectWrapper(GameObject gameObject, HashSet<uint> componentTypeIds) {
		if (gameObject == null || componentTypeIds == null)
			throw new MissingReferenceException();
		
		_gameObject = gameObject;
		_componentTypeIds = componentTypeIds;
	}

	void IEntityManagerAction.perform(){
		if (_gameObject == null || _componentTypeIds == null)
			throw new MissingReferenceException();

		int gameObjectId = _gameObject.GetInstanceID();
		if(EntityManager._gameObjectWrappers.ContainsKey(gameObjectId) == true)
			throw new UnityException(); // own exception
		
		GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(_gameObject, _componentTypeIds);
		EntityManager._gameObjectWrappers.Add(gameObjectId, gameObjectWrapper);
		EntityManager._modifiedGameObjectIds.Add(gameObjectId);
	}
}

// avant lentite sera comme un "fantome" (pas traité dans les familles mais dans la scene car gameObject deja construit)