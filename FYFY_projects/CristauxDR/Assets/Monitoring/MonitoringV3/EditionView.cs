#if UNITY_EDITOR

using petriNetV2;
using monitorV3;
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

public class EditionView : EditorWindow
{
    public Rect windowRect = new Rect(20, 20, 20, 20);
	private static EditorWindow window;
    private static string[] optType = new string[] { "at least (with use)", "at least (without use)", "at the most (without use)" }; //TODO Type arc dans classe arc

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
    static int cptSt = 0;
    private Vector2 scrollPosition;

	private class Family2Monitor {
		public string systemName;
		public string familyName;
		public Family family;
		public string equivMonitor;
	}

    [MenuItem("FYFY/Edit Monitor")]
    public static void ShowWindow()
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

    void loadMonitoredGo()
    {
        go_labels = new List<string>();
		// Get all Game Objects with monitoring component
		monitors = FindObjectsOfType(typeof(ComponentMonitoring)) as ComponentMonitoring[];
		FamilyMonitoring[] families_monitored = FindObjectsOfType(typeof(FamilyMonitoring)) as FamilyMonitoring[];
		// Remove GO used to monitor families (child of Main_Loop GO)
        List<ComponentMonitoring> tmpList = monitors.ToList();
		foreach (FamilyMonitoring fmonitored in families_monitored)
            tmpList.Remove(fmonitored);
        monitors = tmpList.ToArray();

        // Update link counter
        foreach (ComponentMonitoring composant in monitors)
        {
            int cpt = 0;
            foreach(TransitionLink ctr in composant.transitionLinks)
                cpt += ctr.links.Count;
			go_labels.Add(composant.gameObject.name+" (ref: "+composant.id+") "+(composant.PnmlFile==null?"(PN: None":"(PN: "+composant.PnmlFile.name)+"; Total link: "+cpt+")");
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
					ComponentMonitoring monitor = Undo.AddComponent<ComponentMonitoring> (tmp);
					//monitor.hideFlags = HideFlags.HideInInspector;
					loadMonitoredGo ();
				} else {
					EditorUtility.DisplayDialog ("Action aborted", "You can't monitor a child of Main_Loop GameObject.", "Close");
				}
            }

