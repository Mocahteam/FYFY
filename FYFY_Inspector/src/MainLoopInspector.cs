using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FYFY;

namespace FYFY_Inspector {
	/// <summary>
	/// 	Custom inspector automatically used by Unity Editor when a <see cref="FYFY.MainLoop"/> is find in inspector view.
	/// </summary>
	[CustomEditor(typeof(FYFY.MainLoop))]
	public class MainLoopInspector : Editor {

		private SerializedProperty _fixedUpdateSystemDescriptions;
		private SerializedProperty _updateSystemDescriptions;
		private SerializedProperty _lateUpdateSystemDescriptions;
		private SerializedProperty _loadingState;
		private SerializedProperty _specialGameObjects;

		private ReorderableList _fixedUpdateDrawingList;
		private ReorderableList _updateDrawingList;
		private ReorderableList _lateUpdateDrawingList;

		private SystemsMonitor _systemsMonitor;
		private const int HISTORY_DATA_LENGTH = 100;
		private Queue<float> _fixedUpdateStatsHistory;
		private Queue<float> _updateStatsHistory;
		private Queue<float> _lateUpdateStatsHistory;			

		// string elision if the area content is too small
		private string findFittableString(string originalString, Rect textArea){
			int cpt = originalString.Length;
			bool writeSize = false;
			string workingString = "";
			while (!writeSize && cpt > 0) {
				workingString = "";
				if (cpt != originalString.Length)
					workingString = originalString.Substring (0, cpt) + "...";
				else
					workingString = originalString;
				GUIContent textContent = new GUIContent(workingString);
				Vector2 textSize = GUIStyle.none.CalcSize(textContent); // default style
				if (textSize.x < textArea.width)
					writeSize = true;
				else
					cpt--;
			}
			return workingString;
		}

		private void playingModeDrawElementCallBack(FYFY.FSystem system, Rect rect) {
			float buttonSize = EditorGUIUtility.singleLineHeight;

			Color baseColor = GUI.color;
			GUI.color = (system.Pause == true) ? Color.red : Color.green;
			if(GUI.Button(new Rect(rect.x, rect.y + 1.35f, buttonSize, buttonSize), ""))
				system.Pause = !system.Pause;
			GUI.color = baseColor;

			string typeFullName = system.GetType().FullName+string.Format(" avg: {0:00.000}", system.avgExecDuration/1000)+string.Format(" max: {0:00.000}", system.maxExecDuration/1000);
			Rect textArea = new Rect (rect.x + buttonSize + 5, rect.y + 1.35f, rect.width - (buttonSize + 5), buttonSize);

			EditorGUI.LabelField (textArea, findFittableString(typeFullName, textArea));
		}

		private void OnEnableInPlayingMode() {
			_fixedUpdateDrawingList = new ReorderableList(FYFY.FSystemManager._fixedUpdateSystems, typeof(FSystem), true, true, false, false);
			_updateDrawingList = new ReorderableList(FYFY.FSystemManager._updateSystems, typeof(FSystem), true, true, false, false);
			_lateUpdateDrawingList = new ReorderableList(FYFY.FSystemManager._lateUpdateSystems, typeof(FSystem), true, true, false, false);

			_fixedUpdateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.playingModeDrawElementCallBack(FYFY.FSystemManager._fixedUpdateSystems[index], rect);
			};
			_updateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.playingModeDrawElementCallBack(FYFY.FSystemManager._updateSystems[index], rect);
			};
			_lateUpdateDrawingList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused) {
				this.playingModeDrawElementCallBack(FYFY.FSystemManager._lateUpdateSystems[index], rect);
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

			Rect textArea = new Rect (rect.x + buttonSize + 5, rect.y + 1.35f, rect.width - (buttonSize + 35), buttonSize);
			EditorGUI.LabelField(textArea, findFittableString(typeFullName.stringValue, textArea));

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
#if NET3_5
			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies() // impossible de le mettre autre part car peut pas garantir que ca na pas change
#else
			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic) // impossible de le mettre autre part car peut pas garantir que ca na pas change
