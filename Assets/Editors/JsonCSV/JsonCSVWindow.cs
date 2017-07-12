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
	MsgType Key1;
	LanguageType Key2;
	string Value;

	float useTime;

	[MenuItem(".NetUtils/JsonCSV")]
	static void JsonCSVWin()
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

		GUILayout.BeginHorizontal();
		csvPath = Application.dataPath + EditorGUILayout.TextField("CSV File Path", csvPath.Replace(Application.dataPath, ""));
		if (GUILayout.Button("Select", GUILayout.Width(80))) {
			csvPath = EditorUtility.OpenFilePanel("Select", Application.dataPath, "csv");
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		Key1 = (MsgType)EditorGUILayout.EnumPopup("Key 1", Key1);
		Key2 = (LanguageType)EditorGUILayout.EnumPopup("Key 2", Key2);
		GUILayout.EndHorizontal();
		Value = EditorGUILayout.TextField("Value", Value);

		if (GUILayout.Button("Parse")) {
			Stopwatch clock = new Stopwatch();
			clock.Start();
			if (typeDict == null) {
				typeDict = JsonCSV.ToKeyKeyValue<MsgType, LanguageType, string>(csvPath);
			}
			Value = typeDict[Key1][Key2];
			clock.Stop();
			useTime = (float)clock.Elapsed.TotalSeconds;
		}
	}
}
