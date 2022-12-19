using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.ExampleClasses
{
    public class MyDictionary
    {
        public Dictionary<String, Dictionary<String, String>> alphabet { get; }

        public Dictionary<int, string> numbersDic { get; }

        public string[] numbersArray { get; }

        public List<string> numbersList { get; }

        public MyDictionary()
        {
            alphabet = new Dictionary<String, Dictionary<String, String>>();
            alphabet.Add("a", new() { { "apple", "fruit" } });
            alphabet.Add("b", new() { { "banana", "fruit" } });
            alphabet.Add("c", new() { { "cucumber", "vegetable" } });

            numbersDic = new Dictionary<int, string>();
            numbersDic.Add(1, "one");
            numbersDic.Add(2, "two");
            numbersDic.Add(3, "three");

            numbersArray = new[] { "one", "two", "three" };

            numbersList = new List<string>();
            numbersList.Add("one");
            numbersList.Add("two");
            numbersList.Add("three");
        }

    }
}
