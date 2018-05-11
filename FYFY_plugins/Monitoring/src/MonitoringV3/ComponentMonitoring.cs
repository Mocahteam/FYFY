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
        private TransitionLink getTransitionLinkByTransitionLabel(String label)
        {
            foreach(TransitionLink tc in transitionLinks)
            {
                if (tc.transition.label.Equals(label))
                    return tc;
            }
            return null;
        }
		
		/// <summary>
		/// 	Trace game action.
		/// </summary>
		/// <param name="actionName">Action name you want to trace, this name has to match with a transition defined into associated Petri Net <see cref="PnmlFile"/>.</param>
		/// <param name="performedBy">Specify who perform this action, the player or the system. <see cref="MonitoringManager.Source"/></param>
		/// <param name="processLinks">Set to false if the logic expression associated to the action include "+" operators AND the action performed by the player is not allowed by the system. In this case fourth parameters will not be processed. True (default) means fourth parameter will be analysed.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to build the trace: l0 and l3 OR l1 and l3 => <code>this.trace(..., "l0", "l3");</code> OR <code>this.trace(..., "l1", "l3");</code></param>
		/// <return> labels found for this game action if in game analysis is enabled (see: MonitoringManager). return empty Array else </return>
		public string[] trace(string actionName, string performedBy, bool processLinks = true, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called

			string internalName = getInternalName(actionName, exceptionStackTrace, processLinks, linksConcerned);
			return MonitoringManager.processTrace (internalName, performedBy);
		}
		
		/// <summary>
		/// 	Get next actions to perform in order to reach targeted game action.
		/// </summary>
		/// <param name="targetedActionName">Action name you want to reach, this name has to match with a transition defined into associated Petri Net <see cref="PnmlFile"/>.</param>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to look for the trace: l0 and l3 OR l1 and l3 => <code>this.getNextActionToReach(..., "l0", "l3");</code> OR <code>this.getNextActionToReach(..., "l1", "l3");</code></param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach the targeted action, the number of actions returned is less or equal to maxActions parameters.</return>
		public List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(string targetedActionName, int maxActions, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called

			string internalName = getInternalName(targetedActionName, exceptionStackTrace, true, linksConcerned);
			return MonitoringManager.getNextActionsToReach (internalName, maxActions);
		}
		
		private string getInternalName(string actionName, string exceptionStackTrace, bool processLinks = true, params string[] linksConcerned){
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
							return prefix + actionName + "_" + this.id;
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
			
		}

		void Start(){
			// Check if we have to compute a new Id.
			// This is the case if this id is already used by an other ComponentMonitoring
			bool needNewId = MonitoringManager.uniqueMonitoringId2ComponentMonitoring.ContainsKey(id);
			if (needNewId)
				needNewId = MonitoringManager.uniqueMonitoringId2ComponentMonitoring[id] != this;
			// OR if id is not initialized
			needNewId = needNewId || id == -1;
			
			if (needNewId) {
				// Get all used ids
				List <int> ids = new List<int>(MonitoringManager.uniqueMonitoringId2ComponentMonitoring.Keys);
				ids.Sort();
				// Find the first hole available
				int newId = 0;
				foreach (int i in ids)
				{
					if (newId != i)
						break;
					newId++;
				}
				id = newId;
				// update id of petrinet
				if (petriNet != null)
					petriNet.attachID(id);
			}
			// In all cases we reset entry with this
			MonitoringManager.uniqueMonitoringId2ComponentMonitoring[id] = this;
		}
		
		void OnDestroy(){
			// When launching play, all GameObject are destroyed in editor and new Awake, Start... are launched in play mode. Same when we click on Stop button, OnDestroy is called in play mode and Awake, Start... are launched in editor mode (due to [ExecuteInEditMode] metatag) => Then we have to check if dictionnary is already defined in case of MonitoringManager is destroyed before this ComponentMonitoring
			if (MonitoringManager.uniqueMonitoringId2ComponentMonitoring != null)
				MonitoringManager.uniqueMonitoringId2ComponentMonitoring.Remove(id);
		}
    }
}