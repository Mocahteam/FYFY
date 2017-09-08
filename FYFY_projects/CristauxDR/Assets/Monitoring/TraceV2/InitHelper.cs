using petriNetV2;
using monitorV3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitHelper {

    public string label = "";
	public ComponentMonitoring monitor = null;
    

	public InitHelper(string label,  ComponentMonitoring monitor)
    {
        this.label = label;
        this.monitor = monitor;
    }


    public void init(int nbToken)
    {
		Node place = monitor.petriNet.getPlaceByName(label);
        if (place != null)
            place.initialMarking = nbToken;
        else
			Debug.LogError("Warning!!! place \""+label+"\" is not known.");
    }
    void printList(List<int> list)
    {
        string str = "";
        foreach (int i in list)
        {
            str += (i + " ");
        }
    }
}
