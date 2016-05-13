using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	public static class GameObjectManager {
		internal static readonly Dictionary<string, GameObject> _prefabResources        = new Dictionary<string, GameObject>();
		internal static readonly Dictionary<int, GameObjectWrapper> _gameObjectWrappers = new Dictionary<int, GameObjectWrapper>(); // indexed by gameobject's id
		internal static readonly Queue<IGameObjectManagerAction> _delayedActions        = new Queue<IGameObjectManagerAction>();
		internal static readonly HashSet<int> _destroyedGameObjectIds                   = new HashSet<int>(); // destroyGO
		internal static readonly HashSet<int> _modifiedGameObjectIds                    = new HashSet<int>(); // createGO or addComponent or removeComponent

		public static int Count { get { return _gameObjectWrappers.Count; } }

		public static GameObject createGameObject() {
			GameObject gameObject = new GameObject();
			_delayedActions.Enqueue(new CreateGameObjectWrapper(gameObject, new HashSet<uint>{ TypeManager.getTypeId (typeof(Transform)) }));

			return gameObject;
		}

		public static GameObject createPrimitive(PrimitiveType type) {
			GameObject gameObject = GameObject.CreatePrimitive(type);
			_delayedActions.Enqueue(new CreateGameObjectWrapper(gameObject));

			return gameObject;
		}

		public static GameObject instantiatePrefab(string prefabName) {
			if(prefabName == null)
				throw new MissingReferenceException();

			GameObject prefabResource;
			if (_prefabResources.TryGetValue(prefabName, out prefabResource) == false) {
				if ((prefabResource = Resources.Load<GameObject>(prefabName)) == null) {
					Debug.LogWarning("Can't instantiate '" + prefabName + "', because it doesn't exist or it isn't present in 'Assets/Resources' folder.");
					return null;
				}
					
				_prefabResources.Add(prefabName, prefabResource);
			}

			GameObject gameObject = GameObject.Instantiate<GameObject>(prefabResource);
			_delayedActions.Enqueue(new CreateGameObjectWrapper(gameObject));

			return gameObject;
		}

		public static void destroyGameObject(GameObject gameObject){
			if(gameObject == null)
				throw new MissingReferenceException();
			
			_delayedActions.Enqueue(new DestroyGameObject(gameObject));
		}

		public static void enableGameObject(GameObject gameObject){
			if(gameObject == null)
				throw new MissingReferenceException();

			_delayedActions.Enqueue(new EnableGameObject(gameObject));
		}

		public static void disableGameObject(GameObject gameObject){
			if(gameObject == null)
				throw new MissingReferenceException();
			
			_delayedActions.Enqueue(new DisableGameObject(gameObject));
		}

		public static void addComponent<T>(GameObject gameObject, object componentValues = null) where T : Component {
			if(gameObject == null)
				throw new MissingReferenceException();
		
			_delayedActions.Enqueue(new AddComponent<T>(gameObject, componentValues));
		}

		public static void addComponent(GameObject gameObject, System.Type componentType, object componentValues = null) {
			if(gameObject == null || componentType == null)
				throw new MissingReferenceException();
			
			if (componentType.IsSubclassOf(typeof(Component)) == false) {
				Debug.LogWarning("Can't add '" + componentType + "' to " + gameObject.name + " because a '" + componentType + "' isn't a Component!");
				return;
			}

			_delayedActions.Enqueue(new AddComponent(gameObject, componentType, componentValues));
		}

		public static void removeComponent<T>(GameObject gameObject) where T : Component {
			if(gameObject == null)
				throw new MissingReferenceException();
		
			System.Type componentType = typeof(T);
			if (componentType == typeof(Transform)) {
				Debug.Log("Removing 'Transform' from " + gameObject.name + " is not allowed!");
				return;
			}

			_delayedActions.Enqueue(new RemoveComponent<T>(gameObject, componentType));
		}

		public static void removeComponent(Component component) {
			if(component == null)
				throw new MissingReferenceException();

			GameObject gameObject = component.gameObject;
			System.Type componentType = component.GetType();
			if (componentType == typeof(Transform)) {
				Debug.Log("Removing 'Transform' from " + gameObject.name + " is not allowed!");
				return;
			}

			_delayedActions.Enqueue(new RemoveComponent(gameObject, component, componentType));
		}
	}
}

// Charger un xml ou json ou whatever
// Load next_scene + garder les managers etc du coup ??