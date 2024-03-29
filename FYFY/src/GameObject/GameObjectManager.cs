﻿using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TriggerManager")]   // ugly
[assembly: InternalsVisibleTo("CollisionManager")] // ugly
[assembly: InternalsVisibleTo("PointerManager")] // ugly

namespace FYFY {
	/// <summary>
	/// 	Manager of GameObject.
	/// </summary>
	/// <remarks>
	/// 	<para>
	/// 		You must use the <see cref="FYFY.GameObjectManager"/> functions when you deal with FYFY otherwise, FYFY can't retrieve information to maintain
	/// 		uptodate families. 
	/// 	</para>
	/// 	<para>
	/// 		When you call a <see cref="FYFY.GameObjectManager"/> function, the real action is done at the beginning of the next update block.
	/// 		This has the effect to maintain a consistent state inside a update block.
	/// 		For example, in a <c>Update block</c>, if you call the remove function on a component inside a system, the component is really removed in the next FYFY update (just before the <c>Late update block</c> in this example. For all the system in the <c>Update block</c>, the component is yet present.
	/// 	</para>
	/// </remarks>
	public static class GameObjectManager {
		internal static readonly Dictionary<int, GameObjectWrapper> _gameObjectWrappers = new Dictionary<int, GameObjectWrapper>(); // indexed by gameobject's id
		internal static readonly Queue<IGameObjectManagerAction> _delayedActions        = new Queue<IGameObjectManagerAction>();
		internal static readonly HashSet<int> _unbindedGameObjectIds                    = new HashSet<int>();                       // unbindGO
		internal static readonly HashSet<int> _modifiedGameObjectIds                    = new HashSet<int>();                       // bindGO or addComponent or removeComponent
		
		internal static HashSet<GameObject> _ddolObjects = new HashSet<GameObject>(); // used to manage DontDestroyOnLoad mechanism => used in MainLoop Start

		/// <summary>
		/// 	Gets the number of <c>GameObjects</c> of the scene known by FYFY.
		/// </summary>
		public static int Count { get { return _gameObjectWrappers.Count; } }

		/// <summary>
		/// 	Loads the specified scene at the beginning of the next update block.
		/// </summary>
		/// <param name="sceneBuildIndex">
		/// 	Index of the scene in the Build Settings to load.
		/// </param>
		/// <param name="mode">
		/// 	Allows you to specify whether or not to load the Scene additively. See UnityEngine.SceneManagement.LoadSceneMode for more information about the options.
		/// </param>
		public static void loadScene(int sceneBuildIndex, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single) {
			UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex, mode); // done at the beginning of the "Unity" next frame
			if (mode == UnityEngine.SceneManagement.LoadSceneMode.Single)
				MainLoop.sceneChanging = true;
			else if (mode == UnityEngine.SceneManagement.LoadSceneMode.Additive){
				MainLoop.instance.lastFrameSceneLoaded = Time.frameCount;
				MainLoop.instance.loadedSceneById.Add (sceneBuildIndex);
			}
		}

