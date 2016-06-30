using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TriggerManager")] // ugly 

namespace FYFY {
	public static class GameObjectManager {
		internal static readonly Dictionary<string, GameObject> _prefabResources        = new Dictionary<string, GameObject>();
		internal static readonly Dictionary<int, GameObjectWrapper> _gameObjectWrappers = new Dictionary<int, GameObjectWrapper>(); // indexed by gameobject's id
		internal static readonly Queue<IGameObjectManagerAction> _delayedActions        = new Queue<IGameObjectManagerAction>();
		internal static readonly HashSet<int> _destroyedGameObjectIds                   = new HashSet<int>(); // destroyGO
		internal static readonly HashSet<int> _modifiedGameObjectIds                    = new HashSet<int>(); // createGO or addComponent or removeComponent

		public static int Count { get { return _gameObjectWrappers.Count; } }

		internal static int _sceneBuildIndex = -1;
		internal static string _sceneName = null;

		// voir MainLoop lateUpdate pour lutilisation
		public static void loadScene(int sceneBuildIndex) {
			_sceneBuildIndex = sceneBuildIndex;
		}
		// voir MainLoop lateUpdate pour lutilisation
		public static void loadScene(string sceneName) {
			_sceneName = sceneName;
		}

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

		public static GameObject createPrimitive(PrimitiveType type) {System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			GameObject gameObject = GameObject.CreatePrimitive(type);
			_delayedActions.Enqueue(new CreateGameObjectWrapper(gameObject, exceptionStackTrace));

			return gameObject;
		}

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
			_delayedActions.Enqueue(new CreateGameObjectWrapper(gameObject, exceptionStackTrace));

			return gameObject;
		}

		public static void destroyGameObject(GameObject gameObject){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new DestroyGameObject(gameObject, exceptionStackTrace));
		}

		public static void setGameObjectState(GameObject gameObject, bool enabled){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}
			
			_delayedActions.Enqueue(new SetGameObjectState(gameObject, enabled, exceptionStackTrace));
		}

		public static void setGameObjectParent(GameObject gameObject, GameObject parent, bool worldPositionStays){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new SetGameObjectParent(gameObject, parent, worldPositionStays, exceptionStackTrace));
		}

		public static void setGameObjectLayer(GameObject gameObject, int layer){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new SetGameObjectLayer(gameObject, layer, exceptionStackTrace));
		}

		public static void setGameObjectTag(GameObject gameObject, string tag){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null || tag == null){
				throw new ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new SetGameObjectTag(gameObject, tag, exceptionStackTrace));
		}

		public static void addComponent<T>(GameObject gameObject, object componentValues = null) where T : Component {
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1, true);                                  // get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName() + ":" + stackFrame.GetFileLineNumber().ToString() + ")"; // to point where this function was called

			if(gameObject == null) {
				throw new ArgumentNullException(exceptionStackTrace);
			}

			_delayedActions.Enqueue(new AddComponent<T>(gameObject, componentValues, exceptionStackTrace));
		}

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

// Charger un xml ou json ou whatever
// Load next_scene + garder les managers etc du coup ??