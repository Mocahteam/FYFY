using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestList))]
public class TestListInspector : Editor {
	SerializedProperty _systemsNames;

	private void OnEnable(){
		_systemsNames = serializedObject.FindProperty("_systemsNames");
	}

	public override void OnInspectorGUI(){
		serializedObject.Update ();

		// ...

		EditorGUILayout.PropertyField(_systemsNames.FindPropertyRelative("Array.size"));

		for (int i = 0; i < _systemsNames.arraySize; ++i) {			
			EditorGUILayout.PropertyField (_systemsNames.GetArrayElementAtIndex (i)/*, GUIContent.none*/);
		}

		// ...

		serializedObject.ApplyModifiedProperties();
	}
}
