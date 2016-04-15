using UnityEngine;
using System.Reflection;

internal class AddComponentAction<T> : IEntityManagerAction where T : Component {
	private readonly GameObject _gameObject;
	private readonly int _entityWrapperId;
	private readonly System.Type _componentType;
	private readonly uint _componentTypeId;
	private readonly object _componentValues; // anonymous type object

	internal AddComponentAction(GameObject gameObject, object componentValues) {
		_gameObject = gameObject;
		_entityWrapperId = gameObject.GetInstanceID();
		_componentType = typeof(T);
		_componentTypeId = TypeManager.getTypeId(_componentType);
		_componentValues = componentValues;
	}

	void IEntityManagerAction.perform() {
		if (_gameObject.GetComponent<T>() != null)
			throw new UnityException();

		T component = _gameObject.AddComponent<T>();

		if (_componentValues != null) {
			System.Type componentValuesType = _componentValues.GetType();

			foreach (PropertyInfo pi in componentValuesType.GetProperties()) { // in anonymous, all is get property
				FieldInfo fieldInfo = _componentType.GetField(pi.Name);        // check only public field (member variable) in the component
				if (fieldInfo != null) {
					object value = pi.GetValue(_componentValues, null);
					fieldInfo.SetValue (component, System.Convert.ChangeType(value, fieldInfo.FieldType));
				}
			}
		}

		UECS.EntityWrapper entityWrapper = UECS.EntityManager._entityWrappers[_entityWrapperId];
		entityWrapper._componentTypeIds.Add(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_entityWrapperId, entityWrapper);
	}
}