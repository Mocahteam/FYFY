using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[DisallowMultipleComponent]
[AddComponentMenu("")] // hide in Component list
public class MainLoop : MonoBehaviour {
	// EDITING MODE
	public MonoScript[] _systemFiles;
	public bool[] _pause;

	private void Awake() {
		if(_systemFiles == null) { // MainLoop Added in script & not in editor so it can't be kept editor value
			DestroyImmediate(this);
			throw new UnityException();
		}

		GameObject[] sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>(); // -> find also inactive GO
		for (int i = 0; i < sceneGameObjects.Length; ++i) {
			GameObject gameObject = sceneGameObjects[i];
			int gameObjectId = gameObject.GetInstanceID();

			HashSet<uint> componentTypeIds = new HashSet<uint>();
			foreach(Component c in gameObject.GetComponents<Component>()) {
				global::System.Type type = c.GetType();
				uint typeId = TypeManager.getTypeId(type);
				componentTypeIds.Add(typeId);
			}

			GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(gameObject, componentTypeIds);
			GameObjectManager._gameObjectWrappers.Add(gameObjectId, gameObjectWrapper);
		}
	}

	private void Start() {
		for (int i = 0; i < _systemFiles.Length; ++i) {
			if (_systemFiles [i] != null) {
				System.Type systemType = _systemFiles[i].GetClass();
				if (SystemManager._indexes.ContainsKey(systemType) == false) {
					UECS.System system = (UECS.System) System.Activator.CreateInstance(systemType);
					system.Pause = _pause[i];

					SystemManager._indexes.Add(systemType, SystemManager._systems.Count);
					SystemManager._systems.Add(system);
				}
			}
		}
	}

	private void FixedUpdate(){
		int count = GameObjectManager._delayedActions.Count;
		while(count-- > 0)
			GameObjectManager._delayedActions.Dequeue().perform();
		
		foreach(int gameObjectId in GameObjectManager._destroyedGameObjectIds) {
			FamilyManager.updateAfterGameObjectDestroyed(gameObjectId);
			GameObjectManager._modifiedGameObjectIds.Remove(gameObjectId);
		}
		GameObjectManager._destroyedGameObjectIds.Clear();

		foreach(int gameObjectId in GameObjectManager._modifiedGameObjectIds)
			FamilyManager.updateAfterGameObjectModified(gameObjectId);
		GameObjectManager._modifiedGameObjectIds.Clear();

		int currentFrame = Time.frameCount;
		foreach(UECS.System system in SystemManager._systems) {			
			if(system.Pause == false)
				system.process(currentFrame);
		}

		foreach (Family family in FamilyManager._families.Values) {
			family._entries.Clear();
			family._exits.Clear();
		}
	}
}