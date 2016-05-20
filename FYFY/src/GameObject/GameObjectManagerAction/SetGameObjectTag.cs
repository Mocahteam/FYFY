using UnityEngine;

namespace FYFY {
	internal class SetGameObjectTag : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly string _tag;

		internal SetGameObjectTag(GameObject gameObject, string tag) {
			if (gameObject == null || tag == null)
				throw new System.ArgumentNullException();

			_gameObject = gameObject;
			_tag = tag;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null || _tag == null)
				throw new System.NullReferenceException();

			if (_gameObject.tag != _tag) {
				_gameObject.tag = _tag;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}