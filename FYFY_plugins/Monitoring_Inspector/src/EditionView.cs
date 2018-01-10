using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using FYFY;

namespace FYFY_plugins.Monitoring {
	/// <summary>
	/// 	Custom UI for editing monitoring.
	/// </summary>
	public class EditionView : EditorWindow
	{
	    private Rect windowRect = new Rect(20, 20, 20, 20);
		private static EditorWindow window;
	    private static string[] optType = new string[] { "at least", "less than" };

		private int ObjectSelectedFlag = 0;
	    private int oldFlag;

	    //Handling menu buttons

	    private static GUIStyle ToggleButtonStyleNormal = null;
	    private static GUIStyle ToggleButtonStyleToggled = null;

	    private bool goMenuItemActive = true;
	    private bool familyMenuItemActive = false;

		// Pair Key <=> FSystem name
		// Value Key <=> Family included into FSystem name
		private List<Family2Monitor> families;

		private List<string> go_labels;
		private ComponentMonitoring[] monitors;
	    private int flagTransition;
	    private static int cptSt = 0;
	    private Vector2 scrollPosition;

		private bool showStates = false;
		private bool showActions = false;

		private class Family2Monitor {
			internal string systemName;
			internal string familyName;
			internal Family family;
			internal string equivMonitor;
		}

	    [MenuItem("FYFY/Edit Monitoring")]
	    private static void ShowWindow()
	    {
	        //Show existing window instance. If one doesn't exist, make one.
			window = EditorWindow.GetWindow(typeof(EditionView));
			// Add callback to process Undo/Redo events
			Undo.undoRedoPerformed += window.Repaint; 
	    }

	    // Use this for initialization
		void OnEnable () {
	        loadFamilies();
	        loadMonitoredGo();
	    }

	    void loadFamilies()
	    {
			families = new List<Family2Monitor>();
			// Load all FSystem included into assembly
			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
				from type in assembly.GetExportedTypes()
				where (type.IsClass == true && type.IsAbstract == false && type.IsSubclassOf(typeof(FSystem)) == true)
				select type).ToArray();
			// Parse all FSystems
			for (int i = 0; i < systemTypes.Length; ++i) {
				System.Type systemType = systemTypes [i];
				// Create instance of FSystem in order to know its Families types
				FSystem system = (FSystem) System.Activator.CreateInstance(systemType);
				// Load all members this System
				MemberInfo[] members = systemType.GetMembers (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (MemberInfo member in members) {
					if (member.MemberType == MemberTypes.Field) {
						FieldInfo field = (FieldInfo)member;
						if (field.FieldType == typeof(FYFY.Family)) {
							Family f = (Family)field.GetValue (system);
							// Check if this family is equivalent to another family already loaded
							string equivFamily = null;
							foreach (Family2Monitor f_alreadyStored in families)
								if (f.Equals(f_alreadyStored.family))
									equivFamily = f_alreadyStored.equivMonitor;
							// store data and link with equivalent family
							Family2Monitor entry = new Family2Monitor ();
							entry.systemName = systemType.FullName;
							entry.familyName = field.Name;
							entry.family = f;
							if (equivFamily != null)
								entry.equivMonitor = equivFamily; 
							else
								entry.equivMonitor = "equivWith_" + entry.systemName + "_" + field.Name;
							families.Add (entry);
						}
					}
				}
			}
	    }

