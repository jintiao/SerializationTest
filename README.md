## Unity插件开发基础 - 浅谈序列化系统

### 一：前言

在使用Unity进行游戏开发过程中，离不开各式各样插件的使用。然而尽管现成的插件非常多，有时我们还是需要自己动手去制作一些插件。进入插件开发的世界，就避免不了和序列化系统打交道。

可以说Unity编辑器很大程度上是建立在序列化系统之上的，一般来说，编辑器不会去直接操作游戏对象，需要与游戏对象交互时，先触发序列化系统对游戏对象进行序列化，生成序列化数据，然后编辑器对序列化数据进行操作，最后序列化系统根据修改过的序列化数据生成新的游戏对象。

就算不需要与游戏对象交互，编辑器本身也会不断地对所有编辑器窗口触发序列化。如果在制作插件时没有正确地处理序列化甚至忽略序列化系统的存在，做出来的插件很可能会不稳定经常报错，导致数据丢失等后果。

下面的[例子](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test0-BadEditor/BadEditorWindow.cs)展示的是我们新接触插件开发时最常遇到的一种异常情况：插件本来运行地好好地，点了一下播放后插件就发疯地不断报错，某个(些)对象莫名被置空了：

| 插件正常运行 | 点击播放后 |
| ------------- | ------------- |
| ![插件正常运行](https://github.com/jintiao/SerializationTest/blob/master/Image/0-1.png) | ![点击播放后](https://github.com/jintiao/SerializationTest/blob/master/Image/0-2.png) |

如果你曾经遇到过这种情况，而且不明白为什么，这篇文章应该能解答你的疑惑。

### 二：序列化是什么
根据Unity的官方定义，序列化就是将数据结构或对象状态转换成可供Unity保存和随后重建的自动化处理过程。
> Serialization is the automatic process of transforming data structures or object states into a format that Unity can store and reconstruct later.

很多引擎功能会自动触发序列化，比如
* 文件的保存/读取，包括Scene、Asset、AssetBundle，以及自定义的ScriptableObject等。
* Inspector窗口
* 编辑器重加载脚本脚本
* Prefab
* Instantiation

### 三：序列化规则

既然序列化是一个自动化的过程，那我们能做什么呢，是不是只能坐在一边看系统自己表演呢？并不是，序列化确实是一个自动化过程，但引擎并不是完美人工智能，系统的功能受到序列化规则的限制。我们能做的，是通过规则告诉系统，哪些数据需要序列化，哪些数据不需要序列化。

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
| public成员 | private/const/readonly/static成员 |
| 标记了[SerializeField]属性的成员  | 标记了[NonSerialized]属性的成员 |

我们通过[例子1](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test1-SerializationRule/SerializationRuleWindow.cs)来具体讲解一下。

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

点击编辑器菜单"Window -> Serialization Test -> Test 1 - Serialization Rule"打开插件窗口，可以看到窗口中显示着所有对象当前的值，并且可以通过滚动条修改各个对象的值。一切看起来很美好，接下来我们退出编辑器再重新打开，看看插件窗口会出现什么变化：

| 重启编辑器前 | 重启编辑器后 |
| ------------- | ------------- |
| ![重启编辑器前](https://github.com/jintiao/SerializationTest/blob/master/Image/1-1.png) | ![重启编辑器后](https://github.com/jintiao/SerializationTest/blob/master/Image/1-2.png) |

可以看到，`s1`的两个成员`f1`和`i2`保存了原来的值，其它成员都被清零了，我们来具体分析一下为什么会是这样。

编辑器退出前会对所有打开的窗口进行序列化并保存序列化数据到硬盘。在重启编辑器后，序列化系统读入序列化数据，重新生成对应的窗口对象。在对我们的插件对象`SerializationRuleWindow`进行序列化时，只有满足序列化规则的对象的值得以保存，不满足规则的对象则被序列化系统忽略。

我们来仔细看一下规则判定的情况。

首先看`public MyClass m1`，它的类型是`MyClass`，属于“没有标记`[Serializable]`属性的类”，不满足类型规则；它的字段是`public`，满足字段规则；系统要求两条规则同时满足的对象才能序列化，于是它被跳过了。

接下来看`public MyClassSerializable s1`，它的类型是`MyClassSerializable`，属于标记了`[Serializable]`属性的类，满足类型规则；它的字段是`public`，满足字段规则；`s1`同时满足类型规则和字段规则，系统需要对它进行序列化操作。

序列化是一个递归过程，对`s1`进行序列化意味着要对`s1`的所有类成员对象进行序列化判断。所以现在轮到`s1`中的成员进行规则判断了。

`public float f1`，类型`float`是c#原生数据类型，满足类型规则；字段是`public`，满足字段规则；判断通过。

`[NonSerialized]public float f2`，字段被标记了`[NonSerialized]`，不满足字段规则。

`private int i1`，字段是`private`，不满足字段规则。

`[SerializeField]private int i2`，类型`int`是c#原生数据类型，满足类型规则；字段被标记了`[SerializeField]`，满足字段规则；判断通过。

所以`s1`中`f1`和`i2`通过了规则判断，`f2`和`i1`没有通过。所以图中`s1.f1`和`s1.i2`保留了原来的值。

最后我们看`private MyClassSerializable s2`，这时相信我们都能轻易看出来，`private`不满足字段规则，`s2`被跳过。

### 四：跨过序列化的坑

上一节我们通过[例子1](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test1-SerializationRule/SerializationRuleWindow.cs)了解了序列化的规则，我们发现我们好像已经掌握了序列化系统的秘密。但！是！别高兴太早，这个世界并不是我们想象的这么简单，现在是时候让我们来面对系统复杂的一面了。

#### 1. 热重载(hot-reloading)

对脚本进行修改可以即时编译，不需要重启编辑器就看看到效果，这是Unity编辑器的一个令人称赞的机制。你有没有想过它是怎么实现的呢？答案就是热重载。

当编辑器检测到代码环境发生变化(脚本被修改、点击播放)时，会对所有现存的编辑器窗口进行**热重载序列化**。等待环境恢复(编译完成、转换到播放状态)时，编辑器根据之前序列化的值对编辑器窗口进行恢复。

热重载序列化与标准序列化的不同点是，在进行热重载序列化时，字段规则被忽略，只要被处理对象满足类型规则，那么就对其进行序列化。

我们可以通过运行之前讲解序列化规则时的[例子1](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test1-SerializationRule/SerializationRuleWindow.cs)来对比热重载序列化与标准序列化的区别。

记得上一节我们是通过退出重启编辑器触发的标准序列化，现在我们通过点击播放触发热重载序列化，运行结果如下

| 热重载前 | 热重载后 |
| ------------- | ------------- |
| ![热重载前](https://github.com/jintiao/SerializationTest/blob/master/Image/1-3.png) | ![热重载后](https://github.com/jintiao/SerializationTest/blob/master/Image/1-4.png) |

可以看到，之前由于字段为`private`的`s1.i1`以及`s2`都进行了序列化。同时我们也注意到标记了[NonSerialized]的`s1.f2`和`s2.f2`、没有标记`[Serializable]`的`m1`依然被跳过了。

#### 2. 引擎对象的序列化

我们把`UnityEngine.Object`及其派生类(比如`MonoBehaviour`和`ScriptableObject`)称为Unity引擎对象，它们属于引擎内部资源，在序列化时和其他普通类对象的处理机制上有着较大的区别。

引擎对象特有的序列化规则如下：
* 引擎对象需要单独进行序列化。
* 如果别的对象保存着引擎对象的引用，这个对象序列化时只会序列化引擎对象的引用，而不是引擎对象本身。
* 引擎对象的类名必须和文件名完全一致。

对于插件开发，我们最可能接触到的引擎对象就是`ScriptableObject`，我们通过[例子2](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test2-ScriptableObject/ScriptableObjectWindow.cs)来讲解`ScriptableObject`的序列化。

我们新定义一个编辑器窗口`ScriptableObjectWindow`，和一个继承自`ScriptableObject`的类`MyScriptableObject`
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

可以看到，`m`的`InstanceId`在热重载后发生了变化，这意味着原来`m`所引用的对象丢失了，`ScriptableObjectWindow`只能重新生成一个新的`MyScripatable`对象给`m`赋值。

回看第二条规则，我们知道`ScriptableObjectWindow`序列化时只会保存`m`对象的引用。在编辑器状态变化后，`m`所引用的引擎对象被gc释放掉了(序列化后`ScriptableObjectWindow`被销毁，引擎对象没有别的引用了)。所以编辑器在重建`ScriptableObjectWindow`时，发现`m`是个无效引用，于是将`m`置空。

那么，如何避免`m`引用失效呢？很简单，将`m`保存到硬盘就行了。对于引擎对象的引用，Unity不光能找到已经加载的内存对象，还能在对象未加载时找到它对应的文件进行自动加载。在[例子3](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test3-SavedScriptableObject/SavedScriptableObjectWindow.cs)，我们在创建`MyScriptableObject`对象的同时将其保存到硬盘，确保其永久有效

```c#
public class SavedScriptableObjectWindow : EditorWindow {
	public MyScriptableObject m;
	void OnEnable() {
		if(m == null) { // 注意只在新开窗口时 m 才会为 null
			string path = "Assets/Editor/Test3-SavedScriptableObject/SaveData.asset";
			// 先尝试从硬盘中读取asset
			m = AssetDatabase.LoadAssetAtPath<MyScriptableObject>(path);
			if(m == null) { // 当asset不存在时创建并保存
				m = CreateInstance<MyScriptableObject>();
				AssetDatabase.CreateAsset(m, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
	}
}
```

运行，我们可以看到`m`引用的对象再也不会丢失了

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/3-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/3-2.png) |

最后简单说一下第三条规则，类名与文件名相同这是Unity的硬性规定，比如`MyScriptableObject`对应的文件名必须是`MyScriptableObject.cs`。如果你发现编辑器在启动时，而且只在启动时报序列化错误，很大可能是因为类名和文件名不同所导致的。

#### 3. 普通类对象的序列化

由于每个`ScriptableObject`对象都需要单独保存，如果插件使用了多个`ScriptableObject`对象，保存这些对象意味着多个文件，而大量的零碎文件意味着读取速度会变慢。

如果你在考虑这个问题，不妨将目光转向普通类。和引擎对象不一样，普通类对象是按值存储的，所以我们可以将所有的普通类对象混在一起保存成单一文件。

然而按值序列化也有自己的问题，我们下面一一进行说明。

* 不支持空引用。

在[例子4](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test4-NullReference/NullReferenceWindow.cs)里，我们定义了两个普通类：`MyObject`和`MyNestedObject`：
```c#
[Serializable]
public class MyNestedObject {
	public int data;
}

[Serializable]
public class MyObject {
	public MyNestedObject obj2;
}

public class NullReferenceWindow : EditorWindow {
	public MyObject obj;

	void OnEnable() {
		if(obj == null) {
			obj = new MyObject();
		}
	}
}
```

可以看到，我们让`MyObject`保存一个`MyNestedObject`的引用，但不去初始化它，初次运行的时候我们知道它是一个空引用。我们来看看经过序列化后会有什么变化：

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/4-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/4-2.png) |

哈，系统帮我们生成了一个`MyNestedObject`对象！

通过测试我们知道，当系统对普通类对象进行序列化时，会自动给空引用生成对象。在我们的测试例子里，这个功能好像没有带来负面影响。但是在特定情况下会导致序列化失败，比如说带有同类的引用。

来看下面的链表类

```c#
[Serializable]
public class MyListNode {
	public int data;
	public MyListNode next;
}
```

这在我们的代码中很常见，也能正常运行，因为`next`最终会为空，意味着我们的链表是有尽头的。但是到了序列化系统里，回想一下，对啊序列化系统不允许有空引用，系统会帮我们无限地把这个链表链下去！当然，实际上系统检测到这种情况会主动终止序列化，但这意味着我们的类无法正常地进行序列化了。

* 不支持多态。

普通类序列化的另一个问题是不支持多态对象。在编码中我们使用一个基类引用指向一个派生类对象，这是很正常的设计。然而这种设计在序列化中却无法正常运作。

来看[例子5](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test5-Polymorphism/PolymophismWindow.cs)，首先我们定义了一系列的类代表不同的动物
```c#
[Serializable]
public class Animal {
	public virtual string Species { get { return "Animal"; } }
}

[Serializable]
public class Cat : Animal {
	public override string Species { get { return "Cat"; } }
}

[Serializable]
public class Dog : Animal {
	public override string Species { get { return "Dog"; } }
}

[Serializable]
public class Giraffe : Animal {
	public override string Species { get { return "Giraffe"; } }
}

[Serializable]
public class Zoo {
	public List<Animal> animals = new List<Animal>();
}
```

在`Zoo`类中，我们使用`List<Animal>`来记录动物园中的所有动物。我们来看看序列化系统会怎么对待我们的动物

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/5-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/5-2.png) |

