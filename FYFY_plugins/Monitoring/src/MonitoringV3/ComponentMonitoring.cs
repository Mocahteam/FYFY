using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


namespace FYFY_plugins.Monitoring{
	/// <summary>
	/// 	Add monitoring functionalities to a Game Object
	/// </summary>
	[Serializable]
	[ExecuteInEditMode]
    public class ComponentMonitoring : MonoBehaviour
    {
        
		/// <summary> Pnml File associated to the monitor </summary>
		[HideInInspector]
		public UnityEngine.Object PnmlFile;

		/// <summary> Comments of this monitor </summary>
		[HideInInspector]
		public string comments;

		/// <summary> Component id </summary>
		[HideInInspector]
		public int id = -1;

		[HideInInspector]
		[SerializeField]
		private PetriNet petriNet;

		/// <summary> List of transitions influenced by links. </summary>
		[HideInInspector] 
		public List<TransitionLink> transitionLinks = new List<TransitionLink>();
		
		/// <summary> PetriNet getter and setter. Set the PetriNet implies reset transitionLinks. </summary>
		[HideInInspector]
		public PetriNet PetriNet{
			get { return petriNet; }
			set {
				petriNet = value;
				if (value == null) {
					transitionLinks.Clear ();
				} else {
					// Reset transitionLinks
					transitionLinks.Clear ();
					foreach (Node transition in petriNet.transitions)
					{
						TransitionLink t = new TransitionLink();
						t.transition = transition;
						transitionLinks.Add(t);
					}
				}
			}
		}

		/// <summary> Look for a transition matching with label influenced by links </summary>
		/// <param name="label">The label of the transition to find.</param>
		/// <return> If a transition with appropriate label exists, returns this transition and links. Returns null otherwise. </return>
        public TransitionLink getTransitionLinkByTransitionLabel(String label)
        {
            foreach(TransitionLink tc in transitionLinks)
            {
                if (tc.transition.label.Equals(label))
                    return tc;
            }
            return null;
        }

