/* 
 * Copyright © 2016 László Csöndes
 * 
 * This file is part of SlnMerge.
 * 
 * SlnMerge is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SlnMerge is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with SlnMerge. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Onion.SolutionParser.Parser.Model;

namespace SlnMerge
{
    public static class Extensions
    {
        /// <summary>
        /// Add an entry to the dictionary's value List, create one if the key hasn't been encountered.
        /// </summary>
        public static void AddToList<K, V>(this Dictionary<K, List<V>> dict, K key, V value)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new List<V>());
            dict[key].Add(value);
        }

        public static string ToUpperString(this Guid guid)
            => guid.ToString("B").ToUpper();

        public static string ToSlnCase(this GlobalSectionType gst)
            => "p" + gst.ToString().Substring(1);

        public static string ToSlnCase(this ProjectSectionType pst)
            => "p" + pst.ToString().Substring(1);

        /// <summary>
        /// ISolutions that come from the parser only support enumeration once.
        /// Replace each IEnumerable&lt;T&gt; field with an array so we can do it multiple times.
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public static ISolution Enumerated(this ISolution solution)
            => new Solution
                   {
                       Projects = solution.Projects.ToArray(),
                       Global = solution.Global.ToArray()
                   };

        private static void TopologicalSort(KeyValuePair<string, string>[] entries)
        {
            bool swapped;
            do
            {
                swapped = false;
                for (var i = 0; i < entries.Length; ++i)
                    for (var j = i + 1; j < entries.Length; ++j)
                    {
                        // Swap this pair if the first one depends on the second.
                        if (entries[i].Value == entries[j].Key)
                        {
                            var tmp = entries[i];
                            entries[i] = entries[j];
                            entries[j] = tmp;
                            swapped = true;
                        }
                    }
            } while (swapped);
        }

        /// <summary>
        /// This is not strictly needed but it makes the entries come up in the order that VS usually writes them.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> SlnSort(this IDictionary<string, string> dict)
        {
            if (dict.Count == 0)
                return new KeyValuePair<string, string>[0];

            var entries = dict.ToArray();

            // Does the dictionary look like it's guid to guid?
            Guid dummy;
            if (Guid.TryParseExact(entries[0].Key, "B", out dummy) &
                Guid.TryParseExact(entries[0].Value, "B", out dummy))
            {
                // Is it an A=A "list"?
                if (entries[0].Key == entries[0].Value)
                    return entries;

                // We want GUIDs that appear on the key side
                // to come before they appear on the value side.
                TopologicalSort(entries);
            }

            return entries;
        }
    }
}
