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

		private int petriNetFilter = 0;
        private int systemFilter = 0;
        private int monitorSelectedFlag = 0;
        private int familySelectedFlag = 0;
        private int templateSelected = 0;
	    private int oldMonitorRef;
        private string oldFamilyName;

        //Handling menu buttons

        private static GUIStyle ToggleButtonStyleNormal = null;
	    private static GUIStyle ToggleButtonStyleToggled = null;

	    private bool goMenuItemActive = true;
	    private bool familyMenuItemActive = false;

	    private int flagTransition;
	    private static int cptSt = 0;
	    private Vector2 scrollPosition;
        private List<GUIContent> templates_id;

        private bool showStates = false;
		private bool showActions = false;
		private bool showOptions = false;

	    [MenuItem("FYFY/Edit Monitoring")]
	    private static void ShowWindow()
        {
            // be sure that MonitoringManager is instantiated
            if (MonitoringManager.Instance != null){
				//Show existing window instance. If one doesn't exist, make one.
				window = EditorWindow.GetWindow(typeof(EditionView));
				window.minSize = new Vector2 (360f, 600f);
				// Add callback to process Undo/Redo events
				Undo.undoRedoPerformed += window.Repaint;
                Undo.undoRedoPerformed += synchroniseMonitors;
            } else {
				EditorUtility.DisplayDialog ("Action aborted", "You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", "Close");
            }
        }

        static void synchroniseMonitors()
        {
            if (MonitoringManager.Instance != null)
            {
                MonitoringManager mm = MonitoringManager.Instance;
                for (int i = mm.c_monitors.Count-1; i >= 0; i--)
                    if (mm.c_monitors[i] == null)
                        mm.c_monitors.RemoveAt(i);
                for (int i = mm.f_monitors.Count-1; i >= 0; i--)
                    if (mm.f_monitors[i] == null)
                        mm.f_monitors.RemoveAt(i);
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
                if (Event.current != null && Event.current.type == EventType.MouseDown)
                {
                    mm.c_monitors.Sort(delegate (ComponentMonitoring x, ComponentMonitoring y) {
                        if (x == null && y == null)
                            return 0;
                        else if (x == null)
                            return -1;
                        else if (y == null)
                            return 1;
                        else
                            return x.gameObject.name.CompareTo(y.gameObject.name);
                    });
                }

                GameObject tmp = null;
				EditorGUIUtility.labelWidth = 125;
				tmp = EditorGUILayout.ObjectField(new GUIContent("Add Game Object:", "Drag&Drop a game object you want to monitor. A same game object can be added several time."), tmp, typeof(UnityEngine.Object), true) as GameObject;
				if (tmp != null)
				{
					// Forbid to drag and drop child of MainLoop
					if (!tmp.transform.IsChildOf (mainLoop.transform)) {
						ComponentMonitoring newMonitor = Undo.AddComponent<ComponentMonitoring> (tmp);
                        // We set the "ready" to true when unity end deserialization of the component (ComponentMonitoring::OnAfterDeserialize),
                        // but for a new ComponentMonitoring no deserialization occurs and we set "ready" to true right after the instantiation of the component
                        newMonitor.ready = true;
                        //The ComponentMonitoring is now processing a new id and in order to set monitorSelectedFlag we have to wait the monitor id to be defined 
                        //TODO: we have to find a way not to block this script while we are waiting for the id
                        while (newMonitor.id == -1) ;
                        monitorSelectedFlag = mm.c_monitors.FindIndex(x => x.id == newMonitor.id);
                        // reset filter flag
                        petriNetFilter = 0;
                        oldMonitorRef = -1;
                        //newMonitor.hideFlags = HideFlags.HideInInspector;
                    } else {
						EditorUtility.DisplayDialog ("Action aborted", "You can't monitor the Main_Loop GameObject or one of its childs.", "Close");
					}
				}

				if (mm.c_monitors.Count > 0)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

					// compute template list for filter by Petri net
					templates_id = new List<GUIContent>();
					templates_id.Add(new GUIContent("<No filter>"));
					foreach (string name in mm.PetriNetsName)
						templates_id.Add(new GUIContent(name));
					// Display filter 
					petriNetFilter = EditorGUILayout.Popup(new GUIContent("Petri net Filter:", "Filter monitors by Full Petri nets (This list is defined in MonitoringManager component)."), petriNetFilter, templates_id.ToArray());
					if (petriNetFilter >= templates_id.Count) // can occur with Ctrl+Z
						petriNetFilter = templates_id.Count - 1;
                    // Compute list of monitor depending on filter
					List<GUIContent> go_labels = new List<GUIContent>();
					foreach (ComponentMonitoring cm in mm.c_monitors)
					{
                        //cm can be null if you remove the ComponentMonitoring of a disabled gameobject from Unity hierachy (but handled if removed from Monitoring Window)
                        //the null cm stays in the list because it can be removed from the list from the OnDestroy (because it is not called on disabled gameobjects)
                        //the null cm is removed on the next MonitoringManager's Awake because c_monitors is cleared
						if (cm != null && (petriNetFilter == 0 || cm.fullPnSelected == petriNetFilter - 1)){
							int cpt = 0;
                            foreach (TransitionLink ctr in cm.transitionLinks)
								cpt += ctr.links.Count;
							go_labels.Add(new GUIContent(cm.gameObject.name+" (ref: "+cm.id+") "+(cm.PnmlFile==null?"(PN: None":"(PN: "+cm.PnmlFile.name)+"; Total link: "+cpt+")"));
                            // if we found a monitor with the same id of the previous selected, we focus on it
                            if (cm.id == oldMonitorRef)
                                monitorSelectedFlag = go_labels.Count - 1;
                        }
					}
                    
                    if (go_labels.Count <= 0)
						EditorGUILayout.LabelField("No Monitor found");
                    else
                    {
                        if (monitorSelectedFlag >= go_labels.Count)
                            monitorSelectedFlag = go_labels.Count - 1;
                        EditorGUILayout.BeginHorizontal();
                        monitorSelectedFlag = EditorGUILayout.Popup(new GUIContent("Edit Monitor:", "Select the monitor you want to configure."), monitorSelectedFlag, go_labels.ToArray());
                        if (monitorSelectedFlag >= go_labels.Count) // can occur with Ctrl+Z
							monitorSelectedFlag = go_labels.Count - 1;
                        // extract ref from label, just after the '(ref: ' and before ')'
                        string[] refToken = { "(ref: " };
                        string stringRefId = go_labels[monitorSelectedFlag].text.Split(refToken, System.StringSplitOptions.None)[1].Split(')')[0];
                        int refId;
                        if (!Int32.TryParse(stringRefId, out refId))
                        {
                            EditorGUILayout.EndHorizontal();
                            GUIStyle skin = new GUIStyle(GUI.skin.label);
                            skin.normal.textColor = new Color(1f, 0.2f, 0.2f, 1); // soft red
                            EditorGUILayout.LabelField("Warning, unable to decode monitor reference", skin);
                            return;
                        }
                        // try to get monitor with this ref id
                        ComponentMonitoring cm = mm.c_monitors.Find(x => x.id == refId);
                        if (cm == null)
                        {
                            EditorGUILayout.EndHorizontal();
                            GUIStyle skin = new GUIStyle(GUI.skin.label);
                            skin.normal.textColor = new Color(1f, 0.2f, 0.2f, 1); // soft red
                            EditorGUILayout.LabelField("Warning, unable to find monitor with reference: " + refId, skin);
                            return;
                        }
                        if (GUILayout.Button(new GUIContent("X", "Remove selected monitor."), GUILayout.Width(20)))
                        {
                            // in case of monitor's GameObject is not active in hierarchy OnDestroy of ComponentMonitoring will not be called => then we force to free unique id
                            if (!cm.gameObject.activeInHierarchy)
                                cm.freeUniqueId();
                            Undo.DestroyObjectImmediate(cm);
                            go_labels.RemoveAt(monitorSelectedFlag);
                            if (go_labels.Count <= 0)
                                return;
                            if (monitorSelectedFlag >= go_labels.Count)
                                monitorSelectedFlag = go_labels.Count - 1;
                            // reset ComponentMonitoring in case of ObjectSelectedFlag update
							stringRefId = go_labels[monitorSelectedFlag].text.Split(refToken, System.StringSplitOptions.None)[1].Split(')')[0];
                            if (!Int32.TryParse(stringRefId, out refId))
                            {
                                EditorGUILayout.EndHorizontal();
                                GUIStyle skin = new GUIStyle(GUI.skin.label);
                                skin.normal.textColor = new Color(1f, 0.2f, 0.2f, 1); // soft red
                                EditorGUILayout.LabelField("Warning, unable to decode monitor reference", skin);
                                return;
                            }
                            cm = mm.c_monitors.Find(x => x.id == refId);
                            if (cm == null)
                            {
                                EditorGUILayout.EndHorizontal();
                                GUIStyle skin = new GUIStyle(GUI.skin.label);
                                skin.normal.textColor = new Color(1f, 0.2f, 0.2f, 1); // soft red
                                EditorGUILayout.LabelField("Warning, unable to find monitor with reference: " + refId, skin);
                                return;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        if (refId != oldMonitorRef)
                        {
                            Selection.activeGameObject = cm.gameObject;
                            if(Selection.activeGameObject.GetComponentInChildren<Renderer>() || Selection.activeGameObject.GetComponentInChildren<CanvasRenderer>())
                                SceneView.FrameLastActiveSceneView();
                            flagTransition = 0;
                        }
                        oldMonitorRef = refId;

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        // compute template list
                        templates_id = new List<GUIContent>();
                        List<ComponentMonitoring> templates_cm = new List<ComponentMonitoring>();
                        foreach (ComponentMonitoring _cm in mm.c_monitors)
                        {
                            if (_cm.id != cm.id && _cm.PnmlFile != null)
                            { // we exclude from the list monitor with the same id and monitors where pnmlfile is not defined (i.e. non initialized)
                                int cpt = 0;
                                foreach (TransitionLink ctr in _cm.transitionLinks)
                                    cpt += ctr.links.Count;
                                templates_id.Add(new GUIContent(_cm.gameObject.name + " (ref: " + _cm.id + ") (PN: " + _cm.PnmlFile.name + "; Total link: " + cpt + ")"));
                                templates_cm.Add(_cm);
                            }
                        }

                        showOptions = EditorGUILayout.Foldout(showOptions, "Options");
                        if (showOptions)
                        {
                            EditorGUI.indentLevel += 2;
                            EditorGUIUtility.labelWidth = 160;
                            if (templates_id.Count > 0)
                            {
                                if (templateSelected >= templates_id.Count)
                                    templateSelected = templates_id.Count - 1;
                                EditorGUILayout.BeginHorizontal();
                                templateSelected = EditorGUILayout.Popup(new GUIContent("Import from model:", "Select the monitor you want to use as template, all properties will be copied to the editing monitor."), templateSelected, templates_id.ToArray());
                                if (GUILayout.Button("Import", GUILayout.Width(80)))
                                {
                                    // clone from template
                                    cm.clone(templates_cm[templateSelected]);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            // Be sure that Petri net selected is not over the MonitoringManager list
                            if (cm.fullPnSelected >= mm.PetriNetsName.Count)
                                cm.fullPnSelected = 0;
                            templates_id = new List<GUIContent>();
                            foreach (string name in mm.PetriNetsName)
                                templates_id.Add(new GUIContent(name));
                            int pnSelected = EditorGUILayout.Popup(new GUIContent("Affect to Full Petri net:", "Add this monitor to the selected full Petri net. This list is defined in MonitoringManager component."), cm.fullPnSelected, templates_id.ToArray());
                            if (pnSelected != cm.fullPnSelected)
                            {
                                Undo.RecordObject(cm, "Update Export Petri net");
                                cm.fullPnSelected = pnSelected;
                            }
                            EditorGUIUtility.labelWidth = 125;
                            EditorGUI.indentLevel -= 2;
                        }
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        //DrawUI
                        if (cm != null)
                            DrawUI(cm);
                    }
                }
            }
            else if (familyMenuItemActive)
			{
				List<GUIContent> flabels = new List<GUIContent>();
				
				// If at least one family is available
				if (mm.availableFamilies.Count <= 0)
                    EditorGUILayout.LabelField("No family found");
                else
                {
                    FamilyMonitoring fm = null;
                    // compute template list for filter by Petri net
                    templates_id = new List<GUIContent>();
                    templates_id.Add(new GUIContent("<No filter>"));
                    foreach (string name in mm.PetriNetsName)
                        templates_id.Add(new GUIContent(name));
                    // Display filter 
                    petriNetFilter = EditorGUILayout.Popup(new GUIContent("Petri net Filter:", "Filter monitors by Full Petri nets (This list is defined in MonitoringManager component)."), petriNetFilter, templates_id.ToArray());
                    if (petriNetFilter >= templates_id.Count) // can occur with Ctrl+Z
                        petriNetFilter = templates_id.Count - 1;
                    // list to store systems name
                    templates_id = new List<GUIContent>();
                    templates_id.Add(new GUIContent("<No filter>"));
                    // Compute template list for filter by systems' name
                    // Parse all available families
                    foreach (MonitoringManager.FamilyAssociation fa in mm.availableFamilies)
                    {
                        // store system if it is not already included
                        if (templates_id.Find(x => x.text.StartsWith(fa.systemName)) == null)
                            templates_id.Add(new GUIContent(fa.systemName));
                    }
                    // sort systems name list
                    templates_id.Sort(delegate (GUIContent x, GUIContent y) {
                        if (x == null && y == null)
                            return 0;
                        else if (x == null)
                            return -1;
                        else if (y == null)
                            return 1;
                        else
                            return x.text.CompareTo(y.text);
                    });
                    // Display filter 
                    systemFilter = EditorGUILayout.Popup(new GUIContent("System Filter:", "Filter family monitors by Systems' name."), systemFilter, templates_id.ToArray());
                    if (systemFilter >= templates_id.Count) // can occur with Ctrl+Z
                        systemFilter = templates_id.Count - 1;

                    // Build families label for each available families depending on system filter and petriNet filter
                    // Parse all available families
                    foreach (MonitoringManager.FamilyAssociation fa in mm.availableFamilies)
                    {
                        if (systemFilter == 0 || fa.systemName == templates_id[systemFilter].text)
                        {
                            // Try to find associated monitor to current available family
                            fm = mm.getFamilyMonitoring(fa.family);
                            if (fm == null && petriNetFilter == 0) // no monitor found
                                flabels.Add(new GUIContent(fa.systemName + "." + fa.familyName + " (Not monitored)"));
                            else
                            {
                                if (petriNetFilter == 0 || (fm != null && fm.fullPnSelected == petriNetFilter - 1))
                                {
                                    // Monitor found: we compute the number of links
                                    int cpt = 0;
                                    foreach (TransitionLink ctr in fm.transitionLinks)
                                        cpt += ctr.links.Count;
                                    // build rich label
                                    flabels.Add(new GUIContent(fa.systemName + "." + fa.familyName + " (ref: " + fm.id + ") " + (fm.PnmlFile == null ? "(PN: None" : "(PN: " + fm.PnmlFile.name) + "; Total link: " + cpt + ")"));
                                    // if we found a monitor with the same id of the previous selected, we focus on it
                                    if (fa.systemName + "." + fa.familyName == oldFamilyName)
                                        familySelectedFlag = flabels.Count - 1;
                                }
                            }
                        }
                    }

                    if (flabels.Count <= 0)
                        EditorGUILayout.LabelField("No family found");
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 125;
                        familySelectedFlag = EditorGUILayout.Popup(new GUIContent("Select a family:", "Select the monitor you want to configure."), familySelectedFlag, flabels.ToArray());
                        if (familySelectedFlag >= flabels.Count) // can occur with Ctrl+Z
                            familySelectedFlag = flabels.Count - 1;

                        // Extract from selected label data useful to identify family
                        string[] stringArray = flabels[familySelectedFlag].text.Split('.');
                        // the system name is before the last '.'
                        string systemName = stringArray[0];
                        for (int i = 1; i < stringArray.Length - 1; i++)
                            systemName += "." + stringArray[i];
                        string familyName = stringArray[stringArray.Length -1].Split(' ')[0]; // the family name is between the last '.' and before the following space
                        oldFamilyName = systemName+"."+familyName;
                        // Try to get back family based on extracted data
                        MonitoringManager.FamilyAssociation _fa = mm.availableFamilies.Find(x => x.systemName == systemName && x.familyName == familyName);
                        if (_fa == null)
                        {
                            EditorGUILayout.EndHorizontal();
                            GUIStyle skin = new GUIStyle(GUI.skin.label);
                            skin.normal.textColor = new Color(1f, 0.2f, 0.2f, 1); // soft red
                            EditorGUILayout.LabelField("Warning, unable to find family", skin);
                            return;
                        }
                        // Try to get monitor associated to this family
                        fm = mm.getFamilyMonitoring(_fa.family);

                        // If found, add button to remove monitoring
                        if (fm != null)
                        {
                            if (GUILayout.Button(new GUIContent("X", "Remove monitor from this family."), GUILayout.Width(20)))
                            {
                                // in case of monitor is not active in hierarchy OnDestroy of FamilyMonitoring will not be called => then we force to free unique id
                                if (!fm.gameObject.activeInHierarchy)
                                    fm.freeUniqueId();
                                Undo.DestroyObjectImmediate(fm.gameObject);
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (fm == null)
                        {
                            // if Not found, add button to add a monitor to this family
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                            if (GUILayout.Button("Add monitor to this family"))
                            {
                                GameObject go = new GameObject(_fa.equivWith);
                                FamilyMonitoring newMonitor = go.AddComponent<FamilyMonitoring>();
                                newMonitor.equivalentName = _fa.equivWith;
                                newMonitor.descriptor = _fa.family.getDescriptor();
                                go.transform.parent = mainLoop.transform;
                                //go.GetComponent<FamilyMonitoring>().hideFlags = HideFlags.HideInInspector;
                                Undo.RegisterCreatedObjectUndo(go, "Add monitor to family");
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            showOptions = EditorGUILayout.Foldout(showOptions, "Options");
                            if (showOptions)
                            {
                                // Compute template list for import mecanism
                                templates_id = new List<GUIContent>();
                                List<FamilyMonitoring> templates_fm = new List<FamilyMonitoring>();
                                foreach (MonitoringManager.FamilyAssociation fa in mm.availableFamilies)
                                {
                                    // Try to find associated monitor to current available family
                                    FamilyMonitoring fm_template = mm.getFamilyMonitoring(fa.family);
                                    if (fm_template != null && fm_template.equivalentName != fm.equivalentName && fm_template.PnmlFile != null)
                                    {
                                        // Monitor found: we compute the number of links
                                        int cpt = 0;
                                        foreach (TransitionLink ctr in fm_template.transitionLinks)
                                            cpt += ctr.links.Count;
                                        // build rich label
                                        templates_id.Add(new GUIContent(fa.systemName + "." + fa.familyName + " (ref: " + fm_template.id + ") (PN: " + fm_template.PnmlFile.name + "; Total link: " + cpt + ")"));
                                        templates_fm.Add(fm_template);
                                    }
                                }

                                EditorGUI.indentLevel += 2;
                                EditorGUIUtility.labelWidth = 160;
                                if (templates_id.Count > 0)
                                {
                                    if (templateSelected >= templates_id.Count)
                                        templateSelected = templates_id.Count - 1;
                                    EditorGUILayout.BeginHorizontal();
                                    templateSelected = EditorGUILayout.Popup(new GUIContent("Import from model:", "Select the monitor you want to use as template, all properties will be copied to the editing monitor."), templateSelected, templates_id.ToArray());
                                    if (GUILayout.Button("Import", GUILayout.Width(80)))
                                    {
                                        // clone from template
                                        fm.clone((ComponentMonitoring)templates_fm[templateSelected]);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                // Be sure that Petri net selected is not over the MonitoringManager list
                                if (fm.fullPnSelected >= mm.PetriNetsName.Count)
                                    fm.fullPnSelected = 0;
                                templates_id = new List<GUIContent>();
                                foreach (string name in mm.PetriNetsName)
                                    templates_id.Add(new GUIContent(name));
                                int pnSelected = EditorGUILayout.Popup(new GUIContent("Affect to Full Petri net:", "Add this monitor to the selected full Petri net. This list is defined in MonitoringManager component."), fm.fullPnSelected, templates_id.ToArray());
                                if (pnSelected != fm.fullPnSelected)
                                {
                                    Undo.RecordObject(fm, "Update Export Petri net");
                                    fm.fullPnSelected = pnSelected;
                                }
                                EditorGUIUtility.labelWidth = 125;
                                EditorGUI.indentLevel -= 2;
                            }
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                            DrawUI(fm);
                        }
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
                flagTransition = 0;

	        }

	        if (GUILayout.Button("Families", familyMenuItemActive ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
	        {
	            goMenuItemActive = false;
	            familyMenuItemActive = true;
                flagTransition = 0;
	        }

	        GUILayout.EndHorizontal();
	        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

		private void DrawUI(ComponentMonitoring monitor)
        {
            scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);
			// Pnml input field
			UnityEngine.Object tmp = EditorGUILayout.ObjectField(new GUIContent("PNML file:", "Drag&Drop a file with .pnml extension."), monitor.PnmlFile, typeof(UnityEngine.Object), true);
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
			EditorGUILayout.EndScrollView ();
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
						EditorGUIUtility.labelWidth = 100;
						int newMarking = EditorGUILayout.IntField (new GUIContent(place.label, place.label), place.initialMarking, GUILayout.MaxWidth (125));
						EditorGUIUtility.labelWidth = 0; // reset default value
						if (newMarking != place.initialMarking) {
							Undo.RecordObject (monitor, "Update Initial state");
							place.initialMarking = newMarking;
						}
						// Add override field
						//EditorGUIUtility.labelWidth = 150;
						string newLabel = EditorGUILayout.TextField (new GUIContent("Override name:", "You can define a more explicit name (useful to edit links)."), place.overridedLabel/*, GUILayout.MaxWidth (300)*/);
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
					List<GUIContent> labelsBuilt = new List<GUIContent> ();

					foreach (TransitionLink tc in monitor.transitionLinks){
						labelsBuilt.Add (new GUIContent(tc.transition.label + " (links: " + tc.links.Count + ")"));
					}

					flagTransition = EditorGUILayout.Popup (new GUIContent("Action:", "Select action to configure."), flagTransition, labelsBuilt.ToArray ());
					if (flagTransition >= labelsBuilt.Count) // can occur with Ctrl+Z
						flagTransition = labelsBuilt.Count - 1;

					TransitionLink tLink = monitor.transitionLinks [flagTransition];
					
					// Add override field
					string newTrLabel = EditorGUILayout.TextField (new GUIContent("Override name:", "You can define a more explicit name (only used in Laalys UI)."), tLink.transition.overridedLabel);
					if (newTrLabel != tLink.transition.overridedLabel) {
						Undo.RecordObject (monitor, "Update Override Label");
						tLink.transition.overridedLabel = newTrLabel;
					}
					
					// Add check boxes
					bool newToggle = EditorGUILayout.ToggleLeft (new GUIContent("Not a player action (triggered by game simulation)", "If checked this action will be considered as a system action and will be not labelled by Laalys."), tLink.isSystemAction);
					if (newToggle != tLink.isSystemAction) {
						Undo.RecordObject (monitor, "Is Player Action");
						tLink.isSystemAction = newToggle;
					}
					newToggle = EditorGUILayout.ToggleLeft (new GUIContent("Player objective", "If check this action is a player objective. At least one action for each parent Petri net has to be set as payer objective."), tLink.isEndAction);
					if (newToggle != tLink.isEndAction) {
						Undo.RecordObject (monitor, "Is Player Objective");
						tLink.isEndAction = newToggle;
					}
					
					// Add button to add links
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Space (EditorGUI.indentLevel * 20); // Add tiny space in front of button
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
					EditorGUILayout.EndHorizontal ();
					
					// Display links
					foreach (Link link in tLink.links.ToArray()) {
						if (tLink.links.IndexOf (link) != 0)
							EditorGUILayout.Space ();
						Rect rect = EditorGUILayout.BeginHorizontal ();
						{
							rect.x = rect.x - 5 + EditorGUI.indentLevel * 20;
							rect.width = rect.width - 18 - EditorGUI.indentLevel * 20;
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
												if (!link.isCompatibleWithPnName(monitor.fullPnSelected))
													EditorUtility.DisplayDialog ("Warning!!!", "This selected state is part of a full Petri net different from the one associated to this monitor. Affect these two monitors to the same Petri net otherwise final full Petri nets building will fail.", "Close");
											}
											EditorGUIUtility.labelWidth = 0; // reset default value
											EditorGUI.indentLevel += 1;
											EditorGUILayout.EndHorizontal ();
											
											// Add third line
											EditorGUILayout.BeginHorizontal ();
											EditorGUIUtility.labelWidth = 240;
											string newLabel = EditorGUILayout.TextField ("Label for logic expression and traces:", link.label);
											if (newLabel != link.label) {
												Undo.RecordObject (monitor, "Update Link Label");
												link.label = newLabel;
											}
											EditorGUIUtility.labelWidth = 0; // reset default value
											EditorGUILayout.EndHorizontal ();
											// Check label
											GUIStyle s = new GUIStyle (GUI.skin.label);
											s.wordWrap = true;
											// Add warning if label includes "()[]*+ " token
											if (!ExpressionParser.checkPrerequisite(link.label)) {
												s.normal.textColor = Color.red;
												s.fontStyle = FontStyle.Bold;
												EditorGUILayout.LabelField ("Error! \"(\", \")\", \"[\", \"]\", \"*\", \"+\" and \" \" are not allowed into link label.", s);
											} else {
												// Add warning message if new label is not included into links logic expression
												if (tLink.logic != null && tLink.logic != "" && !tLink.logic.Contains (link.label)) {
													s.normal.textColor = new Color (0.9f, 0.5f, 0.1f, 1); // orange
													s.fontStyle = FontStyle.Bold;
													EditorGUILayout.LabelField ("Warning! This label is not currently included into \"Links logic expression\" field. If so, this link will not be used in monitoring process.", s);
												} else {
													// Add warning if two links have the same label
													bool unique = true;
													foreach (Link l in tLink.links)
														if (l != link && l.label == link.label)
															unique = false;
													if (!unique) {
														s.normal.textColor = Color.red;
														s.fontStyle = FontStyle.Bold;
														EditorGUILayout.LabelField ("Warning! Two links have the same label. This is ambiguous in \"Links logic expression\" field.", s);
													} else {
														// Add blank area to avoid flickering on editing labels in case of warning (lost of focus)
														EditorGUILayout.LabelField ("");
													}
												}
											}
										} else {
											EditorGUILayout.LabelField("No Petri Net attached to the chosen game object.");
										}
									} else {
										EditorGUILayout.LabelField ("The game object chosen is not monitored.");
									}
								}
							}
							EditorGUILayout.EndVertical ();
							
							if (GUILayout.Button (new GUIContent("X", "Remove this link."), GUILayout.Width (20))) {
								Undo.RecordObject (monitor, "Delete link");
								tLink.links.Remove (link);
							}
						}
						EditorGUILayout.EndHorizontal ();
					}

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
						if (ExpressionParser.isValid (tLink)) {
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
	}
}