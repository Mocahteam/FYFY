using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainLoop))]
public class MainLoopInspector : Editor {
	private SerializedProperty _systemFiles;
	private GUILayoutOption _minButtonWidth;
	private GUIContent _addButton;
	private GUIContent _moveUpButton;
	private GUIContent _moveDownButton;
	private GUIContent _removeButton;

	private void OnEnable(){
		_systemFiles = serializedObject.FindProperty("_systemFiles");
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
			if (script != null && script.GetClass().IsSubclassOf(typeof(UECS.System)))
				sp.objectReferenceValue = script;
			//---------------------------------------------------------------------------------------------------------------
			if (GUILayout.Button (_moveUpButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i - 1);
			}
			if (GUILayout.Button (_moveDownButton, _minButtonWidth)) {
				_systemFiles.MoveArrayElement (i, i + 1);
			}
			if (GUILayout.Button (_removeButton, _minButtonWidth)) {
				_systemFiles.GetArrayElementAtIndex(i).objectReferenceValue = null;
				_systemFiles.DeleteArrayElementAtIndex(i);
			}
			//---------------------------------------------------------------------------------------------------------------
			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button (_addButton, EditorStyles.miniButtonLeft)) {
			int index = _systemFiles.arraySize;
			_systemFiles.InsertArrayElementAtIndex(index);
			_systemFiles.GetArrayElementAtIndex(index).objectReferenceValue = null;
		}

		EditorGUILayout.HelpBox ("Set all the systems you need", MessageType.Info, true);

		serializedObject.ApplyModifiedProperties();
	}
}