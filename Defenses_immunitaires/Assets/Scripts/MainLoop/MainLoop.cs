using UnityEngine;
using UnityEditor;

public class MainLoop : MonoBehaviour {
	public MonoScript[] _systemFiles;

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
	}

	private void Start(){
		UECS.EntityManager.parseScene();
	}

	private void FixedUpdate(){
		int count = UECS.EntityManager._delayedActions.Count;
		while(count-- > 0)
			UECS.EntityManager._delayedActions.Dequeue().perform();
		
		for (int i = 0; i < _systemsLength; ++i)
			_systems[i].process();
	}
}

// PROBLEME CONSTRUCTOR UNITY MULTIPLE APPEL ?? regarder serialisation etc

// priority ? Order ? Change order ?

// GameObject hiddenMainLoop = new GameObject("Hidden_Main_Loop");
// hiddenMainLoop.hideFlags = HideFlags.HideInHierarchy;
// hiddenMainLoop.AddComponent<> ();