            if (go_labels.Count != 0)
            {
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

				EditorGUILayout.BeginHorizontal();
				ObjectSelectedFlag = EditorGUILayout.Popup("Select Game Object:", ObjectSelectedFlag, go_labels.ToArray());
				if (GUILayout.Button ("X", GUILayout.Width (20))) {
					//Debug.Log("flagObjetSelectionne "+flagObjetSelectionne+" id detruit "+ objetsSuivis[flagObjetSelectionne].GetComponent<suiviV3.SuiviComposant>().id);
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
						GameObject go = (new GameObject(families[ObjectSelectedFlag].equivMonitor, typeof(FamilyMonitoring)));
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

	private void DrawList(ComponentMonitoring monitor)
    {
        if (monitor.petriNet.transitions.Count != 0)
        {
            // Build explicit labels
			List<string> labelsBuilt = new List<string>();

			foreach (TransitionLink tc in monitor.transitionLinks)
                labelsBuilt.Add(tc.transition.label + " (links: "+ tc.links.Count+")");

            flagTransition = EditorGUILayout.Popup("Action: ", flagTransition, labelsBuilt.ToArray());
    
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			TransitionLink tLink = monitor.transitionLinks[flagTransition];
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
				foreach (Link link in tLink.links.ToArray())
                {
                    if (tLink.links.IndexOf(link) != 0)
						EditorGUILayout.Space();
					Rect rect = EditorGUILayout.BeginHorizontal();
					{
						EditorGUI.DrawRect(rect, new Color(0.9f, 0.9f, 0.9f, 1));
						EditorGUILayout.BeginVertical();
                        {
							// Add first line
							EditorGUILayout.BeginHorizontal();
							// Add GameObject Input
                            EditorGUIUtility.labelWidth = 80;
							GameObject newLinkWithGO = (GameObject)EditorGUILayout.ObjectField("Linked with: ", link.objLink, typeof(GameObject), true);
							if (newLinkWithGO != link.objLink) {
								if (newLinkWithGO != null)
									Undo.RecordObject(monitor, "Update \"Linked with\"");
								else
									Undo.RecordObject(monitor, "Remove \"Linked with\"");
								link.objLink = newLinkWithGO;
							}
							EditorGUIUtility.labelWidth = 0; // reset default value
							EditorGUILayout.EndHorizontal();

                            if (link.objLink != null)
							{
								// Check if linked object is monitored, contains at least one monitor component
								if (link.objLink.GetComponent<ComponentMonitoring> () != null) {
									// Check if at least one Pnml file is attached to one of monitor components
									bool pnmlFound = false;
									foreach (ComponentMonitoring m in link.objLink.GetComponents<ComponentMonitoring> ())
										if (m.PnmlFile != null)
											pnmlFound = true;
									if (pnmlFound) {
										// Add second line
										EditorGUILayout.BeginHorizontal ();
										// Add Produce/Require combo box
										EditorGUIUtility.labelWidth = 30;
										bool newDiff = EditorGUILayout.Popup ("And", link.diffusion ? 0 : 1, new string[] { "Produce", "Require" }, GUILayout.MaxWidth (90)) == 0 ? true : false;
										if (newDiff != link.diffusion) {
											Undo.RecordObject (monitor, "Update Produce/Require");
											link.diffusion = newDiff;
										}
										EditorGUIUtility.labelWidth = 0; // reset default value
										// if Require selected, add "at least"/"at most" combo box
										if (!link.diffusion) {
											int newFlag = EditorGUILayout.Popup (link.flagsType, optType, GUILayout.MaxWidth (150));
											if (newFlag != link.flagsType) {
												Undo.RecordObject (monitor, "Update At least/At most"); 
												link.flagsType = newFlag;
											}
										}
										// Add weight input field
										int newWeight = EditorGUILayout.IntField (link.poids, GUILayout.MaxWidth (25));
										if (newWeight != link.poids) {
											Undo.RecordObject (monitor, "Update Link Weight");
											link.poids = newWeight;
										}
										EditorGUIUtility.labelWidth = 20;
										// build list of all places defined into target monitors
										List<string> places = new List<String>();
										foreach (ComponentMonitoring m in link.objLink.GetComponents<ComponentMonitoring> ()) {
											if (m.PnmlFile != null){
												foreach (string newItem in m.petriNet.getPlacesNames ())
													places.Add (newItem+" ("+m.PnmlFile.name+")");
											}
										}
										int newPlaceId = EditorGUILayout.Popup ("in", link.placeId, places.ToArray());
										if (newPlaceId != link.placeId) {
											Undo.RecordObject (monitor, "Update Link Target");
											link.placeId = newPlaceId;
										}
										EditorGUIUtility.labelWidth = 0; // reset default value
										EditorGUILayout.EndHorizontal ();
										// Add third line
										EditorGUILayout.BeginHorizontal ();
										EditorGUIUtility.labelWidth = 220;
										string newLabel = EditorGUILayout.TextField ("Label for logic expression (optional):", link.label);
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
											EditorGUILayout.LabelField ("Error! \"(\", \")\", \"*\", \"+\" and \" \" are not allowed into link label.", s, GUILayout.Width (405));
											EditorGUILayout.EndScrollView ();
										} else {
											// Add warning message if new label is not included into links logic expression
											if (tLink.logic != null && tLink.logic != "" && !tLink.logic.Contains (link.label)) {
												s.normal.textColor = new Color (0.9f, 0.5f, 0.1f, 1); // orange
												s.fontStyle = FontStyle.Bold;
												link.scroll = EditorGUILayout.BeginScrollView (link.scroll, GUILayout.Height (35));
												EditorGUILayout.LabelField ("Warning! This label is not curently included into \"Links logic expression\" field. If so, this link will not be used in monitoring process.", s, GUILayout.Width (865));
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
													EditorGUILayout.LabelField ("Warning! Two links have the same label. This is ambiguous in \"Links logic expression\" field.", s, GUILayout.Width (605));
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
										EditorGUILayout.BeginHorizontal();
										EditorGUILayout.LabelField("No Petri Net attached to the chosen game object.");
										EditorGUILayout.EndHorizontal();
									}
								} else {
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.LabelField("The game object chosen is not monitored.");
									EditorGUILayout.EndHorizontal();
								}
                            }
                        }
                        EditorGUILayout.EndVertical();
						if (GUILayout.Button ("X", GUILayout.Width (20))) {
							Undo.RecordObject (monitor, "Delete link");
							tLink.links.Remove (link);
						}
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add link"))
            {
				Link newLink = new Link();
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
				tLink.links.Add(newLink);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUIUtility.labelWidth = 150;
			string newExpr = EditorGUILayout.TextField("Logic expression of links:", tLink.logic);
			GUIStyle skin = new GUIStyle (GUI.skin.label);
			skin.normal.textColor = new Color (0.4f, 0.4f, 0.4f, 1); // soft grey
			EditorGUILayout.LabelField ("Exemple: (l0+l2)*l3; Default: *", skin);
			if (newExpr != tLink.logic) {
				Undo.RecordObject (monitor, "Update logic expression");
				tLink.logic = newExpr;
			}
			// Check expression 
			if (tLink.logic != null && tLink.logic != "") {
				AriParser arip = new AriParser ();
				if (arip.validAri (tLink)) {
					skin.normal.textColor = new Color (0, 0.6f, 0, 1); // green
					EditorGUILayout.LabelField ("\tValid expression.", skin);
				} else {
					skin.normal.textColor = Color.red;
					EditorGUILayout.LabelField ("\tInvalid expression.", skin);
				}
			}
        }
        else
            EditorGUILayout.LabelField("This Petri Net doesn't contain transitions.");
    }

    public void DrawUI(ComponentMonitoring monitor)
    {
        // Pnml input field
		UnityEngine.Object tmp = EditorGUILayout.ObjectField("PNML file:", monitor.PnmlFile, typeof(UnityEngine.Object), true);
		if (tmp == null) {
			if (tmp != monitor.PnmlFile) {
				Undo.RecordObject (monitor, "Remove PNML file");
				// Unloading
				monitor.PnmlFile = tmp;
				monitor.transitionLinks.Clear ();
				if (monitor.petriNet != null)
					monitor.petriNet = null;
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
						string path = AssetDatabase.GetAssetPath (monitor.PnmlFile);
						flagTransition = 0;
						monitor.transitionLinks.Clear ();
						monitor.petriNet = PetriNet.loadFromFile (path, monitor.id);
						monitor.loadTransitionsLinks ();
						// Add comments field
						EditorGUILayout.LabelField ("Comments:");
						string newComment = EditorGUILayout.TextArea (monitor.comments);
						if (newComment != monitor.comments) {
							Undo.RecordObject (monitor, "Update Comments");
							monitor.comments = newComment;
						}
					}
					// Draw links
					DrawList (monitor);
				}
			}
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
#endif