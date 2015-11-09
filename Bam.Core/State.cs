#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    public static class State
    {
        public class Category :
            System.Collections.Generic.Dictionary<string, object>
        {}

        private static System.Collections.Generic.Dictionary<string, Category> s = new System.Collections.Generic.Dictionary<string, Category>();

        static
        State()
        {
            ReadOnly = false;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void
        Print()
        {
            foreach (var category in s)
            {
                Log.DebugMessage("Category '{0}'", category.Key);
                foreach (var item in category.Value)
                {
                    Log.DebugMessage("\t'{0}' = '{1}'", item.Key, (null != item.Value) ? item.Value.ToString() : "null");
                }
            }
        }

        public static bool ReadOnly
        {
            get;
            set;
        }

        public static void
        AddCategory(
            string category)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s.Add(category, new Category());
        }

        public static bool
        HasCategory(
            string category)
        {
            var hasCategory = s.ContainsKey(category);
            return hasCategory;
        }

        // TODO: is this the same as set?
        // Not quite. This will throw an exception if the entry already exists
        public static void
        Add<Type>(
            string category,
            string key,
            Type value)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s[category].Add(key, value);
        }

        public static bool
        Has(
            string category,
            string key)
        {
            if (!HasCategory(category))
            {
                return false;
            }

            if (!s[category].ContainsKey(key))
            {
                return false;
            }

            return true;
        }

        // TODO: how can I cast this correctly?
        public static object
        Get(
            string category,
            string key)
        {
            object value = null;
            try
            {
                value = s[category][key];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new Exception("Category '{0}' with key '{1}' not found", category, key);
            }
            return value;
        }

        public static T
        Get<T>(
            string category,
            string key,
            T defaultValue) where T:struct
        {
            object value = null;
            try
            {
                value = s[category][key];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return defaultValue;
            }
            return (T)value;
        }

        public static void
        Set(
            string category,
            string key,
            object value)
        {
            if (ReadOnly)
            {
                throw new Exception("State is marked readonly");
            }
            s[category][key] = value;
        }
    }
}
