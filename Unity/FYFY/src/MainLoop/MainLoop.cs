using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	[System.Serializable]
	public struct SystemDescription {
		public string _typeAssemblyQualifiedName;
		public string _typeFullName;
		public bool _pause;
	}

	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class MainLoop : MonoBehaviour {
		public SystemDescription[] _fixedUpdateSystemDescriptions;
		public SystemDescription[] _updateSystemDescriptions;
		public SystemDescription[] _lateUpdateSystemDescriptions;

		private int preprocessingFrame = -1;

		private void Awake() {
			if(_fixedUpdateSystemDescriptions == null) { // MainLoop Added in script & not in editor so it can't be kept editor value so not initialized
				DestroyImmediate(this);
				throw new UnityException();
			}

			GameObject[] sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>(); // -> find also inactive GO (&& other shit not wanted -> ghost unity go used in intern)
			for (int i = 0; i < sceneGameObjects.Length; ++i) {
				GameObject gameObject = sceneGameObjects[i];

				UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType(gameObject);
				if((prefabType != UnityEditor.PrefabType.None) && (prefabType != UnityEditor.PrefabType.PrefabInstance)) // Pour ne pas prendre en compte les prefabs (!= prefab instance) etc... WORK ??
					continue;
				//
				// Debug.Log(gameObject.name);
				//
				HashSet<uint> componentTypeIds = new HashSet<uint>();
				foreach(Component c in gameObject.GetComponents<Component>()) {
					System.Type type = c.GetType();
					uint typeId = TypeManager.getTypeId(type);

					if(componentTypeIds.Contains(typeId)) // avoid two components of same type in a gameobject
						throw new System.Exception();

					componentTypeIds.Add(typeId);
				}

				GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(gameObject, componentTypeIds);
				GameObjectManager._gameObjectWrappers.Add(gameObject.GetInstanceID(), gameObjectWrapper);
			}
		}

		private FSystem createSystemInstance(SystemDescription systemDescription){
			System.Type type = System.Type.GetType(systemDescription._typeAssemblyQualifiedName);

			FSystem system = (FSystem) System.Activator.CreateInstance(type);
			system.Pause = systemDescription._pause;

			return system;
		}

		private void Start() {
			for (int i = 0; i < _fixedUpdateSystemDescriptions.Length; ++i) {
				FSystem system = this.createSystemInstance(_fixedUpdateSystemDescriptions[i]);
				FSystemManager._fixedUpdateSystems.Add(system);
			}
			for (int i = 0; i < _updateSystemDescriptions.Length; ++i) {
				FSystem system = this.createSystemInstance(_updateSystemDescriptions[i]);
				FSystemManager._updateSystems.Add(system);
			}
			for (int i = 0; i < _lateUpdateSystemDescriptions.Length; ++i) {
				FSystem system = this.createSystemInstance(_lateUpdateSystemDescriptions[i]);
				FSystemManager._lateUpdateSystems.Add(system);
			}
		}

		private void preprocess(){
			foreach (Family family in FamilyManager._families.Values) {
				family._entries.Clear();
				family._exits.Clear();
			}

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
		}

		private void FixedUpdate(){
			int currentFrame = Time.frameCount;

			this.preprocess();
			preprocessingFrame = currentFrame;

			foreach(FSystem system in FSystemManager._fixedUpdateSystems) {
				if(system.Pause == false)
					system.process(currentFrame);
			}
		}

		private void Update(){
			int currentFrame = Time.frameCount;

			if(preprocessingFrame != currentFrame) { // due to order execution in Unity
				this.preprocess();
				preprocessingFrame = currentFrame;
			}

			foreach(FSystem system in FSystemManager._updateSystems) {
				if(system.Pause == false)
					system.process(currentFrame);
			}
		}

		private void LateUpdate(){
			int currentFrame = Time.frameCount;
			foreach(FSystem system in FSystemManager._lateUpdateSystems) {
				if(system.Pause == false)
					system.process(currentFrame);
			}
		}
	}
}