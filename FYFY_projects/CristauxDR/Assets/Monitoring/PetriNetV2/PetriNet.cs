using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace monitoring{
	
	[Serializable]
	public class PetriNet{
		
		public string label;
		[HideInInspector]
		public int id;
		public List<Node> transitions;
		public List<Node> places;
		public List<Arc> arcs;

		public PetriNet ()
		{
			id = 0;
			transitions = new List<Node> ();
			places = new List<Node> ();
			arcs = new List<Arc> ();
		}


		//Parse pnml file
		public static PetriNet loadFromFile(string file,int id)
		{
            PetriNet pn = PnmlParser.loadFromPath(file, id);
            pn.attachID(id);
            return pn;
		}

		//Copy contructor
		public PetriNet(PetriNet old, string prefix = ""){

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

		public void attachID(int id)
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
        public void clear(){

			transitions.Clear ();
			places.Clear ();
		}

		public Arc[] getConcernedArcs(Node n){
			List<Arc> lArcs = new List<Arc>();
			foreach (Arc a in arcs) {
				if (a.target.label.Equals(n.label) || a.source.label.Equals(n.label)) {
					lArcs.Add (a);	
				}
			}
			return lArcs.ToArray ();
		}

		public void addSubNet(PetriNet pnet){

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

		public string[] getTransitionsNames(){
			List<string> listeLabelsTransitions = new List<String>();

			foreach (Node transition in transitions) {
				listeLabelsTransitions.Add (transition.label);
			}
		
			return listeLabelsTransitions.ToArray ();
		}

		public string[] getPlacesNames(){
			List<string> listeLabelsPlaces = new List<String>();

			foreach (Node place in places) {
				listeLabelsPlaces.Add (place.label);
			}

			return listeLabelsPlaces.ToArray ();
		}

        public Node getPlaceByName(string str)
        {
            foreach(Node node in places)
            {
                if (node.label.Equals(str))
                    return node;
            }
            return null;
        }

		//Les fonctions width et height suivantes servent à gérer la position du réseau dans sa représentation graphique
		public float getWidth(){
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

		public float getHeight(){
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

		public void addWidth(float width){

			foreach (Node nd in transitions) {
				nd.position.x += width;

			}
			foreach (Node nd in places) {
				nd.position.x += width;

			}
		}

		public void addHeight(float height){

			foreach (Node nd in transitions) {
				nd.position.y += height;

			}
			foreach (Node nd in places) {
				nd.position.y += height;

			}
		}

		public void removeWidth(float width){

			foreach (Node nd in transitions) {
				nd.position.x -= width;

			}
			foreach (Node nd in places) {
				nd.position.x -= width;

			}
		}

		public void removeHeight(float height){

			foreach (Node nd in transitions) {
				nd.position.y -= height;

			}
			foreach (Node nd in places) {
				nd.position.y -= height;

			}
		}

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
