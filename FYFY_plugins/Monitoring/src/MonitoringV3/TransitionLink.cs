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
		
		// copy all data from template except transition because its a reference of PetriNet
		internal void import(TransitionLink template){
			this.links = new List<Link>();
			foreach (Link l in template.links)
				this.links.Add(new Link(l));
			this.isSystemAction = template.isSystemAction;
			this.isEndAction = template.isEndAction;
			this.logic = template.logic;
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