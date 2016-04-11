using UnityEngine;
using System.Collections.Generic;

internal interface IEntityManagerAction {
	void perform();
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

		UECS.EntityWrapper entityWrapper = new UECS.EntityWrapper(gameObject, componentTypeIds);
		UECS.EntityManager._entityWrappers.Add(gameObjectId, entityWrapper);
		FamilyManager.updateAfterEntityAdded(gameObjectId, entityWrapper);
	}
}

// duplicate action -> MAJ en différé donc attention de pas faire 2* le meme truc dans 2 systemes