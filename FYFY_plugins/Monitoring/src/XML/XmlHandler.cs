using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System;
using UnityEngine;
using System.IO;

namespace FYFY_plugins.Monitoring {
	//Singleton
	internal class XmlHandler {

		private static List<XElement> xTraceList = new List<XElement>();
		private static List<XElement> xSpecifList = new List<XElement>();

		/// Add a new trace
		internal static void addTrace(string performedBy, string trace)
	    {
	        xTraceList.Add(
				new XElement("transition", 
				new XAttribute("time", GetTimestamp(DateTime.Now)),
		        new XAttribute("action", trace),
	            new XAttribute("origin", performedBy))
	        );
	    }

	    //Création des fichiers après destruction (comportement potentiellement anormal ?)
	    internal static void saveTraces(string name)
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

	    internal static String GetTimestamp(DateTime value)
	    {
	        return value.ToString("yyyyMMddHHmmssffff");
	    }

		internal static void addSpecif(string id, string label, bool isSystem, bool isEnd)
		{
			xSpecifList.Add(new XElement("transition",
				new XAttribute("id", id),
				new XAttribute("label", label),
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