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
namespace Bam
{
    public enum VisualStudioVersion
    {
        VS2008,
        VS2010
    }

    public static class CSharpProject
    {
        public static void
        Create(
            Core.PackageInformation package,
            VisualStudioVersion version,
            string[] resourceFilePathNames)
        {
            var projectFilename = package.DebugProjectFilename;
            var projectFilenameUri = new System.Uri(projectFilename);
            var packageName = package.Name;
            var scriptFilename = package.Identifier.ScriptPathName;
            var packageDependencyFilename = package.Identifier.DefinitionPathName;

            var projectDirectory = package.ProjectDirectory;
            if (!System.IO.Directory.Exists(projectDirectory))
            {
                System.IO.Directory.CreateDirectory(projectDirectory);
            }

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            using (var xmlWriter = System.Xml.XmlWriter.Create(projectFilename, xmlWriterSettings))
            {
                xmlWriter.WriteComment("Automatically generated by BuildAMation v" + Core.State.VersionString);

                const string ToolsVersion = "12.0";
                const string TargetFrameworkVersion = "v4.0";

                xmlWriter.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                xmlWriter.WriteAttributeString("ToolsVersion", ToolsVersion);
                xmlWriter.WriteAttributeString("DefaultTargets", "Build");
                {
                    xmlWriter.WriteStartElement("PropertyGroup");
                    {
                        xmlWriter.WriteStartElement("ProjectGuid");
                        {
                            var projectGUID = System.Guid.NewGuid();
                            xmlWriter.WriteString(projectGUID.ToString("B").ToUpper());
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("Configuration");
                        xmlWriter.WriteAttributeString("Condition", " '$(Configuration)' == '' ");
                        {
                            xmlWriter.WriteString("Debug");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("Platform");
                        xmlWriter.WriteAttributeString("Condition", " '$(Platform)' == '' ");
                        {
                            xmlWriter.WriteString("AnyCPU");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("OutputType");
                        {
                            xmlWriter.WriteString("Library");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("RootNamespace");
                        {
                            xmlWriter.WriteString(packageName);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("AssemblyName");
                        {
                            xmlWriter.WriteString(System.IO.Path.GetFileNameWithoutExtension(scriptFilename));
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("TargetFrameworkVersion");
                        {
                            xmlWriter.WriteString(TargetFrameworkVersion);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("StartArguments");
                        {
                            xmlWriter.WriteString(System.Environment.NewLine);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("WarningLevel");
                        {
                            xmlWriter.WriteValue(4);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("TreatWarningsAsErrors");
                        {
                            xmlWriter.WriteValue(true);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("PropertyGroup");
                    xmlWriter.WriteAttributeString("Condition", " '$(Platform)' == 'AnyCPU' ");
                    {
                        xmlWriter.WriteStartElement("PlatformTarget");
                        {
                            xmlWriter.WriteString("AnyCPU");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("PropertyGroup");
                    xmlWriter.WriteAttributeString("Condition", " '$(Configuration)' == 'Debug' ");
                    {
                        xmlWriter.WriteStartElement("OutputPath");
                        {
                            xmlWriter.WriteString(@"bin\Debug");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("DebugSymbols");
                        {
                            xmlWriter.WriteValue(true);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("DebugType");
                        {
                            xmlWriter.WriteString("Full");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("Optimize");
                        {
                            xmlWriter.WriteValue(false);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("CheckForOverflowUnderflow");
                        {
                            xmlWriter.WriteValue(true);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("DefineConstants");
                        {
                            var allDefines = new Core.StringArray();
                            allDefines.Add(Core.PackageUtilities.VersionDefineForCompiler);
                            allDefines.Add(Core.PackageUtilities.HostPlatformDefineForCompiler);
                            // custom definitions from all the packages in the compilation
                            foreach (var info in Core.State.PackageInfo)
                            {
                                allDefines.AddRange(info.Identifier.Definition.Definitions);
                                allDefines.Add(info.Identifier.CompilationDefinition);
                            }
                            // command line definitions
                            allDefines.AddRange(Core.State.PackageCompilationDefines);
                            allDefines.Sort();
                            allDefines.RemoveAll(Core.State.PackageCompilationUndefines);

                            xmlWriter.WriteValue(allDefines.ToString(';'));
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("AllowUnsafeBlocks");
                        {
                            xmlWriter.WriteValue(false);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("Import");
                    xmlWriter.WriteAttributeString("Project", @"$(MSBuildBinPath)\Microsoft.CSharp.Targets");
                    {
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("ItemGroup");
                    {
                        xmlWriter.WriteStartElement("Compile");
                        xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(scriptFilename, projectFilenameUri));
                        {
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("None");
                        xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(packageDependencyFilename, projectFilenameUri));
                        {
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("ItemGroup");
                    {
                        // script files
                        {
                            var scripts = package.Scripts;
                            if (null != scripts)
                            {
                                foreach (var scriptFile in scripts)
                                {
                                    xmlWriter.WriteStartElement("Compile");
                                    {
                                        xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(scriptFile, projectFilenameUri));
                                        {
                                            xmlWriter.WriteStartElement("Link");
                                            {
                                                var linkPackageFilename = scriptFile.Replace(package.Identifier.Path + System.IO.Path.DirectorySeparatorChar, string.Empty);
                                                xmlWriter.WriteValue(linkPackageFilename);
                                                xmlWriter.WriteEndElement();
                                            }

                                            xmlWriter.WriteEndElement();
                                        }
                                    }
                                }
                            }
                        }

                        // builder scripts
                        {
                            var builderScripts = package.BuilderScripts;
                            if (null != builderScripts)
                            {
                                foreach (var scriptFile in builderScripts)
                                {
                                    xmlWriter.WriteStartElement("Compile");
                                    xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(scriptFile, projectFilenameUri));
                                    {
                                        xmlWriter.WriteStartElement("Link");
                                        {
                                            var linkFilename = scriptFile.Replace(package.Identifier.Path, "");
                                            linkFilename = linkFilename.TrimStart(new char[] { System.IO.Path.DirectorySeparatorChar });
                                            xmlWriter.WriteValue(linkFilename);
                                            xmlWriter.WriteEndElement();
                                        }

                                        xmlWriter.WriteEndElement();
                                    }
                                }
                            }
                        }

                        xmlWriter.WriteEndElement();
                    }

                    // add dependent package source
                    int dependentPackageCount = Core.State.PackageInfo.Count;
                    // start from one as the first entry is the main package
                    for (int packageIndex = 1; packageIndex < dependentPackageCount; ++packageIndex)
                    {
                        var dependentPackage = Core.State.PackageInfo[packageIndex];

                        Core.Log.DebugMessage("{0}: '{1}' @ '{2}'", packageIndex, dependentPackage.Identifier.ToString("-"), dependentPackage.Identifier.Root.GetSingleRawPath());

                        xmlWriter.WriteStartElement("ItemGroup");
                        {
                            // .cs file
                            xmlWriter.WriteStartElement("Compile");
                            xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(dependentPackage.Identifier.ScriptPathName, projectFilenameUri));
                            {
                                xmlWriter.WriteStartElement("Link");
                                {
                                    var linkPackageFilename = System.IO.Path.Combine("DependentPackages", dependentPackage.Identifier.ToString("-"));
                                    linkPackageFilename = System.IO.Path.Combine(linkPackageFilename, System.IO.Path.GetFileName(dependentPackage.Identifier.ScriptPathName));
                                    xmlWriter.WriteValue(linkPackageFilename);
                                    xmlWriter.WriteEndElement();
                                }

                                xmlWriter.WriteEndElement();
                            }

                            // .xml file
                            xmlWriter.WriteStartElement("None");
                            xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(dependentPackage.Identifier.DefinitionPathName, projectFilenameUri));
                            {
                                xmlWriter.WriteStartElement("Link");
                                {
                                    var linkPackageFilename = System.IO.Path.Combine("DependentPackages", dependentPackage.Identifier.ToString("-"));
                                    linkPackageFilename = System.IO.Path.Combine(linkPackageFilename, System.IO.Path.GetFileName(dependentPackage.Identifier.DefinitionPathName));
                                    xmlWriter.WriteValue(linkPackageFilename);
                                    xmlWriter.WriteEndElement();
                                }

                                xmlWriter.WriteEndElement();
                            }

                            // scripts
                            {
                                var scripts = dependentPackage.Scripts;
                                if (null != scripts)
                                {
                                    foreach (var scriptFile in scripts)
                                    {
                                        xmlWriter.WriteStartElement("Compile");
                                        xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(scriptFile, projectFilenameUri));
                                        {
                                            xmlWriter.WriteStartElement("Link");
                                            {
                                                var prefix = System.IO.Path.Combine("DependentPackages", dependentPackage.Identifier.ToString("-"));
                                                var linkFilename = scriptFile.Replace(dependentPackage.Identifier.Path, prefix);
                                                xmlWriter.WriteValue(linkFilename);
                                                xmlWriter.WriteEndElement();
                                            }

                                            xmlWriter.WriteEndElement();
                                        }
                                    }
                                }
                            }

                            // builder scripts
                            {
                                var builderScripts = dependentPackage.BuilderScripts;
                                if (null != builderScripts)
                                {
                                    foreach (var scriptFile in builderScripts)
                                    {
                                        xmlWriter.WriteStartElement("Compile");
                                        xmlWriter.WriteAttributeString("Include", Core.RelativePathUtilities.GetPath(scriptFile, projectFilenameUri));
                                        {
                                            xmlWriter.WriteStartElement("Link");
                                            {
                                                var prefix = System.IO.Path.Combine("DependentPackages", dependentPackage.Identifier.ToString("-"));
                                                var linkFilename = scriptFile.Replace(dependentPackage.Identifier.Path, prefix);
                                                xmlWriter.WriteValue(linkFilename);
                                                xmlWriter.WriteEndElement();
                                            }

                                            xmlWriter.WriteEndElement();
                                        }
                                    }
                                }
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }

                    // referenced assembles
                    xmlWriter.WriteStartElement("ItemGroup");
                    {
                        // required BuildAMation assemblies
                        foreach (var assembly in package.Identifier.Definition.BamAssemblies)
                        {
                            xmlWriter.WriteStartElement("Reference");
                            xmlWriter.WriteAttributeString("Include", assembly);
                            {
                                xmlWriter.WriteStartElement("SpecificVersion");
                                {
                                    xmlWriter.WriteValue(false);
                                    xmlWriter.WriteEndElement();
                                }

                                xmlWriter.WriteStartElement("HintPath");
                                {
                                    var assemblyFileName = assembly + ".dll";
                                    var assemblyPathName = System.IO.Path.Combine(Core.State.ExecutableDirectory, assemblyFileName);
                                    var assemblyLocationUri = new System.Uri(assemblyPathName);
                                    var relativeAssemblyLocationUri = projectFilenameUri.MakeRelativeUri(assemblyLocationUri);

                                    Core.Log.DebugMessage("Relative path is '{0}'", relativeAssemblyLocationUri.ToString());
                                    xmlWriter.WriteString(relativeAssemblyLocationUri.ToString());
                                    xmlWriter.WriteEndElement();
                                }

                                xmlWriter.WriteEndElement();
                            }
                        }

                        // required DotNet assemblies
                        foreach (var desc in package.Identifier.Definition.DotNetAssemblies)
                        {
                            xmlWriter.WriteStartElement("Reference");
                            xmlWriter.WriteAttributeString("Include", desc.Name);
                            if (null != desc.RequiredTargetFramework)
                            {
                                xmlWriter.WriteStartElement("RequiredTargetFramework");
                                {
                                    xmlWriter.WriteString(desc.RequiredTargetFramework);
                                    xmlWriter.WriteEndElement();
                                }

                                xmlWriter.WriteEndElement();
                            }
                        }

                        if (System.Type.GetType("Mono.Runtime") != null)
                        {
                            xmlWriter.WriteStartElement("Reference");
                            xmlWriter.WriteAttributeString("Include", "Mono.Posix");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement();
                    }

                    // embedded resources
                    xmlWriter.WriteStartElement("ItemGroup");
                    {
                        foreach (var resourceFilePathName in resourceFilePathNames)
                        {
                            xmlWriter.WriteStartElement("EmbeddedResource");
                            {
                                xmlWriter.WriteAttributeString("Include", resourceFilePathName);
                                xmlWriter.WriteStartElement("Generator");
                                {
                                    xmlWriter.WriteString("ResXFileCodeGenerator");
                                    xmlWriter.WriteEndElement();
                                }
                                xmlWriter.WriteEndElement();
                            }
                            xmlWriter.WriteEndElement();
                        }
                    }

                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteWhitespace(xmlWriterSettings.NewLineChars);
                xmlWriter.Close();
            }
        }
    }
}