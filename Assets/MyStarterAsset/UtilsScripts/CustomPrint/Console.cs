using System.Collections.Generic;
using UnityEngine;

namespace MyPrint
{
	public class Console
	{
		
		#region Classic Print
		//Print avec une couleur et un style
		public static void Print(string msg, ColorConsole color, ConsoleStyle style)
		{
			(string, string) styleStr = ConsoleOption.GetStyle(style);
			
			 string s = $"{styleStr.Item1}" +
			            $"{ConsoleOption.GetColor(color)} " +
			            $"{msg}" +
			            "</color> " +
			            $"{styleStr.Item2}";
			
			Debug.Log(s);
		}
		
		//Print avec une couleur uniquement
		public static void Print(string msg, ColorConsole color)
		{
			 string s = "" +
			            $"{ConsoleOption.GetColor(color)}" +
			            $"{msg}" +
			            "</color>";
			
			Debug.Log(s);
		}
		
		//Print avec un style uniquement
		public static void Print(string msg, ConsoleStyle style)
		{
			(string, string) styleStr = ConsoleOption.GetStyle(style);
			
			string s = $"{styleStr.Item1}" +
			           $"{msg}" +
			           "</color> " +
			           $"{styleStr.Item2}";
			
			Debug.Log(s);
		}
		#endregion
		
		#region List Print
		
		//Print List element par element
		public static void PrintList<T>(List<T> list, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Count; i++)
			{
				s += $"Element {i} : {list[i]} \n";
			}
			
			Debug.Log(s);
		}
		
		//Print List element par element avec une couleur
		public static void PrintList<T>(List<T> list, ColorConsole colorElement, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Count; i++)
			{
				s += $"Element {i} : {ConsoleOption.GetColor(colorElement)}{list[i]}</color> \n";
			}
			
			Debug.Log(s);
		}

		
		#endregion
		
		#region Array Print
		
		//Print Array element par element
		public static void PrintList<T>(T[] list, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Length; i++)
			{
				s += $"Element {i} : {list[i]} \n";
			}
			
			Debug.Log(s);
		}
		
		//Print Array element par element avec une couleur
		public static void PrintList<T>(T[] list, ColorConsole colorElement, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Length; i++)
			{
				s += $"Element {i} : {ConsoleOption.GetColor(colorElement)}{list[i]}</color> \n";
			}
			
			Debug.Log(s);
		}
		
		#endregion

		#region Bool Print
		public static void PrintBool(bool b, string boolName = "")
		{
			string s = $"{boolName} ";
			
			string color = b ? ConsoleOption.GetColor(ColorConsole.Green) : ConsoleOption.GetColor(ColorConsole.Red);
			
			s += $"{color}{b}</color>";
			
			Debug.Log(s);
		}
		#endregion

		#region Space Print
		public static void PrintSpace()
		{
			Debug.Log("──────────────────────────────────");
		}
		
		public static void PrintSpace(ColorConsole color)
		{
			Debug.Log($"{ConsoleOption.GetColor(color)}──────────────────────────────────</color>");
		}
		#endregion

		public static void Print(string pickedUpItem)
		{
			throw new System.NotImplementedException();
		}
	}

}

