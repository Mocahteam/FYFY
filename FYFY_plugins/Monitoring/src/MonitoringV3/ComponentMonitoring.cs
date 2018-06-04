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
	[ExecuteInEditMode] // Awake, Start... will be call in edit mode
	[AddComponentMenu("")]
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
		
		/// <summary> Name of the parent Petri net to include this monitor</summary>
		[HideInInspector]
		public string exportPn;

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
		
		internal void clone(ComponentMonitoring template){
			this.PnmlFile = template.PnmlFile;
			this.comments = template.comments;
			this.PetriNet = new PetriNet(template.petriNet); // this set init transitionLinks (see setter defined before)
			this.PetriNet.attachID(this.id); // propagate local id
			for (int i = 0 ; i < template.transitionLinks.Count ; i++){
				this.transitionLinks[i].import(template.transitionLinks[i]);
			}
			this.exportPn = template.exportPn;
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
		
		internal string getInternalName(string actionName, string exceptionStackTrace, bool processLinks = true, params string[] linksConcerned){
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
		
		internal void computeUniqueId(){
			// Check if one MonitoringManager is available
			if (MonitoringManager.Instance != null){
				MonitoringManager mm = MonitoringManager.Instance;
				// Check if we have to compute a new Id.
				// This is the case if this id is already used by an other ComponentMonitoring
				bool needNewId = mm.uniqueMonitoringId2ComponentMonitoring.ContainsKey(id);
				if (needNewId)
					needNewId = mm.uniqueMonitoringId2ComponentMonitoring[id] != this;
				// OR if id is not initialized
				needNewId = needNewId || id == -1;
				
				if (needNewId) {
					// Get all used ids
					List <int> ids = new List<int>(mm.uniqueMonitoringId2ComponentMonitoring.Keys);
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
				mm.uniqueMonitoringId2ComponentMonitoring[id] = this;
				// register this monitor
				MonitoringManager.Instance.registerMonitor(this);
			} else {
				throw new NullReferenceException ("You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).");
			}
		}
		
		internal void freeUniqueId(){
			// When launching play, all GameObject are destroyed in editor and new Awake, Start... are launched in play mode. Same when we click on Stop button, OnDestroy is called in play mode and Awake, Start... are launched in editor mode (due to [ExecuteInEditMode] metatag) => Then we have to check if dictionary is already defined in case of MonitoringManager is destroyed before this ComponentMonitoring
			if (MonitoringManager.Instance != null){
				MonitoringManager.Instance.uniqueMonitoringId2ComponentMonitoring.Remove(id);
				MonitoringManager.Instance.unregisterMonitor(this);
			}
		}

		void Awake(){
			computeUniqueId();
		}
		
		void OnDestroy(){
			freeUniqueId();
		}
    }
}