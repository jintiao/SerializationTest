using System;
using UnityEngine;
using UnityEditor;
using SerializationTest.Common;

namespace SerializationTest.Test2
{

	public class ScriptableObjectWindow : EditorWindow {
		private const string WINDOW_TITLE = "ScriptableObject";

		public MyScriptableObject m;

		void OnEnable() {
			if(m == null) 
				m = CreateInstance<MyScriptableObject>();

			titleContent.text = WINDOW_TITLE;
		}

		void OnGUI() {
			EditorGUILayout.Space();
			m.OnGUI();
			EditorGUILayout.Space();
			if(GUILayout.Button("roll")) {
				m.Roll();
			}
		}

		[MenuItem ("Window/Serialization Test/Test 2 - " + WINDOW_TITLE)]
		public static void  ShowWindow() {
			EditorWindow.GetWindow<ScriptableObjectWindow>();
		}

	} // class TestWindow2

} // namespace SerializationTest.Test2