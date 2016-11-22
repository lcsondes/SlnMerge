## SlnMerge

A tool for merging multiple .sln files into one master file, with dependencies added between the contained projects.

## Building, installation, and usage

You'll need to edit the `ProjectSpecificSetup` method in Program.cs and describe your dependencies, the version of VS you want to use, and your output filename. After this is done, just build SlnMerge, put the executable into your top-level directory, and either run it as needed, or use the `--setup-git-hooks` argument to have it install itself as a Git hook into each folder visited.

## Configuration

All configuration is done from within `ProjectSpecificSetup`. This way the executable you get contains everything you need and you can't lose parts of the output.

## License

SlnMerge is licensed under the terms of the GPLv3, or, at your choice, any later version as published by the Free Software Foundation. As part of the build, ost-onion/SolutionParser on GitHub gets merged into the resulting executable. For their license terms, visit [their GitHub repository](https://github.com/ost-onion/SolutionParser).
