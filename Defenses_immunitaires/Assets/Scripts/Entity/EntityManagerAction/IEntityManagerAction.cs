using UnityEngine;
using System.Collections.Generic;

internal interface IEntityManagerAction {
	void perform();
}

// enable -> attention si dans la scene lors du parse il y avait des entites disabled ?
// disable

//internal class InstantiatePrefabAction : IEntityManagerAction {
//}

//internal class CreatePrimitiveAction : IEntityManagerAction {
//}

internal class CreateEntityAction : IEntityManagerAction {
	private readonly string _gameObjectName;
	private readonly global::System.Type[] _componentDescriptions;
	
	internal CreateEntityAction(string gameObjectName, global::System.Type[] componentDescriptions){
		_gameObjectName = gameObjectName;
		_componentDescriptions = componentDescriptions;
	}

	void IEntityManagerAction.perform(){
		GameObject gameObject = new GameObject(_gameObjectName);
		int entityWrapperId = gameObject.GetInstanceID();
		HashSet<uint> componentTypeIds = new HashSet<uint>();

		// -----------------------------------------------------------------------
		if (_componentDescriptions != null) {
			for (int i = 0; i < _componentDescriptions.Length; ++i) {
				// System.Type componentType = null;
			}
		}
		componentTypeIds.Add(TypeManager.getTypeId(typeof(Transform)));		
		// -----------------------------------------------------------------------

		UECS.EntityWrapper entityWrapper = new UECS.EntityWrapper(gameObject, componentTypeIds);
		UECS.EntityManager._entityWrappers.Add(entityWrapperId, entityWrapper);
		FamilyManager.updateAfterEntityAdded(entityWrapperId, entityWrapper);
	}
}