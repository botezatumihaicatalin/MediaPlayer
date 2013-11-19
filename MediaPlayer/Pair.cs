using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class Pair<T,K>
    {
        public T first
        {
            get;
            set;
        }
        public K second
        {
            get;
            set;
        }
        public Pair(T a, K b)
        {
            first = a;
            second = b;
        }
    }
}
