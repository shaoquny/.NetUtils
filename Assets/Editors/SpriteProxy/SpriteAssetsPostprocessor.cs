using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace YSQ.NetUtils
{

	public class SpriteAssetsPostprocessor : AssetPostprocessor
	{
		private static string ProxyRules = "chess";
		private static List<string> ProxyFolders;
		private static Dictionary<string, KeyValuePair<string, bool>> SpriteProxy = new Dictionary<string, KeyValuePair<string, bool>>();

		[MenuItem(".NetUtils/CleanSpriteProxy")]
		static void CleanSpriteProxy()
		{
			SpriteProxy.Clear();
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			ProxyFolders = new List<string>(ProxyRules.Split(','));

			foreach (string path in importedAssets)
			{
	//			Debug.Log("Reimported Asset: " + path);
				RefreshAssets(path, null);
			}
			for (int idx = 0; idx < movedAssets.Length; idx++)
			{
	//			Debug.Log(string.Format("Moved Asset: {0}, movedFrom: {1}", movedAssets[idx], movedFromAssetPaths[idx]));
				RefreshAssets(movedAssets[idx], movedFromAssetPaths[idx]);
			}
			foreach (string path in deletedAssets)
			{
	//			Debug.Log("Deleted Asset: " + path);
				RefreshAssets(path, null);
			}
		}

		static void ReloadAndProxyAssets()
		{
			string spPath = "Assets/Resources/";
			string spFolder = "SpriteProxy";
			if (!AssetDatabase.IsValidFolder(spPath + spFolder)) {
				AssetDatabase.CreateFolder(spPath, spFolder);
			}
			string spMgrResPath = spFolder + "/SpriteProxyManager";
			string spMgrPath = spPath + spMgrResPath + ".prefab";
			GameObject spMgrGO = Resources.Load<GameObject>(spMgrResPath);
			if (spMgrGO == null) {
				spMgrGO = new GameObject("SpriteProxyManager", typeof(SpriteProxyManager));
				PrefabUtility.CreatePrefab(spMgrPath, spMgrGO);
			}
			SpriteProxyManager spMgr = spMgrGO.GetComponent<SpriteProxyManager>();
			foreach (KeyValuePair<string, KeyValuePair<string, bool>> pair in SpriteProxy) {
				KeyValuePair<string, bool> p = pair.Value;
				if (p.Value) {
					string [] fileEntries = Directory.GetFiles(Application.dataPath+p.Key);
					string spGOResPath = spFolder + "/" + pair.Key;
					string spGOPath = spPath + spGOResPath + ".prefab";
					GameObject go = Resources.Load<GameObject>(spGOResPath);
					if (fileEntries.Length > 0) {
						if (go == null) {
							go = new GameObject(pair.Key, typeof(SpriteProxy));
							go = PrefabUtility.CreatePrefab(spGOPath, go);
						}
						SpriteProxy sp = go.GetComponent<SpriteProxy>();
						sp.sprites.Clear();
						foreach (string fileName in fileEntries) {
							int index = fileName.LastIndexOf("/");
							string localPath = "Assets" + p.Key + fileName.Substring(index);
							Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(localPath);
							if (s != null) {
								sp.sprites.Add(s);
							}
						}
						if (!spMgr.SpriteProxies.Contains(pair.Key)) {
							spMgr.SpriteProxies.Add(pair.Key);
						}
					} else {
						if (go != null) {
							AssetDatabase.DeleteAsset(spGOPath);
							spMgr.SpriteProxies.Remove(pair.Key);
						}
					}
				}
			}
			if (SpriteProxy.Count > 0) {
				string str = "";
				foreach (KeyValuePair<string, KeyValuePair<string, bool>> pair in SpriteProxy) {
					str += pair.Key + ": ";
					KeyValuePair<string, bool> p = pair.Value;
					str += string.Format("({0}, {1})", p.Key, p.Value) + ", ";
				}
				Debug.Log(str);
			}
		}

		static void RefreshAssets(string path, string fromPath)
		{
			int idx = path.LastIndexOf('/');
			path = path.Substring(0, idx);
			idx = path.LastIndexOf('/') + 1;
			string parentPath = path.Substring(idx, path.Length - idx);
			if (ProxyFolders.Contains(parentPath)) {
				RefreshAssetsAtPath(path, parentPath);
			}
			if (!string.IsNullOrEmpty(fromPath)) {
				idx = fromPath.LastIndexOf('/');
				fromPath = fromPath.Substring(0, idx);
				idx = fromPath.LastIndexOf('/') + 1;
				string fromParentPath = fromPath.Substring(idx, fromPath.Length - idx);
				if (ProxyFolders.Contains(fromParentPath)) {
					RefreshAssetsAtPath(fromPath, fromParentPath);
				}
			}
		}

		static void RefreshAssetsAtPath(string path, string parentPath)
		{
			SpriteProxy.Clear();
			path = path.Replace("Assets", "");
			KeyValuePair<string, bool> value = new KeyValuePair<string, bool>(path, true);
			if (!SpriteProxy.ContainsKey(parentPath)) {
				SpriteProxy.Add(parentPath, value);
			} else {
				SpriteProxy[parentPath] = value;
			}

			ReloadAndProxyAssets();
	//		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path + "/");
	//		if (assets.Length > 0) {
	//			if (SpriteProxy.ContainsKey(parentPath)) {
	//				SpriteProxy[parentPath].Clear();
	//			} else {
	//				SpriteProxy.Add(parentPath, new List<Sprite>());
	//			}
	//			foreach (Object obj in assets) {
	//				Sprite s = obj as Sprite;
	//				if (s != null) {
	//					SpriteProxy[parentPath].Add(s);
	//				}
	//			}
	//		} else {
	//			if (SpriteProxy.ContainsKey(parentPath)) {
	//				SpriteProxy.Remove(parentPath);
	//			}
	//		}
		}
	}

}
