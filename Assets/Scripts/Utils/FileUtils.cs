using System;
using System.IO;
using System.Text;

namespace YSQ.NetUtils
{
	public class FileUtils
	{
		private struct FU_CONFIG_PRIVATE
		{
			public static string ERROR_PREFIX = "[-FileUtils-] Error: ";
		}

		public static byte[] ReadFile(string filePath)
		{
			Assert(filePath.Contains("/"), "WRONG file PATH to read a file");
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			int tail = (int)fs.Seek(0, SeekOrigin.End);
			int head = (int)fs.Seek(0, SeekOrigin.Begin);
			byte[] data = new byte[tail - head];
			fs.Read(data, head, tail - head);
			fs.Close();
			return data;
		}

		public static string ReadFile(string filePath, Encoding encoding)
		{
			byte[] data = ReadFile(filePath);
			return encoding.GetString(data);
		}

		public static byte[] ReadFile(string filePath, out string fileName)
		{
			int index = filePath.LastIndexOf("/") + 1;
			fileName = filePath.Substring(index, filePath.Length - index);
			return ReadFile(filePath);
		}

		public static string ReadFile(string filePath, Encoding encoding, out string fileName)
		{
			byte[] data = ReadFile(filePath, out fileName);
			return encoding.GetString(data);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private static void Assert(bool condition, string message)
		{
			DebugUtils.Assert(condition, FU_CONFIG_PRIVATE.ERROR_PREFIX + message);
		}
	}
}
