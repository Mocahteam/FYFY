using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class UnbindGameObject : IGameObjectManagerAction {
		internal readonly GameObject _gameObject; // internal to be accessed in TriggerManager && CollisionManager dlls
		private  readonly string _exceptionStackTrace;
		private readonly int _gameObjectId;

		internal UnbindGameObject(GameObject gameObject, string exceptionStrackTrace) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_exceptionStackTrace = exceptionStrackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(GameObjectManager._gameObjectWrappers.ContainsKey(_gameObjectId) == false){
				throw new UnknownGameObjectException("You try to unbind a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
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