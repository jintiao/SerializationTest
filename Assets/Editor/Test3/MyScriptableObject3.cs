using UnityEngine;
using UnityEditor;

namespace SerializationTest.Test3
{

public class MyScriptableObject3 : ScriptableObject {
	public int i1;
	public int i2;

	public void OnGUI() {
		EditorGUILayout.LabelField("InstanceId", GetInstanceID().ToString());
		i1 = EditorGUILayout.IntSlider("i1", i1, 0, 100);
		i2 = EditorGUILayout.IntSlider("i2", i2, 0, 100);
	}

	public void Roll() {
		i1 = Random.Range(0, 100);
		i2 = Random.Range(0, 100);
	}

} // class MyScriptableObject3

} // namespace SerializationTest.Test3