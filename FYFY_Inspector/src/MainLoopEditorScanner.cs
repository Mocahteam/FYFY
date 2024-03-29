﻿using UnityEngine;
using UnityEditor;
using FYFY;
using System;
using System.Threading;
using System.Reflection;

namespace FYFY_Inspector {
	
	/// <summary>
	/// 	Refresh current active Game Object to be aware from user changes inside inspector
	/// </summary>
	[ExecuteInEditMode] // permet dexecuter awake et start etc aussi en mode edition
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(51)] // start after MainLoop
	public class MainLoopEditorScanner : MonoBehaviour {
		private static MainLoopEditorScanner _mainLoopScanner; // avoid to have more than one component inside scene (see Awake)

		private bool alreadyNotified = false;
		
		private void Awake() {
			// Severals instances of MainLoopScanner are not allowed.
			if(_mainLoopScanner != null) {
				EditorUtility.DisplayDialog("Invalid operation", "Can't add 'MainLoopEditorScanner' to "+gameObject.name+" because a 'MainLoopEditorScanner' is already added to another game object in the scene!", "Ok", "");
				DestroyImmediate(this);
				return;
			}
			_mainLoopScanner = this;
		}
		
		/// <summary></summary>
		public void OnEnable(){
			if(Application.isPlaying)
				return;
			
			alreadyNotified = false;
	
			// After each compilations we have to synchronize families in case of user update them inside systems.
			// Because we are here in FYFY_Inspector package we have no garanty that user use Monitoring plugin.
			// So we have to inspect types dynamically, first we try to find the Monitoring type (it will be the
			// case if user drag&drop the Monitoring libraries inside its Unity project
			Type monitoringManager_Type = Type.GetType("FYFY_plugins.Monitoring.MonitoringManager, Monitoring");
			if (monitoringManager_Type != null && !EditorApplication.isCompiling){ // could be null if user doesn't use Monitoring plugin and be sure compiling is finished before synchronize families
				// Here we found the MonitoringManager type, so we inspect it to find "Instance" field
				System.Reflection.FieldInfo mmInstanceField = monitoringManager_Type.GetField("Instance");
				if (mmInstanceField != null){
					// Here we found MonitoringManager.Instance and we have to check if it is not null.
					// It could be null if user add Monitoring plugin but don't add MonitoringManager component to the scene
					if (mmInstanceField.GetValue(null) != null){
						// We found the Type, we found its static field "Instance", so we can call the function to synchronize families
						// First we have to find the "synchronizeFamilies" method defined inside MonitoringManager type
						MethodInfo synchronizeFamilies = monitoringManager_Type.GetMethod("synchronizeFamilies", new Type[] { });
						if (synchronizeFamilies != null)
							// And call the method on the Instance field
							synchronizeFamilies.Invoke(mmInstanceField.GetValue(null), new object[] { });
						else
							UnityEngine.Debug.LogError("Warning, inconsistent method inside FYFY_plugins.Monitoring.MonitoringManager.Instance, \"synchronizeFamilies\" method is not defined.");
					}
				} else
					UnityEngine.Debug.LogError("Warning, inconsistent field inside FYFY_plugins.Monitoring.MonitoringManager, \"Instance\" field require.");
			}
			
			// We ask MainLoop to call back this script and refresh data base
			if (MainLoop.instance.synchronizeWrappers()){
				AssetDatabase.Refresh();
			}
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
			
			if (EditorApplication.isCompiling && EditorApplication.isPlaying && !alreadyNotified){
				EditorUtility.DisplayDialog("Stop playing", "FYFY doesn't support live compiling.\nPlaying mode stopped.", "Ok", "");
				EditorApplication.isPlaying = false;
				alreadyNotified = true;
			}
			
			if (UnityEditor.Selection.activeGameObject)
				GameObjectManager.refresh(UnityEditor.Selection.activeGameObject);
		}
	}
}