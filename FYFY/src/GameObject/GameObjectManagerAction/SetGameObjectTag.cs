using UnityEngine;

namespace FYFY {
	internal class SetGameObjectTag : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly string _tag;
		private readonly string _exceptionStackTrace;
		private readonly int _gameObjectId;

		internal SetGameObjectTag(GameObject gameObject, string tag, string exceptionStackTrace) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_tag = tag;
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

			if (_gameObject.tag != _tag) {
				_gameObject.tag = _tag;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}