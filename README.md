## Unity插件开发与序列化

### 一：前言

在使用Unity进行游戏开发过程中，离不开各式各样插件的使用。然而尽管现成的插件非常多，有时我们还是需要自己动手去制作一些插件。

Unity插件开发与Unity游戏开发相比有一个显著的区别，就是对于序列化功能的依赖。插件在自身运行、以及操作数据输入输出时，都需要序列化功能的支持。如果对于引擎序列化功能的细节不了解，做出来的插件很可能会不稳定经常报错，导致数据丢失等后果。

那么我们接下来就细数插件开发中需要注意的序列化的细(da)节(keng)。

### 二：序列化规则

根据Unity的官方定义，序列化就是将数据结构或对象状态转换成可供Unity保存和随后重建的自动化处理过程。那么，序列化的自动处理过程是由哪些规则决定的呢，简单来说分两点，一是系统序列化是否支持该对象对应的数据类型，二是对象内部哪些成员字段需要进行序列化。

当引擎要对某对象进行序列化时，引擎首先检查对象是否为支持序列化的数据类型，如果满足条件，引擎将对该对象所有应该进行序列化操作的成员字段进行序列化。如此递归执行直到序列化操作全部完成。

下面两张表简单地总结了序列化的规则：

* 类型规则

| 支持的数据类型 | 不支持的数据类型 |
| ------------- | ------------- |
| c#原生数据类型(int/float/bool/string等，以及枚举)  | 抽象类 |
| Unity内置数据类型(Vector/Rect/Quaternion等)  | 静态类 |
| 继承自UnityEngine.Object的类 | 范型类  |
| 标记了[Serializable]属性的自定义类  | 没有标记[Serializable]属性的自定义类 |
| Array,List容器  | Dictionary或其它容器 |

* 字段规则

| 应该序列化的成员字段 | 不应该序列化的成员字段 |
| ------------- | ------------- |
| public成员 | const/readonly/static成员 |
| 标记了[SerializeField]属性的成员  | 标记了[NonSerialized]属性的成员 |

我们来通过一个简单的例子说明一下这个规则。

我们先自定义两个类，一个叫MyClass，一个叫MyClassSerializable，主要的代码如下
```c#
public class MyClass {
	public string s;
}

[Serializable]
public class MyClassSerializable {
	public float f1;
	[NonSerialized]public float f2;
	private int i1;
	[SerializeField]private int i2;
}
```

然后我们新建一个编辑器窗口，用来触发序列化，看系统会如何处理这两个类的对象。
```c#
public class TestWindow1 : EditorWindow {
	public MyClass m1;
	public MyClassSerializable s1;
	private MyClassSerializable s2;
}
```

GUI相关的代码就不列出来了，完整的代码在[这里](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test1/TestWindow1.cs)。

| 重启编辑器前 | 重启编辑器后 |
| ------------- | ------------- |
| ![重启编辑器前](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-1.png) | ![重启编辑器后](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-2.png) |

在编辑器中打开TestWindow1，分别给各个对象随机赋值

退出编辑器，这时编辑器会对TestWindow1对象进行序列化。再次打开编辑器，编辑器会通过反序列化创建一个新的TestWindow1对象，我们来看经过序列化反序列化后的TestWindow1对象有何变化


可以看到，只有s1的两个成员f1和i2保存了原来的值，其它成员都被清零了，我们来具体分析一下为什么会是这样。

首先看`public MyClass m1`，第一它的成员字段是`public`，满足字段规则；第二它的类型是`MyClass`，属于没有标记`[Serializable]`属性的自定义类，不满足类型规则，于是它在序列化过程中被跳过了。

接下来看`public MyClassSerializable s1`，第一它的成员字段是`public`，满足字段规则；第二它的类型是`MyClassSerializable`，属于标记了`[Serializable]`属性的自定义类，满足类型规则。`s1`同时满足字段规则和类型规则，系统要对它进行序列化操作。
系统接着对`s1`的成员逐一进行规则检查。
```c#
public float f1; // 字段是public,满足；类型float是c#原生数据类型，也满足。系统对f1进行序列化。
[NonSerialized]public float f2; // 字段是public，但是被标记了[NonSerialized]，所以字段规则不满足，f2不会进行序列化。
private int i1; // 字段是private，所以字段规则不满足，i1不会进行序列化。
[SerializeField]private int i2; // 字段是private，但是被标记了[SerializeField]，所以字段规则满足；类型int是c#原生数据类型，也满足。系统对i2进行序列化。
```
这就解释了为什么s1中只有f1和i2保留这原来的值。

最后我们看`private MyClassSerializable s2`，这时我们都能轻易看出来了，`private`字段不满足规则，s2不进行序列化。

### 三：跨过序列化的坑

看到这里，是不是觉得序列化很容易掌握？别高兴太早，这个世界并不是我们想象的这么简单，现在是时候让我们来面对序列化复杂的另一面了。

