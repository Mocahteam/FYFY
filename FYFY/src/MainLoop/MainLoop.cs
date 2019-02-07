using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("Monitoring")] // ugly

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

	/// <summary>
	/// MainLoop enables:
	///  (1) to set systems into the three contexts (fixedUpdate, update and lateUpdate)
	///  (2) to define which game object binding on start
	///  (3) to follow system load and families content during playing mode
	/// </summary>
	[ExecuteInEditMode] // permet dexecuter awake et start etc aussi en mode edition
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class MainLoop : MonoBehaviour {
		internal static MainLoop _mainLoop; // eviter davoir plusieurs composants MainLoop dans toute la scene (cf Awake)
		
		// this static flag is used by monitoring module to know if the scene will change
		internal static bool sceneChanging = false;

		/// <summary>List of systems defined in fixedUpdate context through the Inspector</summary>
		public SystemDescription[] _fixedUpdateSystemDescriptions; // initialized in inspector, otherwise == null
		/// <summary>List of systems defined in Update context through the Inspector</summary>
		public SystemDescription[] _updateSystemDescriptions;      // initialized in inspector, otherwise == null
		/// <summary>List of systems defined in lateUpdate context through the Inspector</summary>
		public SystemDescription[] _lateUpdateSystemDescriptions;  // initialized in inspector, otherwise == null
		
		/// <summary>Used in FSystem to force Inspector redraw when pause is update by code</summary>
		public int _forceUpdateInspector;

		/// <summary>
		/// Define how game objects are binded on start
		/// 0 means bind all game objects on start but exclude game objects defined into _specialGameObjects
		/// 1 means bind only game objects defined into _specialGameObjects
		/// </summary>
		public int _loadingState = 0;
		/// <summary></summary>
		public List<GameObject> _specialGameObjects;

		/// <summary>Show system profiler</summary>
		public bool showSystemProfiler = true;
		/// <summary>Show families used in systems</summary>
		public bool showFamilyInspector = false;
		/// <summary>Show families used in fixed update systems</summary>
		public bool showFamilyInspectorFixedUpdate = false;
		/// <summary>Show families used in update systems</summary>
		public bool showFamilyInspectorUpdate = false;
		/// <summary>Show families used in late update systems</summary>
		public bool showFamilyInspectorLateUpdate = false;

		/// <summary>How much time (ms) the previous fixed update execution lasted</summary>
		public float fixedUpdateStats = 0;
		/// <summary>How much time (ms) the previous update execution lasted</summary>
		public float updateStats = 0;
		/// <summary>How much time (ms) the previous late update execution lasted</summary>
		public float lateUpdateStats = 0;
		private Stopwatch _stopwatch;

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
			sceneChanging = false;
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

			// Parse scene and bind GameObjects to FYFY
			List<GameObject> sceneGameObjects = new List<GameObject>();
			if (_loadingState == 0) {
				// Bind all game objects except ones defined in _specialGameObjects
				GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().GetRootGameObjects ();
				foreach (GameObject root in roots) {
					foreach (Transform childTransform in root.GetComponentsInChildren<Transform>(true)) { // include root transform
						// Check if current game object is not an excluded game object or a child of an excluded game object
						bool needToExclude = false;
						foreach (GameObject excluded in _specialGameObjects) {
							if (excluded != null) {
								if (childTransform.IsChildOf (excluded.transform)) { // true if childTransform == excluded.transform
									needToExclude = true;
									break;
								}
							}
						}
						if (!needToExclude)
							sceneGameObjects.Add (childTransform.gameObject);
					}
				}
			} else {
				// Bind only game objects defined in _specialGameObjects
				foreach (GameObject included in _specialGameObjects) {
					if (included != null) {
						foreach (Transform childTransform in included.GetComponentsInChildren<Transform>(true)) { // include itself
							sceneGameObjects.Add (childTransform.gameObject);
						}
					}
				}
			}
			// Bind all game object tagged as DontDestroyOnLoad
			foreach (GameObject ddol_go in GameObjectManager._ddolObjects) {
				if (ddol_go != null){
					foreach (Transform childTransform in ddol_go.GetComponentsInChildren<Transform>(true)) { // include itself
						sceneGameObjects.Add (childTransform.gameObject);
					}
				}
			}
			
			foreach(GameObject gameObject in sceneGameObjects) {
				HashSet<string> componentTypeNames = new HashSet<string>();
				foreach(Component c in gameObject.GetComponents<Component>()) {
					System.Type type = c.GetType();
					componentTypeNames.Add(type.FullName);
				}

				GameObjectWrapper gameObjectWrapper = new GameObjectWrapper(gameObject, componentTypeNames);
				if (!GameObjectManager._gameObjectWrappers.ContainsKey(gameObject.GetInstanceID())){
					GameObjectManager._gameObjectWrappers.Add(gameObject.GetInstanceID(), gameObjectWrapper);
				}
			}

			// Create all systems
			for (int i = 0; i < _fixedUpdateSystemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _fixedUpdateSystemDescriptions[i];
				try{
					FSystem system = this.createSystemInstance(systemDescription);

					if(system != null) {
						FSystemManager._fixedUpdateSystems.Add(system);	
					} else {
						UnityEngine.Debug.LogError(systemDescription._typeFullName + " class doesn't exist, hooking up this FSystem to FixedUpdate context aborted. Check your MainLoop Game Object.");
					}
				}catch(System.Exception e){
					UnityEngine.Debug.LogException(e);
				}
			}
			for (int i = 0; i < _updateSystemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _updateSystemDescriptions[i];
				try{
					FSystem system = this.createSystemInstance(systemDescription);

					if(system != null) {
						FSystemManager._updateSystems.Add(system);
					} else {
						UnityEngine.Debug.LogError(systemDescription._typeFullName + " class doesn't exist, hooking up this FSystem to Update context aborted. Check your MainLoop Game Object.");
					}
				} catch (System.Exception e){
					UnityEngine.Debug.LogException(e);
				}
			}
			for (int i = 0; i < _lateUpdateSystemDescriptions.Length; ++i) {
				SystemDescription systemDescription = _lateUpdateSystemDescriptions[i];
				try{
					FSystem system = this.createSystemInstance(systemDescription);
					
					if(system != null) {
						FSystemManager._lateUpdateSystems.Add(system);
					} else {
						UnityEngine.Debug.LogError(systemDescription._typeFullName + " class doesn't exist, hooking up this FSystem to LateUpdate context aborted. Check your MainLoop Game Object.");
					}
				} catch (System.Exception e){
					UnityEngine.Debug.LogException(e);
				}
			}

			_stopwatch = new Stopwatch ();
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
				try{
					GameObjectManager._delayedActions.Peek().perform();
				} catch (System.Exception e){
					UnityEngine.Debug.LogException(e);
				}
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

			_stopwatch.Reset ();
			_stopwatch.Start ();
			foreach(FSystem system in FSystemManager._fixedUpdateSystems)
				if(system.Pause == false){
					try{
						system.process(_familiesUpdateCount);
					}
					catch (System.Exception e){
						UnityEngine.Debug.LogException(e);
					}
				}
			_stopwatch.Stop ();
			fixedUpdateStats = _stopwatch.ElapsedMilliseconds;
		}

		private void Update(){ // process systems
			if(Application.isPlaying == false){
				return;
			}

			this.preprocess();

			_stopwatch.Reset ();
			_stopwatch.Start ();
			foreach(FSystem system in FSystemManager._updateSystems)
				if(system.Pause == false){
					try{
						system.process(_familiesUpdateCount);
					} catch (System.Exception e){
						UnityEngine.Debug.LogException(e);
					}
				}
			_stopwatch.Stop ();
			updateStats = _stopwatch.ElapsedMilliseconds;
		}

		private void LateUpdate(){ // process systems
			if(Application.isPlaying == false){
				return;
			}

			this.preprocess();

			_stopwatch.Reset ();
			_stopwatch.Start ();
			foreach(FSystem system in FSystemManager._lateUpdateSystems)
				if(system.Pause == false){
					try{
						system.process(_familiesUpdateCount);
					} catch (System.Exception e){
						UnityEngine.Debug.LogException(e);
					}
				}
			_stopwatch.Stop ();
			lateUpdateStats = _stopwatch.ElapsedMilliseconds;

			if(GameObjectManager._sceneBuildIndex != -1) { // load scene if it's desired
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameObjectManager._sceneBuildIndex); // done at the beginning of the "Unity" next frame
				sceneChanging = true;
			} else if(GameObjectManager._sceneName != null) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(GameObjectManager._sceneName);       // done at the beginning of the "Unity" next frame
				sceneChanging = true;
			}
		}
	}
}