using UnityEngine;
using UnityEditor;

namespace SerializationTest.Test2
{

public class MyScriptableObject2 : ScriptableObject {
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

} // class MyScriptableObject2

} // namespace SerializationTest.Test2