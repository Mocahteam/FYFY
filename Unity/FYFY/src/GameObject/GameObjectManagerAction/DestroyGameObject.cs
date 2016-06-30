using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class DestroyGameObject : IGameObjectManagerAction {
		internal readonly GameObject _gameObject; // internal to be accessed in TriggerManager.dll
		private  readonly string _exceptionStackTrace;

		internal DestroyGameObject(GameObject gameObject, string exceptionStrackTrace) {
			_gameObject = gameObject;
			_exceptionStackTrace = exceptionStrackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			Transform[] childTransforms = _gameObject.GetComponentsInChildren<Transform>(true); // self include in getComponentsInChildren (first item of array usually)
			for (int i = 0; i < childTransforms.Length; ++i) {                                  // GERER LE GAMEOBJECT + SES ENFANTS CAR ILS VONT AUSSI ETRE DETRUITS !
				int childId = childTransforms[i].gameObject.GetInstanceID();

				if(GameObjectManager._gameObjectWrappers.ContainsKey(childId) == false){
					throw new UnknownGameObjectException(_exceptionStackTrace);
				}

				GameObjectManager._gameObjectWrappers.Remove(childId);
				GameObjectManager._destroyedGameObjectIds.Add(childId);
			}

			Object.DestroyImmediate(_gameObject);
		}
	}
}