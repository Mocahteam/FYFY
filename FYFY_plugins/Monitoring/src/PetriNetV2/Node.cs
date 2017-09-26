using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FYFY_plugins.Monitoring{
	
	/// <summary>Defines a node of the PetriNet. It could be a place or a trasition.</summary>
	[Serializable]
	public class Node  {

		//Datas set to match pnml format
		internal int id;
		/// <summary>The label of this node.</summary>
		public string label;
		/// <summary>
		///		If Node is a place, contains the initial marking of this place.
		///		If Node is a transition, contains -1
		/// </summary>
		public int initialMarking;
		internal Vector2 offset;
		internal Vector2 position;

		internal Node(string label,int id, Vector2 offset, int initialMarking,Vector2 position){

			this.label = label;
            this.id = id;
			this.offset = offset;
			this.initialMarking = initialMarking;
			this.position = position;
		}
        internal Node(string label, Vector2 offset, int initialMarking, Vector2 position)
        {

            this.label = label;
            this.offset = offset;
            this.initialMarking = initialMarking;
            this.position = position;
        }
        //Copy constructor
        internal Node(Node old){
			label = old.label;
            id = old.id;
			offset = old.offset;
			initialMarking = old.initialMarking;
			position = old.position;
		}

		/// <summary>Returns a string that represents the current Node.</summary>
		public override string ToString ()
		{
			return string.Format ("[Node: label={0}]", label);
		}
		
	}	

}