		// reload monitored Game Object and select the new one if it exists
		private void loadMonitoredGo(ComponentMonitoring newOne = null)
	    {
	        go_labels = new List<string>();
			// Get all Game Objects with monitoring component
			monitors = Resources.FindObjectsOfTypeAll(typeof(ComponentMonitoring)) as ComponentMonitoring[];
			// Remove GO used to monitor families (child of Main_Loop GO)
			FamilyMonitoring[] families_monitored = Resources.FindObjectsOfTypeAll(typeof(FamilyMonitoring)) as FamilyMonitoring[];
	        List<ComponentMonitoring> tmpList = monitors.ToList();
			foreach (FamilyMonitoring fmonitored in families_monitored)
	            tmpList.Remove(fmonitored);
			// Sort list by game object name
			tmpList.Sort (delegate(ComponentMonitoring x, ComponentMonitoring y) {
				if (x == null && y == null)
					return 0;
				else if (x == null)
					return -1;
				else if (y == null)
					return 1;
				else
					return x.gameObject.name.CompareTo (y.gameObject.name);
			});
			monitors = tmpList.ToArray();

	        // Update link counter
			int selection = 0;
	        foreach (ComponentMonitoring composant in monitors)
	        {
	            int cpt = 0;
	            foreach(TransitionLink ctr in composant.transitionLinks)
	                cpt += ctr.links.Count;
				go_labels.Add(composant.gameObject.name+" (ref: "+composant.id+") "+(composant.PnmlFile==null?"(PN: None":"(PN: "+composant.PnmlFile.name)+"; Total link: "+cpt+")");
				if (newOne != null && newOne == composant)
					ObjectSelectedFlag = selection;
				selection++;
	        }
	    }

	    void OnGUI()
	    {
	        //Menu
	        makeMenu();
			loadMonitoredGo();
			GameObject mainLoop = GameObject.Find("Main_Loop");
	        if (goMenuItemActive)
	        {
				GameObject tmp = null;
				EditorGUIUtility.labelWidth = 125;
				tmp = EditorGUILayout.ObjectField("Add Game Object:", tmp, typeof(UnityEngine.Object), true) as GameObject;
	            if (tmp != null)
	            {
					// Forbid to drag and drop child of MainLoop
					if (!tmp.transform.IsChildOf (mainLoop.transform)) {
						bool isActive = tmp.activeSelf;
						if (!isActive) tmp.SetActive(true); // enable G0 in order to process Awake into ComponentMonitoring and compute ID
						ComponentMonitoring monitor = Undo.AddComponent<ComponentMonitoring> (tmp);
						if (!isActive) tmp.SetActive(false); // reset default state
						//monitor.hideFlags = HideFlags.HideInInspector;
						loadMonitoredGo (monitor);
					} else {
						EditorUtility.DisplayDialog ("Action aborted", "You can't monitor the Main_Loop GameObject or one of its childs.", "Close");
					}
	            }

	            if (go_labels.Count != 0)
	            {
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

					EditorGUILayout.BeginHorizontal();
					ObjectSelectedFlag = EditorGUILayout.Popup("Select Game Object:", ObjectSelectedFlag, go_labels.ToArray());
					if (GUILayout.Button ("X", GUILayout.Width (20))) {
						Undo.DestroyObjectImmediate (monitors [ObjectSelectedFlag]);
						ObjectSelectedFlag = 0;
					}
	                EditorGUILayout.EndHorizontal();
	                
	                if (ObjectSelectedFlag != oldFlag)
	                    flagTransition = 0;
	                oldFlag = ObjectSelectedFlag;

	                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

	                //DrawUI
					if (monitors[ObjectSelectedFlag] != null)
	                	DrawUI(monitors[ObjectSelectedFlag]);
	            }

	        }
	        else if (familyMenuItemActive)
	        {
	            List<string> flabels = new List<string>();


				foreach (Family2Monitor fi in families)
	            {
					Transform child = mainLoop.transform.Find(fi.equivMonitor);
					// Display all families
	                if (child == null)
						flabels.Add(fi.systemName+"."+fi.familyName + " (Not monitored)");
	                else if (child.GetComponent<FamilyMonitoring>() != null)
	                {
	                    FamilyMonitoring sf = child.GetComponent<FamilyMonitoring>();
	                    int cpt = 0;
	                    foreach (TransitionLink ctr in sf.transitionLinks)
	                    {
	                        cpt += ctr.links.Count;
	                    }
						flabels.Add(fi.systemName+"."+fi.familyName + " " + (sf.PnmlFile == null ? "(PN: None" : "(PN: " + sf.PnmlFile.name) + "; Total link: " + cpt + ")");
	                }
	            }
	            
	            if (families.Count != 0)
	            {
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 100;
	                ObjectSelectedFlag = EditorGUILayout.Popup("Select a family:", ObjectSelectedFlag, flabels.ToArray());
					// Find a child of Main_Loop GameObject associated with object selected
					Transform child = mainLoop.transform.Find(families[ObjectSelectedFlag].equivMonitor);
					if (child != null) {
						// If found, add button to remove monitoring
						if (GUILayout.Button ("X", GUILayout.Width (20))) {
							Undo.DestroyObjectImmediate (child.gameObject);
							ObjectSelectedFlag = 0;
						}
					}
	                EditorGUILayout.EndHorizontal();

	                if (child == null)
	                {
						// if Not found, add button to add a monitor to this family
	                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	                    if (GUILayout.Button("Add monitor to this family"))
	                    {
							GameObject go = new GameObject(families[ObjectSelectedFlag].equivMonitor);
							FamilyMonitoring fm = go.AddComponent<FamilyMonitoring>();
							fm.familyName = families[ObjectSelectedFlag].familyName;
	                        go.transform.parent = mainLoop.transform;
	                        //go.GetComponent<FamilyMonitoring>().hideFlags = HideFlags.HideInInspector;
							Undo.RegisterCreatedObjectUndo(go, "Add monitor to family");
	                    }
	                }
	                else
	                {
	                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	                    DrawUI(child.GetComponent<FamilyMonitoring>());
	                }
	            }
	            else
	                EditorGUILayout.LabelField("No family found");
	        }
	        
	        windowRect = GUI.Window(0, windowRect, DoMyWindow, "Editing monitors");
		}

