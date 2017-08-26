using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SerializationTest.Common;

namespace SerializationTest.Test7
{
	[Serializable]
	public class Info
	{
		public float number1;
		public int number2;
	}

	[Serializable]
	public class InfoBook : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public Dictionary<int, Info> dict = new Dictionary<int, Info>();

		[SerializeField]
		private List<int> keyList;
		[SerializeField]
		private List<Info> valueList;

		public void OnBeforeSerialize() {
			keyList = new List<int>();
			valueList = new List<Info>();

			var e = dict.GetEnumerator();
			while(e.MoveNext()) {
				keyList.Add(e.Current.Key);
				valueList.Add(e.Current.Value);
			}
		}

		public void OnAfterDeserialize() {
			dict.Clear();

			for(int i = 0; i < keyList.Count; i++) {
				dict[keyList[i]] = valueList[i];
			}

			keyList = null;
			valueList = null;
		}
	}

	public class DictionarySerializationWindow : EditorWindow {
		private const string WINDOW_TITLE = "Dictionary Serialization";

		public InfoBook infoBook;

		void OnEnable() {
			titleContent.text = WINDOW_TITLE;

			if(infoBook == null) {
				infoBook = new InfoBook();
			}
		}

		void OnGUI() {

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("id");
			EditorGUILayout.LabelField("number1");
			EditorGUILayout.LabelField("number2");
			EditorGUILayout.EndHorizontal();

			var e = infoBook.dict.GetEnumerator();
			while(e.MoveNext()) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(e.Current.Key.ToString());
				e.Current.Value.number1 = EditorGUILayout.Slider(e.Current.Value.number1, 0, 100);
				e.Current.Value.number2 = EditorGUILayout.IntSlider(e.Current.Value.number2, 0, 100);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			if(GUILayout.Button("generate")) {
				var info = new Info();
				info.number1 = UnityEngine.Random.Range(0, 100);
				info.number2 = UnityEngine.Random.Range(0, 100);
				infoBook.dict[UnityEngine.Random.Range(1, 1000000)] = info;
			}
		}

		[MenuItem ("Window/Serialization Test/Test 7 - " + WINDOW_TITLE)]
		public static void  ShowWindow()
		{
			EditorWindow.GetWindow<DictionarySerializationWindow>();
		}

	} // class TestWindow7

} // namespace SerializationTest.Test7