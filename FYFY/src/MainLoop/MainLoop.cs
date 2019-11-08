using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reflection;


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
		/// <summary>MainLoop instance (singleton)</summary>
		public static MainLoop instance; // eviter davoir plusieurs composants MainLoop dans toute la scene (cf Awake)
		
		/// <summary>
		///		Reference to the mainLoopEditorScanner defined into FYFY_Inspector package. Because we have not access
		///		to this package here, we use super class type (MonoBehaviour). This reference will not be null if 
		///		MainLoopEditorScanner.OnEnable start before MainLoop.OnEnable.
		/// </summary>
		internal static MonoBehaviour mainLoopEditorScanner = null;
		
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
			if(instance != null) {
				DestroyImmediate(this);
				return;
			}

			instance = this;

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
		
		
		/// <summary>Call function "functionName" defined inside "systemName" system with "parameter" parameter </summary>
		public static void callAppropriateSystemMethod(string systemName, string functionName, object parameter)
		{
			// Get instances of all systems
			List<FSystem> allSystems = new List<FSystem>(FSystemManager.fixedUpdateSystems());
			allSystems.AddRange(FSystemManager.updateSystems());
			allSystems.AddRange(FSystemManager.lateUpdateSystems());
			// Parse all system to find the one associated to first parameter
			foreach (FSystem system in allSystems)
			{
				Type systemType = system.GetType();
				if (systemType.Name == systemName)
				{
					// We found the system, now found the target funcion
					MethodInfo systemMethod;
					if (parameter != null)
						systemMethod = systemType.GetMethod(functionName, new Type[] { parameter.GetType() });
					else
						systemMethod = systemType.GetMethod(functionName, new Type[] { });
					if (systemMethod != null){
						// We found the function, then we invoke it
						if (parameter != null)
							systemMethod.Invoke(system, new object[] { parameter });
						else
							systemMethod.Invoke(system, new object[] { });
					}
				}
			}
		}
		
		/// <summary>Synchronize systems' wrappers</summary>
		public bool synchronizeWrappers(){

			bool needRefresh = false;

			// Get FSystem type and all public methods. This list is used to exclude inheritance methods from FSystem.
			Type fSystemType = typeof(FSystem);
			List<string> fSystemMethodsName = new List<string>();
			MethodInfo[] fSystemMethods = fSystemType.GetMethods();
			foreach (MethodInfo mi in fSystemMethods)
				fSystemMethodsName.Add(mi.Name);

			// Get all systems' Type affected to one FYFY execution context
			List<SystemDescription> allSystemsDescription = new List<SystemDescription>();
			if (_fixedUpdateSystemDescriptions != null)
				allSystemsDescription.AddRange(_fixedUpdateSystemDescriptions);
			if (_updateSystemDescriptions != null)
				allSystemsDescription.AddRange(_updateSystemDescriptions);
			if (_lateUpdateSystemDescriptions != null)
				allSystemsDescription.AddRange(_lateUpdateSystemDescriptions);
			
			List<string> allSystemsName = new List<string>();
			foreach (SystemDescription systemDesc in allSystemsDescription)
			{
				// Get current system Type
				Type systemType = Type.GetType(systemDesc._typeAssemblyQualifiedName);
				allSystemsName.Add(systemType.FullName);
				
				string compilableName = systemType.FullName.Replace('.', '_'); // class name can't contains '.' character and it could be the case for systems included inside libraries

				string cSharpCode = "using UnityEngine;\n";
				cSharpCode += "using FYFY;\n\n";
				cSharpCode += "[ExecuteInEditMode]\n";
				cSharpCode += "public class " + compilableName + "_wrapper : MonoBehaviour\n{\n"; 
				cSharpCode += "\tprivate void Start()\n";
				cSharpCode += "\t{\n";
				cSharpCode += "\t\tthis.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector\n";
				cSharpCode += "\t}\n\n";

				// Load all methods of this System
				MethodInfo[] methodsInfo = systemType.GetMethods();
				foreach (MethodInfo methodInfo in methodsInfo)
				{
					// Do not process inhéritance methods from FSystem
					if (!fSystemMethodsName.Contains(methodInfo.Name))
					{
						ParameterInfo[] parametersInfo = methodInfo.GetParameters();
						// Unity accept only void functions with only one parameter, so we don't process non void functions and functions with more than one parmater
						if (methodInfo.ReturnType == typeof(void) && parametersInfo.Length < 2)
						{
							cSharpCode += "\tpublic void " + methodInfo.Name + "(";
							// Store the optional parameter
							List<Type> parametersType = new List<Type>();
							if (parametersInfo.Length == 1)
								cSharpCode += parametersInfo[0].ParameterType + " " + parametersInfo[0].Name;
							cSharpCode += ")\n\t{\n";
							cSharpCode += "\t\tMainLoop.callAppropriateSystemMethod (\"" + systemType.FullName + "\", \"" + methodInfo.Name + "\", " + (parametersInfo.Length == 1 ? parametersInfo[0].Name : "null") + ");\n";
							cSharpCode += "\t}\n\n";
						}
					}
				}
				cSharpCode += "}";
				// Write .cs file inside Assets/AutomaticScript/
				Directory.CreateDirectory("Assets/AutomaticScript");
				if (File.Exists("Assets/AutomaticScript/" + compilableName + "_wrapper.cs"))
				{
					if (cSharpCode != File.ReadAllText("Assets/AutomaticScript/" + compilableName + "_wrapper.cs"))
					{
						needRefresh = true;
						File.WriteAllText("Assets/AutomaticScript/" + compilableName + "_wrapper.cs", cSharpCode);
					}
				}
				else
				{
					needRefresh = true;
					File.WriteAllText("Assets/AutomaticScript/" + compilableName + "_wrapper.cs", cSharpCode);
				}
			}

			if (!needRefresh){
				// Add components to the MainLoop for each system registered
				foreach (string systemName in allSystemsName){
					string compilableName = systemName.Replace('.', '_'); // class name can't contains '.' character and it could be the case for systems included inside libraries
					if (gameObject.GetComponent(compilableName + "_wrapper") == null){
						// the ", Assembly-CSharp" enable to find the type that is not included into FYFY namespace (indead it is user type defined inside Unity project)
						gameObject.AddComponent(Type.GetType(compilableName + "_wrapper, Assembly-CSharp")); // Because we are in editmode, we don't use GameObjectManager
					}
				}
			}

			// Remove components from the MainLoop for each system that is not registered to the MainLoop
			List<MonoBehaviour> currentsComponents = new List<MonoBehaviour>(gameObject.GetComponents<MonoBehaviour>());
			foreach (MonoBehaviour currentComponent in currentsComponents)
			{
				// Check if current component is a system wrapper
				Type componentType = currentComponent.GetType();
				if (componentType.FullName.EndsWith("_wrapper"))
				{
					bool found = false;
					for (int i = 0; i < allSystemsName.Count && !found; i++)
						found = (componentType.FullName == (allSystemsName[i].Replace('.', '_') + "_wrapper"));
					if (!found){
						DestroyImmediate(currentComponent);  // Because we are in editmode, we don't use GameObjectManager
					}
				}
			}

			return needRefresh;
		}

		private void OnDestroy(){
			if(instance == this)
				instance = null;
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
		
		private void OnEnable(){
			// if upgrade FYFY, old version instance could be null because Awake is not called again, we set instance reference properly
			if(instance == null)
				instance = this;
			// Check if we have to call back MainLoopEditorScanner OnEnable
			if(mainLoopEditorScanner != null){
				// Yes, we have to do so we do it
				// Because we are here in game context we can't use class defined in inspector context (build will fail)
				// So we have to inspect types dynamically, first we try to find the MainLoopEditorScanner type (it will be the
				// case if we are in Unity editor context
				Type MainLoopEditorScanner_Type = Type.GetType("FYFY_Inspector.MainLoopEditorScanner, FYFY_Inspector");
				if (MainLoopEditorScanner_Type != null){ // could be null if we are not in Unity editor context (game built)
					// Here we found the MainLoopEditorScanner type, so we inspect it to find "OnEnable" method
					MethodInfo OnEnable_Method = MainLoopEditorScanner_Type.GetMethod("OnEnable", new Type[] { });
					if (OnEnable_Method != null)
						// And call the method on the Instance field
						OnEnable_Method.Invoke(mainLoopEditorScanner, new object[] { });
					else
						UnityEngine.Debug.LogError("Warning, inconsistent method inside FYFY_Inspector.MainLoopEditorScanner, \"OnEnable\" method is not defined.");
				}
				mainLoopEditorScanner = null;
			}
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
				// In case of GameObject state (enable/disable) is controlled by Unity tools (animators for instance) Fyfy is not notified from this change => solution add a special component that catch Unity events and submit update to FYFY
				gameObject.AddComponent<FyfyBridge>();
				
				// Compute Wrappers
				HashSet<string> componentTypeNames = new HashSet<string>();
				foreach(Component c in gameObject.GetComponents<Component>()) {
					if (c != null){ // it is possible if a GameObject contains a breaked component (Missing script)
						System.Type type = c.GetType();
						componentTypeNames.Add(type.FullName);
					}
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