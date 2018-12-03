using UnityEngine;
using FYFY;

namespace FYFY_plugins.Monitoring {
    /// <summary>
    /// Data describing the trace built.
    /// </summary>
	public class Trace : MonoBehaviour {
        // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
        /// <summary>
        /// Labels found for this traced game action if in game analysis is enabled (see: MonitoringManager). return empty Array else
        /// </summary>
        public string[] labels;
        /// <summary>
        /// Birth date of the trace.
        /// </summary>
		public float time;
        /// <summary>
        /// The ComponentMonitoring used to build this trace.
        /// </summary>
		public ComponentMonitoring componentMonitoring;
        /// <summary>
        /// Name of the traced action matching a transition in the Petri net of the ComponentMonitoring
        /// </summary>
		public string actionName;
        /// <summary>
        /// Specify who performed this action, the player or the system.
        /// </summary>
		public string performedBy;
		/// <summary>
		/// links label concerned by this action. You can leave empty if only "*" operators
		/// are used in logic expression. Must be defined if logic expression associated
		/// to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3"
		/// you have to indicate which links to use to build the trace: l0 and l3 OR l1 and
		/// l3 => orLabels = new string[] {..., "l0", "l3"}; OR orLabels = new string[] {..., "l1", "l3"};
		/// </summary>
		public string[] orlabels = null;
        /// <summary>
        /// The monitored Family to used to build trace. Null if the field ComponentMonitoring is filled.
        /// </summary>
		public Family family;
	}
}