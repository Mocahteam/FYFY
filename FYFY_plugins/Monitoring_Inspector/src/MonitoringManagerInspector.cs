using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using FYFY;

namespace FYFY_plugins.Monitoring {
	/// <summary>
	/// 	Custom inspector automatically used by Unity Editor when a <see cref="FYFY_plugins.Monitoring.MonitoringManager"/> is found in inspector view.
	/// </summary>
	[CustomEditor(typeof(FYFY_plugins.Monitoring.MonitoringManager))]
	public class MonitoringManagerInspector : Editor {

		private SerializedProperty _pnName;	
		private SerializedProperty _inGameAnalysis;
        private SerializedProperty _debugLogs;
        private SerializedProperty _fullPetriNetsPath;	
		private SerializedProperty _filteredPetriNetsPath;	
		private SerializedProperty _featuresPath;		
		private SerializedProperty _laalysPath;

		private void OnEnable(){
			_pnName = serializedObject.FindProperty ("PetriNetsName");
			_inGameAnalysis = serializedObject.FindProperty ("inGameAnalysis");
            _debugLogs = serializedObject.FindProperty("debugLogs");
            _fullPetriNetsPath = serializedObject.FindProperty ("fullPetriNetsPath");
			_filteredPetriNetsPath = serializedObject.FindProperty ("filteredPetriNetsPath");
			_featuresPath = serializedObject.FindProperty ("featuresPath");
			_laalysPath = serializedObject.FindProperty ("laalysPath");
		}
		
		private void OnDestroy(){
			if (EditionView.window != null){
				EditionView.window.Repaint();
			}
		}