#### 1.EditorWindow热重载
热重载(hot-reloading)是我们最常见却又常常忽略的序列化例外。我们继续用前面的TestWindow1做演示，打开窗口分别给各个对象随机赋值

| 热重载前 | 热重载后 |
| ------------- | ------------- |
| ![热重载前](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-3.png) | ![热重载后](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-4.png) |



现在我们点击编辑器的"播放"按钮，观察TestWindow1窗口中的值有何变化


可以看到，和前一节的例子不同，大部分变量都维持着原来的值；只有不支持序列化的m1，以及s1/s2中标记了[NonSerialized]的f2丢失了旧值。

看过了现象，现在我们对热重载进行详细讲解。

当编辑器检测到代码环境发生变化(脚本被修改需要重新编译、切换播放状态)时，会对所有现存的编辑器窗口进行热重载。等待编译完成(或切换到播放状态)，编辑器根据之前序列化的值对编辑器窗口进行恢复。

热重载序列化与标准序列化的不同点是：热重载时，只要被处理对象能够被序列化，且没有标记[NonSerialized]，那么就对其进行序列化。

#### 2.UnityEngine对象的序列化

Unity对于UnityEngine.Object及其派生类型(以下统一简称为Object)对象也有着特殊的序列化规则，总结如下
* Object对象需要单独进行序列化。
* Object对象的类名必须和文件名完全一致。
* 对于任意一个对象中，如果它保存着Object对象的引用，那么序列化这个对象时只会序列化Object对象的引用。

我们还是通过例子来说明这几条规则。
我们新定义一个编辑器窗口`EditorWindow2`，一个自定义的类`MyScriptableObject2`，继承自`ScriptableObject`
```c#
public class TestWindow2 : EditorWindow {
	public MyScriptableObject2 m;
	void OnEnable() {
		if(m == null)
			m = CreateInstance<MyScriptableObject2>();
	}
} // class TestWindow2

public class MyScriptableObject2 : ScriptableObject {
	public int i1;
	public int i2;
} // class MyScriptableObject2
```

| 热重载前 | 热重载后 |
| ------------- | ------------- |
| ![热重载前](https://github.com/jintiao/SerializationTest/blob/master/Doc/test2-1.png) | ![热重载后](https://github.com/jintiao/SerializationTest/blob/master/Doc/test2-2.png) |

可以看到，`EditorWindow2`保存着一个`MyScriptableObject2`对象，这个对象在热重载后丢失了。原因是，`EditorWindow2`序列化时只是对`m`的引用进行序列化，而`m`本身没有进行序列化，在`EditorWindow2`被释放掉之后，`m`引用的对象被gc掉了。编辑器在重建`EditorWindow2`时，发现`m`引用的对象不存在，于是把引用置空。

这就是规则1和规则3的具体表现。那么怎样处理才是正确的呢？我们看稍加修改的`EditorWindow3`
```c#
public class TestWindow3 : EditorWindow {
	public MyScriptableObject3 m;
	void OnEnable() {
		if(m == null) {
			string path = "Assets/Editor/Test3/SaveData.asset";
			m = AssetDatabase.LoadAssetAtPath<MyScriptableObject3>(path);
			if(m == null) {
				m = CreateInstance<MyScriptableObject3>();
				AssetDatabase.CreateAsset(m, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
	}
} // class TestWindow3
```

| 热重载前 | 热重载后 |
| ------------- | ------------- |
| ![热重载前](https://github.com/jintiao/SerializationTest/blob/master/Doc/test3-1.png) | ![热重载后](https://github.com/jintiao/SerializationTest/blob/master/Doc/test3-2.png) |

`EditorWindow3`在初始化时，先尝试读取m对应的asset，在asset不存在时，创建一个新的`MyScriptableObject3`对象，并显式地对m进行保存。
这样，编辑器在重建`EditorWindow3`时，发现`m`引用的对象并没有载入，会自动进行加载。

最后简单说以下规则2，类名与文件名相同这是Unity的硬性规定，比如`MyScriptableObject3`对应的文件名必须是`MyScriptableObject3.cs`。如果你发现编辑器在启动时，而且只在启动时报序列化错误，很大可能是因为类名和文件名不同所导致的。

#### 3.非UnityEngine对象的序列化

* 序列化时保存值
* 同一对象的多个引用，在序列化后会变成多个对象
* 对对象的空引用，在序列化后会变成新对象
* 不支持多态

#### 4.自定义序列化

### 四：序列化最佳实践


### 参考

[1] [Unity Manual - Script Serialization](https://docs.unity3d.com/Manual/script-Serialization.html)
[2] [Unity Manual - Custom Serialization](https://docs.unity3d.com/Manual/script-Serialization-Custom.html)
[3] [Serialization in-depth with Tim Cooper](https://www.youtube.com/watch?v=MmUT0ljrHNc)