可以看到，序列化之后我们的猫狗都被放跑了，这可不是我们想要的结果。

### 五. 自定义序列化

如之前所说，序列化功能有着各种各样的限制，而我们的项目需求千变万化，实际用到的数据结构只会比本文的例子复杂百倍。如何让这些更复杂的数据结构和序列化系统友好地合作呢？

答案是自定义序列化。Unity为我们提供了`ISerializationCallbackReceiver`接口，允许我们在序列化前后对数据进行操作。它并不能让系统直接处理你的复杂数据结构，但它给了你机会让你把数据"加工"成为系统能支持的形式。

#### 1.多态对象序列化

还记得我们例5的动物园吗，由于系统不支持多态对象造成了数据丢失，现在我们尝试通过自定义序列化来修正这个问题。
在[例子6](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test6-PolymorphismSerialization/PolymorphismSerializationWindow.cs)中，我们重新定义了`Zoo`类让它支持自定义序列化
```c#
[Serializable]
public class Zoo : ISerializationCallbackReceiver
{
	[NonSerialized]
	public List<Animal> animals = new List<Animal>();

	[SerializeField]
	private List<Cat> catList;
	[SerializeField]
	private List<Dog> dogList;
	[SerializeField]
	private List<Giraffe> giraffeList;
	[SerializeField]
	private List<int> indexList;

	public void OnBeforeSerialize() {
		catList = new List<Cat>();
		dogList = new List<Dog>();
		giraffeList = new List<Giraffe>();
		indexList = new List<int>();

		for(int i = 0; i < animals.Count; i++) {
			var type = animals[i].GetType();
			if(type == typeof(Cat)) {
				indexList.Add(0);
				indexList.Add(catList.Count);
				catList.Add((Cat)animals[i]);
			}
			if(type == typeof(Dog)) {
				indexList.Add(1);
				indexList.Add(dogList.Count);
				dogList.Add((Dog)animals[i]);
			}
			if(type == typeof(Giraffe)) {
				indexList.Add(2);
				indexList.Add(giraffeList.Count);
				giraffeList.Add((Giraffe)animals[i]);
			}
		}
	}

	public void OnAfterDeserialize() {
		animals.Clear();
		for(int i = 0; i < indexList.Count; i += 2) {
			switch(indexList[i]) {
			case 0:
				animals.Add(catList[indexList[i + 1]]);
				break;
			case 1:
				animals.Add(dogList[indexList[i + 1]]);
				break;
			case 2:
				animals.Add(giraffeList[indexList[i + 1]]);
				break;
			}
		}

		indexList = null;
		catList = null;
		dogList = null;
		giraffeList = null;
	}
}
```

