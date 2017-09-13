using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace monitoring
{
    public static class IDGenerator
    {

        //Ne marche pas comme convenu, debug à faire...Stack<Tuple<int,int>> inscrit = new Stack<Tuple<int,int>>();


        public static int genID()
        {
            List<int> inscrits = new List<int>();

            foreach (ComponentMonitoring go in UnityEngine.Object.FindObjectsOfType<ComponentMonitoring>())
            {
                if (go.id != -1)
                { //Fonctionne étrangement :o, ne vérifie pas vraiment si null
                    inscrits.Add(go.id);
                    //Debug.Log ("GameObject : "+go+" PN : "+go.pn+" inscrit avec id : " + go.pn.id);
                }
            }

            inscrits.Sort();

            //Debug.Log ("inscrits sort : ");
            //		foreach (int i in inscrits.Distinct()) {
            //			Debug.Log (i);
            //		}
            //Debug.Log ("fin sort");
            int curseur = 0;
            foreach (int i in inscrits.Distinct())
            {
                if (curseur != i)
                {
                    //Debug.Log ("Dans check curseur : " + curseur);
                    return curseur;
                }

                curseur++;
            }
            //Debug.Log ("Après check curseur : " + curseur);
            return curseur;
        }

        internal static bool isAviable(int id)
        {
            List<int> inscrits = new List<int>();

            foreach (ComponentMonitoring go in UnityEngine.Object.FindObjectsOfType<ComponentMonitoring>())
            {
                if (go.id != -1)
                { //Fonctionne étrangement :o, ne vérifie pas vraiment si null
                    inscrits.Add(go.id);
                    //Debug.Log ("GameObject : "+go+" PN : "+go.pn+" inscrit avec id : " + go.pn.id);
                }
            }

            if (inscrits.Contains(id))
                return false;

            return true;
        }
    }
}
