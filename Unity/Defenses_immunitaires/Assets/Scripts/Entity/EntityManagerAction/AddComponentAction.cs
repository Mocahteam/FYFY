using UnityEngine;
using System.Reflection;

internal class AddComponentAction<T> : IEntityManagerAction where T : Component {
	private readonly GameObject _gameObject;
	private readonly int _gameObjectId;
	private readonly System.Type _componentType;
	private readonly uint _componentTypeId;
	private readonly object _componentValues; // anonymous type object

	internal AddComponentAction(GameObject gameObject, object componentValues) {
		_gameObject = gameObject;
		_gameObjectId = gameObject.GetInstanceID();
		_componentType = typeof(T);
		_componentTypeId = TypeManager.getTypeId(_componentType);
		_componentValues = componentValues;
	}

	void IEntityManagerAction.perform() {
		if (_gameObject.GetComponent<T>() != null)
			throw new UnityException();

		T component = _gameObject.AddComponent<T>();
		Debug.Log ("COMPONENT ADDED : " + _componentType);
		if (_componentValues != null) {
			System.Type componentValuesType = _componentValues.GetType();

			foreach (PropertyInfo pi in componentValuesType.GetProperties()) { // in anonymous, all is get property
				FieldInfo fieldInfo = _componentType.GetField(pi.Name);         // check only public field (member variable) in the component
				if (fieldInfo != null) {
					object value = pi.GetValue(_componentValues, null);
					fieldInfo.SetValue (component, System.Convert.ChangeType(value, fieldInfo.FieldType));
				}
			}
		}

		UECS.EntityWrapper entityWrapper = UECS.EntityManager._entityWrappers[_gameObjectId];
		entityWrapper._componentTypeIds.Add(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_gameObjectId, entityWrapper);
	}
}

/*
internal class AddComponentAction<T> : IEntityManagerAction where T : Component {
	private readonly GameObject _gameObject;
	private readonly int _gameObjectId;
	private readonly uint _componentTypeId;
	private readonly Dictionary<string, object> _componentValues;

	internal AddComponentAction(GameObject gameObject, Dictionary<string, object> componentValues) {
		_gameObject = gameObject;
		_gameObjectId = gameObject.GetInstanceID();
		_componentTypeId = TypeManager.getTypeId(typeof(T));
		_componentValues = componentValues;
	}

	void IEntityManagerAction.perform() {
		if (_gameObject.GetComponent<T> () != null)
			throw new UnityException();

		System.Type type = typeof(T);
		T component = _gameObject.AddComponent<T>();

		if (_componentValues != null) {
			foreach (KeyValuePair<string, object> pair in _componentValues) {
				FieldInfo fieldInfo = type.GetField(pair.Key);
				if (fieldInfo != null)
					fieldInfo.SetValue(component, System.Convert.ChangeType(pair.Value, fieldInfo.FieldType));
			}
		}

		UECS.EntityWrapper entityWrapper = UECS.EntityManager._entityWrappers[_gameObjectId];
		entityWrapper._componentTypeIds.Add(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_gameObjectId, entityWrapper);
	}
}
*/