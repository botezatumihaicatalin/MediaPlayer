using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class Pair<T,K>
    {
        public T First
        {
            get;
            set;
        }
        public K Second
        {
            get;
            set;
        }
        public Pair(T a, K b)
        {
            First = a;
            Second = b;
        }
    }
}
