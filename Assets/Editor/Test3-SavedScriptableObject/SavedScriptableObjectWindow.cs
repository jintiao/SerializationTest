using System;
using UnityEngine;
using UnityEditor;
using SerializationTest.Common;

namespace SerializationTest.Test3
{

	public class SavedScriptableObjectWindow : EditorWindow {
		private const string WINDOW_TITLE = "Saved ScriptableObject";

		public MyScriptableObject m;

		void OnEnable() {
			if(m == null) {
				string path = "Assets/Editor/Test3-SavedScriptableObject/SaveData.asset";
				m = AssetDatabase.LoadAssetAtPath<MyScriptableObject>(path);
				if(m == null) {
					m = CreateInstance<MyScriptableObject>();
					AssetDatabase.CreateAsset(m, path);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}

			titleContent.text = WINDOW_TITLE;
		}

		void OnGUI() {
			EditorGUILayout.Space();
			m.OnGUI();

			EditorGUILayout.Space();
			if(GUILayout.Button("roll")) {
				m.Roll();
			}

			if(GUI.changed) {
				EditorUtility.SetDirty(m);
			}
		}

		[MenuItem ("Window/Serialization Test/Test 3 - " + WINDOW_TITLE)]
		public static void  ShowWindow() {
			EditorWindow.GetWindow<SavedScriptableObjectWindow>();
		}

	} // class TestWindow3

} // namespace SerializationTest.Test3