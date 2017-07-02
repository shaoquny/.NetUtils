using UnityEngine;
using UnityEditor;
using YSQ.NetUtils;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public enum MsgType
{
	msg_0,
	msg_1,
}

public enum LanguageType
{
	zh,
	es,
	zh_tw,
}

public class JsonCSVWindow : EditorWindow
{
	string csvPath;
	Dictionary<string, Dictionary<string, string>> dictionary;
	Dictionary<MsgType, Dictionary<LanguageType, string>> typeDict;
	string Key1;
	string Key2;
	string Value;

	float useTime;

	[MenuItem(".NetUtils/JsonCSV")]
	static void MultiFilePacker()
	{
		// Get existing open window or if none, make a new one:
		EditorWindow window = EditorWindow.GetWindow(typeof(JsonCSVWindow));
		window.Show();
	}

	void OnGUI()
	{
		GUILayout.Label(string.Format("TopPath: {0} | UseTime: {1}", Application.dataPath, useTime), EditorStyles.boldLabel);
		if (string.IsNullOrEmpty(csvPath)) {
			csvPath = Application.dataPath + csvPath;
		}

		csvPath = EditorGUILayout.TextField("CSV File Path", csvPath);

		Key1 = EditorGUILayout.TextField("Key 1", Key1);
		Key2 = EditorGUILayout.TextField("Key 2", Key2);
		Value = EditorGUILayout.TextField("Value", Value);

		if (GUILayout.Button("Parse")) {
			Stopwatch clock = new Stopwatch();
			clock.Start();
//			if (dictionary == null) {
//				dictionary = JsonCSV.ToDictionary(csvPath);
//			}
//			Value = dictionary[Key1][Key2];
			if (typeDict == null) {
				typeDict = JsonCSV.ToDictionary<MsgType, LanguageType, string>(csvPath);
			}
			Value = typeDict[(MsgType)System.Enum.Parse(typeof(MsgType), Key1)][(LanguageType)System.Enum.Parse(typeof(LanguageType), Key2)];
			clock.Stop();
			useTime = (float)clock.Elapsed.TotalSeconds;
		}
	}
}
