using UnityEngine;

namespace FYFY {
	internal class SetGameObjectLayer: IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly int _layer;
		private readonly string _exceptionStackTrace;
		private readonly int _gameObjectId;

		internal SetGameObjectLayer(GameObject gameObject, int layer, string exceptionStackTrace) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_layer = layer;
			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException("You try to update a GameObject (id: "+_gameObjectId+") that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			if(GameObjectManager._gameObjectWrappers.ContainsKey(_gameObjectId) == false){
				throw new UnknownGameObjectException("You try to update \"" + _gameObject.name + "\" GameObject (id: "+_gameObjectId+") which is not already binded to FYFY.", _exceptionStackTrace);
			}

			if (_gameObject.layer != _layer) {
				_gameObject.layer = _layer;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObjectId);
			}
		}
	}
}