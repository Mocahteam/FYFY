#if UNITY_EDITOR

using petriNetV2;
using monitorV3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class TraceHelper {

    [MenuItem("FYFY/Generate Trace and Init Helpers")]
    public static void gen()
    {
        genAutoInit();
        genAutoTrace();
    }

    public static void genAutoTrace()
    {
        Debug.Log("Generate");
        //Récupérer tous les objets suivis, familles compris :

        ComponentMonitoring[] suivis = Resources.FindObjectsOfTypeAll(typeof(ComponentMonitoring)) as ComponentMonitoring[];
        Dictionary<string, List<string>> traceSuiviPNmapper = new Dictionary<string, List<string>>();
        //Dictionary<string, List<string>> traceSuiviPNmapper = new Dictionary<string, List<string>>();
        //Debug.Log("count suivi : "+suivis.Length);

        foreach (ComponentMonitoring suivi in suivis)
        {
            //Debug.Log("count tc : "+suivi.transitionsContraintes.Count);

            //Récupération des contraintes dupliquées
            foreach (TransitionLink transitionContrainte in suivi.transitionLinks)
            {


                string pnLabel = transitionContrainte.transition.label;
                //Création de l'entrée dans le dictionnaire :
                if (!traceSuiviPNmapper.ContainsKey(pnLabel))
                {
                    traceSuiviPNmapper.Add(pnLabel, new List<string>());
                }

                AriParser arip = new AriParser();
                if (arip.validAri(transitionContrainte))
                    Debug.Log("ari valide");
                else
                {
                    Debug.Log("ari pas valide");

                }
                string[] ari = arip.getDistribution(transitionContrainte.logic);
                List<string> tmpLabels = new List<string>(); //Tmp list to sort

                if (ari != null)
                {
                    foreach (string s in ari)
                    {
                        if (!s.Equals("+") && !s.Equals("*"))
                        {
                            Link curConst = transitionContrainte.getLabeledLink(s);
                            Debug.Log(s);
                            Debug.Log(curConst.objLink.GetComponent<ComponentMonitoring>().petriNet.label);
                            tmpLabels.Add(curConst.objLink.GetComponent<ComponentMonitoring>().petriNet.label);
                        }
                    }
                    tmpLabels.Sort();

                    int cpt = 0;
                    string sufixLabel = transitionContrainte.transition.label + "_";
                    foreach (string s in ari)
                    {
                        if (!s.Equals("+") && !s.Equals("*"))
                        {
                            //Contrainte curConst = transitionContrainte.getLabeledConstraint(s);
                            sufixLabel += tmpLabels[cpt++]; //Super sale
                        }
                        else if (s.Equals("*"))
                        {
                            sufixLabel += "_and_";
                        }
                        else if (s.Equals("+"))
                        {

                            traceSuiviPNmapper[pnLabel].Add(sufixLabel);
                            sufixLabel = transitionContrainte.transition.label + "_";
                        }
                    }

                    traceSuiviPNmapper[pnLabel].Add(sufixLabel);
                }
                //Une fois ça fait il faudrait ajouter les autres transitions dans la liste avec le label normal :
                /*foreach(Node transition in suivi.petriNet.transitions)
                {
                    traceSuiviPNmapper[pnLabel].Add(transition.label);
                }
                //Pas ici ...*/ 
            }
        }
        foreach (ComponentMonitoring suivi in suivis)
        {
            //Génération des aides de trace
            foreach (KeyValuePair<string, List<string>> kvp in traceSuiviPNmapper)
            {
                Debug.Log("key : " + kvp.Key);
                foreach (string str in kvp.Value)
                    Debug.Log("\tEntry : " + str);
            }
            string path = Application.dataPath + @"/Scripts/Gen/AutoTrace_" + suivi.petriNet.label.Replace(" ", "_") + @".cs";
            Debug.Log("Create file : " + path);

            if (suivi.petriNet != null)
            {

                try
                {

                    // Delete the file if it exists.
                    if (File.Exists(path))
                    {
                        // Note that no lock is put on the
                        // file and the possibility exists
                        // that another process could do
                        // something with it between
                        // the calls to Exists and Delete.
                        File.Delete(path);
                    }

                    // Create the file.
                    using (FileStream fs = File.Create(path))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                        // Add some information to the file.
                        fs.Write(info, 0, info.Length);
                    }
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("using System.Collections;");
                        sw.WriteLine("using System.Collections.Generic;");
						sw.WriteLine("using UnityEngine;");
						sw.WriteLine("using monitorV3;");
                        sw.WriteLine();
                        sw.WriteLine("public static class AutoTrace_" + suivi.petriNet.label.Replace(" ", "_") + " {");
                        sw.WriteLine();

                        foreach (KeyValuePair<string, List<string>> kvp in traceSuiviPNmapper)
                        {
                            Debug.Log("key : " + kvp.Key);
                            foreach (string str in kvp.Value)
                                Debug.Log("\tEntry : " + str);
                        }

                        foreach (Node t in suivi.petriNet.transitions)
                        {
                            
                            if (traceSuiviPNmapper[t.label].Count != 0)
                            {
                                //V1
                                bool first = true;
                                int cptor = 0;
                                foreach (string str in traceSuiviPNmapper[t.label])
                                {

                                    sw.WriteLine("\t" + "public static void genTrace_" + (first ? "" : ("or" + (cptor) + "_")) + str + "(GameObject go){");
                                    sw.WriteLine("\t\t" + "Debug.Log(\"Trace : " +(first?"":("or"+(cptor++)+"_")) +t.label.Replace(" ", "_") + "_\"+go.GetComponent<ComponentMonitoring>().id);");
                                    sw.WriteLine("\t" + "}");
                                    sw.WriteLine();
                                    first = false;
                                }

                                //V2
                                /*sw.WriteLine("\t" + "public static void genTraceV2_" + (first ? "" : ("or" + (cptor) + "_")) + str + "(GameObject go");
                                foreach (string str in traceSuiviPNmapper[t.label])
                                {
                                    sw.WriteLine(",GameObject g1");
                                }
                                sw.WriteLine("){");
                                sw.WriteLine("\t\t" + "Debug.Log(\"Trace : " + (first ? "" : ("or" + (cptor++) + "_")) + t.label.Replace(" ", "_") + "_\"+go.GetComponent<ComponentMonitoring>().id);");
                                sw.WriteLine("\t" + "}");
                                sw.WriteLine();*/
                                
                            }
                            else
                            {
                                sw.WriteLine("\t" + "public static void genTrace_" + t.label.Replace(" ", "_") + "(GameObject go){");
                                sw.WriteLine("\t\t" + "Debug.Log(\"Trace : " + t.label.Replace(" ", "_") + "_\"+go.GetComponent<ComponentMonitoring>().id);");
                                sw.WriteLine("\t" + "}");
                                sw.WriteLine();
                            }


                        }

                        sw.WriteLine("}");
                    }
                    AssetDatabase.Refresh();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }

    //[MenuItem("FYFY/Generate Init Helper")]
    public static void genAutoInit()
    {
        Debug.Log("Generate");
        //Récupérer tous les objets suivis, familles compris :

        ComponentMonitoring[] suivis = Resources.FindObjectsOfTypeAll(typeof(ComponentMonitoring)) as ComponentMonitoring[];

        foreach (ComponentMonitoring suivi in suivis)
        {
            string path = Application.dataPath + @"/Scripts/Gen/AutoInit_" + suivi.petriNet.label.Replace(" ", "_") + @".cs";
            Debug.Log("Create file : " + path);

            if (suivi.petriNet != null)
            {

                try
                {

                    // Delete the file if it exists.
                    if (File.Exists(path))
                    {
                        // Note that no lock is put on the
                        // file and the possibility exists
                        // that another process could do
                        // something with it between
                        // the calls to Exists and Delete.
                        File.Delete(path);
                    }


                    // Create the file.
                    using (FileStream fs = File.Create(path))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                        // Add some information to the file.
                        fs.Write(info, 0, info.Length);
                    }
                    using (StreamWriter sw = File.CreateText(path))
                    { //How to get node ? get by name and id ? just by name ?
                        sw.WriteLine("using System.Collections;");
                        sw.WriteLine("using System.Collections.Generic;");
                        sw.WriteLine("using UnityEngine;");
                        sw.WriteLine("using System.Linq;");
						sw.WriteLine("using petriNetV2;");
						sw.WriteLine("using monitorV3;");
                        sw.WriteLine();
                        sw.WriteLine("public static class AutoInit_" + suivi.petriNet.label.Replace(" ", "_") + " {");
                        sw.WriteLine();

                        foreach (Node t in suivi.petriNet.places)
                        {
                            sw.WriteLine("\t" + "public static void init_" + t.label.Replace(" ", "_") + "(GameObject go, int i){");
                            sw.WriteLine("\t\t Node node = go.GetComponent<ComponentMonitoring>().petriNet.getPlaceByName(\""+t.label+"\");");
                            sw.WriteLine("\t\t if(node != null)");
                            sw.WriteLine("\t\t\t go.GetComponent<ComponentMonitoring>().petriNet.getPlaceByName(\"" + t.label + "\").initialMarking = i;");
                            sw.WriteLine("\t" + "}");
                            sw.WriteLine();

                            sw.WriteLine("\t" + "public static void init_" + t.label.Replace(" ", "_") + "(string str, int i){");
                            sw.WriteLine("\t\t GameObject mainLoop = GameObject.Find(\"Main_Loop\");");
                            sw.WriteLine("\t\t GameObject go = mainLoop.transform.Find(str).gameObject;");
                            sw.WriteLine("\t\t Node node = go.GetComponent<ComponentMonitoring>().petriNet.getPlaceByName(\"" + t.label + "\");");
                            sw.WriteLine("\t\t if(node != null)");
                            sw.WriteLine("\t\t\t go.GetComponent<ComponentMonitoring>().petriNet.getPlaceByName(\"" + t.label + "\").initialMarking = i;");
                            sw.WriteLine("\t" + "}");
                            sw.WriteLine();
                        }

                        sw.WriteLine("}");
                    }
           
                    AssetDatabase.Refresh();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

    }

}
#endif