		/// <summary>
		/// 	Loads the scene at the beginning of the next update block.
		/// </summary>
		/// <param name="sceneName">
		/// 	Name of the scene to load.
		/// </param>
		/// <param name="mode">
		/// 	Allows you to specify whether or not to load the Scene additively. See UnityEngine.SceneManagement.LoadSceneMode for more information about the options.
		/// </param>
		public static void loadScene(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single) {
			UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode); // done at the beginning of the "Unity" next frame
			if (mode == UnityEngine.SceneManagement.LoadSceneMode.Single)
				MainLoop.sceneChanging = true;
			else if (mode == UnityEngine.SceneManagement.LoadSceneMode.Additive){
				MainLoop.instance.lastFrameSceneLoaded = Time.frameCount;
				MainLoop.instance.loadedSceneByName.Add (sceneName);
			}
		}
		
		/// <summary>
		///		Destroys all GameObjects associated with the given Scene and removes the Scene from the SceneManager.
		/// </summary>
		/// <param name="sceneName">
		/// 	Name of the scene to load.
		/// </param>
		public static void unloadScene(string sceneName){
			MainLoop.instance.unloadedSceneByName.Add (sceneName);
		}
		
		/// <summary>
		///		Destroys all GameObjects associated with the given Scene and removes the Scene from the SceneManager.
		/// </summary>
		/// <param name="sceneBuildIndex">
		/// 	Index of the scene in the Build Settings to load.
		/// </param>
		public static void unloadScene(int sceneBuildIndex){
			MainLoop.instance.unloadedSceneById.Add (sceneBuildIndex);
		}

		/// <summary>
		/// 	Makes the game object target not be destroyed automatically when loading a new scene and rebind it automatically with FYFY.
		/// </summary>
		/// <param name="target">
		/// 	The game object to configure.
		/// </param>
		public static void dontDestroyOnLoadAndRebind(GameObject target) {
			Object.DontDestroyOnLoad(target);
			if (!_ddolObjects.Contains(target)){
				_ddolObjects.Add(target);
			}
		}

		/// <summary>
		/// 	Bind a game object with FYFY. The game object will be registered by FYFY at the beginning of the next update block.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to bind.
		/// </param>
		/// <param name="recursive">
		/// 	Should bind child recursively (true default)
		/// </param>
		/// <remarks>
		/// 	In the same frame of binding, you can use it in other <see cref="FYFY.GameObjectManager"/>.
		/// </remarks>
		public static void bind(GameObject gameObject, bool recursive = true) {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called
			
			if(gameObject == null) { // The GO has been destroyed !!!
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			if (recursive)
				// Bind the gameobject and all its children.
				foreach(Transform t in gameObject.GetComponentsInChildren<Transform>(true)) { // gameobject.transform is include
					_delayedActions.Enqueue(new BindGameObject(t.gameObject, exceptionStackTrace, t != gameObject.transform));
				}
			else
				// Bind only this gameobject
				_delayedActions.Enqueue(new BindGameObject(gameObject, exceptionStackTrace));
		}

		/// <summary>
		/// 	Unbind a game object to FYFY at the beginning of the next update block.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to unbind.
		/// </param>
		/// <param name="recursive">
		/// 	Should unbind child recursively (true default)
		/// </param>
		public static void unbind(GameObject gameObject, bool recursive = true){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}
			
			if (recursive)
				// Unbind the gameobject and all its children.
				foreach(Transform t in gameObject.GetComponentsInChildren<Transform>(true)) { // gameobject.transform is include
					_delayedActions.Enqueue(new UnbindGameObject(t.gameObject, exceptionStackTrace, t != gameObject.transform));
				}
			else
				// Unbind only this gameobject
				_delayedActions.Enqueue(new UnbindGameObject(gameObject, exceptionStackTrace));
		}
		
		/// <summary>
		/// 	Return true if the gameObject is bound, false else.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to check.
		/// </param>
		public static bool isBound(GameObject gameObject){
			return _gameObjectWrappers.ContainsKey(gameObject.GetInstanceID());
		}

		/// <summary>
		/// 	Sets the state (enable/disable) of the game object at the beginning of the next update block.
		/// </summary>
		public static void setGameObjectState(GameObject gameObject, bool enabled){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}
			
			_delayedActions.Enqueue(new SetGameObjectState(gameObject, enabled, exceptionStackTrace));
		}

		/// <summary>
		/// 	Sets the game object parent at the beginning of the next update block.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to change.
		/// </param>
		/// <param name="parent">
		/// 	The game object which become the new parent. This parameter can be null to reset the parent of <paramref name="gameObject"/>.
		/// </param>
		/// <param name="worldPositionStays">
		/// 	If true, the parent-relative position, scale and rotation is modified such that the object keeps the same world space position, rotation and scale as before.
		/// </param>
		public static void setGameObjectParent(GameObject gameObject, GameObject parent, bool worldPositionStays){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new SetGameObjectParent(gameObject, parent, worldPositionStays, exceptionStackTrace));
		}

		/// <summary>
		/// 	Sets the game object layer at the beginning of the next update block.
		/// </summary>
		public static void setGameObjectLayer(GameObject gameObject, int layer){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new SetGameObjectLayer(gameObject, layer, exceptionStackTrace));
		}

		/// <summary>
		/// 	Sets the game object tag at the beginning of the next update block.
		/// </summary>
		public static void setGameObjectTag(GameObject gameObject, string tag){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null || tag == null){
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new SetGameObjectTag(gameObject, tag, exceptionStackTrace));
		}

		/// <summary>
		/// 	Adds a component to the game object at the beginning of the next update block.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to change.
		/// </param>
		/// <param name="componentValues">
		/// 	The component values to affect. It must be an anonymous type object.
		/// </param>
		/// <typeparam name="T">
		/// 	The component type to add.
		/// </typeparam>
		public static void addComponent<T>(GameObject gameObject, object componentValues = null) where T : Component {
			addComponent<T>(gameObject, false, componentValues);
		}
		
		// used in pluggins (TriggerSensitive / CollisionSensitive / PointerOver)
		internal static void addComponent<T>(GameObject gameObject, bool internalCall, object componentValues = null) where T : Component {
			string exceptionStackTrace = "";
			if (!internalCall){
				// We need to take the second frame before current frame because user pass through the public call this adds one more indirect call
				System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(2, true);                                  // get caller stackFrame with informations
				exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called
			}

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new AddComponent<T>(gameObject, componentValues, exceptionStackTrace));
		}

		/// <summary>
		/// 	Adds a component to the game object at the beginning of the next update block.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to change.
		/// </param>
		/// <param name="componentType">
		/// 	The component type to add.
		/// </param>
		/// <param name="componentValues">
		/// 	The component values to affect. It must be an anonymous type object.
		/// </param>
		public static void addComponent(GameObject gameObject, System.Type componentType, object componentValues = null) {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null || componentType == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			if (componentType.IsSubclassOf(typeof(Component)) == false) {
				throw new FyfyException("The type \"" + componentType + "\" must be convertible to \"UnityEngine.Component\" in order to use it with this function", exceptionStackTrace);
			}

			_delayedActions.Enqueue(new AddComponent(gameObject, componentType, componentValues, exceptionStackTrace));
		}

		/// <summary>
		/// 	Removes a component of a game object at the beginning of the next update block.
		/// </summary>
		/// <param name="gameObject">
		/// 	The game object to change.
		/// </param>
		/// <typeparam name="T">
		/// 	The component type to remove.
		/// </typeparam>
		public static void removeComponent<T>(GameObject gameObject) where T : Component {
			removeComponent<T>(gameObject, false);
		}
		
		// used in pluggins (TriggerSensitive / CollisionSensitive / PointerOver)
		internal static void removeComponent<T>(GameObject gameObject, bool internalCall) where T : Component {
			string exceptionStackTrace = "";
			if (!internalCall){
				// We need to take the second frame before current frame because user pass through the public call this adds one more indirect call
				System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(2, true);                                  // get caller stackFrame with informations
				exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ") "+stackFrame.GetMethod(); // to point where this function was called
			}

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}
		
			System.Type componentType = typeof(T);
			if (componentType == typeof(Transform)) {
				throw new FyfyException("Removing \"Transform\" Component is not allowed!", exceptionStackTrace);
			}

			_delayedActions.Enqueue(new RemoveComponent<T>(gameObject, exceptionStackTrace));
		}

		/// <summary>
		/// 	Removes the component from its game object at the beginning of the next update block.
		/// </summary>
		public static void removeComponent(Component component) {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(component == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}

			GameObject gameObject = component.gameObject;
			System.Type componentType = component.GetType();
			if (componentType == typeof(Transform)) {
				throw new FyfyException("Removing \"Transform\" Component is not allowed!", exceptionStackTrace);
			}

			_delayedActions.Enqueue(new RemoveComponent(gameObject, component, exceptionStackTrace));
		}

		/// <summary>
		/// 	Force FYFY to refresh families for this GameObject at the beginning of the next update block.
		/// </summary>
		public static void refresh(GameObject gameObject){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new FYFY.ArgumentNullException(exceptionStackTrace);
			}
			
			// Check if gameObject is binded to FYFY
			if (_gameObjectWrappers.ContainsKey(gameObject.GetInstanceID())){
				// rebuild wrappers
				HashSet<string> componentTypeNames = new HashSet<string>();
				foreach(Component c in gameObject.GetComponents<Component>()) {
					if (c != null){ // it is possible if a GameObject contains a breaked component (Missing script)
						System.Type type = c.GetType();
						componentTypeNames.Add(type.FullName);
					}
				}
				_gameObjectWrappers[gameObject.GetInstanceID()] = new GameObjectWrapper(gameObject, componentTypeNames);

				_modifiedGameObjectIds.Add(gameObject.GetInstanceID());
			}
		}
		
		// used in pluggins (TriggerSensitive / CollisionSensitive / PointerOver)
		internal static bool containActionFor(System.Type actionType, Transform[] goTransforms){
			foreach(IGameObjectManagerAction action in _delayedActions) {
				if(action.GetType() == actionType) {
					GameObject go = action.getTarget();
					foreach(Transform t in goTransforms) {
						if(t.gameObject == go) {
							// We find an action of asked type for one of the game object of the set
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}