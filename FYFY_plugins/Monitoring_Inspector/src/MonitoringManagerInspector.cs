using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
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

		private SerializedProperty _fileName;
		private SerializedProperty _inGameAnalysis;	
		private SerializedProperty _fullPetriNetPath;	
		private SerializedProperty _filteredPetriNetPath;	
		private SerializedProperty _featuresPath;		
		private SerializedProperty _laalysPath;

		private void OnEnable(){
			_fileName = serializedObject.FindProperty ("fileName");
			_inGameAnalysis = serializedObject.FindProperty ("inGameAnalysis");
			_fullPetriNetPath = serializedObject.FindProperty ("fullPetriNetPath");
			_filteredPetriNetPath = serializedObject.FindProperty ("filteredPetriNetPath");
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

			string newFileName = EditorGUILayout.TextField (new GUIContent("Base files name:", "This base file name will be used to build full Petri net file with .pnml extension and specifications file with .xml extension. This files will be saved into \"completeNet\" and \"specifs\" folders of the Unity project."), _fileName.stringValue);
			if (newFileName != _fileName.stringValue) {
				Undo.RecordObject ((MonitoringManager)target, "Update Base files name");
				_fileName.stringValue = newFileName;
			}

			if (newFileName == null || newFileName == "") GUI.enabled = false; // disable following fields if in game analysis is disabled
			if (GUILayout.Button ("Build full Petri net and features"))
				GeneratePNandSpecifs (newFileName);
			if (newFileName == null || newFileName == "")	GUI.enabled = true; // reset default

			bool newInGameAnalysis = EditorGUILayout.ToggleLeft (new GUIContent("Enable in game analysis", "If enabled, Laalys will be launched in game and each game action traced will be analysed depending on Full and Filtered Petri nets and spécifications."), _inGameAnalysis.boolValue);
			if (newInGameAnalysis != _inGameAnalysis.boolValue) {
				Undo.RecordObject ((MonitoringManager)target, "Update In Game Analysis");
				_inGameAnalysis.boolValue = newInGameAnalysis;
			}

			if (!newInGameAnalysis)	GUI.enabled = false; // disable following fields if in game analysis is disabled
			string newFullPath = EditorGUILayout.TextField (new GUIContent("Full Petri net path:", "The full Petri net to use in case of in game analysis is enabled."), _fullPetriNetPath.stringValue);
			if (newFullPath != _fullPetriNetPath.stringValue) {
				Undo.RecordObject ((MonitoringManager)target, "Update Full Petri net path");
				_fullPetriNetPath.stringValue = newFullPath;
			}
			string newFilteredPath = EditorGUILayout.TextField (new GUIContent("Filtered Petri net path:", "The filtered Petri net to use in case of in game analysis is enabled."), _filteredPetriNetPath.stringValue);
			if (newFilteredPath != _filteredPetriNetPath.stringValue) {
				Undo.RecordObject ((MonitoringManager)target, "Update Filtered Petri net path");
				_filteredPetriNetPath.stringValue = newFilteredPath;
			}
			string newFeaturesPath = EditorGUILayout.TextField (new GUIContent("Specifications path:", "The specifications to use in case of in game analysis is enabled."), _featuresPath.stringValue);
			if (newFeaturesPath != _featuresPath.stringValue) {
				Undo.RecordObject ((MonitoringManager)target, "Update Specifications path");
				_featuresPath.stringValue = newFeaturesPath;
			}
			if (!newInGameAnalysis)	GUI.enabled = true; // reset default

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			string newLaalysPath = EditorGUILayout.TextField (new GUIContent("Laalys path:", "The path to the Laalys jar file."), _laalysPath.stringValue);
			if (newLaalysPath != _laalysPath.stringValue) {
				Undo.RecordObject ((MonitoringManager)target, "Update Laalys path");
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

		private void GeneratePNandSpecifs (string baseFileName) {
			// Build final PetriNet
			PetriNet petriNet = new PetriNet ();

			float offsetX = 0;

			// Fill final PN
			foreach (ComponentMonitoring monitor in Resources.FindObjectsOfTypeAll<ComponentMonitoring> ()) {
				// Check if PN exists
				if (monitor.PetriNet != null) {
					// Make a copy of local PN in order to organise it spatially without changing original PN
					PetriNet tmpPN = new PetriNet(monitor.PetriNet, monitor.gameObject.name);
					tmpPN.addWidth (offsetX);
					petriNet.addSubNet (tmpPN);

					// Process links
					foreach (TransitionLink transitionLink in monitor.transitionLinks)
					{
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
						// Add this transition to Specifications
						XmlHandler.addSpecif(curTransition_copy.label+"_"+monitor.id, publicLabel, transitionLink.isSystemAction, transitionLink.isEndAction);
						Node oldTransition = curTransition_copy;
						if (isNullOrWhiteSpace(transitionLink.logic))
						{
							// Default : And of all link
							foreach(Link curLink in transitionLink.links)
							{
								if (curLink.linkedObject != null) {
									// Make a copy of linked place and prefix its name with its game object name
									Node linkedPlace = curLink.getPlaceFromLinkedObject (curLink.placeId);
									if (linkedPlace != null) {
										Node linkedPlace_copy = new Node (linkedPlace);
										linkedPlace_copy.label = curLink.linkedObject.name + "_" + linkedPlace_copy.label;
										// Define arc type
										ArcType arcType = curLink.type == 2 ? Arc.stringToArcType (Arc.optType.ElementAt (curLink.flagsType)) : ArcType.regular;
										// Create arc between Transition and linked place (depends on Get/Produce/Require diffusion state)
										petriNet.arcs.Add (curLink.type != 1 ? new Arc (linkedPlace_copy, curTransition_copy, arcType, curLink.weight) : new Arc (curTransition_copy, linkedPlace_copy, arcType, curLink.weight));
									}
								}
							}
						}
						else
						{
							ExpressionParser expr_parser = new ExpressionParser();
							if (expr_parser.isValid(transitionLink))
							{
								// Logic expression is valid

								// Distribute expression
								string[] exp = expr_parser.getDistribution(transitionLink.logic);

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
											Node linkedPlace = curLink.getPlaceFromLinkedObject (curLink.placeId);
											if (linkedPlace != null) {
												Node linkedPlace_copy = new Node (linkedPlace);
												linkedPlace_copy.label = curLink.linkedObject.name + "_" + linkedPlace_copy.label;
												// Define arc type
												ArcType arcType = curLink.type == 2 ? Arc.stringToArcType (Arc.optType.ElementAt (curLink.flagsType)) : ArcType.regular;
												// Create arc between Transition and linked place (depends on Get/Produce/Require diffusion state)
												petriNet.arcs.Add (curLink.type != 1 ? new Arc (linkedPlace_copy, curTransition_copy, arcType, curLink.weight) : new Arc (curTransition_copy, linkedPlace_copy, arcType, curLink.weight));
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
										petriNet.transitions.Add(curTransition_copy);
										// and to specifications
										XmlHandler.addSpecif(curTransition_copy.label+"_"+monitor.id, publicLabel, transitionLink.isSystemAction, transitionLink.isEndAction);
										// Duplicate arcs from old transition
										foreach (Arc a in tmpPN.getConcernedArcs(oldTransition))
										{
											if (a.target.label.Equals(oldTransition.label))
											{
												petriNet.arcs.Add(new Arc(a.source, curTransition_copy, a.type, a.weight));
											}
											else if (a.source.label.Equals(oldTransition.label))
											{
												petriNet.arcs.Add(new Arc(curTransition_copy, a.target, a.type, a.weight));
											}
										}
									}
								}
							}
							else
								UnityEngine.Debug.LogError("Petri Net Building aborted: Logic expression of \""+transitionLink.transition.label+"\" action from \""+monitor.gameObject.name+"\" is not valid => \""+transitionLink.logic+"\". Please check it from Monitor edit view.");
						}
					}
					offsetX += monitor.PetriNet.getWidth ()+50; // Add spaces between PN
				}
			}

			PnmlParser.SaveAtPath (petriNet, baseFileName+".pnml");
			XmlHandler.saveSpecifications(baseFileName);

			EditorUtility.DisplayDialog ("Files built", "Files have been saved into \"completeNet\" and \"specifs\" folders of this Unity project", "Close");

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