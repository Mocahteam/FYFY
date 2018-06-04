using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using FYFY;

using System.IO;
using System;
using System.Linq;


namespace FYFY_plugins.Monitoring{
	/// <summary>
	///		This component trigger the building of PetriNets and Specification on Start and write traces when the game is over.
	/// </summary>
	[ExecuteInEditMode] // Awake, OnEnable, Start, Destroy... will be call in edit mode
	[DisallowMultipleComponent]
	public class MonitoringManager : MonoBehaviour {
		private static MonitoringManager _instance = null;
		/// <summary> Get singleton instance of MonitoringManager </summary>
		// singleton => only one Monitoring Manager (see Awake)
		public static MonitoringManager Instance{
			set{
				_instance = value;
			}
			get{
				if (_instance == null)
					_instance = UnityEngine.Object.FindObjectOfType<MonitoringManager>();
				return _instance;
			}
		}
		internal static string NEXT_ACTION_TOKEN = "NextActionToReach"; // token used with Laalys intercommunication
		
		/// <summary> Association between ids and ComponentMonitoring </summary>
		[HideInInspector]
		public Dictionary<int, ComponentMonitoring> uniqueMonitoringId2ComponentMonitoring = new Dictionary<int, ComponentMonitoring>();

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
		[SerializeField]
		internal List<ComponentMonitoring> c_monitors = new List<ComponentMonitoring>(); // used in EditionView
		[HideInInspector]
		[SerializeField]
		private List<FamilyMonitoring> f_monitors = new List<FamilyMonitoring>();
		internal List<FamilyAssociation> availableFamilies; // used in EditionView
		internal class FamilyAssociation {
			internal string systemName; // The name of the system the family is defined
			internal string familyName; // The name of the family inside associated system
			internal Family family; // The family object
			internal string equivWith; // formated name of the first equivalent family defined in another system
		}

		/// <summary>The file name to save PetriNet, Specifications and logs.</summary>
        public string fileName;
		/// <summary>Is analysis run in game?</summary>
		public bool inGameAnalysis;
		/// <summary>Full Petri net to use if in game analysis is enabled</summary>
		public string fullPetriNetPath;
		/// <summary>Filtered Petri net to use if in game analysis is enabled</summary>
		public string filteredPetriNetPath;
		/// <summary>Specification to use if in game analysis is enabled</summary>
		public string featuresPath;
		/// <summary>Path to the jar file of Laalys</summary>
		public string laalysPath;
		
		/// <summary>
		/// 	Get monitor with asked id.
		/// </summary>
		/// <param name="id">The id of the monitor to get.</param>
		/// <return> The ComponentMonitoring object associated to the id <see cref="ComponentMonitoring"/>. </return>
		public static ComponentMonitoring getMonitorById (int id){
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			foreach (ComponentMonitoring cm in MonitoringManager.Instance.c_monitors)
				if (cm.id == id)
					return cm;
			foreach (FamilyMonitoring fm in MonitoringManager.Instance.f_monitors)
				if (fm.id == id)
					return (ComponentMonitoring) fm;
			throw new ArgumentException ("No ComponentMonitoring or FamilyMonitoring available for id "+id);
		}
		
		/// <summary>
		/// 	Trace game action.
		/// </summary>
		/// <param name="cm">The ComponentMonitoring to use to build trace.</param>
		/// <param name="actionName">Action name you want to trace, this name has to match with a transition defined into associated Petri Net of the "cm" parameter <see cref="ComponentMonitoring.PnmlFile"/>.</param>
		/// <param name="performedBy">Specify who perform this action, the player or the system. <see cref="MonitoringManager.Source"/></param>
		/// <param name="processLinks">Set to false if the logic expression associated to the action include "+" operators AND the action performed by the player is not allowed by the system. In this case fourth parameters will not be processed. True (default) means fourth parameter will be analysed.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to build the trace: l0 and l3 OR l1 and l3 => <code>this.trace(..., "l0", "l3");</code> OR <code>this.trace(..., "l1", "l3");</code></param>
		/// <return> labels found for this game action if in game analysis is enabled (see: MonitoringManager). return empty Array else </return>
		public static string[] trace(ComponentMonitoring cm, string actionName, string performedBy, bool processLinks = true, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);

