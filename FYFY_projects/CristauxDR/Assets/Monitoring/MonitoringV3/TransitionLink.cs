using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using petriNetV2;


namespace monitorV3{
	/// <summary>
	/// 	Links on transition
	/// </summary>
	[Serializable]
	public class TransitionLink {
		public int flagsTransition;
		public List<Link> links;
		public Node transition;
		public string logic;

        public string label; // used for boolean arithmetic and links building during petrinet generation
        public bool diffusion;
        public int flagsLink;
        public Node place;

        public GameObject objLink;

        // Link type
        public int flagsType;

        // Link weight
        public int weight = 1;

		public TransitionLink(){
			links = new List<Link>();
		}

        public Link addLink(Node transition, string label, bool diffusion, int placeId, GameObject obj, int weight)
        {
            this.transition = transition;
			Link ctr = new Link(label, diffusion, placeId, obj, weight);
            links.Add(ctr);
            return ctr;
        }

        public Link getLabeledLink(string label){
			foreach(Link link in links){
				if (link.label.Equals (label))
					return link;
			}
			return null;
		}
	}
}