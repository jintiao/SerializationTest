## 浅谈Unity序列化系统

### 一：前言

在使用Unity进行游戏开发过程中，离不开各式各样插件的使用。然而尽管现成的插件非常多，有时我们还是需要自己动手去制作一些插件。进入插件开发的世界，就避免不了和序列化系统打交道。

可以说Unity编辑器很大程度上是建立在序列化系统之上的，一般来说，编辑器不会去直接操作游戏对象，需要与游戏对象交互时，先触发序列化系统对游戏对象进行序列化，生成序列化数据，然后编辑器对序列化数据进行操作，最后序列化系统根据修改过的序列化数据生成新的游戏对象。

就算不需要与游戏对象交互，编辑器本身也会不断地对所有编辑器窗口触发序列化。如果在制作插件时不小心地处理序列化甚至忽略序列化系统的存在，做出来的插件很可能会不稳定经常报错，导致数据丢失等后果。

下面的[例子](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test0-BadEditor/BadEditorWindow.cs)展示的是我们新接触插件开发时最常遇到的一种异常情况：插件本来运行地好好地，点了一下播放后插件就发疯地不断报错，某个对象莫名被重置了。

| 插件正常运行 | 点击播放后 |
| ------------- | ------------- |
| ![插件正常运行](https://github.com/jintiao/SerializationTest/blob/master/Image/0-1.png) | ![点击播放后](https://github.com/jintiao/SerializationTest/blob/master/Image/0-2.png) |

如果你曾经遇到过这种情况，而且不明白为什么，这篇文章应该能解答你的疑惑。

### 二：序列化是什么
根据Unity的官方定义，序列化就是将数据结构或对象状态转换成可供Unity保存和随后重建的自动化处理过程。

### 三：序列化规则

序列化规则简单来说有两点，一是类型规则，系统据此判断能不能对对象进行序列化；二是字段规则，系统据此判断该不该对对象进行序列化。当对象同时满足类型规则和字段规则时，系统就会对该对象进行序列化。

* 类型规则

| 能序列化的类型 | 不能序列化的类型 |
| ------------- | ------------- |
| c#原生数据类型(int/string/enum...)  | 抽象类 |
| Unity内置数据类型(Vector/Rect/Color...)  | 静态类 |
| 继承自UnityEngine.Object的类 | 泛型类  |
| 标记了[Serializable]属性的类  | 没有标记[Serializable]属性的类 |
| Array,List容器  | 其它容器 |

* 字段规则

| 该序列化的字段 | 不该序列化的字段 |
| ------------- | ------------- |
| public字段 | private/const/readonly/static成员 |
| 标记了[SerializeField]属性的字段  | 标记了[NonSerialized]属性的字段 |

我们通过一个[例子](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test1-SerializationRule/SerializationRuleWindow.cs)来具体讲解一下。

我们定义了两个类，一个叫`MyClass`，另一个叫`MyClassSerializable`
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

接下来我们定义一个插件类`SerializationRuleWindow`
```c#
public class SerializationRuleWindow : EditorWindow {
	public MyClass m1;
	public MyClassSerializable s1;
	private MyClassSerializable s2;
}
```

点击编辑器菜单"Window -> Serialization Test -> Test 1 - Serialization Rule"打开插件窗口，可以看到窗口中显示着所有对象当前的值，并且可以通过滚动条修改各个对象的值。一切看起来很美好，接下来我们关掉编辑器再重新打开，看看插件窗口会出现什么变化：

| 重启编辑器前 | 重启编辑器后 |
| ------------- | ------------- |
| ![重启编辑器前](https://github.com/jintiao/SerializationTest/blob/master/Image/1-1.png) | ![重启编辑器后](https://github.com/jintiao/SerializationTest/blob/master/Image/1-2.png) |

可以看到，s1的两个成员f1和i2保存了原来的值，其它成员都被清零了，我们来具体分析一下为什么会是这样。

编辑器退出前会对所有打开的窗口进行序列化并保存序列化数据到硬盘。在重启编辑器后，序列化系统读入序列化数据，重新生成对应的窗口对象。在对我们的插件对象`SerializationRuleWindow`进行序列化时，只有满足序列化规则的对象的值得以保存，不满足规则的对象则被序列化系统忽略。

我们来仔细看一下规则判定的情况。

首先看`public MyClass m1`，它的类型是`MyClass`，属于“没有标记`[Serializable]`属性的类”，不满足类型规则；它的字段是`public`，满足字段规则；系统要求两条规则同时满足的对象才能序列化，于是它被跳过了。

接下来看`public MyClassSerializable s1`，它的类型是`MyClassSerializable`，属于标记了`[Serializable]`属性的类，满足类型规则；它的字段是`public`，满足字段规则；`s1`同时满足类型规则和字段规则，系统需要对它进行序列化操作。

序列化是一个递归过程，对`s1`进行序列化意味着要对`s1`的所有类成员对象进行序列化判断。所以现在轮到`s1`中的成员进行规则判断了。

`public float f1`，类型float是c#原生数据类型，满足类型规则；字段是public，满足字段规则；判断通过。

`[NonSerialized]public float f2`，字段被标记了[NonSerialized]，不满足字段规则。

`private int i1`，字段是private，不满足字段规则。

`[SerializeField]private int i2`，类型int是c#原生数据类型，满足类型规则；字段被标记了[SerializeField]，满足字段规则；判断通过。

所以s1中f1和i2通过了规则判断，f2和i1没有通过。所以图中s1.f1和s1.i2保留了原来的值。

最后我们看`private MyClassSerializable s2`，这时相信我们都能轻易看出来，`private`不满足字段规则，s2被跳过。

### 四：跨过序列化的坑

上一节我们通过例子了解了序列化的规则，我们发现我们好像已经掌握了序列化系统的秘密。但！是！别高兴太早，这个世界并不是我们想象的这么简单，现在是时候让我们来面对系统复杂的一面了。

#### 1. 热重载(hot-reloading)

对脚本进行修改可以即时编译，不需要重启编辑器就看看到效果，这是Unity编辑器的一个令人称赞的机制。你有没有想过它是怎么实现的呢？答案就是热重载。

当编辑器检测到代码环境发生变化(脚本被修改、点击播放)时，会对所有现存的编辑器窗口进行**热重载序列化**。等待环境恢复(编译完成、转换到播放状态)时，编辑器根据之前序列化的值对编辑器窗口进行恢复。

热重载序列化与标准序列化的不同点是，在进行热重载序列化时，字段规则被忽略，只要被处理对象满足类型规则，那么就对其进行序列化。

我们可以通过运行之前讲解序列化规则时的[例子](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test1-SerializationRule/SerializationRuleWindow.cs)来对比热重载序列化与标准序列化的区别。

记得上一节我们是通过退出重启编辑器触发的标准序列化，现在我们通过点击播放触发热重载序列化，运行结果如下

| 热重载前 | 热重载后 |
| ------------- | ------------- |
| ![热重载前](https://github.com/jintiao/SerializationTest/blob/master/Image/1-3.png) | ![热重载后](https://github.com/jintiao/SerializationTest/blob/master/Image/1-4.png) |

可以看到，之前由于字段为`private`的`s1.i1`以及`s2`都进行了序列化。同时我们也注意到标记了[NonSerialized]的`s1.f2`和`s2.f2`、没有标记[Serializable]的`m1`依然被跳过了。

#### 2. ScriptableObject对象的序列化

我们把UnityEngine.Object及其派生类(比如MonoBehaviour和ScriptableObject)称为Unity引擎对象，它们属于引擎内部资源，在序列化时和其他普通类对象的处理机制上有着较大的区别。

引擎对象特有的序列化规则如下：
* 引擎对象需要单独进行序列化。
* 如果别的对象保存着引擎对象的引用，这个对象序列化时只会序列化引擎对象的引用，而不是引擎对象本身。
* 引擎对象的类名必须和文件名完全一致。

对于插件开发，我们最可能接触到的引擎对象就是ScriptableObject，我们通过这个[例子](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test2-ScriptableObject/ScriptableObjectWindow.cs)来讲解ScriptableObject的序列化。

我们新定义一个编辑器窗口`ScriptableObjectWindow`，一个自定义的类`MyScriptableObject`
```c#
public class MyScriptableObject : ScriptableObject {
	public int i1;
	public int i2;
}

public class ScriptableObjectWindow : EditorWindow {
	public MyScriptableObject m;
	void OnEnable() {
		if(m == null)
			m = CreateInstance<MyScriptableObject>();
	}
}
```

我们把`m`的字段设为`public`确保系统会对它进行序列化。我们来看运行结果

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/2-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/2-2.png) |

可以看到，`m`在热重载后是个空值，`ScriptableObjectWindow`只能重新生成一个新的`MyScripatable`对象。

回看第二条规则，我们知道`ScriptableObjectWindow`序列化时只会保存`m`对象的引用。在编辑器状态变化后，`m`所引用的引擎对象被gc释放掉了(序列化后`ScriptableObjectWindow`被销毁，引擎对象没有别的引用了)。所以编辑器在重建`ScriptableObjectWindow`时，发现`m`是个无效引用，于是将`m`置空。

那么，如何避免`m`引用失效呢？很简单，将`m`保存到硬盘就行了。对于引擎对象的引用，Unity不光能找到已经加载的内存对象，还能在对象未加载时找到它对应的文件进行自动加载。在这个[例子](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test3-SavedScriptableObject/SavedScriptableObjectWindow.cs)，我们在创建`MyScriptableObject`对象的同时将其保存到硬盘，确保其永久有效

```c#
public class SavedScriptableObjectWindow : EditorWindow {
	public MyScriptableObject m;
	void OnEnable() {
		if(m == null) { // 注意只在新开窗口时 m 才会为 null
			string path = "Assets/Editor/Test3-SavedScriptableObject/SaveData.asset";
			// 先尝试从硬盘中读取asset
			m = AssetDatabase.LoadAssetAtPath<MyScriptableObject>(path);
			if(m == null) { // 当asset不存在时创建并保存
				m = CreateInstance<MyScriptableObject>();					AssetDatabase.CreateAsset(m, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
	}
}
```

运行新的例子，我们可以看到`m`引用的对象再也不会丢失了

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/3-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/3-2.png) |

最后简单说一下第三条规则，类名与文件名相同这是Unity的硬性规定，比如`MyScriptableObject`对应的文件名必须是`MyScriptableObject.cs`。如果你发现编辑器在启动时，而且只在启动时报序列化错误，很大可能是因为类名和文件名不同所导致的。

#### 3. 非引擎对象的序列化

由于每个ScriptableObject对象都需要单独保存，插件最终可能会生成大量的asset文件，所以我们一般更倾向于使用自定义类。

* 不支持null引用，系统会自动生成新对象。

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/4-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/4-2.png) |

* 不支持多态。

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/5-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/5-2.png) |

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/6-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/6-2.png) |

* 序列化时保存值。同一对象的多个引用，在序列化后会变成多个对象

#### 4.容器的序列化

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/7-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/7-2.png) |

### 四：序列化最佳实践


### 参考

[1] [Unity Manual - Script Serialization](https://docs.unity3d.com/Manual/script-Serialization.html)

[2] [Unity Manual - Custom Serialization](https://docs.unity3d.com/Manual/script-Serialization-Custom.html)

[3] [Serialization in-depth with Tim Cooper](https://www.youtube.com/watch?v=MmUT0ljrHNc)
