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
namespace Bam
{
namespace V2
{
    public static class DebugProject
    {
        private static readonly string MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static System.Xml.XmlDocument Document = new System.Xml.XmlDocument();
        private static System.Uri RootUri;

        private static System.Xml.XmlElement
        CreateElement(
            string name,
            System.Xml.XmlDocument parent = null)
        {
            var element = Document.CreateElement(name, MSBuildNamespace);
            if (null != parent)
            {
                parent.AppendChild(element);
            }
            return element;
        }

        private static System.Xml.XmlElement
        CreateElement(
            string name,
            string condition = null,
            string value = null,
            System.Xml.XmlElement parent = null)
        {
            var element = Document.CreateElement(name, MSBuildNamespace);
            if (null != parent)
            {
                parent.AppendChild(element);
            }
            if (null != condition)
            {
                CreateAttribute("Condition", condition, element);
            }
            if (null != value)
            {
                element.InnerText = value;
            }
            return element;
        }

        private static void
        CreateAttribute(
            string name,
            string value,
            System.Xml.XmlElement parent)
        {
            parent.SetAttribute(name, value);
        }

        private static System.Xml.XmlElement
        CreatePropertyGroup(
            string condition = null,
            System.Xml.XmlElement parent = null)
        {
            return CreateElement("PropertyGroup", condition: condition, parent: parent);
        }

        private static System.Xml.XmlElement
        CreateItemGroup(
            string condition = null,
            System.Xml.XmlElement parent = null)
        {
            return CreateElement("ItemGroup", condition: condition, parent: parent);
        }

        private static void
        CreateImport(
            string project,
            bool conditional,
            System.Xml.XmlElement parent)
        {
            var import = CreateElement("Import", parent:parent);
            CreateAttribute("Project", project, import);
            if (conditional)
            {
                CreateAttribute("Condition", System.String.Format("Exists('{0}')", project), import);
            }
        }

        private static void
        CreateReference(
            string include,
            System.Xml.XmlElement parent,
            string hintpath = null,
            string targetframework = null)
        {
            var reference = CreateElement("Reference", parent: parent);
            CreateAttribute("Include", include, reference);
            if (null != hintpath)
            {
                CreateElement("HintPath", value: Core.RelativePathUtilities.GetPath(hintpath, RootUri), parent: reference);
                //CreateElement("Private", value: "False", parent: reference); // copylocal
            }
            if (null != targetframework)
            {
                CreateElement("RequiredTargetFramework", value: targetframework, parent: reference);
            }
        }

        private static void
        CreateCompilableSourceFile(
            string include,
            Core.PackageInformation packageInfo,
            System.Xml.XmlElement parent)
        {
            var source = CreateElement("Compile", parent: parent);
            CreateAttribute("Include", Core.RelativePathUtilities.GetPath(include, RootUri), source);
            if (null != packageInfo)
            {
                var linkPath = include.Replace(packageInfo.Identifier.Path, packageInfo.FullName);
                CreateElement("Link", parent: source, value: linkPath);
            }
        }

        private static void
        CreateOtherSourceFile(
            string include,
            Core.PackageInformation packageInfo,
            System.Xml.XmlElement parent)
        {
            var source = CreateElement("None", parent: parent);
            CreateAttribute("Include", Core.RelativePathUtilities.GetPath(include, RootUri), source);
            var linkPath = include.Replace(packageInfo.Identifier.Path, packageInfo.FullName);
            CreateElement("Link", parent: source, value: linkPath);
        }

        private static void
        CreateEmbeddedResourceFile(
            string include,
            System.Xml.XmlElement parent)
        {
            var source = CreateElement("EmbeddedResource", parent: parent);
            CreateAttribute("Include", Core.RelativePathUtilities.GetPath(include, RootUri), source);
        }

        private static string
        GetPreprocessorDefines()
        {
            var allDefines = new Core.StringArray();
            allDefines.Add("DEBUG");
            allDefines.Add("TRACE");
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

            return allDefines.ToString(';');
        }

