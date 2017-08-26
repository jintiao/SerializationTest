using System;
using UnityEngine;
using UnityEditor;

namespace SerializationTest.Test4
{

	[Serializable]
	public class MyObject
	{
		public MyObject2 obj2;
	}

	[Serializable]
	public class MyObject2
	{
		public int data;
	}

	public class NullReferenceWindow : EditorWindow {
		private const string WINDOW_TITLE = "Null Reference";

		private MyObject obj;

		void OnEnable() {
			titleContent.text = WINDOW_TITLE;

			if(obj == null) {
				obj = new MyObject();
			}
		}

		void OnGUI() {

			if(obj.obj2 == null) {
				GUILayout.Label("obj2 is null");
			}
			else {
				GUILayout.Label("obj2");
				obj.obj2.data = EditorGUILayout.IntSlider(obj.obj2.data, 0, 100);
			}
		}

		[MenuItem ("Window/Serialization Test/Test 4 - " + WINDOW_TITLE)]
		public static void  ShowWindow()
		{
			EditorWindow.GetWindow<NullReferenceWindow>();
		}

	} // class TestWindow4

} // namespace SerializationTest.Test4