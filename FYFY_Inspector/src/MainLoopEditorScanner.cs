﻿using UnityEngine;
using UnityEditor;
using FYFY;

namespace FYFY_Inspector {
	
	/// <summary>
	/// 	Refresh current active Game Object to be aware from user changes inside inspector
	/// </summary>
	[ExecuteInEditMode] // permet dexecuter awake et start etc aussi en mode edition
	[DisallowMultipleComponent]
	public class MainLoopEditorScanner : MonoBehaviour {
		private static MainLoopEditorScanner _mainLoopScanner; // avoid to have more than one component inside scene (see Awake)
		
		private void Awake() {
			// Severals instances of MainLoop are not allowed.
			if(_mainLoopScanner != null) {
				EditorUtility.DisplayDialog("Invalid operation", "Can't add 'MainLoopEditorScanner' to "+gameObject.name+" because a 'MainLoopEditorScanner' is already added to another game object in the scene!", "Ok", "");
				DestroyImmediate(this);
				return;
			}
			_mainLoopScanner = this;
		}

		private void OnDestroy(){
			if(_mainLoopScanner == this) {
				_mainLoopScanner = null;
			}
		}
		
		private void Update(){
			if(Application.isPlaying == false){
				return;
			}
			
			if (UnityEditor.Selection.activeGameObject)
				GameObjectManager.refresh(UnityEditor.Selection.activeGameObject);
			
		}
	}
}