        public static void
        WriteEntryPoint(
            string path)
        {
            System.Func<int, string> indent = (level) =>
                {
                    if (0 == level)
                    {
                        return string.Empty;
                    }
                    return new string(' ', level * 4);
                };

            using (System.IO.TextWriter writer = new System.IO.StreamWriter(path))
            {
                writer.WriteLine("{0}namespace Bam", indent(0));
                writer.WriteLine("{0}{{", indent(0));
                writer.WriteLine("{0}class Program", indent(1));
                writer.WriteLine("{0}{{", indent(1));
                writer.WriteLine("{0}static void Main(string[] args)", indent(2));
                writer.WriteLine("{0}{{", indent(2));
                writer.WriteLine("{0}// configure", indent(3));
                writer.WriteLine("{0}Core.State.BuildRoot = \"debug_build\";", indent(3));
                writer.WriteLine("{0}Core.State.VerbosityLevel = Core.EVerboseLevel.Full;", indent(3));
                writer.WriteLine("{0}Core.State.CompileWithDebugSymbols = true;", indent(3));
                writer.WriteLine("{0}Core.State.BuilderName = \"Native\";", indent(3));
                writer.WriteLine("{0}var debug = new Core.V2.Environment();", indent(3));
                writer.WriteLine("{0}debug.Configuration = Core.EConfiguration.Debug;", indent(3));
                writer.WriteLine("{0}var optimized = new Core.V2.Environment();", indent(3));
                writer.WriteLine("{0}optimized.Configuration = Core.EConfiguration.Optimized;", indent(3));
                writer.WriteLine("{0}var activeConfigs = new Core.Array<Core.V2.Environment>(debug, optimized);", indent(3));
                writer.WriteLine("{0}// execute", indent(3));
                writer.WriteLine("{0}try", indent(3));
                writer.WriteLine("{0}{{", indent(3));
                writer.WriteLine("{0}Core.V2.EntryPoint.Execute(activeConfigs, packageAssembly: System.Reflection.Assembly.GetEntryAssembly());", indent(4));
                writer.WriteLine("{0}}}", indent(3));
                writer.WriteLine("{0}catch (Bam.Core.Exception exception)", indent(3));
                writer.WriteLine("{0}{{", indent(3));
                writer.WriteLine("{0}Core.Log.ErrorMessage(exception.Message);", indent(4));
                writer.WriteLine("{0}System.Environment.ExitCode = -1;", indent(4));
                writer.WriteLine("{0}}}", indent(3));
                writer.WriteLine(@"{0}Core.Log.Info((0 == System.Environment.ExitCode) ? ""\nBuild Succeeded"" : ""\nBuild Failed"");", indent(3));
                writer.WriteLine(@"{0}Core.Log.DebugMessage(""Exit code {{0}}"", System.Environment.ExitCode);", indent(3));
                writer.WriteLine("{0}}}", indent(2));
                writer.WriteLine("{0}}}", indent(1));
                writer.WriteLine("{0}}}", indent(0));
            }
        }

