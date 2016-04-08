using UnityEngine;
using System.Collections.Generic;

namespace UECS {
	public static class EntityManager { // -> GameObjectManager
		internal static readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity> ();
		internal static readonly Queue<IEntityManagerAction> _delayedActions = new Queue<IEntityManagerAction>();
		internal static bool _sceneParsed = false;

		public static int Count { get { return _entities.Count; } }

		internal static void parseScene(){ // NE PEUT ETRE APPELE QUUNE FOIS LA SCENE CONSTRUITE A CAUSE DU FIND DONC NE JAMAIS LAPPELER DANS UN CONSTRUCTOR
			if (_sceneParsed)
				return; // throw exception ?

			GameObject[] sceneGameObjects = Object.FindObjectsOfType<GameObject> ();
			for (int i = 0; i < sceneGameObjects.Length; ++i) {
				GameObject go = sceneGameObjects [i];
				Component[] components = go.GetComponents<Component>();
				HashSet<uint> componentTypeIds = new HashSet<uint> ();
		
				for (int j = 0; j < components.Length; ++j) {
					global::System.Type cType = components[j].GetType(); // GLOBAL:: TO TAKE OF
					uint cTypeId = TypeManager.getTypeId(cType);
					componentTypeIds.Add (cTypeId);
				}

				int entityId = go.GetInstanceID ();
				UECS.Entity entity = new Entity (go, componentTypeIds);

				_entities.Add(entityId, entity);
				FamilyManager.updateAfterEntityAdded(entityId, entity);
			}

			_sceneParsed = true;
		}

		public static void removeComponent<T>(GameObject gameObject) where T : Component{
			_delayedActions.Enqueue (new RemoveComponentAction<T> (gameObject));
		}

		public static void addComponent<T>(GameObject gameObject, Dictionary<string, object> componentValues = null) where T : Component{
			_delayedActions.Enqueue (new AddComponentAction<T> (gameObject, componentValues));
		}

		public static void removeGameObject(GameObject gameObject){
			_delayedActions.Enqueue (new RemoveGameObjectAction(gameObject));
		}

//		public static void createGameObject(string name, params global::System.Type[] componentsTypes/*, */) {
//			_delayedActions.Enqueue (new CreateGameObjectAction(name, componentsTypes));
//		}
	}
}

// Charger un xml ou json ou whatever
// Load next_scene + garder les managers etc du coup ??