using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FYFY_plugins.Monitoring{
	
	[Serializable]
	internal enum ArcType{
		regular,
		test,
		inhibitor

	};

	[Serializable]
	internal class Arc {
		internal Node source;
		internal Node target;
		internal ArcType type;
		internal int weight;
		internal static string[] optType = new string[] {"test", "inhibitor"};

		internal Arc (Node source,Node target, ArcType type, int weight)
		{
			this.source = source;
			this.target = target;
			this.type = type;
			this.weight = weight;
		}

		internal static ArcType stringToArcType(string name){
			switch (name) {
				case "test":
					return ArcType.test;

				case "inhibitor":
					return ArcType.inhibitor;

				default:
					return ArcType.regular;
			}

		}

		/// <summary>Returns a string that represents the current Arc.</summary>
		public override string ToString ()
		{
			return string.Format ("[Arc: source={0}, target={1}]", source, target);
		}
				
	}

}