        public static void
        Create()
        {
            Core.PackageUtilities.IdentifyMainAndDependentPackages(true, false);

            var mainPackage = Core.State.PackageInfo.MainPackage;
            var projectFilename = mainPackage.DebugProjectFilename;
            RootUri = new System.Uri(projectFilename);

            if (!System.IO.Directory.Exists(mainPackage.ProjectDirectory))
            {
                System.IO.Directory.CreateDirectory(mainPackage.ProjectDirectory);
            }

            var mainSourceFile = System.IO.Path.Combine(mainPackage.ProjectDirectory, "main.cs");
            WriteEntryPoint(mainSourceFile);

            Document.AppendChild(Document.CreateComment("Automatically generated by BuildAMation"));
            var project = CreateElement("Project", Document);
            CreateAttribute("ToolsVersion", "12.0", project);
            CreateAttribute("DefaultsTarget", "Build", project);

            CreateImport(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props", true, project);

            var generalProperties = CreatePropertyGroup(parent:project);
            CreateElement("Configuration", condition: @" '$(Configuration)' == '' ", parent: generalProperties, value:"Debug");
            CreateElement("Platform", condition: @" '$(Platform)' == '' ", parent: generalProperties, value: "AnyCPU");
            CreateElement("ProjectGuid", parent: generalProperties, value: System.Guid.NewGuid().ToString("B").ToUpper());
            CreateElement("OutputType", parent: generalProperties, value:"Exe");
            CreateElement("RootNamespace", parent: generalProperties, value:mainPackage.Name);
            CreateElement("AssemblyName", parent: generalProperties, value: mainPackage.Name);
            CreateElement("TargetFrameworkVersion", parent: generalProperties, value: "v4.5");
            CreateElement("WarningLevel", parent: generalProperties, value: "4");
            CreateElement("TreatWarningsAsErrors", parent: generalProperties, value: "true");

            var debugProperties = CreatePropertyGroup(condition: @" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ", parent: project);
            CreateElement("PlatformTarget", parent: debugProperties, value: "AnyCPU");
            CreateElement("DebugSymbols", parent: debugProperties, value: "true");
            CreateElement("DebugType", parent: debugProperties, value: "full");
            CreateElement("Optimize", parent: debugProperties, value: "false");
            CreateElement("OutputPath", parent: debugProperties, value: @"bin\Debug\");
            CreateElement("CheckForOverflowUnderflow", parent: debugProperties, value: "true");
            CreateElement("AllowUnsafeBlocks", parent: debugProperties, value: "false");
            CreateElement("DefineConstants", parent: debugProperties, value: GetPreprocessorDefines());

            var references = CreateItemGroup(parent: project);
            foreach (var desc in mainPackage.Identifier.Definition.DotNetAssemblies)
            {
                CreateReference(desc.Name, references, targetframework:desc.RequiredTargetFramework);
            }
            if (Core.State.RunningMono)
            {
                CreateReference("Mono.Posix", references);
            }
            foreach (var assembly in mainPackage.Identifier.Definition.BamAssemblies)
            {
                var assemblyPath = System.IO.Path.Combine(Core.State.ExecutableDirectory, assembly) + ".dll";
                CreateReference(assembly, references, hintpath: assemblyPath);
            }

            var mainSource = CreateItemGroup(parent: project);
            CreateCompilableSourceFile(mainSourceFile, null, mainSource);

            foreach (var package in Core.State.PackageInfo)
            {
                var packageSource = CreateItemGroup(parent: project);

                CreateCompilableSourceFile(package.Identifier.ScriptPathName, package, packageSource);
                CreateOtherSourceFile(package.Identifier.DefinitionPathName, package, packageSource);

                if (null != package.Scripts)
                {
                    foreach (var script in package.Scripts)
                    {
                        CreateCompilableSourceFile(script, package, packageSource);
                    }
                }

                if (null != package.BuilderScripts)
                {
                    foreach (var builderScript in package.BuilderScripts)
                    {
                        CreateCompilableSourceFile(builderScript, package, packageSource);
                    }
                }
            }

            var resourceFilePathName = Core.PackageListResourceFile.WriteResXFile();
            var resources = CreateItemGroup(parent: project);
            CreateEmbeddedResourceFile(resourceFilePathName, resources);

            CreateImport(@"$(MSBuildBinPath)\Microsoft.CSharp.Targets", false, project);

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            using (var writer = System.Xml.XmlWriter.Create(mainPackage.DebugProjectFilename, xmlWriterSettings))
            {
                Document.WriteTo(writer);
            }

            Core.Log.Info("Successfully created debug project for package '{0}'",
                          mainPackage.Identifier.ToString("-"));
            Core.Log.Info("\t{0}",
                          mainPackage.DebugProjectFilename);
        }
    }
}
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
                const string TargetFrameworkVersion = "v4.5";

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