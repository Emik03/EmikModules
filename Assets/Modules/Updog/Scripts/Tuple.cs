namespace Updog
{
    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1>
    {
        public Tuple(T1 item1)
        {
            Item1 = item1;
        }

        public T1 Item1 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2>
    {
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3>
    {
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3, T4>
    {
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3, Item4 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3, T4, T5>
    {
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3, Item4, Item5 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3, T4, T5, T6>
    {
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3, Item4, Item5, Item6 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7>
    {
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }
        public T7 Item7 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3, Item4, Item5, Item6, Item7 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }
        public T7 Item7 { get; set; }
        public T8 Item8 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3, Item4, Item5, Item6, Item7, Item8 }; } }
    }

    /// <summary>
    /// A basic tuple datatype for C# 4.
    /// </summary>
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
            Item9 = item9;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
        public T6 Item6 { get; set; }
        public T7 Item7 { get; set; }
        public T8 Item8 { get; set; }
        public T9 Item9 { get; set; }
        /// <summary>
        /// Returns all elements as one array, in order of items.
        /// </summary>
        public object[] ToArray { get { return new object[] { Item1, Item2, Item3, Item4, Item5, Item6, Item7, Item8, Item9 }; } }
    }
}