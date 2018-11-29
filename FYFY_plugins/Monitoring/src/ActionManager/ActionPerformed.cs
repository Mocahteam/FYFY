using UnityEngine;
using FYFY;

namespace FYFY_plugins.Monitoring {
    /// <summary>
    /// Data describing the traced action.
    /// The system processing these data will search on the gameobject of this component a ComponentMonitoring corresponding to the data filled and trace it.
    /// If no ComponentMonitoring is found and a family is filled in the data, the system will trace the family.
    /// </summary>
	public class ActionPerformed : MonoBehaviour {
		// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
        /// <summary>
        /// Name of the traced action matching a transition in the Petri net of the ComponentMonitoring.
        /// </summary>
		public new string name = string.Empty;
        /// <summary>
        /// Name overriding the name of the traced transition in the Monitoring Editor.
        /// </summary>
		public string overrideName = string.Empty;
        /// <summary>
        /// Specify who perform this action, the player or the system. If not filled or different than "player" or "system", it will be set to "player" by default.
        /// </summary>
        public string performedBy;

		/// <summary>
		/// links label concerned by this action. You can leave empty if only "*" operators
		/// are used in logic expression. Must be defined if logic expression associated
		/// to the action include "+" operators. For instance, if logic expression is "(l0+l1)*l3"
		/// you have to indicate which links to use to build the trace: l0 and l3 OR l1 and
		/// l3 => orLabels = new string[] {..., "l0", "l3"}; OR orLabels = new string[] {..., "l1", "l3"};
		/// </summary>
		public string[] orLabels = null;
        /// <summary>
        /// The monitored Family to use to build trace.
        /// If there is a ComponentMonitoring matching the name and/or the overrideName filled, this field will be ignored.
        /// </summary>
		public Family family = null;

        /// <summary>
        /// String that describes the immediate frames of the call stack when this component is added to a GameObject.
        /// </summary>
		public string exceptionStackTrace;
	}
}