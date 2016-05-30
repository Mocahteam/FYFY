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
	[AddComponentMenu("")] // hide in Component list
	public class MainLoop : MonoBehaviour {
		public SystemDescription[] _systemDescriptions;

		private void Awake() {
			if(_systemDescriptions == null) { // MainLoop Added in script & not in editor so it can't be kept editor value
				DestroyImmediate(this);
				throw new UnityException();
			}

			GameObject[] sceneGameObjects = Resources.FindObjectsOfTypeAll<GameObject>(); // -> find also inactive GO
			for (int i = 0; i < sceneGameObjects.Length; ++i) {
				GameObject gameObject = sceneGameObjects[i];

				UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType(gameObject);
				if((prefabType != UnityEditor.PrefabType.None) && (prefabType != UnityEditor.PrefabType.PrefabInstance)) // Pour ne pas prendre en compte les prefabs (!= prefab instance) etc... WORK ??
					continue;
				
				HashSet<uint> componentTypeIds = new HashSet<uint>();
				foreach(Component c in gameObject.GetComponents<Component>()) {
					System.Type type = c.GetType();
					uint typeId = TypeManager.getTypeId(type);
					componentTypeIds.Add(typeId);
				}

				GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(gameObject, componentTypeIds);
				GameObjectManager._gameObjectWrappers.Add(gameObject.GetInstanceID(), gameObjectWrapper);
			}
		}

		private void Start() {
			for (int i = 0; i < _systemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _systemDescriptions[i];
				System.Type type = System.Type.GetType(systemDescription._typeAssemblyQualifiedName);

				FSystem system = (FSystem) System.Activator.CreateInstance(type);
				system.Pause = systemDescription._pause;
				FSystemManager._systems.Add(system);
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
			foreach(FSystem system in FSystemManager._systems) {
				if(system.Pause == false)
					system.process(currentFrame);
			}

			foreach (Family family in FamilyManager._families.Values) {
				family._entries.Clear();
				family._exits.Clear();
			}
		}
	}
}