我们为`Zoo`添加了`ISerializationCallbackReceiver`接口，在序列化之前，系统会调用`OnBeforeSerialize`，我们在这里把`List<Animal>`一分为三：`List<Cat>`、`List<Dog>`，以及`List<Giraffe>`。新生成的三个链表用于序列化，避免多态的问题。在反序列化之后，系统调用`OnAfterDeserialize`，我们又把三个链表合为一个供用户使用。我们来看这样的处理能否解决问题

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/6-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/6-2.png) |

#### 2.Dictionary容器序列化

在实践中，`Dictionary`容器也是经常使用的容器类。系统不支持`Dictionary`容器的序列化给我们造成了不便，我们也可以通过自定义序列化来解决。

[例7](https://github.com/jintiao/SerializationTest/blob/master/Assets/Editor/Test7-DictionarySerialization/DictionarySerializationWindow.cs)
```c#
[Serializable]
public class Info {
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
```

和之前的处理相似，我们在序列化之前，将`Dictionary`中的`Key`和`Value`分别保存到两个`List`中，然后在反序列化之后重新生成`Dictionary`数据，运行结果如下

| 序列化前 | 序列化后 |
| ------------- | ------------- |
| ![序列化前](https://github.com/jintiao/SerializationTest/blob/master/Image/7-1.png) | ![序列化后](https://github.com/jintiao/SerializationTest/blob/master/Image/7-2.png) |

### 六：参考

[1] [Unity Manual - Script Serialization](https://docs.unity3d.com/Manual/script-Serialization.html)

[2] [Unity Manual - Custom Serialization](https://docs.unity3d.com/Manual/script-Serialization-Custom.html)

[3] [Serialization in-depth with Tim Cooper](https://www.youtube.com/watch?v=MmUT0ljrHNc)
