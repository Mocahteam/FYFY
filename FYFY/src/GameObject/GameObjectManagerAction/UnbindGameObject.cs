using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class UnbindGameObject : IGameObjectManagerAction {
		internal readonly GameObject _gameObject; // internal to be accessed in TriggerManager && CollisionManager dlls
		private readonly string _exceptionStackTrace;
		private readonly int _gameObjectId;
		private readonly bool _recCall;

		internal UnbindGameObject(GameObject gameObject, string exceptionStrackTrace, bool recCall = false) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_exceptionStackTrace = exceptionStrackTrace;
			_recCall = recCall;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(GameObjectManager._gameObjectWrappers.ContainsKey(_gameObjectId) == false){
				if (!_recCall)
					throw new UnknownGameObjectException("You try to unbind "+((_gameObject != null) ? "\"" + _gameObject.name + "\"" : "a") +" GameObject (id: "+_gameObjectId+") which is not already binded to FYFY.", _exceptionStackTrace);
				else
					return; // if this action is call due to unbind parent, do not throw exception
			}

			GameObjectManager._gameObjectWrappers.Remove(_gameObjectId);
			GameObjectManager._unbindedGameObjectIds.Add(_gameObjectId);
			// Remove the bridge if it was added
			if (_gameObject)
			{
				FyfyBridge fb = _gameObject.GetComponent<FyfyBridge>();
				if (fb)
					MonoBehaviour.Destroy(fb);
			}
		}
	}
}