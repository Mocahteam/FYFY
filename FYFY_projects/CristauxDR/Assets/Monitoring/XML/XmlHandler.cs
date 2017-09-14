using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System;
using UnityEngine;
using System.IO;

namespace monitoring {
	//Singleton
	public class XmlHandler {

		private static List<XElement> xTraceList = new List<XElement>();
		private static List<XElement> xSpecifList = new List<XElement>();

		public static void addTrace(string performedBy, bool isTry, string trace)
	    {
	        xTraceList.Add(
				new XElement("transition", 
				new XAttribute("time", GetTimestamp(DateTime.Now)),
		        new XAttribute("action", trace),
	            new XAttribute("origin", performedBy),
				new XAttribute("try", isTry.ToString()))
	        );
	    }

	    //Création des fichiers après destruction (comportement potentiellement anormal ?)
	    public static void saveTraces(string name)
	    {
	        string timeStamp = GetTimestamp(DateTime.Now);
	        XDocument doc = new XDocument();

	        doc = new XDocument(         
	            new XElement("data", 
	                new XElement("map",new XAttribute("id", name+"_"+timeStamp),
	                    xTraceList.Select(x => x)
	                )
	            )
	        );

	        System.IO.Directory.CreateDirectory("logs");
	        doc.Save("logs\\"+name+"_logs_"+ timeStamp+".xml");
	        xTraceList.Clear();
	    }

	    public static String GetTimestamp(DateTime value)
	    {
	        return value.ToString("yyyyMMddHHmmssffff");
	    }

		public static void addSpecif(string label, bool isSystem, bool isEnd)
		{
			xSpecifList.Add(new XElement("transition",
				new XAttribute("id", label),
				new XAttribute("system", isSystem.ToString()),
				new XAttribute("end", isEnd.ToString()))
			);
		}

	    internal static void saveSpecifications(string name)
	    {
			XDocument doc = new XDocument();

			doc = new XDocument(         
				new XElement("transitions", 
					xSpecifList.Select(x => x)
				)
			);

			System.IO.Directory.CreateDirectory("specifs");
			doc.Save("specifs\\"+name+"_specifs.xml");
			xSpecifList.Clear();
	    }
	}
}