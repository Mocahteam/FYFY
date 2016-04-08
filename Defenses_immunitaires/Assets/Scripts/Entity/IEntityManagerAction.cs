using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

internal interface IEntityManagerAction {
	void perform();
}

internal class RemoveComponentAction<T> : IEntityManagerAction where T : Component {
	private readonly int _gameObjectId;
	private readonly uint _componentTypeId;

	internal RemoveComponentAction(GameObject gameObject) {
		Component component = gameObject.GetComponent<T>();

		if (component == null)
			throw new MissingComponentException();

		_gameObjectId = gameObject.GetInstanceID();
		_componentTypeId = TypeManager.getTypeId(typeof(T));

		Object.Destroy(component);
	}

	void IEntityManagerAction.perform() {
		UECS.Entity entity = UECS.EntityManager._entities[_gameObjectId];
		entity._componentTypeIds.Remove(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_gameObjectId, entity);
	}
}

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

		UECS.Entity entity = UECS.EntityManager._entities[_gameObjectId];
		entity._componentTypeIds.Add(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_gameObjectId, entity);
	}
}

internal class RemoveGameObjectAction : IEntityManagerAction {
	private readonly int _gameObjectId;

	internal RemoveGameObjectAction(GameObject gameObject) {
		_gameObjectId = gameObject.GetInstanceID();

		Object.Destroy(gameObject);
	}

	void IEntityManagerAction.perform() {
		FamilyManager.updateAfterEntityRemoved(_gameObjectId);
	}
}

internal class CreateGameObjectAction : IEntityManagerAction {
	private readonly string _gameObjectName;
	private readonly global::System.Type[] _componentsTypes;

	internal CreateGameObjectAction(string gameObjectName, global::System.Type[] componentsTypes){
		_gameObjectName = gameObjectName;
		_componentsTypes = componentsTypes;
	}

	void IEntityManagerAction.perform(){
		GameObject gameObject = new GameObject (_gameObjectName, _componentsTypes);
		int gameObjectId = gameObject.GetInstanceID();
		HashSet<uint> componentTypeIds = new HashSet<uint> ();

		for (int i = 0; i < _componentsTypes.Length; ++i)
			componentTypeIds.Add(TypeManager.getTypeId(_componentsTypes[i]));
		componentTypeIds.Add(TypeManager.getTypeId(typeof(Transform)));

		UECS.Entity entity = new UECS.Entity(gameObject, componentTypeIds);
		UECS.EntityManager._entities.Add(gameObjectId, entity);
		FamilyManager.updateAfterEntityAdded(gameObjectId, entity);
	}
}

// duplicate action -> MAJ en différé donc attention de pas faire 2* le meme truc dans 2 systemes