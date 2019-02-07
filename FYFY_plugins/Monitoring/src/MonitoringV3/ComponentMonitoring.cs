using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Threading;
using FYFY;


namespace FYFY_plugins.Monitoring{
	/// <summary>
	/// 	Add monitoring functionalities to a Game Object
	/// </summary>
	[ExecuteInEditMode] // Awake, Start... will be call in edit mode
	[AddComponentMenu("")]
    public class ComponentMonitoring : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static Mutex mut = new Mutex();
        [NonSerialized]
        internal bool ready = false;

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
		public int fullPnSelected = 0;

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

        /// <summary>
        /// Contructor of the ComponentMonitoring
        /// In Editor: called at compilation, right before and right after Play/Pause, and when a new component is created
        /// At runtime: called when a new component is created
        /// </summary>
        public ComponentMonitoring()
        {
            //Launch a thread to wait for the MonitoringManager, then create an unique ID when it is ready
            Thread thread = new Thread(WaitMonitoringManager);
            thread.Start();
        }

        private void WaitMonitoringManager()
        {
			int timeOut = 0;
            //While MonitoringManager isn't ready or the timeout exceed 5 seconds
            while ((MonitoringManager.Instance == null || !MonitoringManager.Instance.ready || !ready || MainLoop.sceneChanging) && timeOut < 5000)
            {
                //Wait 10 ms not to overload processors
                Thread.Sleep(10);
                timeOut += 10;
            }
			if (timeOut < 5000)
				computeUniqueId();
        }
		
		internal void computeUniqueId() 
        {
            mut.WaitOne();
            try
            {
				MonitoringManager mm = MonitoringManager.Instance;
                // Check if we have to compute a new Id.
                if (id == -1)
                {
                    // Get all used ids
                    List<int> ids = new List<int>();
                    foreach (ComponentMonitoring _cm in mm.c_monitors)
                        if (_cm) ids.Add(_cm.id);
                    foreach (FamilyMonitoring _fm in mm.f_monitors)
                        if (_fm) ids.Add(_fm.id);
                    ids.Sort();
                    // Find the first hole available
                    int newId = 0;
                    int iterationLength = ids.Count+1;
                    for(; newId < iterationLength; newId++)
                    {
                        if (!ids.Contains(newId))
                            break;
                    }
                    id = newId;

                    // update id of petrinet
                    if (petriNet != null)
                        petriNet.attachID(id);
                }
				// register this monitor
				mm.registerMonitor(this);
            }
            catch (Exception e)
            {
                mut.ReleaseMutex();
                throw e;
            }
            mut.ReleaseMutex();
        }
		
		// If we remove a component from an inactive game object, the OnDestroy callback may not be called (depending if an Awake trigger before). In this case the monitoring will not be unregister from MonitoringManager. It's not critical because next time the MonitoringManager will be reload, the list of ComponentMonitoring will be refresh. So we maintain inconsistent ComponentMonitoring only during this session
		void OnDestroy(){
			// When launching play, all GameObject are destroyed in editor and new Awake, Start... are launched in play mode. Same when we click on Stop button, OnDestroy is called in play mode and Awake, Start... are launched in editor mode (due to [ExecuteInEditMode] metatag) => Then we have to check if dictionary is already defined in case of MonitoringManager is destroyed before this ComponentMonitoring
			if (MonitoringManager.Instance != null){
				MonitoringManager.Instance.unregisterMonitor(this);
			}
		}

        /// <summary>
        /// This fonction is called by Unity right before it serializes data from every component
        /// It has to be implemented because ComponentMonitoring inherits from the interface ISerializationCallbackReceiver
        /// </summary>
        public void OnBeforeSerialize()
        {

        }

        /// <summary>
        /// This fonction is called by Unity right after it serializes data from every component
        /// We set the ComponentMonitoring to ready when it is done
        /// </summary>
        public void OnAfterDeserialize()
        {
            // Since ComputeUniqueId is called in a thread launched in the constructor we have to wait the end of deserialization of unity before looking for a new id to have the true values of the component
            ready = true;
        }
		
        internal void clone(ComponentMonitoring template){
			this.PnmlFile = template.PnmlFile;
			this.comments = template.comments;
			this.PetriNet = new PetriNet(template.petriNet); // this set init transitionLinks (see setter defined before)
			this.PetriNet.attachID(this.id); // propagate local id
			for (int i = 0 ; i < template.transitionLinks.Count ; i++){
				this.transitionLinks[i].import(template.transitionLinks[i]);
			}
			this.fullPnSelected = template.fullPnSelected;
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
        /// Each list element is a set of AND links, then the main list model OR set of links.
        /// </summary>
		public List<List<string>> getPossibleSetOfLinks(string actionName){
			return getPossibleSetOfLinks (actionName, "");
		}

		internal List<List<string>> getPossibleSetOfLinks(string actionName, string exceptionStackTrace){
			TransitionLink transitionLink  = getTransitionLinkByTransitionLabel(actionName);
			if (transitionLink != null) {
				string logic = transitionLink.logic;
				// Check logic expression
				if (ExpressionParser.isValid (transitionLink)) {
					string[] exp = ExpressionParser.getDistribution (logic);
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
					return groupAndByOr;
				} else {
					throw new TraceAborted ("Logic expression for \"" + actionName + "\" action in \"" + this.gameObject.name + "\" Game Object is not valid.", exceptionStackTrace);
				}
			} else {
				throw new TraceAborted ("Action \"" + actionName + "\" is not monitored by \"" + this.gameObject.name + "\" Game Object.", exceptionStackTrace);
			}
		}
		
		internal string getInternalName(string actionName, string exceptionStackTrace, bool processLinks = true, params string[] linksConcerned){
			TransitionLink transitionLink  = getTransitionLinkByTransitionLabel(actionName);
			if (transitionLink != null) {
				string logic = transitionLink.logic;
				// Check logic expression
				if (ExpressionParser.isValid (transitionLink)) {
					List<List<string>> groupAndByOr = getPossibleSetOfLinks (actionName, exceptionStackTrace);

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
						if (groupAndByOr.Count <= 1){
							// If logic expression is empty or contains only AND operators, linksConcerned parameter is not useful because there is no ambiguity on this transition.
							linksFound = true;
							if (linksConcerned.Length
							> 0){
								WarningException we = new WarningException ("Because logic expression includes only \"*\" operators, \"linksConcerned\" parameters are ignored. You can remove them to the call.", exceptionStackTrace);
                                Debug.LogException(we);
							}							
						}
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
    }
}