using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace monitoring{
	/// <summary>
	/// Classe contenant les informations d'une contrainte
	/// </summary>
	[Serializable]
	public class Link {

		public string label; //label utilisé pour arithmétique booléenne pour la construction des contraintes pendnant la génération du réseau de petri
		public int type = 2; // 0 means Get, 1 means Produce, 2 means Require
		public int flagsType = 0; // 0 means "at least", 1 means "at most"
		public int weight = 1;
		public int placeId;
		public Vector2 scroll;

		public GameObject linkedObject;

		public Link(string label, int type, int placeId, GameObject obj, int weight)
        {
            this.label = label;
			this.type = type;
            this.placeId = placeId;

			this.linkedObject = obj;
            this.weight = weight;
        }
        public Link()
        {
        }

		public string [] getPlacesNameFromLinkedObject (){
			List<string> places = new List<string> ();
			if (linkedObject != null){
				foreach (ComponentMonitoring m in linkedObject.GetComponents<ComponentMonitoring> ()) {
					if (m.PetriNet != null && m.PnmlFile != null){
						foreach (string newItem in m.PetriNet.getPlacesNames ())
							places.Add (newItem+" ("+m.PnmlFile.name+")");
					}
				}
			}
			return places.ToArray();
		}

		public Node getPlaceFromLinkedObject (int i){
			if (linkedObject != null) {
				foreach (ComponentMonitoring m in linkedObject.GetComponents<ComponentMonitoring> ()) {
					if (m.PetriNet != null) {
						if (m.PetriNet.places.Count <= i)
							i -= m.PetriNet.places.Count;
						else
							return m.PetriNet.places[i];
					}
				}
			}
			return null;
		}
    }
}