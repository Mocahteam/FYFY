using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace petriNetV2{
	
	[Serializable]
	public class PetriNet{
		
		public string label;
		public int id;
		public List<Node> transitions;
		public List<Node> places;
		public List<Arc> arcs;

		public PetriNet ()
		{
			id = 0;//suiviV1.IDGenerator.genID (); //TODO id dans PN ou dans composant ? plus simple dans composant...
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
			/*label = tmpNet.label;
			id = tmpNet.id;
			transitions = tmpNet.transitions;
			places = tmpNet.places;
			arcs = tmpNet.arcs;*/
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
					newTransition.Label = prefix + "_" + newTransition.Label;
				referencesMatcher.Add (oldTransition.label, newTransition);
				transitions.Add (newTransition);
			}

			places = new List<Node> ();
			foreach(Node oldPlace in old.places){
				Node newPlace = new Node (oldPlace);
				if (prefix != null && prefix != "")
					newPlace.Label = prefix + "_" + newPlace.Label;
				referencesMatcher.Add (oldPlace.label, newPlace);
				places.Add (new Node(newPlace));
			}
				
			//copy made this way to link new references
			arcs = new List<Arc> ();
			foreach (Arc oldArc in old.arcs) {

				Node newSource;
				referencesMatcher.TryGetValue (oldArc.Source.label,out newSource);

				Node newTarget;
				referencesMatcher.TryGetValue (oldArc.Target.label,out newTarget);

				arcs.Add (new Arc (newSource, newTarget, oldArc.Type, oldArc.Poid));
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
        public void clear(){

			transitions.Clear ();
			places.Clear ();
		}


		//TODO : Nettoyage des noms de cette merde
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
				//transition.name += "_"+pnet.id; //Permetre différentiation avec autres réseaux identiques
				transitions.Add (transition);
			}

			foreach (Node place in pnet.places) {
				//place.name += "_"+pnet.id;
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

		//Getters & Setters TODO Refactor if readonly
		public string Label {
			get {
				return this.label;
			}
			set {
				label = value;
			}
		}

		public int Id {
			get {
				return this.id;
			}
			set {
				id = value;
			}
		}

		public List<Node> Transitions {
			get {
				return this.transitions;
			}
			set {
				transitions = value;
			}
		}

		public List<Node> Places {
			get {
				return this.places;
			}
			set {
				places = value;
			}
		}
		public List<Arc> Arcs {
			get {
				return this.arcs;
			}
			set {
				arcs = value;
			}
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
				str += "\t\tSource : "+arc.Source.Label+"\n";
				str += "\t\tTarget : "+arc.Target.Label+"\n";
				str += "\t\tType : "+arc.Type+"\n";
				str += "\t\tPoid : "+arc.Poid+"\n";

			}
			str += "Places count : " + places.Count + " \n";
			str += "Transitions count : " + transitions.Count + " \n";
			str += "Edge count : " + arcs.Count + " \n";

			return str;
			//return string.Format ("[PetriNet: id={0}, name={1}, transitions={2}, places={3}, arcs={4}]", id, name, transitions.ToString(), places.ToString(), arcs.ToString());
		}
	}	


}
