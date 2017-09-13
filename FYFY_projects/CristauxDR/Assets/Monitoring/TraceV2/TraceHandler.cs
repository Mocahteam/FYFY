using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System;
using UnityEngine;
using System.IO;

namespace monitoring {
	//Singleton
	public class TraceHandler {

		public static class Source {
			public static string SYSTEM = "system";
			public static string PLAYER = "player";
		};

	    static List<XElement> xelemList = new List<XElement>();

		public static void trace(string performedBy, bool isTry, string trace)
	    {
	        xelemList.Add(
				new XElement("transition", 
				new XAttribute("time", GetTimestamp(DateTime.Now)),
		        new XAttribute("action", trace),
	            new XAttribute("origin", performedBy),
				new XAttribute("try", isTry.ToString()))
	        );
	    }

	    //Création des fichiers après destruction (comportement potentiellement anormal ?)
	    public static void save(string name)
	    {
	        string timeStamp = GetTimestamp(DateTime.Now);
	        //<? xml version = "1.0" encoding = "utf-8" ?>
	        //< data >
	        //< map id = "murDeGlace" nbPrecedingTry = "0" Configuration = "859" BeginningTime = "160" EndingTime = "55436" >
	        XDocument doc = new XDocument();

	        doc = new XDocument(         
	            new XElement("data", 
	                new XElement("map",new XAttribute("id", name+"_"+timeStamp),
	                    xelemList.Select(x => x)
	                )
	            )
	        );

	        System.IO.Directory.CreateDirectory("logs");
	        doc.Save("logs\\"+name+"_logs_"+ timeStamp+".xml");
	        xelemList.Clear();
	    }

	    public static String GetTimestamp(DateTime value)
	    {
	        return value.ToString("yyyyMMddHHmmssffff");
	    }

	    internal static void saveTransitionsLabels(string name,List<Node> transitions)
	    {
	        if(!File.Exists("specifs\\" + name + "_specifs" + ".xml")){
	            List<XElement> xelemListLabels = new List<XElement>();
	            foreach(Node n in transitions)
	            {
	                xelemListLabels.Add(new XElement("transition",
	                            new XAttribute("id", n.label+"_"+n.id),
	                            new XAttribute("system", "false"),
	                            new XAttribute("end", "false"))
	                );
	            }

	            XDocument labs = new XDocument(
	                new XElement("transitions",
	                        xelemListLabels.Select(x => x)
	                )
	            );

	            System.IO.Directory.CreateDirectory("specifs");
	            labs.Save("specifs\\"+name + "_specifs" + ".xml");
	        }
	    }
	}
}