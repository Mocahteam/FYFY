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
		public int poid;
		public static string[] optType = new string[] {"regular", "test", "inhibitor"}; //TODO Type arc dans classe arc

		public Arc (Node source,Node target, ArcType type,int poid)
		{
			this.source = source;
			this.target = target;
			this.type = type;
			this.poid = poid;
		}

		//Pas de sens de copier l'arc, perte des références... 
		//Copy constructor :
		//public Arc(Arc old){
		//	source = old.source;//ref ?
		//	target = old.target; //ref ? 
		//	type = old.Type;
		//	poids = old.poids;			
		//}

		public static ArcType stringToArcType(string name){
			switch (name) {
				case "regular":
					return ArcType.regular;

				case "test":
					return ArcType.test;

				case "inhibitor":
					return ArcType.inhibitor;

				default:
					return ArcType.regular;
			}

		}

		//Getters & Setters TODO Refactor if readonly
		public Node Source {
			get {
				return this.source;
			}
			set {
				source = value;
			}
		}

		public Node Target {
			get {
				return this.target;
			}
			set {
				target = value;
			}
		}

		public ArcType Type {
			get {
				return this.type;
			}
			set {
				type = value;
			}
		}

		public int Poid {
			get {
				return this.poid;
			}
			set {
				poid = value;
			}
		}
	public override string ToString ()
		{
			return string.Format ("[Arc: source={0}, target={1}]", source, target);
		}
				
	}

}
