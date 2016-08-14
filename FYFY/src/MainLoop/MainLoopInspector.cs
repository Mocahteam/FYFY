using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace FYFY {
	/// <summary>
	/// 	Custom inspector automatically used by Unity Editor when a <see cref="FYFY.MainLoop"/> is find in inspector view.
	/// </summary>
	[CustomEditor(typeof(MainLoop))]
	public class MainLoopInspector : Editor {
		private SerializedProperty _fixedUpdateSystemDescriptions;
		private SerializedProperty _updateSystemDescriptions;
		private SerializedProperty _lateUpdateSystemDescriptions;

		private ReorderableList _fixedUpdateDrawingList;
		private ReorderableList _updateDrawingList;
		private ReorderableList _lateUpdateDrawingList;

		private void playingModeDrawElementCallBack(FSystem system, Rect rect) {
			float buttonSize = EditorGUIUtility.singleLineHeight;

			Color baseColor = GUI.color;
			GUI.color = (system.Pause == true) ? Color.red : Color.green;
			if(GUI.Button(new Rect(rect.x, rect.y + 1.35f, buttonSize, buttonSize), ""))
				system.Pause = !system.Pause;
			GUI.color = baseColor;

			string typeFullName = system.GetType().FullName;
			EditorGUI.LabelField(new Rect(rect.x + buttonSize + 5, rect.y + 1.35f, rect.width - (buttonSize + 5), buttonSize), typeFullName);
		}

		private void OnEnableInPlayingMode() {
			_fixedUpdateDrawingList = new ReorderableList(FSystemManager._fixedUpdateSystems, typeof(FSystem), true, true, false, false);
			_updateDrawingList = new ReorderableList(FSystemManager._updateSystems, typeof(FSystem), true, true, false, false);
			_lateUpdateDrawingList = new ReorderableList(FSystemManager._lateUpdateSystems, typeof(FSystem), true, true, false, false);

			_fixedUpdateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.playingModeDrawElementCallBack(FSystemManager._fixedUpdateSystems[index], rect);
			};
			_updateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.playingModeDrawElementCallBack(FSystemManager._updateSystems[index], rect);
			};
			_lateUpdateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.playingModeDrawElementCallBack(FSystemManager._lateUpdateSystems[index], rect);
			};
		}

		private void editingModeDrawElementCallBack(SerializedProperty systemDescriptions, int index, Rect rect) {
			if(index >= systemDescriptions.arraySize) // bug when deleteArrayElementAtIndex -> index passed to the callback (reorderablelist) not update immediatly ??????
				return; 

			SerializedProperty systemDescription = systemDescriptions.GetArrayElementAtIndex(index);
			SerializedProperty typeFullName = systemDescription.FindPropertyRelative("_typeFullName");
			SerializedProperty pause = systemDescription.FindPropertyRelative("_pause");
	
			float buttonSize = EditorGUIUtility.singleLineHeight;

			Color baseColor = GUI.color;
			GUI.color = (pause.boolValue == true) ? Color.red : Color.green;
			if(GUI.Button(new Rect(rect.x, rect.y + 1.35f, buttonSize, buttonSize), ""))
				pause.boolValue = !pause.boolValue;
			GUI.color = baseColor;

			EditorGUI.LabelField(new Rect(rect.x + buttonSize + 5, rect.y + 1.35f, rect.width - (buttonSize + 5), buttonSize), typeFullName.stringValue);

			GUIStyle buttonStyle = new GUIStyle();
			buttonStyle.alignment = TextAnchor.MiddleCenter;
			if(GUI.Button(new Rect (rect.x + rect.width - 25, rect.y + 1.35f, 30 /* largeur du bouton add par defaut*/, buttonSize), "\u2718", buttonStyle)){ // bold cross unicode
				systemDescriptions.DeleteArrayElementAtIndex(index);
				serializedObject.ApplyModifiedProperties();
			}
		}

		private void createMenu(Rect buttonRect, ReorderableList list) {
			HashSet<string> selectedSystemTypeNames = new HashSet<string>(); // impossible de le mettre en dehors car quand tu fais reset dans lediteur, OnEnable nest pas rappele !!!!!
			for(int i = 0; i < _fixedUpdateSystemDescriptions.arraySize; ++i) {
				string fullTypeName = _fixedUpdateSystemDescriptions.GetArrayElementAtIndex(i).FindPropertyRelative("_typeFullName").stringValue;
				selectedSystemTypeNames.Add(fullTypeName);
			}
			for(int i = 0; i < _updateSystemDescriptions.arraySize; ++i) {
				string fullTypeName = _updateSystemDescriptions.GetArrayElementAtIndex(i).FindPropertyRelative("_typeFullName").stringValue;
				selectedSystemTypeNames.Add(fullTypeName);
			}
			for(int i = 0; i < _lateUpdateSystemDescriptions.arraySize; ++i) {
				string fullTypeName = _lateUpdateSystemDescriptions.GetArrayElementAtIndex(i).FindPropertyRelative("_typeFullName").stringValue;
				selectedSystemTypeNames.Add(fullTypeName);
			}

			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies() // impossible de le mettre autre part car peut pas garantir que ca na pas change
				from type in assembly.GetExportedTypes()
				where (type.IsClass == true && type.IsAbstract == false && type.IsSubclassOf(typeof(FSystem)) == true)
				select type).ToArray();
			
			GenericMenu menu = new GenericMenu();
			for (int i = 0; i < systemTypes.Length; ++i) {
				System.Type systemType = systemTypes[i];
				string typeAssemblyName = systemType.Assembly.GetName ().Name;
				string typeName = systemType.FullName;
				string buttonText = (typeAssemblyName != "Assembly-CSharp") ? (typeAssemblyName + "/" + typeName) : typeName;

				if (selectedSystemTypeNames.Contains (typeName)) {
					menu.AddDisabledItem(new GUIContent (buttonText));
				} else {
					menu.AddItem (
						new GUIContent (buttonText),
						false,
						delegate {
							SerializedProperty systemDescriptions = list.serializedProperty;
							int index = systemDescriptions.arraySize;
							systemDescriptions.arraySize++;

							SerializedProperty systemDescription = systemDescriptions.GetArrayElementAtIndex (index);
							systemDescription.FindPropertyRelative ("_typeAssemblyQualifiedName").stringValue = systemType.AssemblyQualifiedName;
							systemDescription.FindPropertyRelative ("_typeFullName").stringValue = systemType.FullName;
							systemDescription.FindPropertyRelative ("_pause").boolValue = false;

							serializedObject.ApplyModifiedProperties ();
						}
					);
				}
			}

			menu.ShowAsContext();
		}

		private void OnEnableInEditingMode() {
			_fixedUpdateSystemDescriptions = serializedObject.FindProperty("_fixedUpdateSystemDescriptions");
			_updateSystemDescriptions = serializedObject.FindProperty("_updateSystemDescriptions");
			_lateUpdateSystemDescriptions = serializedObject.FindProperty("_lateUpdateSystemDescriptions");

			_fixedUpdateDrawingList = new ReorderableList(serializedObject, _fixedUpdateSystemDescriptions, true, false, true, false);
			_updateDrawingList  = new ReorderableList(serializedObject, _updateSystemDescriptions, true, false, true, false);
			_lateUpdateDrawingList = new ReorderableList(serializedObject, _lateUpdateSystemDescriptions, true, false, true, false);

			_fixedUpdateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.editingModeDrawElementCallBack(_fixedUpdateSystemDescriptions, index, rect);
			};
			_updateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.editingModeDrawElementCallBack(_updateSystemDescriptions, index, rect);
			};
			_lateUpdateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.editingModeDrawElementCallBack(_lateUpdateSystemDescriptions, index, rect);
			};

			_fixedUpdateDrawingList.onAddDropdownCallback = createMenu;
			_updateDrawingList.onAddDropdownCallback = createMenu;
			_lateUpdateDrawingList.onAddDropdownCallback = createMenu;
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

			_fixedUpdateDrawingList.drawHeaderCallback = delegate(Rect rect) {
				EditorGUI.LabelField(rect, "FSystems in FixedUpdate");
			};
			_updateDrawingList.drawHeaderCallback = delegate(Rect rect) {
				EditorGUI.LabelField(rect, "FSystems in Update");
			};
			_lateUpdateDrawingList.drawHeaderCallback = delegate(Rect rect) {
				EditorGUI.LabelField(rect, "FSystems in LateUpdate");
			};
		}

		/// <summary></summary>
		public override void OnInspectorGUI(){
			serializedObject.Update();

			_fixedUpdateDrawingList.DoLayoutList();
			_updateDrawingList.DoLayoutList();
			_lateUpdateDrawingList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}
	}
}