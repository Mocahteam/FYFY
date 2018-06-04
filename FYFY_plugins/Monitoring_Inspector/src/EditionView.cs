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
		internal static EditorWindow window;
	    private static string[] optType = new string[] { "Get", "Produce", "Require" };
	    private static string[] optFlag = new string[] { "at least", "less than" };

		private int ObjectSelectedFlag = 0;
		private int TemplateSelected = 0;
	    private int oldFlag;

	    //Handling menu buttons

	    private static GUIStyle ToggleButtonStyleNormal = null;
	    private static GUIStyle ToggleButtonStyleToggled = null;

	    private bool goMenuItemActive = true;
	    private bool familyMenuItemActive = false;

	    private int flagTransition;
	    private static int cptSt = 0;
	    private Vector2 scrollPosition;

		private bool showStates = false;
		private bool showActions = false;

	    [MenuItem("FYFY/Edit Monitoring")]
	    private static void ShowWindow()
	    {
			// be sure that MonitoringManager is instantiated
			if (MonitoringManager.Instance != null){
				//Show existing window instance. If one doesn't exist, make one.
				window = EditorWindow.GetWindow(typeof(EditionView));
				// Add callback to process Undo/Redo events
				Undo.undoRedoPerformed += window.Repaint; 
			} else {
				EditorUtility.DisplayDialog ("Action aborted", "You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", "Close");
			}
	    }

	    void OnGUI()
	    {
			// Check if MonitoringManager is still available
			if (MonitoringManager.Instance == null){
				window.Close();
				return;
			}
	        //Menu
	        makeMenu();
			MonitoringManager mm = MonitoringManager.Instance;
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
						ComponentMonitoring newMonitor = Undo.AddComponent<ComponentMonitoring> (tmp);
						// in case of monitor's GameObject is not active in hierarchy Start of ComponentMonitoring will not be called => then we force to compute unique id
						if (!tmp.activeInHierarchy)
							newMonitor.computeUniqueId();
						//newMonitor.hideFlags = HideFlags.HideInInspector;
					} else {
						EditorUtility.DisplayDialog ("Action aborted", "You can't monitor the Main_Loop GameObject or one of its childs.", "Close");
					}
				}

				List<string> go_labels = new List<string>();
				foreach (ComponentMonitoring cm in mm.c_monitors)
				{
					int cpt = 0;
					foreach(TransitionLink ctr in cm.transitionLinks)
						cpt += ctr.links.Count;
					go_labels.Add(cm.gameObject.name+" (ref: "+cm.id+") "+(cm.PnmlFile==null?"(PN: None":"(PN: "+cm.PnmlFile.name)+"; Total link: "+cpt+")");
				}
				if (go_labels.Count > 0)
				{
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

					EditorGUILayout.BeginHorizontal();
					ObjectSelectedFlag = EditorGUILayout.Popup("Edit Game Object:", ObjectSelectedFlag, go_labels.ToArray());
					if (ObjectSelectedFlag >= go_labels.Count) // can occur with Ctrl+Z
						ObjectSelectedFlag = go_labels.Count - 1;
					if (GUILayout.Button ("X", GUILayout.Width (20))) {
						// in case of monitor's GameObject is not active in hierarchy OnDestroy of ComponentMonitoring will not be called => then we force to free unique id
						if (!mm.c_monitors [ObjectSelectedFlag].gameObject.activeInHierarchy)
							mm.c_monitors [ObjectSelectedFlag].freeUniqueId();
						Undo.DestroyObjectImmediate (mm.c_monitors [ObjectSelectedFlag]);
						go_labels.RemoveAt(ObjectSelectedFlag);
						if (go_labels.Count <= 0)
							return;
						if (ObjectSelectedFlag >= go_labels.Count)
							ObjectSelectedFlag = go_labels.Count - 1;
					}
					EditorGUILayout.EndHorizontal();
					
					if (ObjectSelectedFlag != oldFlag)
						flagTransition = 0;
					oldFlag = ObjectSelectedFlag;

					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					
					// Init from a template
					List<string> templates_id = new List<string>();
					List<ComponentMonitoring> templates_cm = new List<ComponentMonitoring>();
					foreach (ComponentMonitoring cm in mm.c_monitors)
					{
						if (cm.id != mm.c_monitors[ObjectSelectedFlag].id && cm.PnmlFile != null){ // we exclude from the list monitor with the same id and monitors where pnmlfile is not defined (i.e. non initialized)
							int cpt = 0;
							foreach(TransitionLink ctr in cm.transitionLinks)
								cpt += ctr.links.Count;
							templates_id.Add(cm.gameObject.name+" (ref: "+cm.id+") (PN: "+cm.PnmlFile.name+"; Total link: "+cpt+")");
							templates_cm.Add(cm);
						}
					}
					if (templates_id.Count > 0){
						if (TemplateSelected >= templates_id.Count)
							TemplateSelected = templates_id.Count - 1;
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("(Option)", GUILayout.Width (60));
						TemplateSelected = EditorGUILayout.Popup("Import from model:", TemplateSelected, templates_id.ToArray());
						if (GUILayout.Button ("Import", GUILayout.Width (80))) {
							// clone from template
							mm.c_monitors[ObjectSelectedFlag].clone(templates_cm[TemplateSelected]);
						}
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					}
			
					//DrawUI
					if (mm.c_monitors[ObjectSelectedFlag] != null)
						DrawUI(mm.c_monitors[ObjectSelectedFlag]);
				}

			}
			else if (familyMenuItemActive)
			{
				List<string> flabels = new List<string>();

				// Build families label for each available families
				// Parse all available families
				foreach (MonitoringManager.FamilyAssociation fa in mm.availableFamilies)
				{
					// Try to find associated monitor to current available family
					FamilyMonitoring fm = mm.getFamilyMonitoring(fa.family);
					if (fm == null) // no monitor found
						flabels.Add(fa.systemName+"."+fa.familyName + " (Not monitored)");
					else {
						// Monitor found: we compute the number of links
						int cpt = 0;
						foreach (TransitionLink ctr in fm.transitionLinks)
						{
							cpt += ctr.links.Count;
						}
						// build rich label
						flabels.Add(fa.systemName+"."+fa.familyName + " " + (fm.PnmlFile == null ? "(PN: None" : "(PN: " + fm.PnmlFile.name) + "; Total link: " + cpt + ")");
					}
				}
				
				// If at least one family is available
				if (mm.availableFamilies.Count > 0)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 125;
					ObjectSelectedFlag = EditorGUILayout.Popup("Select a family:", ObjectSelectedFlag, flabels.ToArray());
					// Find monitor associated with object selected
					FamilyMonitoring fm = mm.getFamilyMonitoring(mm.availableFamilies[ObjectSelectedFlag].family);
					if (fm != null) {
						// If found, add button to remove monitoring
						if (GUILayout.Button ("X", GUILayout.Width (20))) {
							// in case of monitor is not active in hierarchy OnDestroy of FamilyMonitoring will not be called => then we force to free unique id
							if (!fm.gameObject.activeInHierarchy)
								fm.freeUniqueId();
							Undo.DestroyObjectImmediate (fm.gameObject);
						}
					}
					EditorGUILayout.EndHorizontal();

					if (fm == null)
					{
						// if Not found, add button to add a monitor to this family
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						if (GUILayout.Button("Add monitor to this family"))
						{
							GameObject go = new GameObject(mm.availableFamilies[ObjectSelectedFlag].equivWith);
							FamilyMonitoring newMonitor = go.AddComponent<FamilyMonitoring>();
							newMonitor.equivalentName = mm.availableFamilies[ObjectSelectedFlag].equivWith;
							newMonitor.descriptor = mm.availableFamilies[ObjectSelectedFlag].family.getDescriptor();
							go.transform.parent = mainLoop.transform;
							//go.GetComponent<FamilyMonitoring>().hideFlags = HideFlags.HideInInspector;
							Undo.RegisterCreatedObjectUndo(go, "Add monitor to family");
						}
					}
					else
					{
						// Init from a template
						List<string> templates_id = new List<string>();
						List<FamilyMonitoring> templates_fm = new List<FamilyMonitoring>();
						foreach (MonitoringManager.FamilyAssociation fa in mm.availableFamilies)
						{
							// Try to find associated monitor to current available family
							FamilyMonitoring fm_template = mm.getFamilyMonitoring(fa.family);
							if (fm_template != null && fm_template.equivalentName != fm.equivalentName && fm_template.PnmlFile != null){
								// Monitor found: we compute the number of links
								int cpt = 0;
								foreach (TransitionLink ctr in fm_template.transitionLinks)
									cpt += ctr.links.Count;
								// build rich label
								templates_id.Add(fa.systemName+"."+fa.familyName + " (PN: " + fm_template.PnmlFile.name + "; Total link: " + cpt + ")");
								templates_fm.Add(fm_template);
							}
						}
						if (templates_id.Count > 0){
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
							if (TemplateSelected >= templates_id.Count)
								TemplateSelected = templates_id.Count - 1;
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField ("(Option)", GUILayout.Width (60));
							TemplateSelected = EditorGUILayout.Popup("Import from model:", TemplateSelected, templates_id.ToArray());
							if (GUILayout.Button ("Import", GUILayout.Width (80))) {
								// clone from template
								fm.clone((ComponentMonitoring)templates_fm[TemplateSelected]);
							}
							EditorGUILayout.EndHorizontal ();
						}
						
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						DrawUI(fm);
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
					if (flagTransition >= labelsBuilt.Count) // can occur with Ctrl+Z
						flagTransition = labelsBuilt.Count - 1;

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
												int newType = EditorGUILayout.Popup ("And", link.type, optType, GUILayout.MaxWidth (110));
												if (newType != link.type) {
													Undo.RecordObject (monitor, "Update Type of Link");
													link.type = newType;
												}
												EditorGUI.indentLevel -= 1;
												EditorGUIUtility.labelWidth = 0; // reset default value
												// if Require selected, add "at least"/"at most" combo box
												if (link.type == 2) {
													int newFlag = EditorGUILayout.Popup (link.flagsType, optFlag, GUILayout.MaxWidth (80));
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

	        }

	        if (GUILayout.Button("Families", familyMenuItemActive ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
	        {
	            goMenuItemActive = false;
	            familyMenuItemActive = true;
	            ObjectSelectedFlag = 0;
	            flagTransition = 0;
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