using Data;
using System.Collections.Generic;

namespace Logic
{
    public class LogicClass
    {
        public string SayHi(string name)
        {
            return $"Hi {name}";
        }

        public List<DataClass> CreateBalls()
        {
            return new List<DataClass>
    {
        new DataClass { X = 10, Y = 20 },
        new DataClass { X = 30, Y = 40 }
    };
        }
    }
}
