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
				throw new DestroyedGameObjectException("You try to update a GameObject that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException("You try to update a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
			}

			if (_gameObject.layer != _layer) {
				_gameObject.layer = _layer;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}