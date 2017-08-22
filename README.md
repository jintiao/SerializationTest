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

可以看到，只有s1的两个成员f1和i2保存了原来的值，其它成员都被清零了，我们来具体分析一下为什么会是这样。

首先看`public MyClass m1`，第一它的成员字段是`public`，满足字段规则；第二它的类型是`MyClass`，属于没有标记`[Serializable]`属性的自定义类，不满足类型规则，于是它在序列化过程中被跳过了。

接下来看`public MyClassSerializable s1`，第一它的成员字段是`public`，满足字段规则；第二它的类型是`MyClassSerializable`，属于标记了`[Serializable]`属性的自定义类，满足类型规则。`s1`同时满足字段规则和类型规则，系统要对它进行序列化操作。
系统接着对`s1`的成员逐一进行规则检查。
```
public float f1; // 字段是public,满足；类型float是c#原生数据类型，也满足。系统对f1进行序列化。
[NonSerialized]public float f2; // 字段是public，但是被标记了[NonSerialized]，所以字段规则不满足，f2不会进行序列化。
private int i1; 字段是private，所以字段规则不满足，i1不会进行序列化。
[SerializeField]private int i2; // 字段是private，但是被标记了[SerializeField]，所以字段规则满足；类型int是c#原生数据类型，也满足。系统对i2进行序列化。
```
这就解释了为什么s1中只有f1和i2保留这原来的值。

最后我们看`private MyClassSerializable s2`，这时我们都能轻易看出来了，`private`字段不满足规则，s2不进行序列化。

### 三：跨过序列化的坑

看到这里，是不是觉得序列化很容易掌握？别高兴太早，这个世界并不是我们想象的这么简单，现在是时候让我们来面对序列化复杂的另一面了。

#### 1.热重载
热重载(hot-reload)是我们最常见却又常常忽略的序列化例外。我们继续用前面的TestWindow1做演示，打开窗口分别给各个对象随机赋值
![热重载前](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-3.png)

现在我们点击编辑器的"播放"按钮，观察TestWindow1窗口中的值有何变化
![热重载后](https://github.com/jintiao/SerializationTest/blob/master/Doc/test1-4.png)

可以看到，和前一节的例子不同，大部分变量都维持着原来的值；只有不支持序列化的m1，以及s1/s2中标记了[NonSerialized]的f2丢失了旧值。

看过了现象，现在我们对热重载进行详细讲解。

当编辑器检测到代码环境发生变化(脚本被修改需要重新编译、切换播放状态)时，会对所有现存的编辑器窗口进行热重载。等待编译完成(或切换到播放状态)，编辑器根据之前序列化的值对编辑器窗口进行恢复。

热重载序列化与标准序列化的不同点是：热重载时，只要被处理对象能够被序列化，且没有标记[NonSerialized]，那么就对其进行序列化。

#### 2.UnityEngine.Object只会序列化引用

#### 3.自定义类/结构只会序列化值

#### 4.自定义类/结构不支持null

### 四：序列化最佳实践


### 参考

[1] [Unity Manual - Script Serialization](https://docs.unity3d.com/Manual/script-Serialization.html)