		private void DrawUI(ComponentMonitoring monitor)
		{
			// Pnml input field
			UnityEngine.Object tmp = EditorGUILayout.ObjectField("PNML file:", monitor.PnmlFile, typeof(UnityEngine.Object), true);
			if (tmp == null) {
				if (tmp != monitor.PnmlFile) {
					Undo.RecordObject (monitor, "Remove PNML file");
					// Unloading
					monitor.PnmlFile = null;
					monitor.PetriNet = null;
				}

			} else {
				bool validFileType = Path.GetExtension (AssetDatabase.GetAssetPath (tmp)).Equals (".pnml");
				if (!validFileType)
					EditorUtility.DisplayDialog ("Action aborted", "Only files with .pnml extension are compatible", "Close");
				else {
					// Check if this Pnml file is not already attached to an other monitor of this Game Object
					bool samePnmlFound = false;
					foreach (ComponentMonitoring other in monitor.gameObject.GetComponents<ComponentMonitoring>())
						if (other != monitor && other.PnmlFile != null && other.PnmlFile.name == tmp.name)
							samePnmlFound = true;
					if (samePnmlFound)
						EditorUtility.DisplayDialog ("Action aborted", "A Pnml file with the same name is already attached to this Game Object.", "Close");
					else {
						if (tmp != monitor.PnmlFile) {
							Undo.RecordObject (monitor, "Change PNML file");
							// Loading
							monitor.PnmlFile = tmp;
							monitor.PetriNet = PetriNet.loadFromFile (AssetDatabase.GetAssetPath (monitor.PnmlFile), monitor.id);
							flagTransition = 0;
						}
						// Add comments field
						EditorGUILayout.LabelField ("Comments:");
						string newComment = EditorGUILayout.TextArea (monitor.comments);
						if (newComment != monitor.comments) {
							Undo.RecordObject (monitor, "Update Comments");
							monitor.comments = newComment;
						}
						// Draw states
						EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
						DrawStates (monitor);
						EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
						// Draw actions
						DrawActions (monitor);
						EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
					}
				}
			}
		}

