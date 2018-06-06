using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FYFY;

namespace FYFY_plugins.Monitoring{
	/// <summary>
	/// 	Add monitoring functionalities to a Family
	/// </summary>
	[ExecuteInEditMode] // Awake, Start... will be call in edit mode
	[AddComponentMenu("")]
	public class FamilyMonitoring : ComponentMonitoring
    {
		// TODO: add this boolean to automatically include ComponentMonitoring in all GameObjects of this family
        //public bool suiviGlobal = true;
		
		/// <summary> Name of an equivalent family name in an other system. </summary>
		[HideInInspector] 
		public string equivalentName;
		
		/// <summary> descriptor of the Family associated to this monitor. </summary>
		[HideInInspector]
		public string[] descriptor;
	}
}