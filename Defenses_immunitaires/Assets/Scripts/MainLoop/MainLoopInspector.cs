using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MainLoop))]
public class MainLoopInspector : Editor {
	private GUILayoutOption _minButtonWidth;
	private GUIContent _pauseButton;
	private GUIContent _moveUpButton;
	private GUIContent _moveDownButton;

	// EDITING MODE
	private SerializedProperty _systemFiles;
	private SerializedProperty _pause;
	private GUIContent _addButton;
	private GUIContent _removeButton;

	// PLAYING MODE
	private MainLoop _target;

	private void OnEnable(){
		if (Object.FindObjectsOfType<MainLoop>().Length > 1) { // due to duplicate hotkeys & option on GO
			DestroyImmediate(this.target);
			return;
		}

		_minButtonWidth = GUILayout.MinWidth(20f);
		_pauseButton = new GUIContent("", "pause");
		_moveUpButton = new GUIContent("\u25B2", "move up");
		_moveDownButton = new GUIContent("\u25BC", "move down");

		if (!Application.isPlaying) {
			_systemFiles = serializedObject.FindProperty("_systemFiles");
			_pause = serializedObject.FindProperty("_pause");
			_addButton = new GUIContent("+", "add");
			_removeButton = new GUIContent("-", "remove");
		} else {
			_target = (MainLoop)this.target;
		}
	}

	public override void OnInspectorGUI(){
		if (Application.isPlaying) {
			this.playingModeInspector();
		} else {
			serializedObject.Update();
			this.editingModeInspector();
			serializedObject.ApplyModifiedProperties();
		}
	}

	private void playingModeInspector(){		
		List<UECS.System> systems = _target._systems;
		int systemsCount = systems.Count;

		for(int i = 0; i < systemsCount; ++i) {
			UECS.System system = systems[i];

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(system.GetType().ToString());

			Color baseColor = GUI.color;
			GUI.color = (system.Pause == true) ? Color.red : Color.green;
			if(GUILayout.Button(_pauseButton, _minButtonWidth))
				system.Pause = !system.Pause;
			GUI.color = baseColor;

			if (GUILayout.Button (_moveUpButton, _minButtonWidth)) {
				if (i != 0) {
					UECS.System acc = systems [i - 1];
					systems[i - 1] = systems [i];
					systems[i] = acc;
				}
			}

			if (GUILayout.Button (_moveDownButton, _minButtonWidth)) {
				if (i != systemsCount - 1) {
					UECS.System acc = systems [i + 1];
					systems [i + 1] = systems [i];
					systems [i] = acc;
				}
			}

			EditorGUILayout.EndHorizontal();
		}
	}

	private void editingModeInspector() {
		for (int i = 0; i < _systemFiles.arraySize; ++i) {
			EditorGUILayout.BeginHorizontal();

			SerializedProperty systemFile = _systemFiles.GetArrayElementAtIndex(i);
			MonoScript script = (MonoScript) EditorGUILayout.ObjectField(systemFile.objectReferenceValue, typeof(MonoScript), false);
			if (systemFile.objectReferenceValue != script && script != null && script.GetClass().IsSubclassOf(typeof(UECS.System)))
				systemFile.objectReferenceValue = script;
			
			Color baseColor = GUI.color;
			SerializedProperty pause = _pause.GetArrayElementAtIndex(i);
			GUI.color = (pause.boolValue == true) ? Color.red : Color.green;
			if(GUILayout.Button(_pauseButton, _minButtonWidth))
				pause.boolValue = !pause.boolValue;
			GUI.color = baseColor;

			if (GUILayout.Button (_moveUpButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i - 1);
				_pause.MoveArrayElement (i, i - 1);
			}

			if (GUILayout.Button (_moveDownButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i + 1);
				_pause.MoveArrayElement (i, i + 1);
			}

			if(GUILayout.Button (_removeButton, _minButtonWidth)) {
				_systemFiles.GetArrayElementAtIndex(i).objectReferenceValue = null;
				_systemFiles.DeleteArrayElementAtIndex(i);

				_pause.DeleteArrayElementAtIndex(i);
			}

			EditorGUILayout.EndHorizontal();
		}

		if(GUILayout.Button (_addButton, EditorStyles.miniButtonLeft)) {
			int index = _systemFiles.arraySize;
			_systemFiles.InsertArrayElementAtIndex(index);
			_systemFiles.GetArrayElementAtIndex(index).objectReferenceValue = null;

			_pause.InsertArrayElementAtIndex(index);
			_pause.GetArrayElementAtIndex(index).boolValue = true;
		}
	}
}