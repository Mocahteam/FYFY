using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public static class EntityManager {
	internal static readonly Dictionary<int, Entity> _entities;

	public static int Count { get { return _entities.Count; } }

	// ATTENTION, IL FAUT SAVOIR SI APPELE APRES LA CREATION DES SCENES_O 
	// SINON NE PAS FAIRE DE STATIC CONSTRUCTOR MAIS FONCTION QUE TU APPELERAS A LA MAIN
	// MAIS CA NE GARANTI PLUS QUE CA NE POURRA ETRE APPELE QUUNE SEULE FOIS
	static EntityManager(){
		_entities = new Dictionary<int, Entity> ();

		GameObject[] sceneGameObjects = Object.FindObjectsOfType<GameObject> ();
		for (int i = 0; i < sceneGameObjects.Length; ++i) {
			GameObject go = sceneGameObjects [i];	
			Component[] components = go.GetComponents<Component>();
			HashSet<uint> componentTypeIds = new HashSet<uint> ();

			for (int j = 0; j < components.Length; ++j) {
				System.Type cType = components[j].GetType();
				uint cTypeId = TypeManager.getTypeId(cType);
				componentTypeIds.Add (cTypeId);
			}

			_entities.Add (go.GetInstanceID (), new Entity(go, componentTypeIds));
		}
	}
}

//	public static T addComponent<T>(GameObject e, Dictionary<string, object> initialisationValues = null) where T : Component {
//		System.Type type = typeof(T);
//		T component = e.AddComponent<T>();
//		// ------------------------------
//		if (initialisationValues != null) {
//			foreach (KeyValuePair<string, object> pair in initialisationValues) {
//				FieldInfo fieldInfo = type.GetField(pair.Key);    // /!\ FIELD AND NOT PROPERTY
//				System.Type fieldType = fieldInfo.FieldType;      // NECESSARY ?
//				System.Convert.ChangeType(pair.Value, fieldType); // NECESSARY ?
//
//				fieldInfo.SetValue(component, pair.Value);
//			}
//		}
//		// ------------------------------
//		// update entityComponentsType + families
//		// ------------------------------
//		return component;
//	}

//	public static Component addComponent(GameObject e, System.Type t, Dictionary<string, object> initialisationValues = null) {
//		Component component = e.AddComponent(t);
//		// ------------------------------
//		if (initialisationValues != null) {
//			foreach (KeyValuePair<string, object> pair in initialisationValues) {
//				t.GetProperty(pair.Key).SetValue(component, pair.Value, null);
//			}
//		}
//		// ------------------------------
//		return component;
//	}

// componentAdded(e, newListOfcomponents)
// 		for all family (maybe you can filter family with the new component or whatever ??)
// 			check if newListOfcomponents matchs
//				check if e isnt already inside
// 				...
// componentRemoved(e, newListOfcomponents)
// 		...