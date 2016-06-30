using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	internal class CreateGameObjectWrapper : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly HashSet<uint> _componentTypeIds;
		private readonly string _exceptionStackTrace;

		internal CreateGameObjectWrapper(GameObject gameObject, string exceptionStackTrace) {
			_gameObject = gameObject;
			_componentTypeIds = new HashSet<uint>();

			foreach(Component c in gameObject.GetComponents<Component>()) {
				System.Type type = c.GetType();
				uint typeId = TypeManager.getTypeId(type);
				_componentTypeIds.Add(typeId);
			}

			_exceptionStackTrace = exceptionStackTrace;
		}

		internal CreateGameObjectWrapper(GameObject gameObject, HashSet<uint> componentTypeIds, string exceptionStackTrace) {
			_gameObject = gameObject;
			_componentTypeIds = componentTypeIds;
			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform(){
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(_gameObject, _componentTypeIds);
			GameObjectManager._gameObjectWrappers.Add(gameObjectId, gameObjectWrapper);
			GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
		}
	}
}

// avant lentite sera comme un "fantome" (pas traité dans les familles mais dans la scene car gameObject deja construit)