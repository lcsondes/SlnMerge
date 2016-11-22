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

using System.Collections;
using System.Collections.Generic;

namespace SlnMerge
{
    /// <summary>
    /// Keeps track of inter-solution dependencies.
    /// </summary>
    public class Dependencies : IEnumerable<KeyValuePair<string, List<string>>>
    {
        private readonly Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        /// <summary>
        /// Add a dependency.
        /// </summary>
        public void Add(string solution, string dependsOn)
            => dependencies.AddToList(solution, dependsOn);

        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
            => dependencies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
