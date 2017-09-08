using petriNetV2;
using monitorV3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraceHelper {
	
    public string type = "";
    public ComponentMonitoring monitor = null;
    

    public TraceHelper(string typeOfTrace,  ComponentMonitoring monitor)
    {
        this.type = typeOfTrace;
        this.monitor = monitor;
    }


    public void trace(string type, params GameObject[] objs)
    {

        string prefix = "";
        bool aMap = false;
        TransitionLink transitionLink  = monitor.getTransitionLinkByTransitionLabel(type);
        if (transitionLink != null)
        {
            AriParser arip = new AriParser();
            string logique = transitionLink.logic;
            if (arip.validAri(transitionLink))
            {
                ////Debug.Log("ari valide");
                string[] ari = arip.getDistribution(logique);
                List<List<int>> mapper = new List<List<int>>();
                List<int> tmpList = new List<int>();
                //Debug.Log("count : " + ari.Length);
                foreach (string s in ari)
                {
                    if (!s.Equals("+") && !s.Equals("*"))
                    {
						Link curLink = transitionLink.getLabeledLink(s);
						Node place = curLink.getPlaceFromLinkedObject(curLink.placeId);
                        tmpList.Add(place.id);
                    }
                    else if (s.Equals("+"))
                    {
                        mapper.Add(tmpList);
                        tmpList = new List<int>();
                    }
                }
                mapper.Add(tmpList);

                List<int> test = new List<int>();
                foreach(GameObject obj in objs)
                {
                    ComponentMonitoring objSuivi = obj.GetComponent<ComponentMonitoring>();
                    if (objSuivi != null && objSuivi.petriNet != null)
                    {
                        test.Add(objSuivi.petriNet.id);
                    }
                }
                test.Sort();
                //Debug.Log("liste test : ");
                printList(test);


                for(int i = 0; i < mapper.Count; i++) //Pour chaque groupe
                {
                    mapper[i].Sort();

                    //Debug.Log("liste mapper : ");
                    printList(mapper[i]);

                    if (mapper[i].SequenceEqual(test))
                    {
                        //Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa");
                        if(i > 0)prefix = "or" + (i - 1)+"_";
                        //Debug.Log("J'ai mappé");
                        aMap = true;
                    }

                }
                //Debug groups :
                /*foreach (List<string> group in mapper)
                {
                    ////Debug.Log(label + "Group :");
                    foreach (string label in group)
                    {
                        //Dans un groupe
                        //Debug.Log("\tlabel : " + label);
                    }
                }*/
            }
            else
            {
                ////Debug.Log("ari pas valide");
            }

            //Verif suivi pour construction des Or
            if (aMap) { /*Debug.Log(system+" : "+prefix + label + "_" + suivi.id);*/ TraceHandler.trace(type, prefix + type + "_" + monitor.id); } //<--- C'est ici qu'on trace
            else { Debug.Log("Erreur paramètres"); }
        }
        else
        {
            Debug.Log("Erreur label trace...");
        }
    }
    void printList(List<int> list)
    {
        string str = "";
        foreach (int i in list)
        {
            str += (i + " ");
        }
        //Debug.Log("List : " + str);
    }
}
