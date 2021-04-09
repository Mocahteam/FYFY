using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using FYFY;

using System.IO;
using System;
using System.Linq;


namespace FYFY_plugins.Monitoring{
	/// <summary>
	///		This component trigger the building of PetriNets and Features and write traces when the game is over.
	/// </summary>
	[ExecuteInEditMode] // Awake, OnEnable, Start, Destroy... will be call in edit mode
	[DisallowMultipleComponent]
	public class MonitoringManager : MonoBehaviour {
		internal static string NEXT_ACTION_TOKEN = "NextActionToReach"; // token used with Laalys intercommunication

		private static Mutex mut = new Mutex();
		
		/// <summary>Define the different source that can trigger a game action.</summary>
		public static class Source {
			/// <summary></summary>
			public static string SYSTEM = "system";
			/// <summary></summary>
			public static string PLAYER = "player";
		};
		
		private TcpListener serverSocket = null;
		private TcpClient clientSocket = null;
		private NetworkStream networkStream = null;
		private Process LaalysProcess = null;
		private EventHandler LaalysEH = null;
		
		[HideInInspector]
		[NonSerialized]
		internal List<ComponentMonitoring> c_monitors = new List<ComponentMonitoring>(); // used in EditionView 
		[HideInInspector]
		[NonSerialized]
		internal List<FamilyMonitoring> f_monitors = new List<FamilyMonitoring>();
		internal List<FamilyAssociation> availableFamilies; // used in EditionView
		internal class FamilyAssociation {
			internal string systemName; // The name of the system the family is defined
			internal string familyName; // The name of the family inside associated system
			internal Family family; // The family object
			internal string equivWith; // formated name of the first equivalent family defined in another system
		}
        [NonSerialized]
        internal bool ready = false; //true at the beginning of OnEnable and used in ComponentMonitoring
        /// <summary>
        /// This boolean is set to false when Laalys is connected or if an error occured while trying to launch Laalys
        /// </summary>
        public bool waitingForLaalys = true;

		/// <summary>List of Petri Nets name</summary>
		public List<string> PetriNetsName = null;
		/// <summary>Is analysis run in game?</summary>
		public bool inGameAnalysis;
        /// <summary>Is debug logs displayed?</summary>
        public bool debugLogs;
        /// <summary>Full Petri nets location to use if in game analysis is enabled</summary>
        public string fullPetriNetsPath;
		/// <summary>Filtered Petri nets location to use if in game analysis is enabled</summary>
		public string filteredPetriNetsPath;
		/// <summary>Features location to use if in game analysis is enabled</summary>
		public string featuresPath;
		/// <summary>Path to the jar file of Laalys</summary>
		public string laalysPath;
		
        /// <summary>
        /// Instance of the singleton MonitoringManager
        /// The value is set in the constructor
        /// </summary>
		public static MonitoringManager Instance = null;
		
		/// <summary> Set singleton instance of MonitoringManager </summary>
		// singleton => only one Monitoring Manager (see Awake)
        public MonitoringManager()
        {
			// Set the last object of MonitoringManager as current Instance
			Instance = this;
        }

        /// <summary>
        /// 	Get monitor with asked id.
        /// </summary>
        /// <param name="id">The id of the monitor to get.</param>
        /// <return> The ComponentMonitoring object associated to the id if it exists. Return null otherwise. <see cref="ComponentMonitoring"/> </return>
        public static ComponentMonitoring getMonitorById (int id){
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			foreach (ComponentMonitoring cm in MonitoringManager.Instance.c_monitors)
				if (cm != null && cm.id == id){
					return cm;
				}
			foreach (FamilyMonitoring fm in MonitoringManager.Instance.f_monitors)
				if (fm != null && fm.id == id)
					return (ComponentMonitoring) fm;
			return null;
		}
		
