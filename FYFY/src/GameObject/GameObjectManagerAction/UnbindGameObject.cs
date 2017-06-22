using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class UnbindGameObject : IGameObjectManagerAction {
		internal readonly GameObject _gameObject; // internal to be accessed in TriggerManager && CollisionManager dlls
		private  readonly string _exceptionStackTrace;
		private readonly int _gameObjectId;

		internal UnbindGameObject(GameObject gameObject, string exceptionStrackTrace) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_exceptionStackTrace = exceptionStrackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(GameObjectManager._gameObjectWrappers.ContainsKey(_gameObjectId) == false){
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			GameObjectManager._gameObjectWrappers.Remove(_gameObjectId);
			GameObjectManager._unbindedGameObjectIds.Add(_gameObjectId);
		}
	}
}