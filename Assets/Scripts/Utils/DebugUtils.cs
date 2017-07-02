#if UNITY_EDITOR
using UnityEngine;
#endif

namespace YSQ.NetUtils
{
	public class DebugUtils
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Assert(bool condition, string message) 
		{
			if (!condition) {
				#if UNITY_EDITOR
				Debug.Assert(condition, message);
				#else
				System.Diagnostics.Debug.Assert(condition, message);
				#endif
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void Log(string message)
		{
			#if UNITY_EDITOR
			Debug.Log(message);
			#else
			System.Diagnostics.Debug.WriteLine(message);
			#endif
		}
	}
}
