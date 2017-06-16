using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class UnbindGameObject : IGameObjectManagerAction {
		internal readonly GameObject _gameObject; // internal to be accessed in TriggerManager && CollisionManager dlls
		private  readonly string _exceptionStackTrace;

		internal UnbindGameObject(GameObject gameObject, string exceptionStrackTrace) {
			_gameObject = gameObject;
			_exceptionStackTrace = exceptionStrackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();

			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			GameObjectManager._gameObjectWrappers.Remove(gameObjectId);
			GameObjectManager._unbindedGameObjectIds.Add(gameObjectId);
		}
	}
}