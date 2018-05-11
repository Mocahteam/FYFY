using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using UnityEngine;

using System.IO;
using System;
using System.Linq;


namespace FYFY_plugins.Monitoring{
	/// <summary>
	///		This component trigger the building of PetriNets and Specification on Start and write traces when the game is over.
	/// </summary>
	[ExecuteInEditMode] // Awake, Start... will be call in edit mode
	public class MonitoringManager : MonoBehaviour {
		internal static MonitoringManager _monitoringManager = null; // singleton => only one Monitoring Manager (see Awake)
		internal static string NEXT_ACTION_TOKEN = "NextActionToReach"; // token used with Laalys intercommunication
		internal static Dictionary<int, ComponentMonitoring> uniqueMonitoringId2ComponentMonitoring = null;

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

		/// <summary>Ask to Laalys to provide the next actions to perform in order to reach one of the expert end actions</summary>
		/// <param name="maxActions">Maximum number of actions returned.</param>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action useful to reach one of the expert end actions, the number of actions returned is less or equal to maxActions parameters.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReachEnd(int maxActions){
			// Ask next actions
			return getNextActionsToReach ("end", maxActions);
		}
		
		/// <summary>Ask to Laalys to provide all triggerable actions</summary>
		/// <return>List of Pairs including a ComponentMonitoring and its associated game action that may be triggered.</return>
		public static List<KeyValuePair<ComponentMonitoring, string>> getTriggerableActions(){
			if (_monitoringManager != null) {
				string[] actions = _monitoringManager.analyseToken ("TriggerableActions");
				return extractTuplesFromActionsName(actions);
			} else
				return new List<KeyValuePair<ComponentMonitoring, string>>();
		}

		internal static string[] processTrace(string actionName, string performedBy){
			string[] labels = new string[] {};
			XmlHandler.addTrace (actionName, performedBy);
			if (_monitoringManager != null) {
				labels = _monitoringManager.analyseToken (actionName, performedBy);
			}
			return labels;
		}

		internal static List<KeyValuePair<ComponentMonitoring, string>> getNextActionsToReach(string targetAction, int maxActions){
			// be sure that maxActions is greater or equal to 0
			if (maxActions < 0)
				maxActions = 0;
			
			if (_monitoringManager != null) {
				string[] actions = _monitoringManager.analyseToken ("NextActionToReach", targetAction, maxActions.ToString());
				return extractTuplesFromActionsName(actions);
			} else
				return new List<KeyValuePair<ComponentMonitoring, string>>();
		}
		
		// Extract from Laalys actions' name the associated ComponentMonitoring and game action
		internal static List<KeyValuePair<ComponentMonitoring, string>> extractTuplesFromActionsName (string [] actions){
			List<KeyValuePair<ComponentMonitoring, string>> results = new List<KeyValuePair<ComponentMonitoring, string>>();
			// extract Monitoring id from actions' name
			foreach (string action in actions){
				string[] tokens = action.Split('_');
				// last token is id
				int id;
				if (tokens.Length > 2 && Int32.TryParse(tokens[tokens.Length-1], out id)){
					ComponentMonitoring cm;
					if (uniqueMonitoringId2ComponentMonitoring.TryGetValue(id, out cm)){
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

		void Awake (){
			// Several instances of MonitoringManager are not allowed
			if (_monitoringManager != null) {
				UnityEngine.Debug.Log ("Only one MonitoringManager component could be instantiate in this scene");
				DestroyImmediate (this);
				return;
			}
			_monitoringManager = this;
			
			// reset association table between id and ComponentMonitoring
			uniqueMonitoringId2ComponentMonitoring = new Dictionary<int, ComponentMonitoring>();

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

		private void OnLaalysExit(object sender, System.EventArgs e){
			UnityEngine.Debug.Log (LaalysProcess.ExitCode);
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
			if (_monitoringManager == this) {
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
				XmlHandler.saveTraces (fileName);
				_monitoringManager = null;
			}
        }
    }
}