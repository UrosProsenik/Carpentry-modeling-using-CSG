//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
using UnityEngine;

public class FileHandler : MonoBehaviour{
	/// Look of an CSG operation . Everything in one line seperated with ,
	/// 
	/// 1. geometry(sphere or cube)
	/// 2. position
	/// 3. rotation
	/// 4. scale
	/// 5. operation
	
//	public void WriteFile(List<String> CSG_construction)
//	{
//		var path = EditorUtility.SaveFilePanel(
//			"Save texture as PNG",
//			"",
//			 ".txt",
//			"txt");
//		
//		// Create a file to write to.
//		using(StreamWriter writetext = new StreamWriter(path))
//		{
//			foreach (string line in CSG_construction)
//			{
//				writetext.WriteLine(line);
//			}
//		}
//	}
//
//	public List<String> ReadFile()
//	{
//		List<String> CSG_construction = new List<string>();
//
//		string path = EditorUtility.OpenFilePanel("Overwrite with txt", "", "txt");
//		string line;
//		
//		// Read text from file
//		using(StreamReader readtext = new StreamReader(path))
//		{
//			while ((line = readtext.ReadLine()) != null)
//			{
//				CSG_construction.Add(line);
//			}
//		}
//		return CSG_construction;
//	}
}
