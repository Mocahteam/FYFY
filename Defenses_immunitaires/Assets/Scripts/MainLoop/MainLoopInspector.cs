using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainLoop))]
public class MainLoopInspector : Editor {
	private SerializedProperty _systemFiles;
	private SerializedProperty _systemFilesChanged;
	private GUILayoutOption _minButtonWidth;
	private GUIContent _addButton;
	private GUIContent _moveUpButton;
	private GUIContent _moveDownButton;
	private GUIContent _removeButton;

	private void OnEnable(){
		if (Object.FindObjectsOfType<MainLoop>().Length > 1) { // due to duplicate hotkeys & option on GO
			DestroyImmediate(target);
			return;
		}

		_systemFiles = serializedObject.FindProperty("_systemFiles");
		_systemFilesChanged = serializedObject.FindProperty("_systemFilesChanged");
		_minButtonWidth = GUILayout.MinWidth(20f);
		_addButton = new GUIContent("+", "add");
		_moveUpButton = new GUIContent("\u25B2", "move up");
		_moveDownButton = new GUIContent("\u25BC", "move down");
		_removeButton = new GUIContent("-", "remove");
	}

	public override void OnInspectorGUI(){
		serializedObject.Update ();

		for (int i = 0; i < _systemFiles.arraySize; ++i) {
			EditorGUILayout.BeginHorizontal ();
			//---------------------------------------------------------------------------------------------------------------
			SerializedProperty sp = _systemFiles.GetArrayElementAtIndex(i);
			MonoScript script = (MonoScript) EditorGUILayout.ObjectField(sp.objectReferenceValue, typeof(MonoScript), false);
			if (sp.objectReferenceValue != script && script != null && script.GetClass().IsSubclassOf(typeof(UECS.System))) {
				sp.objectReferenceValue = script;
				_systemFilesChanged.boolValue = true;
			}
			//---------------------------------------------------------------------------------------------------------------
			if (GUILayout.Button (_moveUpButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement(i, i - 1);
				_systemFilesChanged.boolValue = true;
			}
			if (GUILayout.Button (_moveDownButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i + 1);
				_systemFilesChanged.boolValue = true;
			}
			if (!Application.isPlaying && GUILayout.Button (_removeButton, _minButtonWidth)) {
				_systemFiles.GetArrayElementAtIndex(i).objectReferenceValue = null;
				_systemFiles.DeleteArrayElementAtIndex(i);
				_systemFilesChanged.boolValue = true;
			}
			//---------------------------------------------------------------------------------------------------------------
			EditorGUILayout.EndHorizontal();
		}

		if (!Application.isPlaying && GUILayout.Button (_addButton, EditorStyles.miniButtonLeft)) {
			int index = _systemFiles.arraySize;
			_systemFiles.InsertArrayElementAtIndex(index);
			_systemFiles.GetArrayElementAtIndex(index).objectReferenceValue = null;
			_systemFilesChanged.boolValue = true;
		}

		EditorGUILayout.HelpBox ("Set all the systems you need", MessageType.Info, true);

		serializedObject.ApplyModifiedProperties();
	}
}