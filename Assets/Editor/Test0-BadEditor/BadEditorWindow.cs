using System;
using UnityEngine;
using UnityEditor;

namespace SerializationTest.Test0
{

	public class MyDataObject {
		public int data;
	}

	[Serializable]
	public class MyDataModel {
		public MyDataObject dataObj; 
	}

	public class BadEditorWindow : EditorWindow {
		private const string WINDOW_TITLE = "Bad Editor";

		private MyDataModel model;

		public void LoadData(int data) {
			model = new MyDataModel();
			model.dataObj = new MyDataObject();
			model.dataObj.data = data;
		}

		void OnGUI() {
			if(model == null)
				return;

			model.dataObj.data = EditorGUILayout.IntSlider("data", model.dataObj.data, 0, 100);
		}

		void OnEnable() {
			titleContent.text = WINDOW_TITLE;
		}

		[MenuItem ("Window/Serialization Test/Test 0 - " + WINDOW_TITLE)]
		public static void  ShowWindow()
		{
			var window = EditorWindow.GetWindow<BadEditorWindow>();
			window.LoadData(UnityEngine.Random.Range(0, 100));
		}

	} // class BadWindow

} // namespace SerializationTest.Test0