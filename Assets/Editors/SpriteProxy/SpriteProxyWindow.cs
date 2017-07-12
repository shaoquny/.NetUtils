using UnityEngine;
using UnityEditor;
using YSQ.NetUtils;

public class SpriteProxyWindow : EditorWindow
{
	string TexName;
	Texture Tex;

	[MenuItem(".NetUtils/SpriteProxy")]
	static void SpriteProxyWin()
	{
		// Get existing open window or if none, make a new one:
		EditorWindow window = EditorWindow.GetWindow(typeof(SpriteProxyWindow));
		window.Show();
	}

	void OnGUI()
	{
		TexName = EditorGUILayout.TextField("Sprite Name", TexName);

		if (GUILayout.Button("Test")) {
			ResourceManager.Create();
			Sprite s = ResourceManager.Instance.LoadSprite(TexName);
			if (s == null) {
				Tex = new Texture2D(256, 256);
			} else {
				Tex = s.texture;
			}
			ResourceManager.Release();
		}
		GUILayout.Label(Tex);
	}
}