		/// <summary></summary>
		public override void OnInspectorGUI(){
			serializedObject.Update();

			MonitoringManager mm = (MonitoringManager)target;
			
			// Guarantee that the list include at least one parameter with the scene name
			EditorGUILayout.PropertyField(_pnName, new GUIContent("Petri Nets Name", "Add names to this list to define available Full Petri nets name. The first element of this list is reserved to the scene name."), true);
			if (mm.PetriNetsName.Count <= 0)
				mm.PetriNetsName.Add(SceneManager.GetActiveScene().name);
			if (mm.PetriNetsName[0] != SceneManager.GetActiveScene().name)
				mm.PetriNetsName[0] = SceneManager.GetActiveScene().name;
			// Check that no elements equal the first one or equal ""
			for(int i = 1 ; i < mm.PetriNetsName.Count ; i++)
				if (mm.PetriNetsName[0] == mm.PetriNetsName[i] || mm.PetriNetsName[i] == "")
					mm.PetriNetsName[i] = "Unnamed_"+i;
			
			if (GUILayout.Button ("Build full Petri nets and features"))
				GeneratePNandFeatures ();
			
			bool newInGameAnalysis = EditorGUILayout.ToggleLeft (new GUIContent("Enable in game analysis", "If enabled, Laalys will be launched in game and each game action traced will be analysed depending on Full and Filtered Petri nets and features."), _inGameAnalysis.boolValue);
			if (newInGameAnalysis != _inGameAnalysis.boolValue) {
				Undo.RecordObject (mm, "Update In Game Analysis");
				_inGameAnalysis.boolValue = newInGameAnalysis;
            }

            if (!newInGameAnalysis)	GUI.enabled = false; // disable following fields if in game analysis is disabled
            bool newDebugLogs = EditorGUILayout.ToggleLeft(new GUIContent("Enable debug logs", "If enabled, Laalys will print debug logs."), _debugLogs.boolValue);
            if (newDebugLogs != _debugLogs.boolValue)
            {
                Undo.RecordObject(mm, "Update Debug Logs");
                _debugLogs.boolValue = newDebugLogs;
            }
            string newFullPath = EditorGUILayout.TextField (new GUIContent("Full P. nets path:", "The full Petri nets location to use in case of in game analysis is enabled (default: \"./completeNets/\")."), _fullPetriNetsPath.stringValue);
			if (newFullPath != _fullPetriNetsPath.stringValue) {
				Undo.RecordObject (mm, "Update Full Petri nets path");
				_fullPetriNetsPath.stringValue = newFullPath;
			}
			string newFilteredPath = EditorGUILayout.TextField (new GUIContent("Filtered P. nets path:", "The filtered Petri nets location to use in case of in game analysis is enabled (default: \"./filteredNets/\")."), _filteredPetriNetsPath.stringValue);
			if (newFilteredPath != _filteredPetriNetsPath.stringValue) {
				Undo.RecordObject (mm, "Update Filtered Petri nets path");
				_filteredPetriNetsPath.stringValue = newFilteredPath;
			}
			string newFeaturesPath = EditorGUILayout.TextField (new GUIContent("Features path:", "The features location to use in case of in game analysis is enabled (default: \"./features/\")."), _featuresPath.stringValue);
			if (newFeaturesPath != _featuresPath.stringValue) {
				Undo.RecordObject (mm, "Update Features path");
				_featuresPath.stringValue = newFeaturesPath;
			}
			if (!newInGameAnalysis)	GUI.enabled = true; // reset default

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			string newLaalysPath = EditorGUILayout.TextField (new GUIContent("Laalys path:", "The path to the Laalys jar file."), _laalysPath.stringValue);
			if (newLaalysPath != _laalysPath.stringValue) {
				Undo.RecordObject (mm, "Update Laalys path");
				_laalysPath.stringValue = newLaalysPath;
			}

			if (GUILayout.Button ("Launch Laalys")) {
				Process LaalysProcess = new Process ();
				LaalysProcess.StartInfo.FileName = "java.exe";
				LaalysProcess.StartInfo.Arguments = "-jar "+_laalysPath.stringValue;
				LaalysProcess.Start();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void GeneratePNandFeatures () {
			// List of full Petri nets
			// Key => Petri net name defined inside monitor.fullPn
			// Value => Pair <PetriNet, offsetX>
			Dictionary<string, KeyValuePair<PetriNet, float>> petriNets = new Dictionary <string, KeyValuePair<PetriNet, float>>();
			
			bool abort = false;
			MonitoringManager mm = (MonitoringManager)target;

			// Fill final PN
			foreach (ComponentMonitoring monitor in Resources.FindObjectsOfTypeAll<ComponentMonitoring> ()) {
				// Get full Petri net for this monitor
				if (monitor.fullPnSelected >= mm.PetriNetsName.Count)
					monitor.fullPnSelected = 1;
				string fullName = mm.PetriNetsName[monitor.fullPnSelected];
				if (!petriNets.ContainsKey(fullName))
					petriNets[fullName] = new KeyValuePair<PetriNet, float>(new PetriNet(), 0);
				PetriNet fullPn = petriNets[fullName].Key;
				float offsetX = petriNets[fullName].Value;
				
				// Check if PN exists
				if (monitor.PetriNet != null && !abort) {
					// Make a copy of local PN in order to organise it spatially without changing original PN
					PetriNet tmpPN = new PetriNet(monitor.PetriNet, monitor.gameObject.name);
					tmpPN.addWidth (offsetX);
					fullPn.addSubNet (tmpPN);

					// Process links
					foreach (TransitionLink transitionLink in monitor.transitionLinks)
					{
						// Check if all links target the same fullPn
						foreach(Link curLink in transitionLink.links){
							if (!curLink.isCompatibleWithPnName(monitor.fullPnSelected)){
								abort = true;
								EditorUtility.DisplayDialog ("Error, building aborted", "The action \""+transitionLink.transition.label+"\" of monitor \""+monitor.gameObject.name+" (ref: "+monitor.id+")\" is linked with GameObject \""+curLink.linkedObject.name+"\" (label: \""+curLink.label+"\"). But these two elements are not affected to the same full Petri net.\n\nPlease review your configurations and try again.", "Close");
							}
						}
						
						// Make a copy of current transition and prefix its name with its game object name
						Node curTransition_copy = new Node(transitionLink.transition);
						string publicLabel = curTransition_copy.label+" ";
						if (curTransition_copy.overridedLabel != null && !curTransition_copy.overridedLabel.Equals(""))
							publicLabel = curTransition_copy.overridedLabel+" ";
						if (monitor is FamilyMonitoring)
							publicLabel = publicLabel+((FamilyMonitoring)monitor).equivalentName;
						else
							publicLabel = publicLabel+monitor.gameObject.name;
						curTransition_copy.label = monitor.gameObject.name+"_"+curTransition_copy.label;
						
						// Add this transition to Features
						XmlHandler.addFeature(fullName, curTransition_copy.label+"_"+monitor.id, publicLabel, transitionLink.isSystemAction, transitionLink.isEndAction);
						
						Node oldTransition = curTransition_copy; 
						if (isNullOrWhiteSpace(transitionLink.logic))
						{
							// Default : And of all link
							foreach(Link curLink in transitionLink.links)
							{
								if (curLink.linkedObject != null) {
									// Make a copy of linked place and prefix its name with its game object name
									Node linkedPlace = curLink.getPlace ();
									if (linkedPlace != null) {
										Node linkedPlace_copy = new Node (linkedPlace);
										linkedPlace_copy.label = curLink.linkedObject.name + "_" + linkedPlace_copy.label;
										// Define arc type
										ArcType arcType = curLink.type == 2 ? Arc.stringToArcType (Arc.optType.ElementAt (curLink.flagsType)) : ArcType.regular;
										// Create arc between Transition and linked place (depends on Get/Produce/Require diffusion state)
										fullPn.arcs.Add (curLink.type != 1 ? new Arc (linkedPlace_copy, curTransition_copy, arcType, curLink.weight) : new Arc (curTransition_copy, linkedPlace_copy, arcType, curLink.weight));
									}
								}
							}
						}
						else
						{
							if (ExpressionParser.isValid(transitionLink))
							{
								// Logic expression is valid

								// Distribute expression
								string[] exp = ExpressionParser.getDistribution(transitionLink.logic);

								int or = 0;

								// Parse distributed expression
								foreach (string token in exp)
								{
									// Check if current token is an operator
									if (!token.Equals("+") && !token.Equals("*"))
									{
										// It's not an operator => we load the link
										Link curLink = transitionLink.getLabeledLink(token);
										if (curLink.linkedObject != null) {
											// Make a copy of linked place and prefix its name with its game object name
											Node linkedPlace = curLink.getPlace ();
											if (linkedPlace != null) {
												Node linkedPlace_copy = new Node (linkedPlace);
												linkedPlace_copy.label = curLink.linkedObject.name + "_" + linkedPlace_copy.label;
												// Define arc type
												ArcType arcType = curLink.type == 2 ? Arc.stringToArcType (Arc.optType.ElementAt (curLink.flagsType)) : ArcType.regular;
												// Create arc between Transition and linked place (depends on Get/Produce/Require diffusion state)
												fullPn.arcs.Add (curLink.type != 1 ? new Arc (linkedPlace_copy, curTransition_copy, arcType, curLink.weight) : new Arc (curTransition_copy, linkedPlace_copy, arcType, curLink.weight));
											}
										}
									}
									else if (token.Equals("+"))
									{
										// We detect OR operator => add a new transition and set it as current node

										// Build new transition, we keep old transition to build links after
										// Add offset to position
										curTransition_copy.position.x += offsetX;
										curTransition_copy.position.y += 50;
										curTransition_copy = new Node("or" + (or++) + "_" + oldTransition.label, curTransition_copy.id, curTransition_copy.offset, curTransition_copy.initialMarking, curTransition_copy.position);
										// Add this new transition to PN
										fullPn.transitions.Add(curTransition_copy);
										// and to features
										XmlHandler.addFeature(fullName, curTransition_copy.label+"_"+monitor.id, publicLabel, transitionLink.isSystemAction, transitionLink.isEndAction);
										// Duplicate arcs from old transition
										foreach (Arc a in tmpPN.getConcernedArcs(oldTransition))
										{
											if (a.target.label.Equals(oldTransition.label))
											{
												fullPn.arcs.Add(new Arc(a.source, curTransition_copy, a.type, a.weight));
											}
											else if (a.source.label.Equals(oldTransition.label))
											{
												fullPn.arcs.Add(new Arc(curTransition_copy, a.target, a.type, a.weight));
											}
										}
									}
								}
							}
							else
								UnityEngine.Debug.LogError("Petri Net Building aborted: Logic expression of \""+transitionLink.transition.label+"\" action from \""+monitor.gameObject.name+"\" is not valid => \""+transitionLink.logic+"\". Please check it from Monitor edit view.");
						}
					}
					petriNets[fullName] = new KeyValuePair<PetriNet, float>(fullPn, offsetX + monitor.PetriNet.getWidth ()+50); // Add spaces between PN
				}
			}

			if (!abort){
				// Save all Petri nets
				foreach (KeyValuePair<string, KeyValuePair<PetriNet, float>> pn in petriNets)
					PnmlParser.SaveAtPath (pn.Value.Key, pn.Key+".pnml");
				// and features
				XmlHandler.saveFeatures();

				EditorUtility.DisplayDialog ("Files built", "Files have been saved into \"completeNets\" and \"features\" folders of this Unity project", "Close");
			}

		}

		private static bool isNullOrWhiteSpace(string str)
		{
			return string.IsNullOrEmpty(str) || onlySpaces(str);
		}

		private static bool onlySpaces(string str)
		{
			foreach(char c in str)
			{
				if (!char.IsWhiteSpace(c))
					return false;
			}
			return true;
		}
	}
}