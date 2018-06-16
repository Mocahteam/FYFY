using UnityEngine;

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
				bool isChild = (_gameObject.transform.parent != null);
				bool lastState = _gameObject.activeSelf;
				bool newState  = _enabled;

				if(isChild){ // quand pas de parent -> activeInHierarchy == activeSelf donc pas bon faire ca tt le tps
					lastState = _gameObject.activeInHierarchy;
					newState &= _gameObject.transform.parent.gameObject.activeInHierarchy;
				}

				if(lastState != newState){
					GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
					this.propagate(_gameObject, newState); // je propage aux enfants car ils peuvent potentiellement changer detat eux aussi !
				}

				_gameObject.SetActive(_enabled); // a ne faire que en dernier pour pouvoir comparer ancien etat / nouveau
			}
		}

		private void propagate(GameObject source, bool newState) {
			foreach(Transform childTransform in source.transform) {
				GameObject child = childTransform.gameObject;
				bool childLastState = child.activeInHierarchy && child.activeSelf;
				bool childNewState = newState && child.activeSelf;

				if(childNewState != childLastState) {  // MAJ SI ETAT CHANGE
					GameObjectManager._modifiedGameObjectIds.Add(child.GetInstanceID());
					this.propagate(child, childNewState); // repeat for childs of child
				}
			}
		}
	}
}