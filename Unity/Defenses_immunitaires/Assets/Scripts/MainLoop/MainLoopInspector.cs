using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainLoop))]
public class MainLoopInspector : Editor {
	private SerializedProperty _systemFiles;
	private SerializedProperty _activate;
	private SerializedProperty _order;

	private GUILayoutOption _minButtonWidth;
	private GUIContent _addButton;
	private GUIContent _activateButton;
	private GUIContent _moveUpButton;
	private GUIContent _moveDownButton;
	private GUIContent _removeButton;

	private void OnEnable(){
		if (Object.FindObjectsOfType<MainLoop>().Length > 1) { // due to duplicate hotkeys & option on GO
			DestroyImmediate(target);
			return;
		}

		_systemFiles = serializedObject.FindProperty("_systemFiles");
		_activate = serializedObject.FindProperty("_activate");
		_order = serializedObject.FindProperty("_order");

		_minButtonWidth = GUILayout.MinWidth(20f);
		_addButton = new GUIContent("+", "add");
		_activateButton = new GUIContent("", "activate");
		_moveUpButton = new GUIContent("\u25B2", "move up");
		_moveDownButton = new GUIContent("\u25BC", "move down");
		_removeButton = new GUIContent("-", "remove");
	}

	private void editingModeInspector() {
		for (int i = 0; i < _systemFiles.arraySize; ++i) {
			EditorGUILayout.BeginHorizontal ();

			SerializedProperty systemFile = _systemFiles.GetArrayElementAtIndex(i);
			MonoScript script = (MonoScript) EditorGUILayout.ObjectField(systemFile.objectReferenceValue, typeof(MonoScript), false);
			if (systemFile.objectReferenceValue != script && script != null && script.GetClass().IsSubclassOf(typeof(UECS.System)))
				systemFile.objectReferenceValue = script;
			
			this.showActivateButton(_activate.GetArrayElementAtIndex(i));
			if (GUILayout.Button (_moveUpButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i - 1);
				_activate.MoveArrayElement (i, i - 1);
			}
			if (GUILayout.Button (_moveDownButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i + 1);
				_activate.MoveArrayElement (i, i + 1);
			}
			if(GUILayout.Button (_removeButton, _minButtonWidth)) {
				_systemFiles.GetArrayElementAtIndex(i).objectReferenceValue = null;
				_systemFiles.DeleteArrayElementAtIndex(i);

				_activate.DeleteArrayElementAtIndex(i);

				_order.DeleteArrayElementAtIndex(i);
				for(int j = i; j < _order.arraySize; ++j)
					_order.GetArrayElementAtIndex (j).intValue = j;
			}

			EditorGUILayout.EndHorizontal();
		}

		if(GUILayout.Button (_addButton, EditorStyles.miniButtonLeft)) {
			int index = _systemFiles.arraySize;
			_systemFiles.InsertArrayElementAtIndex(index);
			_systemFiles.GetArrayElementAtIndex(index).objectReferenceValue = null;

			_activate.InsertArrayElementAtIndex(index);
			_activate.GetArrayElementAtIndex(index).boolValue = false;

			_order.InsertArrayElementAtIndex(index);
			_order.GetArrayElementAtIndex (index).intValue = index;
		}
	}

	private void playingModeInspector(){
		for (int i = 0; i < _order.arraySize; ++i) {
			EditorGUILayout.BeginHorizontal ();

			int index = _order.GetArrayElementAtIndex(i).intValue;
			SerializedProperty systemFile = _systemFiles.GetArrayElementAtIndex(index);
			EditorGUILayout.ObjectField (systemFile.objectReferenceValue, typeof(MonoScript), false);

			this.showActivateButton(_activate.GetArrayElementAtIndex(index));
			if (GUILayout.Button (_moveUpButton, _minButtonWidth))
				_order.MoveArrayElement(i, i - 1);
			if (GUILayout.Button (_moveDownButton, _minButtonWidth))
				_order.MoveArrayElement (i, i + 1);

			EditorGUILayout.EndHorizontal();
		}
	}

	private void showActivateButton(SerializedProperty elementIsActivatedProperty){
		Color baseColor = GUI.color;

		GUI.color = (elementIsActivatedProperty.boolValue == true) ? Color.green : Color.red;
		if(GUILayout.Button(_activateButton, _minButtonWidth))
			elementIsActivatedProperty.boolValue = !elementIsActivatedProperty.boolValue;

		GUI.color = baseColor;
	}

	public override void OnInspectorGUI(){
		serializedObject.Update();

		if (!Application.isPlaying)
			this.editingModeInspector();
		else
			this.playingModeInspector();

		serializedObject.ApplyModifiedProperties();
	}
}