using UnityEngine;
using UnityEditor;
using FYFY;
using System;
using System.Threading;

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
		
		/// <summary></summary>
		public void OnEnable(){
			if(Application.isPlaying)
				return;
			
			// OnEnable is called after script compilation. We use this mechanism to synchronize systems public functions and families
			while (EditorApplication.isCompiling)
				//Wait 10 ms not to overload processors
				Thread.Sleep(10);
				
			// After each compilations we have to synchronize families in case of user update them inside systems.
			// Because we are here in FYFY_Inspector package we have no garanty that user use Monitoring plugin.
			// So we have to inspect types dynamically, first we try to find the Monitoring type (it will be the
			// case if user drag&drop the Monitoring libraries inside its Unity project
			Type monitoringManager_Type = Type.GetType("FYFY_plugins.Monitoring.MonitoringManager, Monitoring");
			if (monitoringManager_Type != null){ // could be null if user doesn't use Monitoring plugin
				// Here we found the MonitoringManager type, so we inspect it to find "Instance" field
				System.Reflection.FieldInfo mmInstanceField = monitoringManager_Type.GetField("Instance");
				if (mmInstanceField != null){
					// Here we found MonitoringManager.Instance and we have to check if it is not null.
					// It could be null if user add Monitoring plugin but don't add MonitoringManager component to the scene
					MonoBehaviour monitoringManager_Class = (mmInstanceField.GetValue(null) as MonoBehaviour);
					if (monitoringManager_Class != null)
						// We found the Type, we found its static field "Instance", so we can call the function to synchronize families
						monitoringManager_Class.Invoke("synchronizeFamilies", 0);
				} else
					UnityEngine.Debug.LogError("Warning, inconsistent field inside FYFY_plugins.Monitoring.MonitoringManager, \"Instance\" field require.");
			}
			
			// depending on script execution order, the MainLoopEditorScanner can process before MainLoop
			// in this case MainLoop.instance == null and we can't call synchronizerWrappers.
			// Then we ask MainLoop to call back this script and refresh data base
			if (MainLoop.instance != null){
				if (MainLoop.instance.synchronizeWrappers())
					AssetDatabase.Refresh();
			}
			else{
				// Notify MainLoop to call back MainLoopEditorScanner OnEnable
				MainLoop.mainLoopEditorScanner = this;
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
			
			if (UnityEditor.Selection.activeGameObject)
				GameObjectManager.refresh(UnityEditor.Selection.activeGameObject);
		}
	}
}