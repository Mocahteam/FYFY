using UnityEngine;
using UnityEditor;

namespace FYFY_plugins.Monitoring {
	/// <summary>
	/// 	Avoid to modify property in Inspector if [ReadOnly] meta tag is used
	/// </summary>
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		/// <summary>
		/// 	
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property,
												GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		/// <summary>
		/// 	
		/// </summary>
		public override void OnGUI(Rect position,
								   SerializedProperty property,
								   GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}