		private void DrawStates(ComponentMonitoring monitor)
		{
			
			showStates = EditorGUILayout.Foldout (showStates, "Set initial states");
			if (showStates) {
				EditorGUI.indentLevel += 1;
				if (monitor.PetriNet != null && monitor.PetriNet.places.Count == 0)
					EditorGUILayout.LabelField ("This Petri Net doesn't contain places.");
				else {
					foreach (Node place in monitor.PetriNet.places) {
						EditorGUILayout.BeginHorizontal ();
						// Add input field to change initial marking
						EditorGUIUtility.labelWidth = 150;
						int newMarking = EditorGUILayout.IntField (place.label, place.initialMarking, GUILayout.MaxWidth (175));
						EditorGUIUtility.labelWidth = 0; // reset default value
						if (newMarking != place.initialMarking) {
							Undo.RecordObject (monitor, "Update Initial state");
							place.initialMarking = newMarking;
						}
						// Add override field
						//EditorGUIUtility.labelWidth = 150;
						string newLabel = EditorGUILayout.TextField ("Override name:", place.overridedLabel/*, GUILayout.MaxWidth (300)*/);
						//EditorGUIUtility.labelWidth = 0; // reset default value
						if (newLabel != place.overridedLabel) {
							Undo.RecordObject (monitor, "Update Override Label");
							place.overridedLabel = newLabel;
						}
						EditorGUILayout.EndHorizontal ();
					}
				}
				EditorGUI.indentLevel -= 1;
			}
		}

