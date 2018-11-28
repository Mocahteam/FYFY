using UnityEngine;
using FYFY;

namespace FYFY_plugins.Monitoring {
	public class ActionPerformed : MonoBehaviour {
		// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
		public new string name = string.Empty;
		public string overrideName = string.Empty;
		/// <summary>
		/// "player" or "system"
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
		public Family family = null;

		public string exceptionStackTrace;
	}
}