		/// <summary>
		/// 	Trace game action.
		/// </summary>
		/// <param name="monitor">The ComponentMonitoring to use to build trace.</param>
		/// <param name="actionName">Action name you want to trace, this name has to match with a transition defined into associated Petri Net of the "monitor" parameter <see cref="ComponentMonitoring.PnmlFile"/>.</param>
		/// <param name="performedBy">Specify who perform this action, the player or the system. <see cref="MonitoringManager.Source"/></param>
		/// <param name="processLinks">Set to false if the logic expression associated to the action include "+" operators AND the action performed by the player is not allowed by the system. In this case fourth parameters will not be processed. True (default) means fourth parameter will be analysed.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to build the trace: l0 and l3 OR l1 and l3 => <code>MonitoringManager.trace(..., "l0", "l3");</code> OR <code>MonitoringManager.trace(..., "l1", "l3");</code></param>
		/// <return> labels found for this game action if in game analysis is enabled (see: MonitoringManager). return empty Array else </return>
		public static string[] trace(ComponentMonitoring monitor, string actionName, string performedBy, bool processLinks = true, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);

			string internalName = monitor.getInternalName(actionName, exceptionStackTrace, processLinks, linksConcerned);
			if (monitor.fullPnSelected >= MonitoringManager.Instance.PetriNetsName.Count)
				monitor.fullPnSelected = 1;
			string pnName = MonitoringManager.Instance.PetriNetsName[monitor.fullPnSelected];
			return MonitoringManager.processTrace (pnName, internalName, performedBy);
		}
		
		/// <summary>
		/// 	Trace game action.
		/// </summary>
		/// <param name="family">The monitored Family to use to build trace.</param>
		/// <param name="actionName">Action name you want to trace, this name has to match with a transition defined into associated Petri Net of the "family" parameter <see cref="ComponentMonitoring.PnmlFile"/>.</param>
		/// <param name="performedBy">Specify who perform this action, the player or the system. <see cref="MonitoringManager.Source"/></param>
		/// <param name="processLinks">Set to false if the logic expression associated to the action include "+" operators AND the action performed by the player is not allowed by the system. In this case fourth parameters will not be processed. True (default) means fourth parameter will be analysed.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to build the trace: l0 and l3 OR l1 and l3 => <code>MonitoringManager.trace(..., "l0", "l3");</code> OR <code>MonitoringManager.trace(..., "l1", "l3");</code></param>
		/// <return> labels found for this game action if in game analysis is enabled (see: MonitoringManager). return empty Array else </return>
		public static string[] trace(Family family, string actionName, string performedBy, bool processLinks = true, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			FamilyMonitoring fm = MonitoringManager.Instance.getFamilyMonitoring(family);
			if (fm == null)
				throw new TraceAborted ("No monitor found for this family.", null);

			string internalName = fm.getInternalName(actionName, exceptionStackTrace, processLinks, linksConcerned);
			if (fm.fullPnSelected >= MonitoringManager.Instance.PetriNetsName.Count)
				fm.fullPnSelected = 1;
			string pnName = MonitoringManager.Instance.PetriNetsName[fm.fullPnSelected];
			return MonitoringManager.processTrace (pnName, internalName, performedBy);
		}
		
		/// <summary>
		/// 	Get next actions to perform in order to reach targeted game action.
		/// </summary>
		/// <param name="monitor">The ComponentMonitoring on which you want reach action.</param>
		/// <param name="targetedActionName">Action name you want to reach, this name has to match with a transition defined into associated Petri Net of the "monitor" parameter <see cref="ComponentMonitoring.PnmlFile"/>. The special key word "##playerObjectives##" enable to target all player objective actions defined inside full Petri Net from which the monitor is part of (in this special case, "linksConcerned" parameter will be ignore).</param>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to look for the trace: l0 and l3 OR l1 and l3 => <code>MonitoringManager.getNextActionToReach(..., "l0", "l3");</code> OR <code>MonitoringManager.getNextActionToReach(..., "l1", "l3");</code></param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach the targeted action, the number of actions returned is less or equal to maxActions parameters.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(ComponentMonitoring monitor, string targetedActionName, int maxActions, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);

