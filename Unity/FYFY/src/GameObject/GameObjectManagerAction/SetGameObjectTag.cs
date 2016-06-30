using UnityEngine;

namespace FYFY {
	internal class SetGameObjectTag : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly string _tag;
		private readonly string _exceptionStackTrace;

		internal SetGameObjectTag(GameObject gameObject, string tag, string exceptionStackTrace) {
			_gameObject = gameObject;
			_tag = tag;
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

			if (_gameObject.tag != _tag) {
				_gameObject.tag = _tag;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}