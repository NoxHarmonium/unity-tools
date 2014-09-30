#if !UNITY

using System;

namespace UnityEngine
{
	// These classes allow testing to occur in a non-unity environment.

	public class MonoBehaviour {
		public GameObject gameObject {
			get; set;
		}
	}

	public class Component {
	}

	public class GameObject {
		public void AddComponent<T>() {
		}

		public void AddComponent(Type t) {
		}
	}

	public class Debug {
		public static void Log(string message) {
			Console.WriteLine("[{0:T}] LOG: {1}", DateTime.Now, message);
		}
		public static void LogWarning(string message) {
			Console.WriteLine("[{0:T}] WARNING: {1}", DateTime.Now, message);
		}
		public static void LogError(string message) {
			Console.WriteLine("[{0:T}] ERROR: {1}", DateTime.Now, message);
		}
		public static void LogException(Exception ex) {
			Console.WriteLine("[{0:T}] EXCEPTION: {1}: {2} \n{3}", DateTime.Now, ex.ToString() ,ex.Message, ex.StackTrace);
		}
	}
}

#endif