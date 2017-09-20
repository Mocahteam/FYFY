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
				throw new DestroyedGameObjectException("You try to update a GameObject that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException("You try to update a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
			}

			System.Type componentType = typeof(T);
			// Check if the component added is the first one in the GO of this type, if true update wrapper
			if (_gameObject.GetComponent<T>() == null) {
				uint componentTypeId = TypeManager.getTypeId(componentType);
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Add(componentTypeId);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
			}

			T component = _gameObject.AddComponent<T>();
			if (component != null){
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
			} else {
				// In case of AddComponent fail, UnityEngine log a message to explain. I don't find a solution to intersept this message and rebuild it with appropriate stacktrace. In consequence we suggest to look for this log for more detail
				throw new FyfyException("AddComponent fails, please see above default log message for more details.", _exceptionStackTrace);
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
				throw new DestroyedGameObjectException("You try to update a GameObject that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				throw new UnknownGameObjectException("You try to update a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
			}

			// Check if the component added is the first one in the GO of this type, if true update wrapper
			if (_gameObject.GetComponent(_componentType) == null) {
				uint componentTypeId = TypeManager.getTypeId(_componentType);
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Add(componentTypeId);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
			}

			Component component = _gameObject.AddComponent(_componentType);
			if (component != null){
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
			} else {
				// In case of AddComponent fail, UnityEngine log a message to explain. I don't find a solution to intersept this message and rebuild it with appropriate stacktrace. In consequence we suggest to look for this log for more detail
				throw new FyfyException("AddComponent fails, please see above default log message for more details.", _exceptionStackTrace);
			}
		}
	}
}