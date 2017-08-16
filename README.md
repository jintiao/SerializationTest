## Unity插件开发与序列化

### 前言

在使用Unity进行游戏开发过程中，离不开各式各样插件的使用。然而尽管现成的插件非常多，有时我们还是需要自己动手去制作一些插件。

Unity插件开发与Unity游戏开发相比有一个显著的区别，就是对于序列化功能的依赖。插件在自身运行、以及操作数据输入输出时，都需要序列化功能的支持。如果对于引擎序列化功能的细节不了解，做出来的插件很可能会不稳定经常报错，导致数据丢失等后果。

那么我们接下来就细数插件开发中需要注意的序列化的细(da)节(keng)。

### 序列化规则

根据Unity的官方定义，序列化就是将数据结构或对象状态转换成可供Unity保存和随后重建的自动化处理过程。那么，序列化的自动处理过程是由哪些规则决定的呢，简单来说分两点，一是系统序列化是否支持该对象对应的数据类型，二是对象内部哪些成员字段需要进行序列化。

当引擎要对某对象进行序列化时，引擎首先检查对象是否为支持序列化的数据类型，如果满足条件，引擎将对该对象所有应该进行序列化操作的成员字段进行序列化。如此递归执行直到序列化操作全部完成。

下面两张表简单地总结了序列化的规则：

| 支持的数据类型 | 不支持的数据类型 |
| ------------- | ------------- |
| c#原生数据类型(int/float/bool/string等，以及枚举)  | 抽象类 |
| Unity内置数据类型(Vector/Rect/Quaternion等)  | 静态类 |
| 继承自UnityEngine.Object的类 | 范型类  |
| 标记了[Serializable]属性的自定义类  | 没有标记[Serializable]属性的自定义类 |
| Array,List容器  | Dictionary或其它容器 |

| 应该序列化的成员字段 | 不应该序列化的成员字段 |
| ------------- | ------------- |
| public成员 | const/readonly/static成员 |
| 标记了[SerializeField]属性的成员  | 标记了[NonSerialized]属性的成员 |

我们来通过一个简单的例子说明一下这个规则。

我们先自定义两个类，一个叫MyClass，一个叫MyClassSerializable，主要的代码如下
```
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
```
public class TestWindow1 : EditorWindow {
	public MyClass m1;
	public MyClassSerializable s1;
	private MyClassSerializable s2;
}
```

GUI相关的代码就不列出来了，完整的代码在[这里](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/test1/TestWindow1.cs)。

在编辑器中打开TestWindow1，分别给各个对象随机赋值
![重启编辑器前](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-1.png)

退出编辑器，这时编辑器会对TestWindow1对象进行序列化。再次打开编辑器，编辑器会通过反序列化创建一个新的TestWindow1对象，我们来看经过序列化反序列化后的TestWindow1对象有何变化
![重启编辑器后](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-2.png)

### 参考

[1] [Unity Manual - Script Serialization](https://docs.unity3d.com/Manual/script-Serialization.html)
