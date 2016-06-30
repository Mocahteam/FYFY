﻿using UnityEngine;

namespace FYFY {
	internal class SetGameObjectParent : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly GameObject _parent;
		private readonly bool _worldPositionStays;
		private readonly string _exceptionStackTrace;

		internal SetGameObjectParent(GameObject gameObject, GameObject parent, bool worldPositionStays, string exceptionStackTrace) {
			_gameObject = gameObject;
			_parent = parent;
			_worldPositionStays = worldPositionStays;
			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			Transform lastParentTransform = _gameObject.transform.parent;

			if(lastParentTransform != null && _parent == lastParentTransform.gameObject)
				return;

			if (lastParentTransform != null && lastParentTransform.childCount == 1) // MAJ DE LANCIEN PARENT (IL AVAIT UN ENFANT -> PLUS MTN)
				GameObjectManager._modifiedGameObjectIds.Add(lastParentTransform.gameObject.GetInstanceID());

			if ((lastParentTransform == null && _parent != null) || (lastParentTransform != null && _parent == null)) // MAJ DE MOI (JAVAIS PAS DE PARENT -> MTN SI // JAVAIS UN PARENT -> PLUS MTN)
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);

			if (_parent != null) {
				if (_parent.transform.childCount == 0) // MAJ DU NOUVEAU PARENT (IL NAVAIT PAS DENFANT -> MTN SI)
					GameObjectManager._modifiedGameObjectIds.Add(_parent.GetInstanceID());
				
				_gameObject.transform.SetParent(_parent.transform, _worldPositionStays);
			} else {
				_gameObject.transform.parent = null;
			}
		}
	}
}