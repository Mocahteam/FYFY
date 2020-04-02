using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	internal class BindGameObject : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly HashSet<string> _componentTypeNames;
		private readonly string _exceptionStackTrace;

		internal BindGameObject(GameObject gameObject, string exceptionStackTrace) {
			_gameObject = gameObject;
			_componentTypeNames = new HashSet<string>();

			foreach(Component c in gameObject.GetComponents<Component>()) {
				System.Type type = c.GetType();
				_componentTypeNames.Add(type.FullName);
			}

			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform(){ // before this call GO is like a ghost for FYFY (not known by families but present into the scene)
			if(_gameObject == null) { // The GO has been destroyed !!!
				throw new DestroyedGameObjectException("You try to bind a GameObject that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if (!GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId)){
				GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(_gameObject, _componentTypeNames);
				GameObjectManager._gameObjectWrappers.Add(gameObjectId, gameObjectWrapper);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
				// Add the bridge if not already added
				if (!_gameObject.GetComponent<FyfyBridge>())
					_gameObject.AddComponent<FyfyBridge>();
			} else
				throw new FyfyException("A game object can be binded to Fyfy only once. The game object \""+_gameObject.name+"\" (instance id:"+gameObjectId+") is already binded.", _exceptionStackTrace);
		}
	}
}