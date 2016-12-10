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
			// Check if the component added is the first one in the GO of this type, if true update wrapper
			if (_gameObject.GetComponent<T>() == null) {
				uint componentTypeId = TypeManager.getTypeId(componentType);
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Add(componentTypeId);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
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

			// Check if the component added is the first one in the GO of this type, if true update wrapper
			if (_gameObject.GetComponent(_componentType) == null) {
				uint componentTypeId = TypeManager.getTypeId(_componentType);
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Add(componentTypeId);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
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
		}
	}
}