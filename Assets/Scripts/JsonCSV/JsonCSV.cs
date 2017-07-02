using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YSQ.NetUtils
{
	public class JsonCSV
	{
		private struct JC_CONFIG_PRIVATE
		{
			public static string ERROR_PREFIX = "[-JsonCSV-] Error: ";
		}

		public static Dictionary<string, Dictionary<string, string>> ToDictionary(string filePath)
		{
			Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
			string[] fileLine = File.ReadAllLines(filePath, Encoding.UTF8);
			string[] keys = fileLine[0].Split(',');
			for (int line = 1; line < fileLine.Length; line++) {
				string[] data = fileLine[line].Split(',');
				Dictionary<string, string> dict = new Dictionary<string, string>();
				for (int index = 1; index < data.Length; index++) {
					dict.Add(keys[index], data[index]);
				}
				dictionary.Add(data[0], dict);
			}
			string str = JsonConvert.SerializeObject(dictionary);
			DebugUtils.Log(str);
			return dictionary;
		}

		public static Dictionary<K1, Dictionary<K2, V>> ToDictionary<K1, K2, V>(string filePath)
		{
			string str = JsonConvert.SerializeObject(ToDictionary(filePath));
			return JsonConvert.DeserializeObject<Dictionary<K1, Dictionary<K2, V>>>(str);
		}

		public static Dictionary<T1, Dictionary<T2, T3>> ToJson<T1, T2, T3>(string filePath, TypeSwitch typeSwitch)
		{
			string[] fileData = File.ReadAllLines(filePath, Encoding.UTF8);
			string[] keys = fileData[0].Split(',');
			Dictionary<T1, Dictionary<T2, T3>> dict = new Dictionary<T1, Dictionary<T2, T3>>();

//			while()
			return null;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private static void Assert(bool condition, string message)
		{
			DebugUtils.Assert(condition, JC_CONFIG_PRIVATE.ERROR_PREFIX + message);
		}
	}

	public class TypeSwitch
	{
		public static TypeSwitch Default = new TypeSwitch();

		public delegate T DelegateSwitch<T>(string value);
		Dictionary<Type, DelegateSwitch<object>> matches = new Dictionary<Type, DelegateSwitch<object>>();

		public TypeSwitch()
		{
			this.Case<int>(IntSwitch)
				.Case<float>(FloatSwitch)
				.Case<bool>(BoolSwitch)
				.Case<byte>(ByteSwitch)
				.Case<string>(StringSwitch);
		}

		public TypeSwitch Case<T>(DelegateSwitch<T> action)
		{
			matches.Add(typeof(T), (x) => action(x));
			return this;
		}

		public T Switch<T>(string x) 
		{
			return (T)matches[typeof(T)](x);
		}

		public T EnumSwitch<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value);
		}

		public int IntSwitch(string value)
		{
			return Convert.ToInt32(value);
		}

		public float FloatSwitch(string value)
		{
			return Convert.ToSingle(value);
		}

		public bool BoolSwitch(string value)
		{
			return Convert.ToBoolean(value);
		}

		public byte ByteSwitch(string value)
		{
			return Convert.ToByte(value);
		}

		public string StringSwitch(string value)
		{
			return value;
		}
	}
}