			string internalName = cm.getInternalName(actionName, exceptionStackTrace, processLinks, linksConcerned);
			return MonitoringManager.processTrace (internalName, performedBy);
		}
		
		/// <summary>
		/// 	Trace game action.
		/// </summary>
		/// <param name="family">The monitored Family to use to build trace.</param>
		/// <param name="actionName">Action name you want to trace, this name has to match with a transition defined into associated Petri Net of the "family" parameter <see cref="ComponentMonitoring.PnmlFile"/>.</param>
		/// <param name="performedBy">Specify who perform this action, the player or the system. <see cref="MonitoringManager.Source"/></param>
		/// <param name="processLinks">Set to false if the logic expression associated to the action include "+" operators AND the action performed by the player is not allowed by the system. In this case fourth parameters will not be processed. True (default) means fourth parameter will be analysed.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to build the trace: l0 and l3 OR l1 and l3 => <code>this.trace(..., "l0", "l3");</code> OR <code>this.trace(..., "l1", "l3");</code></param>
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
			return MonitoringManager.processTrace (internalName, performedBy);
		}
		
		/// <summary>
		/// 	Get next actions to perform in order to reach targeted game action.
		/// </summary>
		/// <param name="cm">The ComponentMonitoring on which you want reach action.</param>
		/// <param name="targetedActionName">Action name you want to reach, this name has to match with a transition defined into associated Petri Net  of the "cm" parameter <see cref="ComponentMonitoring.PnmlFile"/>.</param>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to look for the trace: l0 and l3 OR l1 and l3 => <code>this.getNextActionToReach(..., "l0", "l3");</code> OR <code>this.getNextActionToReach(..., "l1", "l3");</code></param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach the targeted action, the number of actions returned is less or equal to maxActions parameters.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(ComponentMonitoring cm, string targetedActionName, int maxActions, params string[] linksConcerned)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame (1, true);										// get caller stackFrame with informations
			string exceptionStackTrace = "(at " + stackFrame.GetFileName () + ":" + stackFrame.GetFileLineNumber ().ToString () + ")";	// to point where this function was called
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);

			string internalName = cm.getInternalName(targetedActionName, exceptionStackTrace, true, linksConcerned);
			return MonitoringManager.getNextActionsToReach (internalName, maxActions);
		}
		
		/// <summary>
		/// 	Get next actions to perform in order to reach targeted game action.
		/// </summary>
		/// <param name="family">The monitored Family on which you want reach action.</param>
		/// <param name="targetedActionName">Action name you want to reach, this name has to match with a transition defined into associated Petri Net  of the "cm" parameter <see cref="ComponentMonitoring.PnmlFile"/>.</param>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <param name="linksConcerned">links label concerned by this action. You can leave empty if only "*" operators are used in logic expression. Must be defined if logic expression associated to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3" you have to indicate which links to use to look for the trace: l0 and l3 OR l1 and l3 => <code>this.getNextActionToReach(..., "l0", "l3");</code> OR <code>this.getNextActionToReach(..., "l1", "l3");</code></param>
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

			string internalName = fm.getInternalName(targetedActionName, exceptionStackTrace, true, linksConcerned);
			return MonitoringManager.getNextActionsToReach (internalName, maxActions);
		}
		
		/// <summary>Ask to Laalys to provide the next actions to perform in order to reach one of the expert end actions</summary>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach one of the expert end actions, the number of actions returned is less or equal to maxActions parameters.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReachEnd(int maxActions){
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			// Ask next actions
			return MonitoringManager.getNextActionsToReach ("end", maxActions);
		}
		
		/// <summary>Ask to Laalys to provide all triggerable actions</summary>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action that may be triggered.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getTriggerableActions(){
			
			if (MonitoringManager.Instance == null)
				throw new TraceAborted ("No MonitoringManager found. You must add MonitoringManager component to one of your GameObject first (the Main_Loop for instance).", null);
			
			string[] actions = MonitoringManager.Instance.analyseToken ("TriggerableActions");
			return extractTuplesFromActionsName(actions);
		}

		private static string[] processTrace(string actionName, string performedBy){
			XmlHandler.addTrace (actionName, performedBy);
			return MonitoringManager.Instance.analyseToken (actionName, performedBy);
		}

		private static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(string targetAction, int maxActions){
			// be sure that maxActions is greater or equal to 0
			if (maxActions < 0)
				maxActions = 0;
			
			string[] actions = MonitoringManager.Instance.analyseToken ("NextActionToReach", targetAction, maxActions.ToString());
			return extractTuplesFromActionsName(actions);
		}
		
		// Extract from Laalys actions' name the associated ComponentMonitoring and game action
		private static List<KeyValuePair<ComponentMonitoring, string>> extractTuplesFromActionsName (string [] actions){
			List<KeyValuePair<ComponentMonitoring, string>> results = new List<KeyValuePair<ComponentMonitoring, string>>();
			// extract Monitoring id from actions' name
			foreach (string action in actions){
				string[] tokens = action.Split('_');
				// last token is id
				int id;
				if (tokens.Length > 2 && Int32.TryParse(tokens[tokens.Length-1], out id)){
					ComponentMonitoring cm;
					if (Instance.uniqueMonitoringId2ComponentMonitoring.TryGetValue(id, out cm)){
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
			string[] results = new string[] {};

			if (networkStream != null) {
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
					results = inLinelabels.Split('\t');
				} catch (Exception e){
					UnityEngine.Debug.Log (e.Message);
					UnityEngine.Debug.Log (" >> Close Client Socket");
					clientSocket.Close ();
					clientSocket = null;
				}
			}
			return results;
		}
		
		private void OnLaalysExit(object sender, System.EventArgs e){
			if (LaalysProcess.ExitCode < 0) {
				switch (LaalysProcess.ExitCode) {
					case -2:
					case -3:
						UnityEngine.Debug.LogError ("Laalys Error: in Monitoring Manager component unable to load the \"Full Petri Net Path\": "+fullPetriNetPath);
						break;
					case -5:
					case -6:
						UnityEngine.Debug.LogError ("Laalys Error: in Monitoring Manager component unable to load the \"Filtered Petri Net Path\": "+filteredPetriNetPath);
						break;
					case -8:
					case -9:
						UnityEngine.Debug.LogError ("Laalys Error: in Monitoring Manager component unable to load the \"Specifications Path\": "+featuresPath);
						break;
					default:
						UnityEngine.Debug.LogError (LaalysProcess.StandardError.ReadToEnd());
						break;
				}
			}
		}
		
		internal void registerMonitor (ComponentMonitoring cm){
			if (cm is FamilyMonitoring)
				f_monitors.Add((FamilyMonitoring)cm);
			else{
				c_monitors.Add(cm);
				c_monitors.Sort (delegate(ComponentMonitoring x, ComponentMonitoring y) {
					if (x == null && y == null)
						return 0;
					else if (x == null)
						return -1;
					else if (y == null)
						return 1;
					else
						return x.gameObject.name.CompareTo (y.gameObject.name);
				});
			}
		}
		
		internal void unregisterMonitor (ComponentMonitoring cm){
			if (cm is FamilyMonitoring)
				f_monitors.Remove((FamilyMonitoring)cm);
			else
				c_monitors.Remove(cm);
		}
		
		internal FamilyMonitoring getFamilyMonitoring (Family family){
			foreach (FamilyMonitoring fm in f_monitors){
				if (family.Equals(fm.descriptor)){
					return fm;
				}
			}
			return null;
		}

		void Awake (){
			// Several instances of MonitoringManager are not allowed
			if (Instance != null && Instance != this) {
				UnityEngine.Debug.LogError ("Only one MonitoringManager component could be instantiate in a scene.");
				DestroyImmediate (this);
				return;
			}

			if (Application.isPlaying && inGameAnalysis) {
				try {
					// Start server
					serverSocket = new TcpListener (IPAddress.Parse ("127.0.0.1"), 12000);
					serverSocket.Start ();

					// Launch Laalys
					if (!File.Exists(laalysPath))
						UnityEngine.Debug.LogError ("You enabled in game analysis into Monitoring Manager component but you don't defined Laalys path.");
					else if (fullPetriNetPath == null || fullPetriNetPath == "")
						UnityEngine.Debug.LogError ("You enabled in game analysis into Monitoring Manager component but you don't defined a full Petri net.");
					else if (filteredPetriNetPath == null || filteredPetriNetPath == "")
						UnityEngine.Debug.LogError ("You enabled in game analysis into Monitoring Manager component but you don't defined a filtered Petri net.");
					else if (featuresPath == null || featuresPath == "")
						UnityEngine.Debug.LogError ("You enabled in game analysis into Monitoring Manager component but you don't defined a specifications.");
					else {
						LaalysProcess = new Process ();
						LaalysProcess.StartInfo.FileName = "java.exe";
						LaalysProcess.StartInfo.Arguments = "-jar "+laalysPath+" -fullPn "+fullPetriNetPath+" -filteredPn "+filteredPetriNetPath+" -features "+featuresPath+" -serverIP localhost -serverPort 12000";
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
				} catch (Exception e) {
					UnityEngine.Debug.Log (e.Message);
				}
			}
		}
		
		void OnEnable () {
			// avoid to inspect System in playing mode
			if (Application.isPlaying)
				return;
			
			// OnEnable is called after script compilation (due to [ExecuteInEditMode]). We use this mechanism to update list of available families
			availableFamilies = new List<FamilyAssociation>();
			// Load all FSystem included into assembly
			System.Type[] systemTypes = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
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
			for (int i = f_monitors.Count-1 ; i >= 0 ; i--){
				bool found = false;
				foreach (FamilyAssociation fa in availableFamilies){
					if (fa.family.Equals(f_monitors[i].descriptor)){
						found = true; // we found one
						f_monitors[i].equivalentName = fa.equivWith;
						f_monitors[i].gameObject.name = fa.equivWith;
						break;
					}
				}
				if (!found)
					DestroyImmediate(f_monitors[i].gameObject);
			}
		}

		void Update (){
			// wait client connection
			if (serverSocket != null && clientSocket == null && serverSocket.Pending ()) {
				// client connection pending => accept this new connection
				clientSocket = serverSocket.AcceptTcpClient ();
				networkStream = clientSocket.GetStream ();
				// Sends data immediately upon calling NetworkStream.Write.
				clientSocket.NoDelay = true;
			}
		}

        void OnDestroy()
		{
			if (Instance == this) {
				// close Socket
				if (clientSocket != null)
					clientSocket.Close ();
				if (serverSocket != null)
					serverSocket.Stop ();
				// Stop process
				if (LaalysProcess != null && !LaalysProcess.HasExited) {
					// Stop to capture output stream
					LaalysProcess.Exited -= LaalysEH;
					LaalysProcess.Kill ();
				}
				// Save traces
				if (Application.isPlaying)
					XmlHandler.saveTraces (fileName);
				
				// Destroy all ComponentMonitors
				for (int i = c_monitors.Count-1 ; i >= 0 ; i--)
					DestroyImmediate(c_monitors[i]);
				// Destroy all FamilyMonitorings
				for (int i = f_monitors.Count-1 ; i >= 0 ; i--)
					if (f_monitors[i])
						DestroyImmediate(f_monitors[i].gameObject);
				
				Instance = null;
			}
        }
    }
}