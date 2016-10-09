using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TriggerManager")]   // ugly
[assembly: InternalsVisibleTo("CollisionManager")] // ugly

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
	/// 		For example, in a <c>Update block</c>, if you call the remove function on a component inside a system, the component is really removed in the 
	/// 		<c>Late update block</c>. For all the system in the <c>Update block</c>, the component is yet present.
	/// 	</para>
	/// </remarks>
	public static class GameObjectManager {
		internal static readonly Dictionary<string, GameObject> _prefabResources        = new Dictionary<string, GameObject>();     // indexed by prefab name
		internal static readonly Dictionary<int, GameObjectWrapper> _gameObjectWrappers = new Dictionary<int, GameObjectWrapper>(); // indexed by gameobject's id
		internal static readonly Queue<IGameObjectManagerAction> _delayedActions        = new Queue<IGameObjectManagerAction>();
		internal static readonly HashSet<int> _destroyedGameObjectIds                   = new HashSet<int>();                       // destroyGO
		internal static readonly HashSet<int> _modifiedGameObjectIds                    = new HashSet<int>();                       // createGO or addComponent or removeComponent

		internal static IGameObjectManagerAction _currentAction = null; // used in CollisionManager and TriggerManager dlls
		internal static int _sceneBuildIndex = -1; // used in MainLoop LateUpdate
		internal static string _sceneName = null;  // used in MainLoop LateUpdate

		/// <summary>
		/// 	Gets the number of <c>GameObjects</c> of the scene known by FYFY.
		/// </summary>
		public static int Count { get { return _gameObjectWrappers.Count; } }

		/// <summary>
		/// 	Loads the specified scene at the beginning of the next update block.
		/// </summary>
		/// <remarks>
		/// 	The scene is always loaded after closing the current scene.
		/// </remarks>
		/// <param name="sceneBuildIndex">
		/// 	Index of the scene in the Build Settings to load.
		/// </param>
		public static void loadScene(int sceneBuildIndex) {
			_sceneBuildIndex = sceneBuildIndex;
		}

		/// <summary>
		/// 	Loads the scene at the beginning of the next update block.
		/// </summary>
		/// <remarks>
		/// 	The scene is always loaded after closing the current scene.
		/// </remarks>
		/// <param name="sceneName">
		/// 	Name of the scene to load.
		/// </param>
		public static void loadScene(string sceneName) {
			_sceneName = sceneName;
		}

		/// <summary>
		/// 	Creates a game object and returns it. The game object will be registered by FYFY at the beginning of the next update block.
		/// </summary>
		/// <remarks>
		/// 	Even if the game object is not registered, you can use it in other <see cref="FYFY.GameObjectManager">functions</see> in current frame.
		/// </remarks>
		/// <returns>
		/// 	The game object created but not yet registered.
		/// </returns>
		public static GameObject createGameObject() {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			GameObject gameObject = new GameObject();
			_delayedActions.Enqueue(new CreateGameObjectWrapper(
				gameObject, 
				new HashSet<uint>{ TypeManager.getTypeId (typeof(Transform)) }, 
				exceptionStackTrace)
			);

			return gameObject;
		}

		/// <summary>
		/// 	Creates a game object with a primitive mesh renderer and appropriate collider and returns it. The game object will be registered by FYFY at the beginning of the next update block.
		/// </summary>
		/// <remarks>
		/// 	Even if the game object is not registered, you can use it in other <see cref="FYFY.GameObjectManager">functions</see> in current frame.
		/// </remarks>
		/// <returns>
		/// 	The game object created but not yet registered.
		/// </returns>
		/// <param name="type">
		/// 	The type of primitive object to create.
		/// </param>
		public static GameObject createPrimitive(PrimitiveType type) {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			GameObject gameObject = GameObject.CreatePrimitive(type);
			_delayedActions.Enqueue(new CreateGameObjectWrapper(gameObject, exceptionStackTrace));

			return gameObject;
		}

		/// <summary>
		/// 	Creates a game object as a copy of the prefab and returns it. The game object will be registered by FYFY at the beginning of the next update block.
		/// </summary>
		/// <remarks>
		/// 	Even if the game object is not registered, you can use it in other <see cref="FYFY.GameObjectManager">functions</see> in current frame.
		/// </remarks>
		/// <returns>
		/// 	The game object created but not yet registered.
		/// </returns>
		/// <param name="prefabName">
		/// 	The pathname of the target.
		/// </param>
		public static GameObject instantiatePrefab(string prefabName) {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(prefabName == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}

			GameObject prefabResource;
			if (_prefabResources.TryGetValue(prefabName, out prefabResource) == false) {
				if ((prefabResource = Resources.Load<GameObject>(prefabName)) == null) {
					Debug.LogWarning("Can't instantiate '" + prefabName + "', because it doesn't exist or it isn't present in 'Assets/Resources' folder.");
					return null;
				}
					
				_prefabResources.Add(prefabName, prefabResource);
			}

			GameObject gameObject = GameObject.Instantiate<GameObject>(prefabResource);

			// Register the gameobject and all its children.
			foreach(Transform t in gameObject.GetComponentsInChildren<Transform>(true)) { // gameobject.transform is include
				_delayedActions.Enqueue(new CreateGameObjectWrapper(t.gameObject, exceptionStackTrace));
			}

			return gameObject;
		}

		/// <summary>
		/// 	Destroies the game object at the beginning of the next update block.
		/// </summary>
		public static void destroyGameObject(GameObject gameObject){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new DestroyGameObject(gameObject, exceptionStackTrace));
		}

		/// <summary>
		/// 	Sets the state (enable/disable) of the game object at the beginning of the next update block.
		/// </summary>
		public static void setGameObjectState(GameObject gameObject, bool enabled){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
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
				throw new ArgumentNullException(exceptionStackTrace);
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
				throw new ArgumentNullException(exceptionStackTrace);
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
				throw new ArgumentNullException(exceptionStackTrace);
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
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
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
				throw new ArgumentNullException(exceptionStackTrace);
			}

			if (componentType.IsSubclassOf(typeof(Component)) == false) {
				Debug.LogWarning("Can't add '" + componentType + "' to " + gameObject.name + " because a '" + componentType + "' isn't a Component!");
				return;
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
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}
		
			System.Type componentType = typeof(T);
			if (componentType == typeof(Transform)) {
				Debug.Log("Removing 'Transform' from " + gameObject.name + " is not allowed!");
				return;
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
				throw new ArgumentNullException(exceptionStackTrace);
			}

			GameObject gameObject = component.gameObject;
			System.Type componentType = component.GetType();
			if (componentType == typeof(Transform)) {
				Debug.Log("Removing 'Transform' from " + gameObject.name + " is not allowed!");
				return;
			}

			_delayedActions.Enqueue(new RemoveComponent(gameObject, component, exceptionStackTrace));
		}
	}
}