using UnityEngine;

namespace FYFY {
	internal class RemoveComponent<T> : IGameObjectManagerAction where T : Component {
		private readonly GameObject _gameObject;
		private readonly string _exceptionStackTrace;

		internal RemoveComponent(GameObject gameObject, string exceptionStackTrace) {
			_gameObject = gameObject;
			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			T component = _gameObject.GetComponent<T>();
			System.Type componentType = typeof(T);
			if (component == null) {
				Debug.LogWarning("Can't remove '" + componentType + "' from " + _gameObject.name + " because a '" + componentType + "' is'nt attached to the game object!");
				return;
			}

			uint componentTypeId = TypeManager.getTypeId(componentType);
			if(GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Contains(componentTypeId) == false) {
				throw new UnknownComponentException();
			}

			Object.DestroyImmediate(component);
			
			// Check if an other component of this type is already included into the GO => if no, update wrapper
			if (_gameObject.GetComponent<T>() == null){
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Remove(componentTypeId);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
			}
		}
	}

	internal class RemoveComponent : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly Component _component;
		private readonly string _exceptionStackTrace;

		internal RemoveComponent(GameObject gameObject, Component component, string exceptionStackTrace) {
			_gameObject = gameObject;
			_component = component;
			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			if(_component == null) {
				throw new DestroyedComponentException(_exceptionStackTrace);
			}
			
			System.Type componentType = _component.GetType();
			uint componentTypeId = TypeManager.getTypeId(componentType);
			if(GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Contains(componentTypeId) == false) {
				throw new UnknownComponentException();
			}

			Object.DestroyImmediate(_component);
			
			// Check if an other component of this type is already included into the GO => if no, update wrapper
			if (_gameObject.GetComponent(componentType) == null){
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Remove(componentTypeId);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
			}
		}
	}
}