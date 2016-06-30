using UnityEngine;
using System.Reflection;

namespace FYFY {
	internal class AddComponent<T> : IGameObjectManagerAction where T : Component {
		private readonly GameObject _gameObject;
		private readonly object _componentValues;
		private readonly string _exceptionStackTrace;

		internal AddComponent(GameObject gameObject, object componentValues, string exceptionStackTrace) {
			_gameObject = gameObject;
			_componentValues = componentValues;
			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){ // FAIRE ICI car si Create puis Add au sein dune m frame, le GO nest vraiment dans le systeme quen N+1
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			System.Type componentType = typeof(T);
			if(_gameObject.GetComponent<T>() != null){
				Debug.LogWarning("Can't add '" + componentType + "' to " + _gameObject.name + " because a '" + componentType + "' is already added to the game object!");
				return;
			}

			T component = _gameObject.AddComponent<T>();
			if (_componentValues != null) {
				System.Type componentValuesType = _componentValues.GetType();
				foreach (PropertyInfo pi in componentValuesType.GetProperties()) { // in anonymous, all is get property
					FieldInfo fieldInfo = componentType.GetField(pi.Name);
					PropertyInfo propertyInfo = componentType.GetProperty(pi.Name);
					object value = pi.GetValue(_componentValues, null);

					if (fieldInfo != null)
						fieldInfo.SetValue(component, System.Convert.ChangeType(value, fieldInfo.FieldType));
					else if (propertyInfo != null)
						propertyInfo.SetValue(component, System.Convert.ChangeType(value, propertyInfo.PropertyType), null);
				}
			}

			uint componentTypeId = TypeManager.getTypeId(componentType);
			GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Add(componentTypeId);
			GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
		}
	}

	internal class AddComponent : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly System.Type _componentType;
		private readonly object _componentValues;
		private readonly string _exceptionStackTrace;

		internal AddComponent(GameObject gameObject, System.Type componentType, object componentValues, string exceptionStackTrace) {
			_gameObject = gameObject;
			_componentType = componentType;
			_componentValues = componentValues;
			_exceptionStackTrace = exceptionStackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){ // FAIRE ICI car si Create puis Add au sein dune m frame, le GO nest vraiment dans le systeme quen N+1
				throw new UnknownGameObjectException(_exceptionStackTrace);
			}

			if (_gameObject.GetComponent(_componentType) != null) {
				Debug.LogWarning("Can't add '" + _componentType + "' to " + _gameObject.name + " because a '" + _componentType + "' is already added to the game object!");
				return;
			}

			Component component = _gameObject.AddComponent(_componentType);
			if (_componentValues != null) {
				System.Type componentValuesType = _componentValues.GetType();
				foreach (PropertyInfo pi in componentValuesType.GetProperties()) { // in anonymous, all is get property
					FieldInfo fieldInfo = _componentType.GetField(pi.Name);
					PropertyInfo propertyInfo = _componentType.GetProperty(pi.Name);
					object value = pi.GetValue(_componentValues, null);
			
					if (fieldInfo != null)
						fieldInfo.SetValue(component, System.Convert.ChangeType(value, fieldInfo.FieldType));
					else if (propertyInfo != null)
						propertyInfo.SetValue(component, System.Convert.ChangeType(value, propertyInfo.PropertyType), null);
				}
			}

			uint componentTypeId = TypeManager.getTypeId(_componentType);
			GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Add(componentTypeId);
			GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
		}
	}
}

// equal overload so to check if a gameobject has been destroyed :
// _gameObject == null && !ReferenceEquals(_gameObject, null)
// Note: GO get destroyed at the end of the current frame. Therefore, checks against null in the same frame will not work