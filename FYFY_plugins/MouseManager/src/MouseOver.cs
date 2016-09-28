using UnityEngine;

namespace FYFY_plugins.MouseManager {
	/// <summary>
	/// 	Component specifying that the mouse is over the <c>GameObject</c>.
	/// </summary>
	/// <remarks>
	/// 	Automatically added or removed by the relative <see cref="FYFY_plugins.MouseManager.MouseSensitive">component</see>.
	/// </remarks>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MouseSensitive))]
	[AddComponentMenu("")]
	public class MouseOver : MonoBehaviour {
	}
}