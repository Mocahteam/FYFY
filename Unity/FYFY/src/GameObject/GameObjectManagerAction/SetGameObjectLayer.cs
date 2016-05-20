using UnityEngine;

namespace FYFY {
	internal class SetGameObjectLayer: IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly int _layer;

		internal SetGameObjectLayer(GameObject gameObject, int layer) {
			if (gameObject == null)
				throw new System.ArgumentNullException();

			_gameObject = gameObject;
			_layer = layer;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null)
				throw new System.NullReferenceException();
			
			if (_gameObject.layer != _layer) {
				_gameObject.layer = _layer;
				GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
			}
		}
	}
}