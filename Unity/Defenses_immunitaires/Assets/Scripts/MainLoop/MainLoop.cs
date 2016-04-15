using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[AddComponentMenu("")] // hide in Component list
public class MainLoop : MonoBehaviour {
	public MonoScript[] _systemFiles;
	public bool[] _activate;
	public int[] _order;

	private UECS.System[] _systems;

	private void Awake() {
		if(_systemFiles == null) { // MainLoop Added in script & not in editor so it can't be kept editor value
			DestroyImmediate(this);
			throw new UnityException();
		}

		_systems = new UECS.System[_systemFiles.Length];

		for (int i = 0; i < _systemFiles.Length; ++i) {
			if (_systemFiles [i] != null) {
				System.Type systemType = _systemFiles [i].GetClass ();
				_systems [i] = (UECS.System) System.Activator.CreateInstance(systemType);
			} else
				_systems [i] = null;
		}
	}

	private void Start(){
		UECS.EntityManager.parseScene();
	}

	private void FixedUpdate(){
		int count = UECS.EntityManager._delayedActions.Count;
		while(count-- > 0)
			UECS.EntityManager._delayedActions.Dequeue().perform();

		// _onEntityEnteredCallbacks && _onEntityExitedCallbacks

		for (int i = 0; i < _order.Length; ++i) {
			int index = _order[i];
			if(_activate[index] == true && _systems[index] != null)
				_systems[index].process();
		}
	}
}

// _systemFiles; // = {}; // -> to avoid runtime add problem if mainLoopMenu active during playing mod
// variable to know if main kept editor value, so it is initialized ? => avoid to add component main in script | Probleme si cest plus en editor mode car elle sera jamais setted ??

// GameObject hiddenMainLoop = new GameObject("Hidden_Main_Loop");
// hiddenMainLoop.hideFlags = HideFlags.HideInHierarchy;
// hiddenMainLoop.AddComponent<>();