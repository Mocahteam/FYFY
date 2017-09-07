using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using petriNetV2;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace monitorV3{
	/// <summary>
	/// Composant à attacher au gameObject pour gérer sa représentation par un rdp
	/// </summary>
	[Serializable]
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ComponentMonitoring : MonoBehaviour
    {

        public delegate void traceFunc(string system ,params GameObject[] objects);
        public Dictionary<string, traceFunc> trace = new Dictionary<string, traceFunc>();
        
        //[HideInInspector]
        public UnityEngine.Object PnmlFile;

		public string comments;

		//[HideInInspector]
		public int id = -1;

		//[HideInInspector] //It's perhaps intersting to know the PN into inspector?
		public PetriNet petriNet;

		//[HideInInspector] 
		public List<TransitionLink> transitionLinks = new List<TransitionLink>();

		// Usefull to detect copies in order to generate new ID
		[SerializeField]
		private int instanceID = 0;

        //Fonctions utilitaires :
        public int getID(){
			id = IDGenerator.genID ();
			return id;
		}


        public TransitionLink getTransitionLinkByTransitionLabel(String label)
        {
			//Debug.Log("tc count : "+transitionLinks.Count);
            foreach(TransitionLink tc in transitionLinks)
            {
                //Debug.Log("getByLabel : "+tc.label);
                if (tc.transition.label.Equals(label))
                    return tc;
            }
            return null;
        }

        public void loadData()
        {
            //Debug.Log("loadData");
            /*string file = AssetDatabase.GetAssetPath(fichierPNML);
            getID();
            petriNet = PetriNet.loadFromFile(file, id);
            oldFichierPNML = fichierPNML;*/
            foreach(Node transition in petriNet.transitions)
            {
                Trace traceur = new Trace(transition.label, this);
                trace.Add(transition.label,traceur.trace);
            }

        }
        string printList(List<int> list)
        {
            string str = "";
            foreach (int i in list)
            {
                str += (i + " ");
            }
            //Debug.Log("List : " + str);
            return str;
        }

        public void Start()
        {
#if UNITY_EDITOR
            
            /*if (!Application.isPlaying && id == -1 && IDGenerator.isAviable(id))
            {
                id = getID();
                if (fichierPNML != null)
                    petriNet = PetriNet.loadFromFile(AssetDatabase.GetAssetPath(fichierPNML), id);
            }*/
            if (Application.isPlaying)
            {
#endif
                //Debug.Log("Start");
                ComponentMonitoring sc = GetComponent<ComponentMonitoring>();
                if (sc != null)
                {
                    //Debug.Log("PetriNet : " + sc.petriNet.label);
                    //Debug.Log("Places : " + sc.petriNet.transitions.Count);
                    //Debug.Log("Transitions : " + sc.petriNet.places.Count);
                    //Debug.Log("TransitionsLinks : " + sc.transitionLinks.Count);
                }
                loadData();
                //if (trace.ContainsKey("deverouiller"))
                //    debugTest();
#if UNITY_EDITOR
            }
#endif            
        }


        public void debugTest()
        {
            GameObject cle1 = GameObject.Find("Cle1");
            GameObject cle2 = GameObject.Find("Cle2");
            GameObject cle3 = GameObject.Find("Cle3");
            GameObject cle4 = GameObject.Find("Cle4");
            trace["ouvrir"]("player");
            trace["ouvrir"]("player",cle1);
            trace["deverouiller"]("player");
            trace["deverouiller"]("player",cle2);
            trace["deverouiller"]("player",cle2,cle1);
            trace["deverouiller"]("player",cle1,cle2);
            trace["deverouiller"]("player",cle1, cle3);
            trace["deverouiller"]("player",cle4, cle2);
            trace["deverouiller"]("player",cle4);
            trace["deverouiller"]("player",cle3);
            trace["deverouiller"]("player",cle4, cle3);
        }
#if UNITY_EDITOR

        //TODO: Move this code somewhere else to disconnect editor from go
        void Awake()
        {
            if (!Application.isPlaying && (id == -1 || !IDGenerator.isAviable(id)))
            {
                if (instanceID != GetInstanceID())
                {
                    if (instanceID == 0)
                    {
                        instanceID = GetInstanceID();
                        id = getID();
                        if (PnmlFile != null)
                            petriNet = PetriNet.loadFromFile(AssetDatabase.GetAssetPath(PnmlFile), id);
                    }
                    else
                    {
                        instanceID = GetInstanceID();
                        if (instanceID < 0)
                        {
                            id = getID();
                            if(PnmlFile != null)
                                petriNet = PetriNet.loadFromFile(AssetDatabase.GetAssetPath(PnmlFile), id);
                        }
                    }
                }

            }
        }

        internal void loadTransitionsLinks()
        {
            if (transitionLinks.Count != petriNet.transitions.Count)
            {
                foreach (Node transition in petriNet.transitions)
                {
                    TransitionLink t = new TransitionLink();
                    t.transition = transition;
                    transitionLinks.Add(t);
                }
            }
        }
#endif
    }
}