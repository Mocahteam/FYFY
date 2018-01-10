using UnityEngine;

namespace FYFY_plugins.PointerManager {
	/// <summary>
	/// 	Component specifying that the pointer is over the <c>GameObject</c>.
	/// </summary>
	/// <remarks>
	/// 	Automatically added or removed by the relative <see cref="FYFY_plugins.PointerManager.PointerSensitive">component</see>.
	/// </remarks>
	[RequireComponent(typeof(PointerSensitive))]
	[AddComponentMenu("")]
	public class PointerOver : MonoBehaviour {
	}
}