		/// <summary>
		/// This function enables you to match Petri Net initial marking with families content on start. Use this function only into systems' constructor.
		/// </summary>
		/// <param name="stateName">State name you want to initialize, this name has to match with a place defined into associated Petri Net <see cref="PnmlFile"/>.</param>
		/// <param name="initialValue">The initial positive or null value.</param>
		public void initState (string stateName, uint initialValue){
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called

			if (petriNet != null) {
				Node place = petriNet.getPlaceByName (name);
				if (place != null)
					place.initialMarking = (int)initialValue;
				else {
					throw new InitFailed("Place \"" + name + "\" is not known into associated Petri Net.", exceptionStackTrace);
				}
			} else {
				throw new InitFailed("No Petri Net defined.", exceptionStackTrace);
			}
		}
		/// <summary>
		/// 	Trace game action.
		/// </summary>
		/// <param name="actionName">Action name you want to trace, this name has to match with a transition defined into associated Petri Net <see cref="PnmlFile"/>.</param>
		/// <param name="performedBy">Specify who perform this action, the player or the system. <see cref="MonitoringManager.Source"/></param>
		/// <param name="processLinks">Set to false if the logic expression associated to the action include "+" operators and the action performed by the player is not allowed by the system. In this case fourth parameters will not be processed. True (default) means fourth parameter will be analysed.</param>
		/// <param name="linksConcerned">links label concerned by this action. Very important if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to build the trace: l0 and l3 OR l1 and l3 => <code>this.trace(..., "l0", "l3");</code> OR <code>this.trace(..., "l1", "l3");</code></param>
		/// <return> labels found for this game action if in game analysis is enabled (see: MonitoringManager). return empty Array else </return>
		public string[] trace(string actionName, string performedBy, bool processLinks = true, params string[] linksConcerned)
		{
			string[] labels = new string[] {};

			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called

			TransitionLink transitionLink  = getTransitionLinkByTransitionLabel(actionName);
			if (transitionLink != null) {
				ExpressionParser exp_parser = new ExpressionParser ();
				string logic = transitionLink.logic;
				// Check logic expression
				if (exp_parser.isValid (transitionLink)) {
					string[] exp = exp_parser.getDistribution (logic);
					List<string> groupLinksByAnd = new List<string> ();
					List<List<string>> groupAndByOr = new List<List<string>> ();
					foreach (string s in exp) {
						if (!s.Equals ("+") && !s.Equals ("*"))
							groupLinksByAnd.Add (s);
						else if (s.Equals ("+")) {
							groupAndByOr.Add (groupLinksByAnd);
							groupLinksByAnd = new List<string> ();
						}
					}
					groupAndByOr.Add (groupLinksByAnd);

					// If we have to process links and linksConcerned is empty and we have at least one OR statement into logic expression (i.e. at least 2 AND groups) => problem, developer has to specify the set of links concerned by this transition.
					if (processLinks && linksConcerned.Length == 0 && groupAndByOr.Count > 1) {
						string availableCombination = "\nAvailable combination of links:\n";
						foreach (List<string> ands in groupAndByOr) {
							availableCombination = availableCombination + " -";
							foreach (string token in ands) {
								availableCombination = availableCombination + " " + token;
							}
							availableCombination = availableCombination + "\n";
						}
						throw new TraceAborted ("Distributed logic expression for \"" + actionName + "\" action in \"" + this.gameObject.name + "\" Game Object contains \"+\" operator. You have to specify which links are concerned to perform this game action. " + availableCombination, exceptionStackTrace);
					} else {
						string prefix = this.gameObject.name + "_";
						bool linksFound = false;
						if (groupAndByOr.Count <= 1)
							// If logic expression is empty or contains only AND operators, linksConcerned parameter is not useful because there is no ambiguity on this transition.
							linksFound = true;
						else {
							if (!processLinks){
								// Developer wants to ignore linksConcerned. So we trace the first one (From Laalys point of view this is not a problem because all or actions have the same public name).
								prefix = "or0_"+prefix;
								linksFound = true;
							} else {
								// Look for links concerned into distributed logic expression
								List<string> linksConcerned_sorted = linksConcerned.ToList ();
								linksConcerned_sorted.Sort ();
								for (int i = 0; i < groupAndByOr.Count; i++) {
									groupAndByOr [i].Sort ();

									if (groupAndByOr [i].SequenceEqual (linksConcerned_sorted)) {
										if (i > 0)
											prefix = "or" + (i - 1) + "_"+prefix;
										linksFound = true;
										break;
									}
								}
							}
						}

						if (linksFound) {
							labels = MonitoringManager.processTrace (prefix + actionName + "_" + this.id, performedBy);
						} else {
							string debug = "";
							foreach (string link in linksConcerned)
								debug = debug + " \"" + link + "\"";

							string availableCombination = "\nAvailable combination of links:\n";
							foreach (List<string> ands in groupAndByOr) {
								availableCombination = availableCombination + " -";
								foreach (string token in ands) {
									availableCombination = availableCombination + " " + token;
								}
								availableCombination = availableCombination + "\n";
							}
							throw new TraceAborted (debug + " not found into distributed logic expression for \"" + actionName + "\" action in \"" + this.gameObject.name + "\" Game Object. " + availableCombination, exceptionStackTrace);
						}
					}
				} else {
					throw new TraceAborted ("Logic expression for \"" + actionName + "\" action in \"" + this.gameObject.name + "\" Game Object is not valid.", exceptionStackTrace);
				}
			} else {
				throw new TraceAborted ("Action \"" + actionName + "\" is not monitored by \"" + this.gameObject.name + "\" Game Object.", exceptionStackTrace);
			}
			return labels;
		}

		void Awake(){
			if (id == -1 || !IDGenerator.isUnique (this)) {
				// get new unique id
				id = IDGenerator.genID();
				if (petriNet != null)
					// update id of petrinet
					petriNet.attachID(id);
			}
		}
    }
}