#endif
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
			_loadingState = serializedObject.FindProperty("_loadingState");
			_specialGameObjects = serializedObject.FindProperty("_specialGameObjects");

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
			if(Application.isPlaying) {
				this.OnEnableInPlayingMode();
			} else {
				this.OnEnableInEditingMode();
			}

			_fixedUpdateDrawingList.drawHeaderCallback = delegate(Rect rect) {
				GUIStyle style = new GUIStyle();
				if (Application.isPlaying)
					style.normal.textColor = Color.magenta;
				EditorGUI.LabelField(rect, "FSystems in FixedUpdate", style);
			};
			_updateDrawingList.drawHeaderCallback = delegate(Rect rect) {
				GUIStyle style = new GUIStyle();
				if (Application.isPlaying)
					style.normal.textColor = new Color(255, 215, 0); // gold
				EditorGUI.LabelField(rect, "FSystems in Update", style);
			};
			_lateUpdateDrawingList.drawHeaderCallback = delegate(Rect rect) {
				GUIStyle style = new GUIStyle();
				if (Application.isPlaying)
					style.normal.textColor = new Color(0, 139, 139); // dark cyan
				EditorGUI.LabelField(rect, "FSystems in LateUpdate", style);
			};
		}

		private void displayFamilies(List<FSystem> systems){
			EditorGUI.indentLevel += 1;
			foreach (FSystem system in systems) {
				system.showFamilies = EditorGUILayout.Foldout (system.showFamilies, system.GetType ().FullName);
				if (system.showFamilies) {
					EditorGUI.indentLevel += 1;
					MemberInfo[] members = system.GetType ().GetMembers (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					foreach (MemberInfo member in members) {
						if (member.MemberType == MemberTypes.Field) {
							FieldInfo field = (FieldInfo)member;
							if (field.FieldType == typeof(FYFY.Family)) {
								Family f = (Family)field.GetValue (system);
								f.showContent = EditorGUILayout.Foldout (f.showContent, field.Name);
								if (f.showContent) {
									EditorGUI.indentLevel += 1;
									foreach (GameObject go in f)
										EditorGUILayout.ObjectField (go, typeof(GameObject), false);
									EditorGUI.indentLevel -= 1;
								}
							}
						}
					}
					EditorGUI.indentLevel -= 1;
				}
			}
			EditorGUI.indentLevel -= 1;
		}

		/// <summary></summary>
		public override void OnInspectorGUI(){
			serializedObject.Update();

			_fixedUpdateDrawingList.DoLayoutList();
			_updateDrawingList.DoLayoutList();
			_lateUpdateDrawingList.DoLayoutList();

			if (!Application.isPlaying) {
				EditorGUILayout.Space ();
				string[] options = new string[] {
					"Do not bind specified Game Objects on Start", "Bind only specified Game Objects on Start", 
				};
				int newValue = EditorGUILayout.Popup ("", _loadingState.intValue, options);
				if (newValue != _loadingState.intValue) {
					_loadingState.intValue = newValue;
				}
				EditorGUI.indentLevel += 1;
				for (int i = 0; i < _specialGameObjects.arraySize; i++) {
					GameObject currentGO = (GameObject)_specialGameObjects.GetArrayElementAtIndex (i).objectReferenceValue;
					if (currentGO == null) {
						_specialGameObjects.DeleteArrayElementAtIndex (i);
						i--;
					} else {
						GameObject updatedGO = (GameObject)EditorGUILayout.ObjectField (currentGO, typeof(GameObject), true);
						_specialGameObjects.GetArrayElementAtIndex (i).objectReferenceValue = updatedGO;
					}
				}
				_specialGameObjects.InsertArrayElementAtIndex (0);
				_specialGameObjects.GetArrayElementAtIndex (0).objectReferenceValue = (GameObject)EditorGUILayout.ObjectField (null, typeof(GameObject), true);

				EditorGUI.indentLevel -= 1;
			} else {
				MainLoop ml = (MainLoop)target;
				// init monitor if required
				if (_systemsMonitor == null) {
					_systemsMonitor = new SystemsMonitor (HISTORY_DATA_LENGTH);
					_fixedUpdateStatsHistory = new Queue<float> (new float[HISTORY_DATA_LENGTH]);
					_updateStatsHistory = new Queue<float> (new float[HISTORY_DATA_LENGTH]);
					_lateUpdateStatsHistory = new Queue<float> (new float[HISTORY_DATA_LENGTH]);
				}
				if (!EditorApplication.isPaused) {
					_fixedUpdateStatsHistory.Dequeue ();
					_fixedUpdateStatsHistory.Enqueue(ml.fixedUpdateStats);
					_updateStatsHistory.Dequeue ();
					_updateStatsHistory.Enqueue(ml.updateStats);
					_lateUpdateStatsHistory.Dequeue ();
					_lateUpdateStatsHistory.Enqueue(ml.lateUpdateStats);
				}

				EditorGUILayout.Space ();
				ml.showSystemProfiler = EditorGUILayout.Foldout (ml.showSystemProfiler, "FSystem profiler (ms)");
				if (ml.showSystemProfiler)
					_systemsMonitor.Draw (_fixedUpdateStatsHistory.ToArray (), _updateStatsHistory.ToArray (), _lateUpdateStatsHistory.ToArray (), 100f);

				EditorGUILayout.Space ();
				ml.showFamilyInspector = EditorGUILayout.Foldout (ml.showFamilyInspector, "Families inspector");
				if (ml.showFamilyInspector) {
					EditorGUI.indentLevel += 1;
					ml.showFamilyInspectorFixedUpdate = EditorGUILayout.Foldout (ml.showFamilyInspectorFixedUpdate, "FixedUpdate");
					if (ml.showFamilyInspectorFixedUpdate)
						displayFamilies (FSystemManager._fixedUpdateSystems);
					ml.showFamilyInspectorUpdate = EditorGUILayout.Foldout (ml.showFamilyInspectorUpdate, "Update");
					if (ml.showFamilyInspectorUpdate)
						displayFamilies (FSystemManager._updateSystems);
					ml.showFamilyInspectorLateUpdate = EditorGUILayout.Foldout (ml.showFamilyInspectorLateUpdate, "LateUpdate");
					if (ml.showFamilyInspectorLateUpdate)
						displayFamilies (FSystemManager._lateUpdateSystems);
					EditorGUI.indentLevel -= 1;
				}
				this.Repaint ();
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}