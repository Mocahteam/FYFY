using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace petriNetV2{
	
	[Serializable]
	public class Node  {

		//Datas set to match pnml format
		[HideInInspector]
		public int id;
		public string label;
		public int initialMarking;
		[HideInInspector]
		public Vector2 offset;
		[HideInInspector]
		public Vector2 position;

		public Node(string label,int id, Vector2 offset, int initialMarking,Vector2 position){

			this.label = label;
            this.id = id;
			this.offset = offset;
			this.initialMarking = initialMarking;
			this.position = position;
		}
        public Node(string label, Vector2 offset, int initialMarking, Vector2 position)
        {

            this.label = label;
            this.offset = offset;
            this.initialMarking = initialMarking;
            this.position = position;
        }
        //Copy constructor
        public Node(Node old){
			label = old.label;
            id = old.id;
			offset = old.offset;
			initialMarking = old.initialMarking;
			position = old.position;
		}

		public override string ToString ()
		{
			return string.Format ("[Node: label={0}]", label);
		}
		
	}	

}
