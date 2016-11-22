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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Onion.SolutionParser.Parser;
using Onion.SolutionParser.Parser.Model;

namespace SlnMerge
{
    /// <summary>
    /// Represents the final solution file.
    /// </summary>
    public class SlnCollection
    {
        // The GUID of solution folders
        private static readonly Guid SolutionFolderTypeGuid = new Guid("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
        private static readonly Guid[] ExcludedTypeGuids =
            {
                SolutionFolderTypeGuid,
            };

        private readonly IDictionary<string, ISolution> solutions = new Dictionary<string, ISolution>();
        private readonly IList<string> solutionFiles = new List<string>();
        private readonly IDictionary<string, Guid> solutionFolderGuids = new Dictionary<string, Guid>();

        public IEnumerable<string> FileList
            => solutionFiles;

        /// <summary>
        /// The header used for the output .sln
        /// </summary>
        public string OutputHeader { get; set; }

        public void Load(string key, string filename, Guid folderGuid)
        {
            solutions.Add(key, SolutionParser.Parse(filename));
            solutionFiles.Add(filename);
            solutionFolderGuids.Add(key, folderGuid);
        }

        public void FinishLoading()
        {
            foreach (var key in solutions.Keys.ToArray())
                solutions[key] = solutions[key].Enumerated();
        }

        public void AddDependencies(Dependencies deps)
        {
            // TODO check if FinishLoading was called
            var q = from dependency in deps
                    let depFrom = dependency.Key
                    from depOn in dependency.Value
                    select new { From = solutions[depFrom], On = solutions[depOn] };
            foreach (var dep in q)
                AddDependency(dep.From, dep.On);
        }

        private static void AddDependency(ISolution from, ISolution on)
        {
            // Make every project in 'from' depend on all the projects in 'on'
            foreach (var proj in from.Projects)
            {
                // Dependencies for these don't make sense
                if (ExcludedTypeGuids.Contains(proj.TypeGuid))
                    continue;

                var ps = proj.ProjectSection;
                if (ps == null)
                {
                    ps = new ProjectSection("ProjectDependencies", ProjectSectionType.PostProject);
                    proj.ProjectSection = ps;
                }
                Contract.Assert(ps.Name == "ProjectDependencies");
                Contract.Assert(ps.Type == ProjectSectionType.PostProject);
                foreach (var onProject in on.Projects)
                {
                    if (ExcludedTypeGuids.Contains(onProject.TypeGuid))
                        continue;
                    var onGuid = onProject.Guid.ToUpperString();
                    ps.Entries[onGuid] = onGuid; // This is how solutions do lists...
                }
            }
        }

        public void FixPaths()
        {
            foreach (var kvp in solutions)
                foreach (var proj in kvp.Value.Projects.Where(p => !ExcludedTypeGuids.Contains(p.TypeGuid)))
                    proj.Path = Path.Combine(kvp.Key, proj.Path);
        }

        /// <summary>Add one folder for each source solution</summary>
        public void AddSolutionFolders()
        {
            // this assumes all solutions are enumerated (and they should be at this point)
            foreach (var kvp in solutions)
            {
                // Make a project for the key
                var project = new Project(SolutionFolderTypeGuid, kvp.Key, kvp.Key, solutionFolderGuids[kvp.Key]);

                // Get to nesting
                var section = kvp.Value.Global.SingleOrDefault(gs => gs.Name == "NestedProjects");
                Contract.Assert(section != null); // TODO implement creation if needed

                // For each project that's not nested underneath something else, nest it under the new project
                foreach (var proj in kvp.Value.Projects)
                {
                    // Solutions use upper-case GUIDs...
                    var projGuid = proj.Guid.ToUpperString();
                    if (!section.Entries.ContainsKey(projGuid))
                        section.Entries[projGuid] = project.Guid.ToUpperString();
                }

                // Prepend the new project
                ((Solution)kvp.Value).Projects = new[] { project }.Concat(kvp.Value.Projects).ToArray();
            }
        }

        public void Save(string outfile)
        {
            using (var wrt = new StreamWriter(File.Create(outfile), Encoding.UTF8 /*this places a BOM*/))
            {
                wrt.WriteMergedSolution(OutputHeader, solutions);
            }
        }
    }
}
