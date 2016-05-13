using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace FYFY {
	[CustomEditor(typeof(MainLoop))]
	public class MainLoopInspector : Editor {
		private SerializedProperty _systemDescriptions;
		private ReorderableList _drawingList;

		private void OnEnableInPlayingMode() {
			_drawingList = new ReorderableList(FSystemManager._systems, typeof(FSystem), true, true, false, false);

			_drawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				FSystem system = FSystemManager._systems[index];
				string typeFullName = system.GetType().FullName;

			 	EditorGUI.LabelField(new Rect (rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), typeFullName);

				Color baseColor = GUI.color;
				GUI.color = (system.Pause == true) ? Color.red : Color.green;
				if(GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), ""))
					system.Pause = !system.Pause;
				GUI.color = baseColor;
			};
		}

		private void onAddCallback(object type) {
			System.Type systemType = (System.Type) type;
			int index = _systemDescriptions.arraySize;
			_systemDescriptions.arraySize++;

			SerializedProperty systemDescription = _systemDescriptions.GetArrayElementAtIndex(index);
			systemDescription.FindPropertyRelative("_typeAssemblyQualifiedName").stringValue = systemType.AssemblyQualifiedName;
			systemDescription.FindPropertyRelative("_typeFullName").stringValue = systemType.FullName;
			systemDescription.FindPropertyRelative("_pause").boolValue = false;

			serializedObject.ApplyModifiedProperties();
		}

		private void OnEnableInEditingMode() {
			_systemDescriptions = serializedObject.FindProperty("_systemDescriptions");
			_drawingList = new ReorderableList(serializedObject, _systemDescriptions, true, false, true, true);

			_drawingList.onAddDropdownCallback = delegate(Rect buttonRect, ReorderableList list) {
				HashSet<string> selectedSystemTypeNames = new HashSet<string>(); // impossible de le mettre en dehors car quand tu fais reset, OnEnable nest pas rappele !!!!!
				for(int i = 0; i < _systemDescriptions.arraySize; ++i) {
					string fullTypeName = _systemDescriptions.GetArrayElementAtIndex(i).FindPropertyRelative("_typeFullName").stringValue;
					selectedSystemTypeNames.Add(fullTypeName);
				}

				System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
					from type in assembly.GetExportedTypes()
					where type.IsSubclassOf(typeof(FSystem)) == true
					select type).ToArray();

				GenericMenu menu = new GenericMenu();
				for(int i = 0; i < systemTypes.Length; ++i) {
					System.Type systemType = systemTypes[i];
					string typeAssemblyName = systemType.Assembly.GetName().Name;
					string typeName = systemType.FullName;
					string buttonText = (typeAssemblyName != "Assembly-CSharp") ? (typeAssemblyName + "/" + typeName) : typeName;

					if(selectedSystemTypeNames.Contains(typeName))
						menu.AddDisabledItem(new GUIContent(buttonText));
					else
						menu.AddItem(new GUIContent(buttonText), false, onAddCallback, systemType);
				}
				menu.ShowAsContext();
			};

			_drawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				SerializedProperty systemDescription = _systemDescriptions.GetArrayElementAtIndex(index);
				SerializedProperty typeFullName = systemDescription.FindPropertyRelative("_typeFullName");
				SerializedProperty pause = systemDescription.FindPropertyRelative("_pause");

				EditorGUI.LabelField (new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), typeFullName.stringValue);

				Color baseColor = GUI.color;
				GUI.color = (pause.boolValue == true) ? Color.red : Color.green;
				if (GUI.Button (new Rect(rect.x + rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), ""))
					pause.boolValue = !pause.boolValue;
				GUI.color = baseColor;
			};
		}

		private void OnEnable(){
			if (Object.FindObjectsOfType<MainLoop>().Length > 1) { // due to duplicate hotkeys & option on GO
				DestroyImmediate(this.target);
				return;
			}

			if (Application.isPlaying)
				this.OnEnableInPlayingMode();
			else
				this.OnEnableInEditingMode();
			
			_drawingList.drawHeaderCallback = delegate(Rect rect) {
				EditorGUI.LabelField (rect, "FSystems");
			};
		}

		public override void OnInspectorGUI(){
			serializedObject.Update();
			_drawingList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}