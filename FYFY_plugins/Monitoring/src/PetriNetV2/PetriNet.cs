﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Monitoring_Inspector")]   // ugly

namespace FYFY_plugins.Monitoring{
	
	/// <summary>
	///		Store the PetriNet structure used by its monitor.
	/// </summary>
	[Serializable]
	public class PetriNet{
		[SerializeField]
		internal string label;
		[SerializeField]
		internal int id;
		[SerializeField]
		internal List<Node> transitions;
		/// <summary>List of places of this PetriNet.</summary> 
		public List<Node> places;
		[SerializeField]
		internal List<Arc> arcs;

		internal PetriNet ()
		{
			id = 0;
			transitions = new List<Node> ();
			places = new List<Node> ();
			arcs = new List<Arc> ();
		}


		/// <summary>Parse and load pnml file.</summary>
		public static PetriNet loadFromFile(string file,int id)
		{
            PetriNet pn = PnmlParser.loadFromPath(file);
            pn.attachID(id);
            return pn;
		}

		//Copy contructor
		internal PetriNet(PetriNet old, string prefix = ""){

			label = old.label;
            id = old.id;


			Dictionary<string,Node> referencesMatcher = new Dictionary<string,Node>();

			transitions = new List<Node> ();
			foreach (Node oldTransition in old.transitions) {
				Node newTransition = new Node (oldTransition);
				if (prefix != null && prefix != "")
					newTransition.label = prefix + "_" + newTransition.label;
				referencesMatcher.Add (oldTransition.label, newTransition);
				transitions.Add (newTransition);
			}

			places = new List<Node> ();
			foreach(Node oldPlace in old.places){
				Node newPlace = new Node (oldPlace);
				if (prefix != null && prefix != "")
					newPlace.label = prefix + "_" + newPlace.label;
				referencesMatcher.Add (oldPlace.label, newPlace);
				places.Add (new Node(newPlace));
			}
				
			//copy made this way to link new references
			arcs = new List<Arc> ();
			foreach (Arc oldArc in old.arcs) {

				Node newSource;
				referencesMatcher.TryGetValue (oldArc.source.label,out newSource);

				Node newTarget;
				referencesMatcher.TryGetValue (oldArc.target.label,out newTarget);

				arcs.Add (new Arc (newSource, newTarget, oldArc.type, oldArc.weight));
			}	
		}

		internal void attachID(int id)
        {
            this.id = id;
            foreach (Node transition in transitions)
            {
                transition.id = id;
            }


            foreach (Node place in places)
            {
                place.id = id;
            }
        }

        //PetriNet constructions fonctions :
        //Add Remove etc... 
        internal void clear(){

			transitions.Clear ();
			places.Clear ();
		}

		internal Arc[] getConcernedArcs(Node n){
			List<Arc> lArcs = new List<Arc>();
			foreach (Arc a in arcs) {
				if (a.target.label.Equals(n.label) || a.source.label.Equals(n.label)) {
					lArcs.Add (a);	
				}
			}
			return lArcs.ToArray ();
		}

		internal void addSubNet(PetriNet pnet){

			foreach (Node transition in pnet.transitions) {
				transitions.Add (transition);
			}

			foreach (Node place in pnet.places) {
				places.Add (place);
			}
			foreach (Arc arc in pnet.arcs) {
				arcs.Add (arc);
			}

		}

		/// <summary>Returns the name of transitions defined into the PetriNet.</summary>
		public string[] getTransitionsNames(){
			List<string> listeLabelsTransitions = new List<String>();
			foreach (Node transition in transitions) {
				if (transition.overridedLabel != null && !transition.overridedLabel.Equals(""))
					listeLabelsTransitions.Add (transition.overridedLabel);
				else
					listeLabelsTransitions.Add (transition.label);
			}
		
			return listeLabelsTransitions.ToArray ();
		}

		/// <summary>Returns the name of places defined into the PetriNet.</summary>
		public string[] getPlacesNames(){
			List<string> listeLabelsPlaces = new List<String>();

			foreach (Node place in places) {
				if (place.overridedLabel != null && !place.overridedLabel.Equals(""))
					listeLabelsPlaces.Add (place.overridedLabel);
				else
					listeLabelsPlaces.Add (place.label);
			}

			return listeLabelsPlaces.ToArray ();
		}

        internal Node getPlaceByName(string str)
        {
            foreach(Node node in places)
            {
                if (node.label.Equals(str))
                    return node;
            }
            return null;
        }

		internal float getWidth(){
			float max = 0;
			foreach (Node nd in transitions) {
				if (nd.position.x > max)
					max = nd.position.x;

			}
			foreach (Node nd in places) {
				if (nd.position.x > max)
					max = nd.position.x;

			}
			return max;
		}

		internal float getHeight(){
			float max = 0;
			foreach (Node nd in transitions) {
				if (nd.position.y > max)
					max = nd.position.y;

			}
			foreach (Node nd in places) {
				if (nd.position.y > max)
					max = nd.position.y;

			}
			return max;
		}

		internal void addWidth(float width){

			foreach (Node nd in transitions) {
				nd.position.x += width;

			}
			foreach (Node nd in places) {
				nd.position.x += width;

			}
		}

		internal void addHeight(float height){

			foreach (Node nd in transitions) {
				nd.position.y += height;

			}
			foreach (Node nd in places) {
				nd.position.y += height;

			}
		}

		internal void removeWidth(float width){

			foreach (Node nd in transitions) {
				nd.position.x -= width;

			}
			foreach (Node nd in places) {
				nd.position.x -= width;

			}
		}

		internal void removeHeight(float height){

			foreach (Node nd in transitions) {
				nd.position.y -= height;

			}
			foreach (Node nd in places) {
				nd.position.y -= height;

			}
		}
		
		/// <summary>Returns a string that represents the current PetriNet.</summary>
		public override string ToString ()
		{

			string str = "PetriNet:  name=" + label+"\n";
			int cpt = 0;
			foreach (Node transition in transitions) {

				str += "\tTransition " + (++cpt) + " :\n";
				//			str += "\t\tNom : "+transition.id+"\n";
				str += "\t\tNom : "+transition.label+"\n";
				str += "\t\tMarquage initial : "+transition.initialMarking+"\n";
				str += "\t\tOffset : "+transition.offset+"\n";
				str += "\t\tPosition : "+transition.position+"\n";
			}
			cpt = 0;
			foreach (Node place in places) {
				str += "\tPlace " + (++cpt) + " :\n";
				//			str += "\t\tNom : "+place.id+"\n";
				str += "\t\tNom : "+place.label+"\n";
				str += "\t\tMarquage initial : "+place.initialMarking+"\n";
				str += "\t\tOffset : "+place.offset+"\n";
				str += "\t\tPosition : "+place.position+"\n";

			}

			cpt = 0;

			foreach (Arc arc in arcs) {
				str += "\tEdge " + (++cpt) + " :\n";
				str += "\t\tSource : "+arc.source.label+"\n";
				str += "\t\tTarget : "+arc.target.label+"\n";
				str += "\t\tType : "+arc.type+"\n";
				str += "\t\tPoid : "+arc.weight+"\n";

			}
			str += "Places count : " + places.Count + " \n";
			str += "Transitions count : " + transitions.Count + " \n";
			str += "Edge count : " + arcs.Count + " \n";

			return str;
			//return string.Format ("[PetriNet: id={0}, name={1}, transitions={2}, places={3}, arcs={4}]", id, name, transitions.ToString(), places.ToString(), arcs.ToString());
		}
	}	


}