			string internalName = targetedActionName;
			if (!targetedActionName.Equals("##playerObjectives##"))
				internalName = monitor.getInternalName(targetedActionName, exceptionStackTrace, true, linksConcerned);
			if (monitor.fullPnSelected >= MonitoringManager.Instance.PetriNetsName.Count)
				monitor.fullPnSelected = 1;
			string pnName = MonitoringManager.Instance.PetriNetsName[monitor.fullPnSelected];
			return MonitoringManager.getNextActionsToReach (pnName, internalName, maxActions);
		}
		
		/// <summary>
		/// 	Get next actions to perform in order to reach targeted game action.
		/// </summary>
		/// <param name="family">The monitored Family on which you want reach action.</param>
		/// <param name="targetedActionName">Action name you want to reach, this name has to match with a transition defined into associated Petri Net  of the "family" parameter <see cref="ComponentMonitoring.PnmlFile"/> The special key word "##playerObjectives##" enable to target all player objective actions defined inside full Petri Net from which the monitor is part of (in this special case, "linksConcerned" parameter will be ignore).</param>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to look for the trace: l0 and l3 OR l1 and l3 => <code>MonitoringManager.getNextActionToReach(..., "l0", "l3");</code> OR <code>MonitoringManager.getNextActionToReach(..., "l1", "l3");</code></param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach the targeted action, the number of actions returned is less or equal to maxActions parameters.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(Family family, string targetedActionName, int maxActions, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			FamilyMonitoring fm = MonitoringManager.Instance.getFamilyMonitoring(family);
			if (fm == null)
				throw new TraceAborted ("No monitor found for this family", null);

			string internalName = targetedActionName;
			if (!targetedActionName.Equals("##playerObjectives##"))
				internalName = fm.getInternalName(targetedActionName, exceptionStackTrace, true, linksConcerned);
			if (fm.fullPnSelected >= MonitoringManager.Instance.PetriNetsName.Count)
				fm.fullPnSelected = 1;
			string pnName = MonitoringManager.Instance.PetriNetsName[fm.fullPnSelected];
			return MonitoringManager.getNextActionsToReach (pnName, internalName, maxActions);
		}
		
		/// <summary>
		/// 	Get next actions to perform in order to reach the player objective of the Petri net.
		/// </summary>
		/// <param name="pnName">The Petri net name to process.</param>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach the player objective, the number of actions returned is less or equal to maxActions parameters.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReachPlayerObjective(string pnName, int maxActions)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			if (!MonitoringManager.Instance.PetriNetsName.Contains(pnName))
				throw new TraceAborted ("No Petri net with name \""+pnName+"\" found", null);

			string internalName = "##playerObjectives##";
			return MonitoringManager.getNextActionsToReach (pnName, internalName, maxActions);
		}
		
		/// <summary>Ask to Laalys to provide all triggerable actions</summary>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action that may be triggered.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getTriggerableActions(){
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			string[] actions = MonitoringManager.Instance.analyseToken ("TriggerableActions");
			return extractTuplesFromActionsName(actions);
		}

		private static string[] processTrace(string pnName, string actionName, string performedBy){
			if (pnName == null || pnName == "")
				pnName = SceneManager.GetActiveScene().name;
			XmlHandler.addTrace (pnName, actionName, performedBy);
			return MonitoringManager.Instance.analyseToken (pnName, actionName, performedBy);
		}

		private static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(string pnName, string targetAction, int maxActions){
			// be sure that maxActions is greater or equal to 0
			if (maxActions < 0)
				maxActions = 0;
			
			string[] actions = MonitoringManager.Instance.analyseToken ("NextActionToReach", pnName, targetAction, maxActions.ToString());
			return extractTuplesFromActionsName(actions);
		}

		/// <summary>
        /// Returns a non readable list of markings of the complete and filtered nets
        /// </summary>
        /// <returns></returns>
		public static List<string> getPetriNetsMarkings()
        {
			return new List<string>(MonitoringManager.Instance.analyseToken("GetPetriNetsMarkings"));
        }

		/// <summary>
		/// Takes a list generated by "getPetriNetsMarkings" and sends it to Laalys to set markings of complete and filtered nets
		/// </summary>
		/// <param name="markingsList"></param>
		public static void setPetriNetsMarkings(List<string> markingsList)
        {
			if(markingsList != null && markingsList.Count > 0)
            {
				string markings = "";
				for (int i = 0; i < markingsList.Count; i++)
					markings += "\t" + markingsList[i];

				MonitoringManager.Instance.analyseToken("SetPetriNetsMarkings", markings);
			}
        }
		
		// Extract from Laalys actions' name the associated ComponentMonitoring and game action
		private static List<KeyValuePair<ComponentMonitoring, string>> extractTuplesFromActionsName (string [] actions){
			List<KeyValuePair<ComponentMonitoring, string>> results = new List<KeyValuePair<ComponentMonitoring, string>>();
            // extract Monitoring id from actions' name
            foreach (string action in actions)
            {
                string[] tokens = action.Split('_');
				// last token is id
				int id;
				if (tokens.Length > 2 && Int32.TryParse(tokens[tokens.Length-1], out id)){
					ComponentMonitoring cm = MonitoringManager.getMonitorById(id);
					if (cm != null){
						// second to last is game action name => Add pair
						results.Add(new KeyValuePair<ComponentMonitoring, string>(cm, tokens[tokens.Length-2]));
					} else
						UnityEngine.Debug.LogError ("No MonitoringComponent with id: "+id);
				}
				else
					UnityEngine.Debug.LogError ("Action name malformed: "+action);
			}
			return results;
		}

		internal string [] analyseToken (string token, params string[] options){
			mut.WaitOne(); // in case of analyseToken is called by several thread, control access in order to avoid that the threads read bytes from an other analyses
			string[] results = new string[] {};
			if (networkStream != null && clientSocket != null) {
				try{
					// Aggregate options into the token
					foreach (string option in options)
						token = token+"\t"+option;
					// Send data to Laalys
					byte[] sendBytes = Encoding.UTF8.GetBytes (token);
					networkStream.Write (sendBytes, 0, sendBytes.Length);
					// Wait results
					byte[] receiveBytes = new byte[1024];
					int numberOfBytesRead = 0;
					string inLinelabels = "";
					do {
						numberOfBytesRead = networkStream.Read(receiveBytes, 0, receiveBytes.Length);
						inLinelabels += Encoding.UTF8.GetString (receiveBytes, 0, numberOfBytesRead);
					} while (networkStream.DataAvailable);
                    //Laalys always return the new line character as last charater so we have to remove it in inLinelabels
                    inLinelabels = inLinelabels.Replace(System.Environment.NewLine, "");
                    if (inLinelabels != "")
					    results = inLinelabels.Split('\t');
				} catch (Exception e){
					UnityEngine.Debug.Log (e.Message);
					UnityEngine.Debug.Log (" >> Close Client Socket");
					clientSocket.Close ();
					clientSocket = null;
				}
			}
			mut.ReleaseMutex();
			
			return results;
		}
		
		private void OnLaalysExit(object sender, System.EventArgs e){
			if (LaalysProcess.ExitCode < 0) {
				switch (LaalysProcess.ExitCode) {
					case -2:
						UnityEngine.Debug.LogError ("Laalys Error: in Monitoring Manager component \""+fullPetriNetsPath+"\" no such directory");
						break;
					case -4:
						UnityEngine.Debug.LogError ("Laalys Error: in Monitoring Manager component \""+filteredPetriNetsPath+"\" no such directory");
						break;
					case -6:
						UnityEngine.Debug.LogError ("Laalys Error: in Monitoring Manager component \""+featuresPath+"\" no such directory");
						break;
					default:
                        if (!LaalysProcess.StandardError.EndOfStream)
						    UnityEngine.Debug.LogError (LaalysProcess.StandardError.ReadToEnd());
						break;
                }
            }
		}
		
		internal void registerMonitor (ComponentMonitoring cm)
        {
            if (cm is FamilyMonitoring){
				if (!f_monitors.Contains((FamilyMonitoring)cm)) {
					f_monitors.Add((FamilyMonitoring)cm);
                }
			}
			else{
				if (!c_monitors.Contains(cm)){
					c_monitors.Add(cm);
				}
			}
		}
		
		internal void unregisterMonitor (ComponentMonitoring cm){
			if (cm is FamilyMonitoring)
				f_monitors.Remove((FamilyMonitoring)cm);
			else
				c_monitors.Remove(cm);
		}
		
		/// <summary>
		/// 	Get the FamilyMonitoring associated to a family if it exists.
		/// </summary>
		/// <param name="family">The Family to ask for.</param>
		/// <return>The FamilyMonitoring associated to the family if it exists. Null otherwise.</return>
		public FamilyMonitoring getFamilyMonitoring (Family family){
			foreach (FamilyMonitoring fm in f_monitors){
				if (family.Equals(fm.descriptor)){
					return fm;
				}
			}
			return null;
		}

		void Awake ()
        {
			// Several instances of MonitoringManager are not allowed
            if (Instance != null && Instance != this) {
				UnityEngine.Debug.LogError ("Only one MonitoringManager component could be instantiate in a scene.");
				DestroyImmediate (this);
				return;
            }

            //Clear lists to remove erroneous component
            //The others will be registered again in their start function
            c_monitors = new List<ComponentMonitoring>();
            f_monitors = new List<FamilyMonitoring>();

            if (PetriNetsName == null){
				PetriNetsName = new List<string>();
				PetriNetsName.Add(SceneManager.GetActiveScene().name);
			}

			if (Application.isPlaying && inGameAnalysis) {
				try {
					// Start server
					serverSocket = new TcpListener (IPAddress.Parse ("127.0.0.1"), 12000);
					serverSocket.Start ();

					// Launch Laalys
					if (!File.Exists(laalysPath))
                    {
                        UnityEngine.Debug.LogError("You enabled in game analysis into Monitoring Manager component but you don't defined Laalys path.");
                        waitingForLaalys = false;
                    }
					else {
						if (fullPetriNetsPath == null || fullPetriNetsPath == "")
							fullPetriNetsPath = "./completeNets/";
						if (filteredPetriNetsPath == null || filteredPetriNetsPath == "")
							filteredPetriNetsPath = "./filteredNets/";
						if (featuresPath == null || featuresPath == "")
							featuresPath = "./features/";
						LaalysProcess = new Process ();
						LaalysProcess.StartInfo.FileName = "java";
						LaalysProcess.StartInfo.Arguments = "-jar "+laalysPath+" -fullPn "+fullPetriNetsPath+" -filteredPn "+filteredPetriNetsPath+" -features "+featuresPath+" -serverIP localhost -serverPort 12000";
                        if (debugLogs)
                            LaalysProcess.StartInfo.Arguments += " -d";
                        // Options to capture exit code
                        LaalysProcess.StartInfo.CreateNoWindow = false;
						LaalysProcess.EnableRaisingEvents = true;
						LaalysEH = new EventHandler(OnLaalysExit);
						LaalysProcess.Exited += LaalysEH;
						// Options to capture standard output stream
						LaalysProcess.StartInfo.UseShellExecute = false;
                        LaalysProcess.StartInfo.RedirectStandardError = true;
						// Launch Laalys
						LaalysProcess.Start();
                    }
				} catch (Exception e)
                {
                    waitingForLaalys = false;
                    UnityEngine.Debug.Log (e.Message);
				}
			}
		}
		
		void OnEnable(){
			ready = true;
		}
		
		/// <summary>Parse all systems and inspect their families</summary>
		public void synchronizeFamilies(){
            // avoid to inspect System in playing mode
            if (Application.isPlaying)
				return;
			
			availableFamilies = new List<FamilyAssociation>();
			
			// Load all FSystem included into assembly
#if NET3_5
			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
#else
			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic)
