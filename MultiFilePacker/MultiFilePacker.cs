using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MFPSeek {
	public int head;
	public int tail;
}

public class MFPHandle {
	public FileStream fs;
	public int seek_pos;
	public Dictionary<string, MFPSeek> config;
	public List<MFPSeek> space;
}

public class MultiFilePacker
{
	private struct MFPCONFIG {
		public static string ERROR_PREFIX = "[-MultiFilePacker-] Error: ";
		public static string REUSE_SPACE = "REUSE_SPACE";
		public static float REUSE_FILE_PERCENT = 0.0f;
		public static int FILE_HEAD_DATA_LENGTH = 10;
	}

	public MultiFilePacker()
	{
		
	}

	public MFPHandle OpenOrCreate(string path)
	{
		MFPHandle handle = new MFPHandle() {
			fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite),
			seek_pos = 2*MFPCONFIG.FILE_HEAD_DATA_LENGTH,
		};
		LoadMFPConfig(handle);
		return handle;
	}

	public bool FileExist(MFPHandle handle, string filename)
	{
		FileStream fs = handle.fs;
		Assert(fs != null, "The MFPHandle has been CLOSED or something WRONG with it");
		return handle.config.ContainsKey (filename);
	}
		
	public void AddFileBytes(MFPHandle handle, byte[] file, string filename)
	{
		FileStream fs = handle.fs;
		Assert(fs != null, "The MFPHandle has been CLOSED or something WRONG with it");
		Assert(!handle.config.ContainsKey(filename), "The file with name '" + filename + "' has EXISTS");
		List<MFPSeek> space = handle.space;
		if (space.Count > 0) {
			for (int i = 0; i < space.Count; ++i) {
				MFPSeek s = space[i];
				int slength = s.tail - s.head;
				float r = (float)file.Length / slength;
				if (r >= MFPCONFIG.REUSE_FILE_PERCENT && r<= 1.0f) {
					MFPSeek sf = new MFPSeek() {
						head = s.head,
						tail = s.head+file.Length
					};
					InsertFile(handle, sf, file, filename);
					s.head += file.Length;
					if (s.head == s.tail) {
						space.RemoveAt(i);
					}
					return;
				}
			}
		}
		MFPSeek seek = new MFPSeek() {
			head = handle.seek_pos,
			tail = handle.seek_pos+file.Length
		};
		handle.seek_pos = seek.tail;
		InsertFile(handle, seek, file, filename);
	}

	public void AddFile(MFPHandle handle, string file, string filename)
	{
		byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(file);
		AddFileBytes(handle, fileBytes, filename);
	}

	public void DeleteFile(MFPHandle handle, string filename)
	{
		FileStream fs = handle.fs;
		Assert(fs != null, "The MFPHandle has been CLOSED or something WRONG with it");
		Assert(handle.config.ContainsKey(filename), "The file with name '" + filename + "' does NOT EXISTS");
		MFPSeek seek = handle.config[filename];
		handle.config.Remove(filename);
		if (seek.tail == handle.seek_pos) {
			handle.seek_pos = seek.head;
			return;
		}
		List<MFPSeek> space = handle.space;
		if (space.Count > 0) {
			MFPSeek head = null;
			MFPSeek tail = null;
			for (int i = 0; i < space.Count; ++i) {
				MFPSeek s = space[i];
				if (s.tail == seek.head) {
					s.tail = seek.tail;
					head = s;
				}
				if (s.head == seek.tail) {
					s.head = seek.head;
					tail = s;
				}
			}
			if (head != null && tail != null) {
				head.tail = tail.tail;
				space.Remove(tail);
			}
			if (head != null || tail != null) {
				return;
			}
		}
		space.Add(seek);
	}

	public byte[] ReadFileBytes(MFPHandle handle, string filename)
	{
		FileStream fs = handle.fs;
		Assert(fs != null, "The MFPHandle has been CLOSED or something WRONG with it");
		Assert(handle.config.ContainsKey(filename), "The file with name '" + filename + "' does NOT EXISTS");
		MFPSeek seek = handle.config[filename];
		fs.Seek(seek.head, SeekOrigin.Begin);
		byte[] file_byte = new byte[seek.tail - seek.head];
		fs.Read(file_byte, 0, file_byte.Length);
		return file_byte;
	}

	public string ReadFile(MFPHandle handle, string filename)
	{
		return System.Text.Encoding.UTF8.GetString(ReadFileBytes(handle, filename));
	}

	public void UpdateFile (MFPHandle handle, string file, string filename)
	{
		DeleteFile (handle, filename);
		AddFile (handle, file, filename);
	}

	public void Close(MFPHandle handle)
	{
		SaveMFPConfig(handle);
		handle.fs.Close();
		handle.fs = null;
		handle.config = null;
	}

	private void LoadMFPConfig(MFPHandle handle)
	{
		FileStream fs = handle.fs;
		if (fs.Length != 0) {
			fs.Seek(0, SeekOrigin.Begin);
			byte[] file_head_data = new byte[MFPCONFIG.FILE_HEAD_DATA_LENGTH];
			fs.Read (file_head_data, 0, MFPCONFIG.FILE_HEAD_DATA_LENGTH);
			int config_length = Convert.ToInt32 (System.Text.Encoding.UTF8.GetString(file_head_data));
			fs.Read (file_head_data, 0, MFPCONFIG.FILE_HEAD_DATA_LENGTH);
			int space_length = Convert.ToInt32 (System.Text.Encoding.UTF8.GetString(file_head_data));
			handle.seek_pos = (int)fs.Length - config_length - space_length;
			fs.Seek(-config_length, SeekOrigin.End);
			byte[] config_byte = new byte[config_length];
			fs.Read(config_byte, 0, config_length);
			string config_string = System.Text.Encoding.UTF8.GetString(config_byte);
			handle.config = JsonConvert.DeserializeObject<Dictionary<string, MFPSeek>>(config_string);
			fs.Seek(-config_length - space_length, SeekOrigin.End);
			byte[] space_byte = new byte[space_length];
			fs.Read(space_byte, 0, space_length);
			string space_string = System.Text.Encoding.UTF8.GetString(space_byte);
			handle.space = JsonConvert.DeserializeObject<List<MFPSeek>>(space_string);
		} else {
			handle.config = new Dictionary<string, MFPSeek>();
			handle.space = new List<MFPSeek>();
		}
	}

	private void SaveMFPConfig(MFPHandle handle)
	{
		FileStream fs = handle.fs;
		Assert(fs != null, "The MFPHandle has been CLOSED or something WRONG with it");
		fs.Seek(handle.seek_pos, SeekOrigin.Begin);
		string space_string = JsonConvert.SerializeObject(handle.space);
		fs.Write (System.Text.Encoding.UTF8.GetBytes (space_string), 0, space_string.Length);
		string config_string = JsonConvert.SerializeObject(handle.config);
		fs.Write (System.Text.Encoding.UTF8.GetBytes (config_string), 0, config_string.Length);
		fs.SetLength(handle.seek_pos + space_string.Length + config_string.Length);
		fs.Seek(0, SeekOrigin.Begin);
		string number_string = config_string.Length.ToString();
		number_string = number_string.PadLeft(MFPCONFIG.FILE_HEAD_DATA_LENGTH, '0');
		fs.Write (System.Text.Encoding.UTF8.GetBytes (number_string), 0, number_string.Length);
		number_string = space_string.Length.ToString();
		number_string = number_string.PadLeft(MFPCONFIG.FILE_HEAD_DATA_LENGTH, '0');
		fs.Write (System.Text.Encoding.UTF8.GetBytes (number_string), 0, number_string.Length);
	}

	private void InsertFile(MFPHandle handle, MFPSeek s, byte[] file, string filename)
	{
		FileStream fs = handle.fs;
		fs.Seek(s.head, SeekOrigin.Begin);
		fs.Write (file, 0, file.Length);
		handle.config.Add(filename, s);
	}

	private void Assert(bool condition, string message)
	{
		DebugUtils.Assert(condition, MFPCONFIG.ERROR_PREFIX + message);
	}
}
