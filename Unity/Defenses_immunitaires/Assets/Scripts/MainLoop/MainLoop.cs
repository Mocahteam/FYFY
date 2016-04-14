using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[AddComponentMenu("")] // hide in Component list
public class MainLoop : MonoBehaviour {
	public MonoScript[] _systemFiles; // = {}; // -> to avoid runtime add problem if mainLoopMenu active during playing mode
	// variable to know if main kept editor value, so it is initialized ? => avoid to add component main in script | Probleme si cest plus en editor mode car elle sera jamais setted ??

	#if UNITY_EDITOR
	[SerializeField]
	private bool _systemFilesChanged;
	#endif

	private UECS.System[] _systems;
	private int _systemsLength;

	private void Awake() {
		_systems = new UECS.System[_systemFiles.Length];
		_systemsLength = 0;

		for (int i = 0; i < _systemFiles.Length; ++i) {
			MonoScript systemFile = _systemFiles [i];
			if(systemFile != null)
				_systems[_systemsLength++] = (UECS.System) System.Activator.CreateInstance(systemFile.GetClass());
		}

		#if UNITY_EDITOR
		_systemFilesChanged = false;
		#endif
	}

	private void Start(){
		UECS.EntityManager.parseScene();
	}

	private void FixedUpdate(){
		#if UNITY_EDITOR
		if (_systemFilesChanged == true) {
			// Family f = FamilyManager.getFamily(new AllOfTypes(typeof(Transform)));
			_systemFilesChanged = false;
		}
		#endif

		int count = UECS.EntityManager._delayedActions.Count;
		while(count-- > 0)
			UECS.EntityManager._delayedActions.Dequeue().perform();
		
		for (int i = 0; i < _systemsLength; ++i)
			_systems[i].process();
	}
}

// GameObject hiddenMainLoop = new GameObject("Hidden_Main_Loop");
// hiddenMainLoop.hideFlags = HideFlags.HideInHierarchy;
// hiddenMainLoop.AddComponent<>();