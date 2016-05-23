using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class DestroyGameObject : IGameObjectManagerAction {
		private readonly GameObject _gameObject;

		internal DestroyGameObject(GameObject gameObject) {
			if (gameObject == null)
				throw new MissingReferenceException();
			
			_gameObject = gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null)
				throw new MissingReferenceException();

			Transform[] childTransforms = _gameObject.GetComponentsInChildren<Transform>(true); // self include in getComponentsInChildren (first item of array usually)

			for (int i = 0; i < childTransforms.Length; ++i) { // GERER LE GAMEOBJECT + SES ENFANTS CAR ILS VONT AUSSI ETRE DETRUITS !
				int childId = childTransforms[i].gameObject.GetInstanceID();

				if(GameObjectManager._gameObjectWrappers.ContainsKey(childId) == false)
					throw new UnityException(); // own exception

				GameObjectManager._gameObjectWrappers.Remove(childId);
				GameObjectManager._destroyedGameObjectIds.Add(childId);
			}

			Object.DestroyImmediate(_gameObject);
		}
	}
}