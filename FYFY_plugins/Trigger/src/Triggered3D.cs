using UnityEngine;

namespace FYFY_plugins.Trigger {
	/// <summary>
	/// 	Component specifying that the <c>GameObject</c>(the source) is in contact with at least one other <c>GameObject</c>(the target).
	/// </summary>
	/// <remarks>
	/// 	<para>Automatically added, updated or removed by the relative <see cref="FYFY_plugins.Trigger.TriggerSensitive3D">component</see>.</para>
	/// 	<para>DO NOT TOUCH MANUALLY OTHERWISE WE CAN'T GUARANTEE THE COMPORTMENT.</para>
	/// </remarks>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(TriggerSensitive3D))]
	public class Triggered3D : Triggered {
		//* <summary>
		//* 	Called after the <c>component</c> was created to initialized the pointer on the relative <see cref="FYFY_plugins.Trigger.TriggerSensitive3D._targets"/>.
		//* </summary>
		//* <remarks>
		//* 	<para>Callback automatically called by Unity.</para>
		//* </remarks>
		private void Awake() {
			TriggerSensitive3D ts3D = this.gameObject.GetComponent<TriggerSensitive3D>();
			_targets = ts3D._targets;
		}
	}
}