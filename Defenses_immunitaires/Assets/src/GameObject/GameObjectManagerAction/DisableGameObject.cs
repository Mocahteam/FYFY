using UnityEngine;

namespace FYFY {
	internal class DisableGameObject : IGameObjectManagerAction {
		private readonly GameObject _gameObject;

		internal DisableGameObject(GameObject gameObject) {
			if (gameObject == null)
				throw new MissingReferenceException();

			_gameObject = gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null)
				throw new MissingReferenceException();

			if(_gameObject.activeSelf == true) {
				_gameObject.SetActive(false);
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}