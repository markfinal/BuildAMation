#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
    public static class LinkerUtilities
    {
        // TODO: deprecate this - it is still used in the MakeFile builder
        public static void
        AppendLibrariesToCommandLine(
            Bam.Core.StringArray commandLineBuilder,
            ILinkerTool linkerTool,
            ILinkerOptions linkerOptions,
            Bam.Core.StringArray otherLibraryPaths)
        {
            var includeStandardLibraries = linkerOptions.DoNotAutoIncludeStandardLibraries && linkerOptions.StandardLibraries.Count > 0;
            var includeOtherLibraries = (null != otherLibraryPaths) && (otherLibraryPaths.Count > 0);
            if (includeStandardLibraries ||
                (linkerOptions.Libraries.Count > 0) ||
                includeOtherLibraries)
            {
                commandLineBuilder.Add(linkerTool.StartLibraryList);

                if (includeOtherLibraries)
                {
                    commandLineBuilder.AddRange(otherLibraryPaths);
                }

                // TODO: replace with 'var'
                foreach (Bam.Core.Location library in linkerOptions.Libraries)
                {
                    var libraryPath = library.AbsolutePath;
                    if (libraryPath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("\"{0}\"", libraryPath));
                    }
                    else
                    {
                        commandLineBuilder.Add(libraryPath);
                    }
                }

                if (includeStandardLibraries)
                {
                    // TODO: replace with 'var'
                    foreach (string standardLibraryPath in linkerOptions.StandardLibraries)
                    {
                        if (standardLibraryPath.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("\"{0}\"", standardLibraryPath));
                        }
                        else
                        {
                            commandLineBuilder.Add(standardLibraryPath);
                        }
                    }
                }

                commandLineBuilder.Add(linkerTool.EndLibraryList);
            }
        }

        public static void
        AppendLibrariesToCommandLine(
            Bam.Core.StringArray commandLineBuilder,
            ILinkerTool linkerTool,
            ILinkerOptions linkerOptions,
            Bam.Core.LocationArray otherLibraryPaths)
        {
            var includeStandardLibraries = linkerOptions.DoNotAutoIncludeStandardLibraries && linkerOptions.StandardLibraries.Count > 0;
            var includeOtherLibraries = (null != otherLibraryPaths) && (otherLibraryPaths.Count > 0);
            if (includeStandardLibraries ||
                (linkerOptions.Libraries.Count > 0) ||
                includeOtherLibraries)
            {
                commandLineBuilder.Add(linkerTool.StartLibraryList);

                if (includeOtherLibraries)
                {
                    foreach (var library in otherLibraryPaths)
                    {
                        commandLineBuilder.Add(library.GetSinglePath());
                    }
                }

                // TODO: replace with 'var'
                foreach (Bam.Core.Location library in linkerOptions.Libraries)
                {
                    var libraryPath = library.AbsolutePath;
                    if (libraryPath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("\"{0}\"", libraryPath));
                    }
                    else
                    {
                        commandLineBuilder.Add(libraryPath);
                    }
                }

                if (includeStandardLibraries)
                {
                    // TODO: replace with 'var'
                    foreach (string standardLibraryPath in linkerOptions.StandardLibraries)
                    {
                        if (standardLibraryPath.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("\"{0}\"", standardLibraryPath));
                        }
                        else
                        {
                            commandLineBuilder.Add(standardLibraryPath);
                        }
                    }
                }

                commandLineBuilder.Add(linkerTool.EndLibraryList);
            }
        }
    }
}
