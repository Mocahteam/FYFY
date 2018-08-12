﻿using UnityEngine;

namespace FYFY {
	internal class SetGameObjectState : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly bool _enabled;
		private readonly string _exceptionStackTrace;

		internal SetGameObjectState(GameObject gameObject, bool enabled, string exceptionStackTrace) {
			_gameObject = gameObject;
			_enabled = enabled;
			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException("You try to update a GameObject that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException("You try to update a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
			}
			
			if(_gameObject.activeSelf != _enabled) {
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
				this.propagate(_gameObject, _enabled); // we propagate on children because they can enter/exit family due to this new state on parent
				
				_gameObject.SetActive(_enabled);
			}
		}

		private void propagate(GameObject source, bool newState) {
			foreach(Transform childTransform in source.transform) {
				GameObject child = childTransform.gameObject;
				
				GameObjectManager._modifiedGameObjectIds.Add(child.GetInstanceID());
				this.propagate(child, newState); // repeat for childs of child
			}
		}
	}
}