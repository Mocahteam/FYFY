using UnityEngine;

namespace FYFY {
	/// <summary>
	/// Base class for systems' wrappers
	/// </summary>
	public class BaseWrapper : MonoBehaviour
	{
		/// <summary>Reference to the wrapped system</summary>
		[HideInInspector]
		public FSystem system;
	}
}