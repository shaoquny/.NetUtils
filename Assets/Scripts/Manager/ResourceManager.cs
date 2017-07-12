using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace YSQ.NetUtils
{
	public class ResourceManager 
	{
		public static ResourceManager Instance;

		private Dictionary<string, Dictionary<string, Sprite>> _sprites = new Dictionary<string, Dictionary<string, Sprite>>();

		public static void Create()
		{
			Instance = new ResourceManager();
			Instance.Init();
		}

		public static void Release()
		{
			Instance = null;
		}

		void Init()
		{
	//		LoadSprites();
		}

		void LoadSprites()
		{
			GameObject spmGO = Resources.Load<GameObject>("SpriteProxy/SpriteProxyManager");
			SpriteProxyManager spMgr = spmGO.GetComponent<SpriteProxyManager>();
			foreach (string name in spMgr.SpriteProxies) {
				LoadSpriteProxy(name);
			}
			foreach (KeyValuePair<string, Dictionary<string, Sprite>> item in _sprites) {
				string str = "";
				str += string.Format("{0} : ", item.Key);
				foreach (KeyValuePair<string, Sprite> it in item.Value) {
					str += it.Key + ", ";
				}
				Debug.Log(str);
			}
		}

		void LoadSpriteProxy(string name)
		{
			GameObject go = Resources.Load<GameObject>(string.Format("SpriteProxy/{0}", name));
			if (go == null) {
				return;
			}
			SpriteProxy proxy = go.GetComponent<SpriteProxy>();
			Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
			foreach (Sprite s in proxy.sprites) {
				dict.Add(s.name, s);
			}
			_sprites.Add(name, dict);
		}

		public Sprite LoadSprite(string name)
		{
			string[] keys = name.Split('/');
			if (!_sprites.ContainsKey(keys[0])) {
				LoadSpriteProxy(keys[0]);
			}
			if (_sprites.ContainsKey(keys[0])) {
				var dict = _sprites[keys[0]];
				if (dict.ContainsKey(keys[1])) {
					return dict[keys[1]];
				}
			}
			return null;
		}
	}
}
