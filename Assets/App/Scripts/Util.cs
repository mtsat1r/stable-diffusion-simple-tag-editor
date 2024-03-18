using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Util
{
	public static bool WriteText(string path, string str, bool append) {
		try
		{
			using (StreamWriter writer = new StreamWriter(path, append))
			{
				writer.Write(str);
				writer.Flush();
				writer.Close();
			}
		}
		catch (System.Exception e)
		{
			//Assert(false, e.Message);
			return false;
		}
		return true;
	}
}
