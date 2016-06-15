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

		private int _familiesUpdateCount = 0;

		private void Awake() {
			if(_fixedUpdateSystemDescriptions == null) { // MainLoop Added in script & not in editor so it can't be kept editor value so not initialized
				DestroyImmediate(this);
				throw new UnityException();
			}

			// vider toutes les classes static (au cas ou apres un loadscene il y ait tj des trucs dedans)
			FamilyManager._families.Clear();
			FSystemManager._fixedUpdateSystems.Clear();
			FSystemManager._updateSystems.Clear();
			FSystemManager._lateUpdateSystems.Clear();
			GameObjectManager._gameObjectWrappers.Clear();
			GameObjectManager._delayedActions.Clear();
			GameObjectManager._destroyedGameObjectIds.Clear();
			GameObjectManager._modifiedGameObjectIds.Clear();
			GameObjectManager._sceneBuildIndex = -1;
			GameObjectManager._sceneName = null;

			GameObject[] sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>(); // -> find also inactive GO (&& other shit not wanted -> ghost unity gameO used in intern)
			for (int i = 0; i < sceneGameObjects.Length; ++i) {
				GameObject gameObject = sceneGameObjects[i];
				UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType(gameObject);
				if(gameObject.hideFlags != HideFlags.None || prefabType == UnityEditor.PrefabType.Prefab || prefabType == UnityEditor.PrefabType.ModelPrefab)
					continue; // on veut que les objets de la scene (pas les prefabs, les objets internes a unity etc)
				
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
			// allow trick (Add action when inside perform action -> so the new action is added to the queue && will treated during this dequeue loop !!!!)
			while (GameObjectManager._delayedActions.Count != 0) {
				GameObjectManager._delayedActions.Dequeue().perform();
			}
			
			foreach(int gameObjectId in GameObjectManager._destroyedGameObjectIds) {
				FamilyManager.updateAfterGameObjectDestroyed(gameObjectId);
				GameObjectManager._modifiedGameObjectIds.Remove(gameObjectId);
			}
			GameObjectManager._destroyedGameObjectIds.Clear();

			foreach(int gameObjectId in GameObjectManager._modifiedGameObjectIds)
				FamilyManager.updateAfterGameObjectModified(gameObjectId);
			GameObjectManager._modifiedGameObjectIds.Clear();

			++_familiesUpdateCount;
		}

		// FIXEDUPDATE CAN RUN FASTER OR SLOWER THAN UPDATE
		// BE CAREFULL WITH TIME.FRAMECOUNT (ENTIRE FRAME), so if multiple fixedUpdate call in one frame, framecount not incremented for each fixedupdate
		// ONE PREPROCESSING PER CYCLE (if more than one fixedUpdate per frame, each fixedUpdate call preprocessing !)
		// if no fixedUpdate in this frame, update call preprocessing
		private void FixedUpdate(){
			this.preprocess();

			foreach(FSystem system in FSystemManager._fixedUpdateSystems)
				if(system.Pause == false)
					system.process(_familiesUpdateCount);
		}

		private void Update(){
			this.preprocess();

			foreach(FSystem system in FSystemManager._updateSystems)
				if(system.Pause == false)
					system.process(_familiesUpdateCount);
		}

		private void LateUpdate(){
			this.preprocess();

			foreach(FSystem system in FSystemManager._lateUpdateSystems)
				if(system.Pause == false)
					system.process(_familiesUpdateCount);

			if(GameObjectManager._sceneBuildIndex != -1) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameObjectManager._sceneBuildIndex); // done at the beginning of the "Unity" next frame
			} else if(GameObjectManager._sceneName != null) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameObjectManager._sceneName); // done at the beginning of the "Unity" next frame
			}
		}
	}
}