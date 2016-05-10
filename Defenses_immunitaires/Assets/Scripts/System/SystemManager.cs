using UnityEngine;
using System.Collections.Generic;

public static class SystemManager {
	internal static readonly List<UECS.System>            _systems = new List<UECS.System>();
	internal static readonly Dictionary<System.Type, int> _indexes = new Dictionary<System.Type, int>();

	public static int Count { get { return _systems.Count; } }

	public static UECS.System getSystem<T>() where T : UECS.System {
		int index;
		if(_indexes.TryGetValue(typeof(T), out index) == true)
			return _systems[index];
		return null;
	}

	public static UECS.System getSystem(System.Type systemType) {
		if(systemType == null)
			throw new MissingReferenceException();

		if (systemType.IsSubclassOf(typeof(UECS.System)) == false) {
			Debug.LogWarning("Can't get '" + systemType + " because a '" + systemType + "' isn't a System!");
			return null;
		}

		int index;
		if(_indexes.TryGetValue(systemType, out index) == true)
			return _systems[index];
		return null;
	}
}

//	public static bool addSystem<T>(bool pause) where T : UECS.System, new() {
//		System.Type systemType = typeof(T);
//		if (_indexes.ContainsKey(systemType) == false) {
//			UECS.System system = new T();
//			system.Pause = pause;
//
//			_indexes.Add(systemType, _systems.Count);
//			_systems.Add(system);
//			return true;
//		}
//		return false;
//	}
//
//	public static bool addSystem(System.Type systemType, bool pause) {
//		if(systemType == null)
//			throw new MissingReferenceException();
//
//		if (systemType.IsSubclassOf(typeof(UECS.System)) == false) {
//			Debug.LogWarning("Can't add '" + systemType + " because a '" + systemType + "' isn't a System!");
//			return false;
//		}
//
//		if (_indexes.ContainsKey(systemType) == false) {
//			UECS.System system = (UECS.System) System.Activator.CreateInstance(systemType);
//			system.Pause = pause;
//
//			_indexes.Add(systemType, _systems.Count);
//			_systems.Add(system);
//			return true;
//		}
//		return false;
//	}

//	public static bool removeSystem<T>() where T : UECS.System {
//		System.Type systemType = typeof(T);
//		int index;
//		if (_indexes.TryGetValue(systemType, out index) == true) {
//			_systems.RemoveAt(index);
//			_indexes.Remove(systemType);
//			return true;
//		}
//		return false;
//	}
//
//	public static bool removeSystem(System.Type systemType) {
//		if(systemType == null)
//			throw new MissingReferenceException();
//
//		if (systemType.IsSubclassOf(typeof(UECS.System)) == false) {
//			Debug.LogWarning("Can't remove '" + systemType + " because a '" + systemType + "' isn't a System!");
//			return false;
//		}
//
//		int index;
//		if (_indexes.TryGetValue(systemType, out index) == true) {
//			_systems.RemoveAt(index);
//			_indexes.Remove(systemType);
//			return true;
//		}
//		return false;
//	}