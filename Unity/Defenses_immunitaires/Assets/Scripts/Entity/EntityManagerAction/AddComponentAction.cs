using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

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