using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public struct Range<T> where T : IComparable<T>
    {
        public Range(T singleValue)
        {
            MinValue = singleValue;
            MaxValue = singleValue;
        }

        public Range(T minValue, T maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public T MinValue { get; }
        public T MaxValue { get; }

        /// <summary>
        /// Is true when both Min and Max values are the same
        /// </summary>
        public bool IsSingleValue => MinValue.Equals(MaxValue);
    }
}