using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace FYFY_plugins.Monitoring{
	/// <summary>
	/// 	Links on transition
	/// </summary>
	[Serializable]
	public class TransitionLink {
		/// <summary>List of links associated to this transition.</summary>
		public List<Link> links;
		/// <summary>The transition linked.</summary>
		public Node transition;
		/// <summary>Define if this transition is a system action.</summary>
		public bool isSystemAction = false;
		/// <summary>Define if this transition is an end action.</summary>
		public bool isEndAction = false;
		/// <summary>The logic expression for this transition.</summary>
		public string logic = "";

		internal TransitionLink(){
			links = new List<Link>();
		}

		internal Link addLink(Node transition, string label, int type, int placeId, GameObject obj, int weight)
        {
            this.transition = transition;
			Link ctr = new Link(label, type, placeId, obj, weight);
            links.Add(ctr);
            return ctr;
        }

        internal Link getLabeledLink(string label){
			foreach(Link link in links){
				if (link.label.Equals (label))
					return link;
			}
			return null;
		}
	}
}