﻿using UnityEngine;
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
			EntityManager._gameObjectWrappers.Add(gameObjectId, gameObjectWrapper);
		}
	}

	private void Start() {
		for (int i = 0; i < _systemFiles.Length; ++i)
			if (_systemFiles[i] != null)
				SystemManager.addSystem(_systemFiles[i].GetClass(), _pause[i]);
	}

	private void FixedUpdate(){
		int count = EntityManager._delayedActions.Count;
		while(count-- > 0)
			EntityManager._delayedActions.Dequeue().perform();
		
		foreach(int gameObjectId in EntityManager._destroyedGameObjectIds) {
			FamilyManager.updateAfterGameObjectDestroyed(gameObjectId);
			EntityManager._modifiedGameObjectIds.Remove(gameObjectId);
		}
		EntityManager._destroyedGameObjectIds.Clear();

		foreach(int gameObjectId in EntityManager._modifiedGameObjectIds)
			FamilyManager.updateAfterGameObjectModified(gameObjectId);
		EntityManager._modifiedGameObjectIds.Clear();

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