#endif
				from type in assembly.GetExportedTypes()
				where (type.IsClass == true && type.IsAbstract == false && type.IsSubclassOf(typeof(FSystem)) == true)
				select type).ToArray();
            // Parse all FSystems
            for (int i = 0; i < systemTypes.Length; ++i) {
				System.Type systemType = systemTypes [i];
				try{

					// Create instance of FSystem in order to know its Families types
					FSystem system = (FSystem) System.Activator.CreateInstance(systemType);
					// Load all members of this System
					MemberInfo[] members = systemType.GetMembers (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					foreach (MemberInfo member in members) {
						if (member.MemberType == MemberTypes.Field) {
							FieldInfo field = (FieldInfo)member;
							if (field.FieldType == typeof(FYFY.Family)) {
								Family f = (Family)field.GetValue (system);
								// Check if this family is equivalent to another family already loaded
								string equivFamily = null;
								foreach (FamilyAssociation f_alreadyStored in availableFamilies)
									if (f.Equals(f_alreadyStored.family))
										equivFamily = f_alreadyStored.equivWith;
								// store data and link with equivalent family
								FamilyAssociation entry = new FamilyAssociation ();
								entry.systemName = systemType.FullName;
								entry.familyName = field.Name;
								entry.family = f;
								if (equivFamily != null)
									entry.equivWith = equivFamily; 
								else
									entry.equivWith = "equivWith_" + entry.systemName + "_" + entry.familyName;
								availableFamilies.Add (entry);
                            }
						}
                    }
                } catch (Exception){
					UnityEngine.Debug.LogError (systemType.FullName+": Instance creation failed (all families of this system are ignored). MonitoringManager requires to instantiate your systems in order to inspect their families. Common solution: Check in your constructor if Application.isPlaying is true.");
				}
            }

            // Check if associations between FamilyMonitoring components and new available families are still stable
            for (int i = f_monitors.Count-1 ; i >= 0 ; i--)
            {
                bool found = false;
				foreach (FamilyAssociation fa in availableFamilies){
					if (fa.family.Equals(f_monitors[i].descriptor)){
						found = true; // we found one
						f_monitors[i].equivalentName = fa.equivWith;
						f_monitors[i].gameObject.name = fa.equivWith;
						break;
					}
				}
				if (!found){
					DestroyImmediate(f_monitors[i].gameObject);
				}
			}
		}

		void Update (){
			if (Application.isPlaying && inGameAnalysis){
				// wait client connection
				if (serverSocket != null && clientSocket == null && serverSocket.Pending ()) {
					// client connection pending => accept this new connection
					clientSocket = serverSocket.AcceptTcpClient ();
					networkStream = clientSocket.GetStream ();
					// Sends data immediately upon calling NetworkStream.Write.
					clientSocket.NoDelay = true;
					waitingForLaalys = false;
				}
			}
		}

        void OnDestroy()
		{
			// ask Laalys to quit
			if (networkStream != null){
				byte[] sendBytes = Encoding.UTF8.GetBytes ("Quit");
				networkStream.Write (sendBytes, 0, sendBytes.Length);
			}
			// close Socket 
			if (clientSocket != null)
				clientSocket.Close ();
			if (serverSocket != null)
				serverSocket.Stop ();
			// Stop process
			if (LaalysProcess != null && !LaalysProcess.HasExited) {
				// Stop to capture output stream
				LaalysProcess.Exited -= LaalysEH;
				LaalysProcess.Kill();
			}
			// Save traces
			if (Application.isPlaying)
				XmlHandler.saveTraces (SceneManager.GetActiveScene().name);
			if (Instance == this) {
				Instance = null;
			}
        }
    }
}