using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	/// <summary></summary>
	[System.Serializable]
	public struct SystemDescription {
		/// <summary></summary>
		public string _typeAssemblyQualifiedName;
		/// <summary></summary>
		public string _typeFullName;
		/// <summary></summary>
		public bool _pause;
	}

	/// <summary></summary>
	[ExecuteInEditMode] // permet dexecuter awake et start etc aussi en mode edition
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class MainLoop : MonoBehaviour {
		internal static MainLoop _mainLoop; // eviter davoir plusieurs composants MainLoop dans toute la scene (cf Awake)

		/// <summary></summary>
		public SystemDescription[] _fixedUpdateSystemDescriptions; // initialized in inspector, otherwise == null
		/// <summary></summary>
		public SystemDescription[] _updateSystemDescriptions;      // initialized in inspector, otherwise == null
		/// <summary></summary>
		public SystemDescription[] _lateUpdateSystemDescriptions;  // initialized in inspector, otherwise == null
		
		/// <summary></summary>
		// force update Inspector
		public int _forceUpdateInspector;		

		private int _familiesUpdateCount = 0;

		// Parse scene to get all entities.
		private void Awake() {
			// Severals instances of MainLoop are not allowed.
			if(_mainLoop != null) {
				DestroyImmediate(this);
				return;
			}

			_mainLoop = this;

			if(Application.isPlaying == false){
				return;
			}

			// vider toutes les classes static (au cas ou apres un loadscene il y ait tj des trucs dedans)
			FamilyManager._families.Clear();
			FSystemManager._fixedUpdateSystems.Clear();
			FSystemManager._updateSystems.Clear();
			FSystemManager._lateUpdateSystems.Clear();
			GameObjectManager._gameObjectWrappers.Clear();
			GameObjectManager._delayedActions.Clear();
			GameObjectManager._unbindedGameObjectIds.Clear();
			GameObjectManager._modifiedGameObjectIds.Clear();
			GameObjectManager._sceneBuildIndex = -1;
			GameObjectManager._sceneName = null;
		}

		private void OnDestroy(){
			if(_mainLoop == this) {
				_mainLoop = null;
			}
		}

		private FSystem createSystemInstance(SystemDescription systemDescription){
			System.Type type = System.Type.GetType(systemDescription._typeAssemblyQualifiedName);

			if(type == null) { // modification done between time when you set the system in the mainloop && when you run -> class deleted or renamed ?? // When inspector doesnt work and you set manually
				return null;
			}

			FSystem system = (FSystem) System.Activator.CreateInstance(type);
			system.Pause = systemDescription._pause;

			return system;
		}

		// Parse scene and bind all GameObjects to FYFY
		// Create all systems
		private void Start() {
			if(Application.isPlaying == false){
				return;
			}

			// Parse scene and bind all GameObjects to FYFY
			GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			List<GameObject> sceneGameObjects = new List<GameObject>();
			foreach (GameObject root in roots) {
				foreach(Transform childTransform in root.GetComponentsInChildren<Transform>(true)) { // include root transform
					sceneGameObjects.Add(childTransform.gameObject);
				}
			}
			foreach(GameObject gameObject in sceneGameObjects) {
				HashSet<uint> componentTypeIds = new HashSet<uint>();
				foreach(Component c in gameObject.GetComponents<Component>()) {
					System.Type type = c.GetType();
					uint typeId = TypeManager.getTypeId(type);
					componentTypeIds.Add(typeId);
				}

				GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(gameObject, componentTypeIds);
				GameObjectManager._gameObjectWrappers.Add(gameObject.GetInstanceID(), gameObjectWrapper);
			}

			// Create all systems
			for (int i = 0; i < _fixedUpdateSystemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _fixedUpdateSystemDescriptions[i];
				FSystem system = this.createSystemInstance(systemDescription);

				if(system != null) {
					FSystemManager._fixedUpdateSystems.Add(system);	
				} else {
					Debug.LogError("FSystems in FixedUpdate : " + systemDescription._typeFullName + " class doesnt exist.");
				}
			}
			for (int i = 0; i < _updateSystemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _updateSystemDescriptions[i];
				FSystem system = this.createSystemInstance(systemDescription);

				if(system != null) {
					FSystemManager._updateSystems.Add(system);
				} else {
					Debug.LogError("FSystems in Update : " + systemDescription._typeFullName + " class doesnt exist.");
				}
			}
			for (int i = 0; i < _lateUpdateSystemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _lateUpdateSystemDescriptions[i];
				FSystem system = this.createSystemInstance(systemDescription);
				
				if(system != null) {
					FSystemManager._lateUpdateSystems.Add(system);
				} else {
					Debug.LogError("FSystems in LateUpdate : " + systemDescription._typeFullName + " class doesnt exist.");
				}
			}
		}

		private void preprocess(){ // do action && update families
			//
			// The way we are dequeuing the actions allows trick with On**** unity callback
			// and Actions.perform function.
			// ->
			// If we add an action with FYFY functions in OnDestroy unity callback, this new
			// action is added to the queue and will be treated during the same dequeue loop 
			// of the remove action at the origin of the OnDestroy calling.
			// Same principle inside Actions.perform function.
			//
			while (GameObjectManager._delayedActions.Count != 0) {
				// During the action perform (and so the Unity callbacks), the current action is always present on the queue top.
				// This is used in TriggerManager && CollisionManager dlls.
				GameObjectManager._delayedActions.Peek().perform();
				GameObjectManager._delayedActions.Dequeue();
			}
			
			foreach(int gameObjectId in GameObjectManager._unbindedGameObjectIds) {
				FamilyManager.updateAfterGameObjectUnbinded(gameObjectId);
				GameObjectManager._modifiedGameObjectIds.Remove(gameObjectId);
			}
			GameObjectManager._unbindedGameObjectIds.Clear();

			foreach(int gameObjectId in GameObjectManager._modifiedGameObjectIds)
				FamilyManager.updateAfterGameObjectModified(gameObjectId);
			GameObjectManager._modifiedGameObjectIds.Clear();

			++_familiesUpdateCount;
		}

		// FIXEDUPDATE CAN RUN FASTER OR SLOWER THAN UPDATE
		// BE CAREFULL WITH TIME.FRAMECOUNT (ENTIRE FRAME), so if multiple fixedUpdate call in one frame, framecount not incremented for each fixedupdate
		// ONE PREPROCESSING PER CYCLE (if more than one fixedUpdate per frame, each fixedUpdate call preprocessing !)
		// if no fixedUpdate in this frame, update call preprocessing
		private void FixedUpdate(){ // process systems
			if(Application.isPlaying == false){
				return;
			}

			this.preprocess();

			foreach(FSystem system in FSystemManager._fixedUpdateSystems)
				if(system.Pause == false)
					system.process(_familiesUpdateCount);
		}

		private void Update(){ // process systems
			if(Application.isPlaying == false){
				return;
			}

			this.preprocess();

			foreach(FSystem system in FSystemManager._updateSystems)
				if(system.Pause == false)
					system.process(_familiesUpdateCount);
		}

		private void LateUpdate(){ // process systems
			if(Application.isPlaying == false){
				return;
			}

			this.preprocess();

			foreach(FSystem system in FSystemManager._lateUpdateSystems)
				if(system.Pause == false)
					system.process(_familiesUpdateCount);

			if(GameObjectManager._sceneBuildIndex != -1) { // load scene if it's desired
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameObjectManager._sceneBuildIndex); // done at the beginning of the "Unity" next frame
			} else if(GameObjectManager._sceneName != null) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameObjectManager._sceneName);       // done at the beginning of the "Unity" next frame
			}
		}
	}
}