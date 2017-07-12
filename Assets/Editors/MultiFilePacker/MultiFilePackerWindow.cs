using UnityEngine;
using UnityEditor;
using YSQ.NetUtils;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class MultiFilePackerWindow : EditorWindow 
{
	int pathCount = 0;
	string multiFilePath;
	List<bool> elementShow = new List<bool>();
	List<string> filePath = new List<string>();
	List<string> filePattern = new List<string>();
	// pack all file matched without specified extension
	List<string> excludeExtension = new List<string>();
	List<SearchOption> searchOption = new List<SearchOption>();
	bool optionalSetting;
//	bool compress = true;
	string encryptKey;
	float useTime;

	[MenuItem(".NetUtils/MultiFilePacker")]
	static void MultiFilePackerWin()
	{
		// Get existing open window or if none, make a new one:
		MultiFilePackerWindow window = (MultiFilePackerWindow)EditorWindow.GetWindow(typeof(MultiFilePackerWindow));
		window.Show();
	}

	void OnGUI()
	{
		GUILayout.Label(string.Format("TopPath: {0} | UseTime: {1}", Application.dataPath, useTime), EditorStyles.boldLabel);
		if(string.IsNullOrEmpty(multiFilePath))
			multiFilePath = "/";
		multiFilePath = EditorGUILayout.TextField("MultiFilePacker Name", multiFilePath);
		int prePathCount = pathCount;
		pathCount = EditorGUILayout.IntField("Pack Path Size", pathCount);
		if (pathCount == 0) {
			elementShow = new List<bool>();
			filePath = new List<string>();
			filePattern = new List<string>();
			excludeExtension = new List<string>();
			searchOption = new List<SearchOption>();
		}
		if (pathCount > prePathCount) {
			for (int i = prePathCount; i < pathCount; i++) {
				elementShow.Add(true);
				filePath.Add("/");
				filePattern.Add("*");
				excludeExtension.Add(MFP_CONFIG.MULTI_FILE_PACKER_EXTENSION + ",.DS_Store,.meta");
				searchOption.Add(SearchOption.AllDirectories);
			}
		}
		if (pathCount > 0) {
			for (int i = 0; i < pathCount; i++) {
				elementShow[i] = EditorGUILayout.Foldout(elementShow[i], "Element " + i);
				if (elementShow[i]) {
					GUIStyle style = new GUIStyle();
					style.padding = new RectOffset(30, 0, 0, 0);
					EditorGUILayout.BeginVertical(style);
					filePath[i] = EditorGUILayout.TextField("File Directory", filePath[i]);
					filePattern[i] = EditorGUILayout.TextField("Search Pattern", filePattern[i]);
					excludeExtension[i] = EditorGUILayout.TextField("Exclude Extension", excludeExtension[i]);
					searchOption[i] = (SearchOption)EditorGUILayout.EnumPopup("Search Option", searchOption[i]);
					EditorGUILayout.EndVertical();
				}
			}
		}

		optionalSetting = EditorGUILayout.BeginToggleGroup("Optional Settings", optionalSetting);
//		compress = EditorGUILayout.Toggle("Compress", optionalSetting);
		encryptKey = EditorGUILayout.TextField("Encrypt Key", encryptKey);
		EditorGUILayout.EndToggleGroup();

		if (GUILayout.Button("Pack")) {
			Stopwatch clock = new Stopwatch();
			clock.Start();
			string path = Application.dataPath + multiFilePath + MFP_CONFIG.MULTI_FILE_PACKER_EXTENSION;
			if (File.Exists(path)) {
				File.Delete(path);
			}
			MultiFilePacker packer = new MultiFilePacker();
			MFPHandle mfpHandle = packer.OpenOrCreate(path);
			for (int i = 0; i < pathCount; i++) {
				HashSet<string> extensions = new HashSet<string>(excludeExtension[i].Split(','));
				var info = new DirectoryInfo(Application.dataPath + filePath[i]);
				var fileInfo = info.GetFiles(filePattern[i], searchOption[i]);
				foreach (var file in fileInfo) {
					if (!extensions.Contains(file.Extension)) {
						packer.AddFile(mfpHandle, file.FullName);
					}
				}
			}
			packer.Close(mfpHandle);
			clock.Stop();
			useTime = (float)clock.Elapsed.TotalSeconds;
		}
	}
}
