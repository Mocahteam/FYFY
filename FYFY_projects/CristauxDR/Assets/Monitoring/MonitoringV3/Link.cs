using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using petriNetV2;

namespace monitorV3{
	/// <summary>
	/// Classe contenant les informations d'une contrainte
	/// </summary>
	[Serializable]
	public class Link {

		public string label; //label utilisé pour arithmétique booléenne pour la construction des contraintes pendnant la génération du réseau de petri
		public bool diffusion;
		public int placeId;
		public Vector2 scroll;

		public GameObject objLink;

		//Type de la contrainte
		public int flagsType;

		//Poid de la contrainte
		public int poids = 1;

		public Link(string label,bool diffusion, int placeId, GameObject obj, int poids)
        {
            this.label = label;
            this.diffusion = diffusion;
            this.placeId = placeId;

            this.objLink = obj;
            this.poids = poids;
        }
        public Link()
        {
        }

    }
}