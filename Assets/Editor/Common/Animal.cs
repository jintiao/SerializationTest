using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerializationTest.Common
{

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

} // namespace SerializationTest.Common
