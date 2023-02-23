using UnityEngine;
using System.Reflection;

namespace FYFY {
	internal class AddComponent<T> : IGameObjectManagerAction where T : Component {
		private readonly GameObject _gameObject;
		private readonly object _componentValues;
		private readonly string _exceptionStackTrace;
		private readonly int _gameObjectId;

		internal AddComponent(GameObject gameObject, object componentValues, string exceptionStackTrace) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_componentValues = componentValues;
			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException("You try to update a GameObject (id: "+_gameObjectId+") that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
			}

			if(GameObjectManager._gameObjectWrappers.ContainsKey(_gameObjectId) == false){
				throw new UnknownGameObjectException("You try to update a GameObject from \"" + _gameObject.name + "\" GameObject (id: "+_gameObjectId+") which is not already binded to FYFY.", _exceptionStackTrace);
			}

			System.Type componentType = typeof(T);
			// Check if the component added is the first one in the GO of this type, if true update wrapper
			if (_gameObject.GetComponent<T>() == null) {
				GameObjectManager._gameObjectWrappers[_gameObjectId]._componentTypeNames.Add(componentType.FullName);
				GameObjectManager._modifiedGameObjectIds.Add(_gameObjectId);
			}

			T component = _gameObject.AddComponent<T>();
            if (componentType == typeof(ActionPerformed))
                componentType.GetField("exceptionStackTrace").SetValue(component, System.Convert.ChangeType(_exceptionStackTrace, componentType.GetField("exceptionStackTrace").FieldType));

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
		private readonly int _gameObjectId;

		internal AddComponent(GameObject gameObject, System.Type componentType, object componentValues, string exceptionStackTrace) {
			_gameObject = gameObject;
			_gameObjectId = _gameObject.GetInstanceID();
			_componentType = componentType;
			_componentValues = componentValues;
			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				if (_exceptionStackTrace != "")
					throw new DestroyedGameObjectException("You try to update a GameObject (id: "+_gameObjectId+") that will be destroyed during this frame. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
				else
					return;
			}

			if(GameObjectManager._gameObjectWrappers.ContainsKey(_gameObjectId) == false){
				if (_exceptionStackTrace != "")
					throw new UnknownGameObjectException("You try to update a GameObject \"" + _gameObject.name + "\" GameObject (id: "+_gameObjectId+") which is not already binded to FYFY.", _exceptionStackTrace);
				else
					return;
			}

			// Check if the component added is the first one in the GO of this type, if true update wrapper
			if (_gameObject.GetComponent(_componentType) == null) {
				GameObjectManager._gameObjectWrappers[_gameObjectId]._componentTypeNames.Add(_componentType.FullName);
				GameObjectManager._modifiedGameObjectIds.Add(_gameObjectId);
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
				if (_exceptionStackTrace != ""){
					// In case of AddComponent fail, UnityEngine log a message to explain. I don't find a solution to intersept this message and rebuild it with appropriate stacktrace. In consequence we suggest to look for this log for more detail
					throw new FyfyException("AddComponent fails, please see above default log message for more details.", _exceptionStackTrace);
				}
			}
		}
	}
}