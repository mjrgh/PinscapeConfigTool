using System;
using System.Collections.Generic;
using System.Linq;

// Collection Utilities.  This adds severak convenience methods to Arrays 
// and other collection types, in most cases to provide more uniform
// functionality and syntax among the types.
namespace CollectionUtils
{
    public static class CollectionUtils
    {
        // array.ForEach(Action)
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach(array, action);
        }

        // Dictionary.ForEach(Action<TKey, TValue>) - invoke callback with each key and value
        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey, TValue> action)
        {
            foreach (KeyValuePair<TKey, TValue> kv in dict)
                action(kv.Key, kv.Value);
        }

        // Dictionary.ForEach(Action<TKey>) - invoke callback with each key
        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey> action)
        {
            foreach (KeyValuePair<TKey, TValue> kv in dict)
                action(kv.Key);
        }

        // IEnumerable.ForEach(Action)
        public static void ForEach<T>(this IEnumerable<T> lst, Action<T> action)
        {
            foreach (T val in lst)
                action(val);
        }

        // array.Select(Func)
        public static TResult[] Select<TSource, TResult>(this TSource[] array, Func<TSource, TResult> func)
        {
            TResult[] result = new TResult[array.Length];
            for (int i = 0; i < array.Length; ++i)
                result[i] = func(array[i]);
            return result;
        }

        // Add default value padding to the end of an array to get it to
        // the desired number of element.
        public static T[] PadTo<T>(this T[] array, int totalElements)
        {
            if (array.Length < totalElements)
                return array.Concat(new T[totalElements - array.Length]);
            else
                return array;
        }

        // array.Slice(startIdx)
        // Return a new array consisting of all elements of the source
        // array from startIdx to the last element.  The original array
        // is unchanged.
        public static T[] Slice<T>(this T[] array, int startIdx)
        {
            return array.Slice(startIdx, array.Length - startIdx);
        }

        // array.Slice(startIdx, len)
        // Returns a new array consisting of 'len' elements from the source
        // array, starting at 'startIdx'.  The original array is unchanged.
        public static T[] Slice<T>(this T[] array, int startIdx, int len)
        {
            if (startIdx < 0) startIdx = 0;
            if (len < 0) len = 0;
            if (len > array.Length - startIdx) len = array.Length - startIdx;
            T[] slice = new T[len];
            Array.Copy(array, startIdx, slice, 0, len);
            return slice;
        }

        // array1.Concat(array2)
        // Concateneate elements of array2 to end of array1, returning
        // a new array.  The original arrays are unchanged.
        public static T[] Concat<T>(this T[] array1, T[] array2)
        {
            T[] result = new T[array1.Length + array2.Length];
            Array.Copy(array1, result, array1.Length);
            Array.Copy(array2, 0, result, array1.Length, array2.Length);
            return result;
        }

        // src.IndexOf(pat)
        // Searches for the contents of 'pat' within 'src'.  Returns the
        // index of the first match, or -1 if the pattern isn't found.
        public static int IndexOf<T>(this T[] src, T[] pat)
        {
            for (int i = 0; i < src.Length - pat.Length; ++i)
            {
                bool match = true;
                for (int j = 0; j < pat.Length; ++j)
                {
                    if (!src[i + j].Equals(pat[j]))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            return -1;
        }

        // dictionary.ValueOrDefault(key)
        // Returns the value associated with the key in the given dictionary, or
        // the default value for the type is the key doesn't exist.
        public static ValueType ValueOrDefault<KeyType, ValueType>(
            this Dictionary<KeyType, ValueType> dictionary, KeyType key)
        {
            return dictionary.ValueOrDefault(key, default(ValueType));
        }

        // dictionary.ValueOrDefault(key, defaultValue)
        // Returns the value associated with the given key in the dictionary, or
        // the default value if the key doesn't exist.
        public static ValueType ValueOrDefault<KeyType, ValueType>(
            this Dictionary<KeyType, ValueType> dictionary, KeyType key, ValueType defaultValue)
        {
            ValueType value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        // Join a list in serial comma format
        public static String SerialJoin<T>(this IEnumerable<T> e)
        {
            int c = e.Count();
            if (c == 0)
                return "";
            else if (c == 1)
                return e.ElementAt(0).ToString();
            else if (c == 2)
                return e.ElementAt(0).ToString() + " and " + e.ElementAt(1).ToString();
            else
                return String.Join(", ", e.ToArray().Slice(0, c - 1).ToList()) + ", and " + e.ElementAt(c - 1);
        }
    }
}
