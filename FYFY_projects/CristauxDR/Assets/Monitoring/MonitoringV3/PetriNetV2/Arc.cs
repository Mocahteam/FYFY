using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace petriNetV2{
	
	[Serializable]
	public enum ArcType{
		regular,
		test,
		inhibitor

	};

	[Serializable]
	public class Arc {
		public Node source;
		public Node target;
		public ArcType type;
		public int weight;
		public static string[] optType = new string[] {"test", "inhibitor"};

		public Arc (Node source,Node target, ArcType type, int weight)
		{
			this.source = source;
			this.target = target;
			this.type = type;
			this.weight = weight;
		}

		public static ArcType stringToArcType(string name){
			switch (name) {
				case "test":
					return ArcType.test;

				case "inhibitor":
					return ArcType.inhibitor;

				default:
					return ArcType.regular;
			}

		}

		public override string ToString ()
		{
			return string.Format ("[Arc: source={0}, target={1}]", source, target);
		}
				
	}

}
