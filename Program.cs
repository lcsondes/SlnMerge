/* 
 * Copyright © 2016-2017 László Csöndes
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
using System.IO;
using System.Linq;

namespace SlnMerge
{
    public static class Program
    {
        /// <summary>
        /// Edit me to suit your development environment!
        /// </summary>
        private static void ProjectSpecificSetup()
        {
            // Change this for a different version of VS.
            // It doesn't need to match what you're using but it has to be compatible with it.
            solutions.OutputHeader = "\n" +
                                     "Microsoft Visual Studio Solution File, Format Version 11.00\n" +
                                     "# Visual Studio 2010";

            // Add your dependencies here, each line makes every project in the source .sln
            // depend on every project in the target .sln
            // These can be in any order
#if EXAMPLE
            dependencies.Add("MyProduct", dependsOn: "MySupportLibraries");
            dependencies.Add("MyProduct", dependsOn: "MyAbstractionLibraries");
            dependencies.Add("MySupportLibraries, dependsOn: "MyAbstractionLibraries");
#endif

            // Load your solutions here, generate a new, static GUID for each manually and don't change them
            // VS uses these GUIDs to remember the expand/collapse state of the Solution Explorer
#if EXAMPLE
            solutions.Load("MyAbstractionLibraries", @"MyAbstractionLibraries\MyAbstractionLibraries.sln", new Guid("{your guid here 1}"));
            solutions.Load("MySupportLibraries"    , @"MySupportLibraries\MySupportLibraries.sln"        , new Guid("{your guid here 2}"));
            solutions.Load("MyProduct"             , @"MyProduct\MyProduct.sln"                          , new Guid("{your guid here 3}"));
#endif

            // Your output file, rename if you have something cooler in mind
            outfile = "merged.sln";
        }

        //--------------------------------------------------------------------
        // This is the end of customization, the lines below should not be
        // edited for normal usage, only if you want to tweak the code
        //--------------------------------------------------------------------

        private static readonly Dependencies dependencies = new Dependencies();
        private static readonly SlnCollection solutions = new SlnCollection();
        private static string outfile;

        private static void SetupGitHooks()
        {
            var hooks = new[] { "post-checkout", "post-merge" };

            foreach (var slnfolder in solutions.FileList
                                               .Select(file => Path.GetDirectoryName(file))
                                               .Distinct())
            {
                foreach (var hook in hooks)
                {
                    var hookfile = Path.Combine(slnfolder, ".git", "hooks", hook);
                    if (File.Exists(hookfile))
                    {
                        Console.Error.WriteLine($"Not writing git hook for {slnfolder}, {hook} already exists");
                        continue;
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(hookfile));
                    using (var wrt = new StreamWriter(File.Create(hookfile)))
                    {
                        wrt.WriteLine("#!/bin/sh");
                        wrt.WriteLine("cd ..");
                        wrt.WriteLine("./slnmerge.exe");
                        wrt.WriteLine($"echo {outfile} updated");
                    }
                }
            }
            Console.WriteLine("Git hooks installed");
        }

        public static void Main(string[] args)
        {
            ProjectSpecificSetup();

            if (args.Contains("--setup-git-hooks"))
                SetupGitHooks();

            solutions.FinishLoading(); // required to work around ISolution limitations
            solutions.FixPaths();
            solutions.AddDependencies(dependencies);
            solutions.AddSolutionFolders();
            solutions.Save(outfile);
        }
    }
}
