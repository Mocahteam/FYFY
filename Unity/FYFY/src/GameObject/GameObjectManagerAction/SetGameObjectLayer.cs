using UnityEngine;

namespace FYFY {
	internal class SetGameObjectLayer: IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly int _layer;
		private readonly string _exceptionStackTrace;

		internal SetGameObjectLayer(GameObject gameObject, int layer, string exceptionStackTrace) {
			_gameObject = gameObject;
			_layer = layer;
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

			if (_gameObject.layer != _layer) {
				_gameObject.layer = _layer;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}