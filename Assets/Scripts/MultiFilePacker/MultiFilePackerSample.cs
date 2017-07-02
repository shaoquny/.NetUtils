using UnityEngine;
using System.Collections;
using YSQ.NetUtils;

public class MultiFilePackerSample : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		testMultiFilePacker();
	}
	
	void testMultiFilePacker()
	{
		string path = Application.dataPath + "/StreamingAssets/config.mfp";
		MultiFilePacker packer = new MultiFilePacker();
		MFPHandle handle = packer.OpenOrCreate(path);

		int count = 5;
		string[] configs = new string[count];
		configs[0] = "<helaofaisdlkfajlsdklaknviaofaksdjflanvlkajfhlanwebgjasdvlkajfslgalsdflkasjdgj>";
		configs [1] = configs [0] + configs [0];
		for (int i = 2; i < count; ++i) {
			configs [i] = configs [i - 1] + configs [i - 2];
		}
		for (int i = 0; i < count; ++i) {
			packer.AddFile(handle, configs[i], "file_" + i);
		}
		packer.AddFile(handle, configs[2], "shit");
		string shit = packer.ReadFile(handle, "shit");
		Debug.Log(shit);
		packer.DeleteFile(handle, "file_2");
		packer.DeleteFile(handle, "file_4");
		packer.DeleteFile(handle, "file_3");
		packer.AddFile(handle, configs[3], "file_3");
		packer.AddFile(handle, configs[4], "file_4");
		packer.AddFile(handle, configs[4], "file_5");
		packer.AddFile(handle, configs[2], "file_2");
		packer.DeleteFile(handle, "file_5");
		packer.DeleteFile(handle, "file_2");
		packer.AddFile(handle, configs[2], "file_2");

		packer.Close(handle);
	}
}
