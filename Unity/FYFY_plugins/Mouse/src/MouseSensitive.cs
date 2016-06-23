using UnityEngine;

namespace FYFY_plugins.Mouse {
	/// <summary>
	/// 	Component allowing <c>GameObject</c> to be sensitive to the mouse.
	/// 	Add automatically a <see cref="FYFY_plugins.Mouse.MouseOver">component</see> when the mouse points the <c>GameObject</c>.
	/// </summary>
	/// <remarks>
	/// 	<para>
	///			This component works only on objects that aren't on Ignore Raycast layer 
	///			and which have a collider marked as Trigger (with <c>Physics.queriesHitTriggers</c> true).
	///		</para>
	/// </remarks>
	//* <remarks>
	//* 	<para>Overlay of the <c>Unity Mouse System</c>.</para>
	//* 	<para>It's a tricky component with non FYFY conventional design.</para>
	//* 	<para>It uses Unity logic inside so it can't be considered like a real FYFY component.</para>
	//* </remarks>
	[DisallowMultipleComponent]
	public class MouseSensitive : MonoBehaviour {
		//* <summary>
		//* 	Called when the mouse enters the <c>GameObject</c>.
		//* </summary>
		//* <remarks>
		//* 	<para>Callback automatically called by Unity.</para>
		//* 	<para>Add a <see cref="FYFY_plugins.Mouse.MouseOver">component</see> to the <c>GameObject</c>.</para>
		//* </remarks>
		private void OnMouseEnter() {
			FYFY.GameObjectManager.addComponent<MouseOver>(this.gameObject);
		}

		//* <summary>
		//* 	Called when the mouse is not any longer over the <c>GameObject</c>.
		//* </summary>
		//* <remarks>
		//* 	<para>Callback automatically called by Unity.</para>
		//* 	<para>Remove the <see cref="FYFY_plugins.Mouse.MouseOver">component</see> attached to the <c>GameObject</c>.</para>
		//* </remarks>
		private void OnMouseExit() {
			FYFY.GameObjectManager.removeComponent<MouseOver>(this.gameObject);
		}
	}
}