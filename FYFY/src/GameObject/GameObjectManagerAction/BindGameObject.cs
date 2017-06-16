using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	internal class BindGameObject : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly HashSet<uint> _componentTypeIds;
		private readonly string _exceptionStackTrace;

		internal BindGameObject(GameObject gameObject, string exceptionStackTrace) {
			_gameObject = gameObject;
			_componentTypeIds = new HashSet<uint>();

			foreach(Component c in gameObject.GetComponents<Component>()) {
				System.Type type = c.GetType();
				uint typeId = TypeManager.getTypeId(type);
				_componentTypeIds.Add(typeId);
			}

			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform(){ // before this call GO is like a ghost for FYFY (not known by families but present into the scene)
			if(_gameObject == null) { // The GO has been destroyed !!!
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(_gameObject, _componentTypeIds);
			GameObjectManager._gameObjectWrappers.Add(gameObjectId, gameObjectWrapper);
			GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
		}
	}
}