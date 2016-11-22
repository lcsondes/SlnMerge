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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Onion.SolutionParser.Parser.Model;

namespace SlnMerge
{
    public static class SlnWriter
    {
        public static void WriteMergedSolution(this StreamWriter wrt, string header, IDictionary<string,ISolution> solutions)
        {
            // Get all projects and sections
            var projs = solutions.Values.SelectMany(sln => sln.Projects).ToArray();
            var globals = solutions.Values.SelectMany(sln => sln.Global).ToArray();

            wrt.WriteLine(header);

            foreach (var proj in projs)
            {
                wrt.WriteLine($"Project(\"{proj.TypeGuid.ToUpperString()}\") = \"{proj.Name}\", \"{proj.Path}\", \"{proj.Guid.ToUpperString()}\"");
                wrt.WriteSection(proj.ProjectSection, "ProjectSection");
                wrt.WriteLine("EndProject");
            }

            // Merge global sections by name
            var mergedSections = new Dictionary<string, GlobalSection>();

            // Checks for GUID-looking things, not strict
            var regex = new Regex(@"\{[0-9a-f-]+\}", RegexOptions.Compiled);

            foreach (var sourceSection in globals)
            {
                if (!mergedSections.ContainsKey(sourceSection.Name))
                    mergedSections.Add(sourceSection.Name, new GlobalSection(sourceSection.Name, sourceSection.Type));

                var mergedSection = mergedSections[sourceSection.Name];
                Contract.Assert(mergedSection.Type == sourceSection.Type);

                foreach (var entry in sourceSection.Entries)
                {
                    // Sanity check: check for duplicates if the key looks like it has a GUID in it,
                    // otherwise let them through
                    if (regex.IsMatch(entry.Key))
                        mergedSection.Entries.Add(entry);
                    else
                        mergedSection.Entries[entry.Key] = entry.Value;
                }
            }

            // Write the merged sections
            wrt.WriteLine("Global");
            foreach (var section in mergedSections.Values)
                wrt.WriteSection(section, "GlobalSection");
            wrt.WriteLine("EndGlobal");
        }

        private static void WriteSection(this StreamWriter wrt, dynamic section, string name) // sections have a base interface but it's empty yay
        {
            if (section == null)
                return;

            wrt.WriteLine($"\t{name}({section.Name}) = {Extensions.ToSlnCase(section.Type)}");
            foreach (var kvp in Extensions.SlnSort(section.Entries))
                wrt.WriteLine($"\t\t{kvp.Key} = {kvp.Value}");
            wrt.WriteLine($"\tEnd{name}");
        }
    }
}
