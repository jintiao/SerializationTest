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

	public class TestWindow4 : EditorWindow {
		private MyObject obj;

		void OnEnable() {
			titleContent.text = "TestWindow4";

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

		[MenuItem ("Window/Serialization Test/Test Window 4")]
		public static void  ShowWindow()
		{
			EditorWindow.GetWindow<TestWindow4>();
		}

	} // class TestWindow4

} // namespace SerializationTest.Test4