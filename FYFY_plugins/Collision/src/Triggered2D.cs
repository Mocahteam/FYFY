using UnityEngine;

namespace FYFY_plugins.Trigger {
	/// <summary>
	/// 	Component specifying that the <c>GameObject</c>(the source) is in contact with at least one other <c>GameObject</c>(the target).
	/// </summary>
	/// <remarks>
	/// 	<para>Automatically added, updated or removed by the relative <see cref="FYFY_plugins.Trigger.TriggerSensitive2D">component</see>.</para>
	/// 	<para>DO NOT TOUCH MANUALLY OTHERWISE WE CAN'T GUARANTEE THE COMPORTMENT.</para>
	/// </remarks>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(TriggerSensitive2D))]
	public class Triggered2D : Triggered {
		// Called after the component was created, to initialized its variable to the relative TriggerSensitive2D._targets.
		// Callback automatically called by Unity.
		private void Awake() {
			TriggerSensitive2D ts2D = this.gameObject.GetComponent<TriggerSensitive2D>();
			_targets = ts2D._targets;
		}
	}
}