		private void DrawActions(ComponentMonitoring monitor)
	    {
			showActions = EditorGUILayout.Foldout (showActions, "Define links on actions");
			if (showActions) {
				EditorGUI.indentLevel += 1;
				if (monitor.PetriNet != null && monitor.PetriNet.getTransitionsNames().Length == 0)
					EditorGUILayout.LabelField("This Petri Net doesn't contain transitions.");
				else
				{
					// Build explicit labels
					List<string> labelsBuilt = new List<string> ();

					foreach (TransitionLink tc in monitor.transitionLinks){
						labelsBuilt.Add (tc.transition.label + " (links: " + tc.links.Count + ")");
					}

					flagTransition = EditorGUILayout.Popup ("Action: ", flagTransition, labelsBuilt.ToArray ());

					TransitionLink tLink = monitor.transitionLinks [flagTransition];
					
					// Add override field
					string newTrLabel = EditorGUILayout.TextField ("Override name:", tLink.transition.overridedLabel);
					if (newTrLabel != tLink.transition.overridedLabel) {
						Undo.RecordObject (monitor, "Update Override Label");
						tLink.transition.overridedLabel = newTrLabel;
					}
					
					bool newToggle = EditorGUILayout.ToggleLeft ("Not a player action (triggered by game simulation)", tLink.isSystemAction);
					if (newToggle != tLink.isSystemAction) {
						Undo.RecordObject (monitor, "Is Player Action");
						tLink.isSystemAction = newToggle;
					}
					newToggle = EditorGUILayout.ToggleLeft ("Player objective", tLink.isEndAction);
					if (newToggle != tLink.isEndAction) {
						Undo.RecordObject (monitor, "Is Player Objective");
						tLink.isEndAction = newToggle;
					}
					scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);
					{
						foreach (Link link in tLink.links.ToArray()) {
							if (tLink.links.IndexOf (link) != 0)
								EditorGUILayout.Space ();
							Rect rect = EditorGUILayout.BeginHorizontal ();
							{
								EditorGUI.DrawRect (rect, new Color (0.9f, 0.9f, 0.9f, 1));
								EditorGUILayout.BeginVertical ();
								{
									// Add first line
									EditorGUILayout.BeginHorizontal ();
									// Add GameObject Input
									EditorGUIUtility.labelWidth = 100;
									GameObject newLinkWithGO = (GameObject)EditorGUILayout.ObjectField ("Linked with: ", link.linkedObject, typeof(GameObject), true);
									if (newLinkWithGO != link.linkedObject) {
										if (newLinkWithGO != null)
											Undo.RecordObject (monitor, "Update \"Linked with\"");
										else
											Undo.RecordObject (monitor, "Remove \"Linked with\"");
										link.linkedObject = newLinkWithGO;
									}
									EditorGUIUtility.labelWidth = 0; // reset default value
									EditorGUILayout.EndHorizontal ();

									if (link.linkedObject != null) {
										// Check if linked object is monitored, contains at least one monitor component
										if (link.linkedObject.GetComponent<ComponentMonitoring> () != null) {
											// Check if at least one Pnml file is attached to one of monitor components
											bool pnmlFound = false;
											foreach (ComponentMonitoring m in link.linkedObject.GetComponents<ComponentMonitoring> ())
												if (m.PnmlFile != null)
													pnmlFound = true;
											if (pnmlFound) {
												// Add second line
												EditorGUILayout.BeginHorizontal ();
												// Add Produce/Require combo box
												EditorGUIUtility.labelWidth = 50;
												int newType = EditorGUILayout.Popup ("And", link.type, new string[] { "Get", "Produce", "Require" }, GUILayout.MaxWidth (110));
												if (newType != link.type) {
													Undo.RecordObject (monitor, "Update Type of Link");
													link.type = newType;
												}
												EditorGUI.indentLevel -= 1;
												EditorGUIUtility.labelWidth = 0; // reset default value
												// if Require selected, add "at least"/"at most" combo box
												if (link.type == 2) {
													int newFlag = EditorGUILayout.Popup (link.flagsType, optType, GUILayout.MaxWidth (80));
													if (newFlag != link.flagsType) {
														Undo.RecordObject (monitor, "Update At least/At most"); 
														link.flagsType = newFlag;
													}
												}
												// Add weight input field
												int newWeight = EditorGUILayout.IntField (link.weight, GUILayout.MaxWidth (25));
												if (newWeight != link.weight) {
													Undo.RecordObject (monitor, "Update Link Weight");
													link.weight = newWeight;
												}
												// Add in/from field
												EditorGUIUtility.labelWidth = 20;
												string src = "in";
												if (link.type == 0) {
													EditorGUIUtility.labelWidth = 40;
													src = "from";
												}
												int newPlaceId = EditorGUILayout.Popup (src, link.placeId, link.getPlacesNameFromLinkedObject ());
												if (newPlaceId != link.placeId) {
													Undo.RecordObject (monitor, "Update Link Target");
													link.placeId = newPlaceId;
												}
												EditorGUIUtility.labelWidth = 0; // reset default value
												EditorGUILayout.EndHorizontal ();
												// Add third line
												EditorGUILayout.BeginHorizontal ();
												EditorGUIUtility.labelWidth = 240;
												EditorGUI.indentLevel += 1;
												string newLabel = EditorGUILayout.TextField ("Label for logic expression and traces:", link.label);
												if (newLabel != link.label) {
													Undo.RecordObject (monitor, "Update Link Label");
													link.label = newLabel;
												}
												EditorGUIUtility.labelWidth = 0; // reset default value
												EditorGUILayout.EndHorizontal ();
												// Check label
												// Add warning if label includes "()*+ " token
												GUIStyle s = new GUIStyle (GUI.skin.label);
												if (link.label.Contains ("(") || link.label.Contains (")") || link.label.Contains ("*") || link.label.Contains ("+") || link.label.Contains (" ")) {
													s.normal.textColor = Color.red;
													s.fontStyle = FontStyle.Bold;
													link.scroll = EditorGUILayout.BeginScrollView (link.scroll, GUILayout.Height (35));
													EditorGUILayout.LabelField ("Error! \"(\", \")\", \"*\", \"+\" and \" \" are not allowed into link label.", s, GUILayout.Width (425));
													EditorGUILayout.EndScrollView ();
												} else {
													// Add warning message if new label is not included into links logic expression
													if (tLink.logic != null && tLink.logic != "" && !tLink.logic.Contains (link.label)) {
														s.normal.textColor = new Color (0.9f, 0.5f, 0.1f, 1); // orange
														s.fontStyle = FontStyle.Bold;
														link.scroll = EditorGUILayout.BeginScrollView (link.scroll, GUILayout.Height (35));
														EditorGUILayout.LabelField ("Warning! This label is not curently included into \"Links logic expression\" field. If so, this link will not be used in monitoring process.", s, GUILayout.Width (885));
														EditorGUILayout.EndScrollView ();
													} else {
														// Add warning if two links have the same label
														bool unique = true;
														foreach (Link l in tLink.links)
															if (l != link && l.label == link.label)
																unique = false;
														if (!unique) {
															s.normal.textColor = Color.red;
															s.fontStyle = FontStyle.Bold;
															link.scroll = EditorGUILayout.BeginScrollView (link.scroll, GUILayout.Height (35));
															EditorGUILayout.LabelField ("Warning! Two links have the same label. This is ambiguous in \"Links logic expression\" field.", s, GUILayout.Width (625));
															EditorGUILayout.EndScrollView ();
														} else {
															// Add blank area to avoid flickering on editing labels in case of warning (lost of focus)
															link.scroll = EditorGUILayout.BeginScrollView (link.scroll, GUILayout.Height (35));
															EditorGUILayout.LabelField ("", s, GUILayout.Width (80));
															EditorGUILayout.EndScrollView ();
														}
													}
												}
											} else {
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.LabelField ("No Petri Net attached to the chosen game object.");
												EditorGUILayout.EndHorizontal ();
											}
										} else {
											EditorGUILayout.BeginHorizontal ();
											EditorGUILayout.LabelField ("The game object chosen is not monitored.");
											EditorGUILayout.EndHorizontal ();
										}
									}
								}
								EditorGUILayout.EndVertical ();
								if (GUILayout.Button ("X", GUILayout.Width (20))) {
									Undo.RecordObject (monitor, "Delete link");
									tLink.links.Remove (link);
								}
							}
							EditorGUILayout.EndHorizontal ();
						}
					}
					EditorGUILayout.EndScrollView ();
					GUILayout.BeginHorizontal ();
					GUILayout.Space (EditorGUI.indentLevel * 20);
					if (GUILayout.Button ("Add link")) {
						Link newLink = new Link ();
						newLink.label = "l" + (cptSt++);
						// Check if another link have the same name
						bool unique;
						do {
							unique = true;
							foreach (Link l in tLink.links)
								if (l.label == newLink.label)
									unique = false;
							if (!unique)
								newLink.label = "l" + (cptSt++);
						} while (!unique);
						Undo.RecordObject (monitor, "Create link");
						tLink.links.Add (newLink);
					}
					GUILayout.EndHorizontal ();

					EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

					EditorGUIUtility.labelWidth = 170;
					string newExpr = EditorGUILayout.TextField ("Logic expression of links:", tLink.logic);
					GUIStyle skin = new GUIStyle (GUI.skin.label);
					skin.normal.textColor = new Color (0.4f, 0.4f, 0.4f, 1); // soft grey
					EditorGUILayout.LabelField ("Example: (l0+l2)*l3; Default: *", skin);
					if (newExpr != tLink.logic) {
						Undo.RecordObject (monitor, "Update logic expression");
						tLink.logic = newExpr;
					}
					// Check expression 
					if (tLink.logic != null && tLink.logic != "") {
						ExpressionParser exp_parser = new ExpressionParser ();
						if (exp_parser.isValid (tLink)) {
							skin.normal.textColor = new Color (0, 0.6f, 0, 1); // green
							EditorGUILayout.LabelField ("\tValid expression.", skin);
						} else {
							skin.normal.textColor = Color.red;
							EditorGUILayout.LabelField ("\tInvalid expression.", skin);
						}
					}
				}
				EditorGUI.indentLevel -= 1;
	        }
	    }

	    void makeMenu()
	    {
	        if (ToggleButtonStyleNormal == null)
	        {
	            ToggleButtonStyleNormal = "Button";
	            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
	            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;

	        }

	        GUILayout.BeginHorizontal();
	        if (GUILayout.Button("GameObjects", goMenuItemActive ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
	        {
	            goMenuItemActive = true;
	            familyMenuItemActive = false;
	            ObjectSelectedFlag = 0;
	            flagTransition = 0;
	            //loadDatas(families[flagFamily], components);

	        }

	        if (GUILayout.Button("Families", familyMenuItemActive ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
	        {
	            goMenuItemActive = false;
	            familyMenuItemActive = true;
	            ObjectSelectedFlag = 0;
	            flagTransition = 0;
	            //loadDatas(families[flagFamily], layers);
	        }

	        GUILayout.EndHorizontal();
	        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	    }

	    void DoMyWindow(int windowID)
	    {
	        GUI.DragWindow(new Rect(20, 20, 50, 50));
	    }
	}
}