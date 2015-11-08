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
    public static class DebugProject
    {
        private static readonly string MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static System.Xml.XmlDocument Document = new System.Xml.XmlDocument();
        private static System.Uri RootUri = null;

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
            Core.PackageDefinition packageDefinition,
            System.Xml.XmlElement parent)
        {
            var source = CreateElement("Compile", parent: parent);
            CreateAttribute("Include", Core.RelativePathUtilities.GetPath(include, RootUri), source);
            if (null != packageDefinition)
            {
                var linkPath = include.Replace(packageDefinition.GetPackageDirectory(), packageDefinition.FullName);
                linkPath = linkPath.Replace(Core.PackageUtilities.BamSubFolder + System.IO.Path.DirectorySeparatorChar, string.Empty);
                CreateElement("Link", parent: source, value: linkPath);
            }
        }

        private static void
        CreateOtherSourceFile(
            string include,
            Core.PackageDefinition packageDefinition,
            System.Xml.XmlElement parent)
        {
            var source = CreateElement("None", parent: parent);
            CreateAttribute("Include", Core.RelativePathUtilities.GetPath(include, RootUri), source);
            var linkPath = include.Replace(packageDefinition.GetPackageDirectory(), packageDefinition.FullName);
            linkPath = linkPath.Replace(Core.PackageUtilities.BamSubFolder + System.IO.Path.DirectorySeparatorChar, string.Empty);
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
            foreach (var package in Core.Graph.Instance.Packages)
            {
                allDefines.AddRange(package.Definitions);
            }
            allDefines.Sort();

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
                writer.WriteLine("{0}Core.Graph.Instance.VerbosityLevel = Core.EVerboseLevel.Full;", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.CompileWithDebugSymbols = true;", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.BuildRoot = \"debug_build\";", indent(3));
                writer.WriteLine("{0}Core.Graph.Instance.Mode = \"Native\";", indent(3));
                writer.WriteLine("{0}var debug = new Core.Environment();", indent(3));
                writer.WriteLine("{0}debug.Configuration = Core.EConfiguration.Debug;", indent(3));
                writer.WriteLine("{0}var optimized = new Core.Environment();", indent(3));
                writer.WriteLine("{0}optimized.Configuration = Core.EConfiguration.Optimized;", indent(3));
                writer.WriteLine("{0}var activeConfigs = new Core.Array<Core.Environment>(debug, optimized);", indent(3));
                writer.WriteLine("{0}// execute", indent(3));
                writer.WriteLine("{0}try", indent(3));
                writer.WriteLine("{0}{{", indent(3));
                writer.WriteLine("{0}Core.EntryPoint.Execute(activeConfigs, packageAssembly: System.Reflection.Assembly.GetEntryAssembly());", indent(4));
                writer.WriteLine("{0}}}", indent(3));
                writer.WriteLine("{0}catch (Bam.Core.Exception exception)", indent(3));
                writer.WriteLine("{0}{{", indent(3));
                writer.WriteLine("{0}Core.Exception.DisplayException(exception);", indent(4));
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
            Core.PackageUtilities.IdentifyAllPackages();

            var masterPackage = Core.Graph.Instance.MasterPackage;
            var masterPackageName = masterPackage.Name;
            var projectPathname = masterPackage.GetDebugPackageProjectPathname();
            RootUri = new System.Uri(projectPathname);

            var projectDir = System.IO.Path.GetDirectoryName(projectPathname);
            if (!System.IO.Directory.Exists(projectDir))
            {
                System.IO.Directory.CreateDirectory(projectDir);
            }

            var mainSourceFile = System.IO.Path.Combine(projectDir, "main.cs");
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
            CreateElement("RootNamespace", parent: generalProperties, value:masterPackageName);
            CreateElement("AssemblyName", parent: generalProperties, value: masterPackageName);
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
            foreach (var desc in masterPackage.DotNetAssemblies)
            {
                CreateReference(desc.Name, references, targetframework: desc.RequiredTargetFramework);
            }
            if (Core.State.RunningMono)
            {
                CreateReference("Mono.Posix", references);
            }
            foreach (var assembly in masterPackage.BamAssemblies)
            {
                var assemblyPath = System.IO.Path.Combine(Core.State.ExecutableDirectory, assembly) + ".dll";
                CreateReference(assembly, references, hintpath: assemblyPath);
            }

            var mainSource = CreateItemGroup(parent: project);
            CreateCompilableSourceFile(mainSourceFile, null, mainSource);
            foreach (var package in Core.Graph.Instance.Packages)
            {
                var packageSource = CreateItemGroup(parent: project);

                CreateOtherSourceFile(package.XMLFilename, package, packageSource);

                foreach (var script in package.GetScriptFiles(allBuilders: true))
                {
                    CreateCompilableSourceFile(script, package, packageSource);
                }
            }

            var resourceFilePathName = Core.PackageListResourceFile.WriteResXFile(projectPathname);
            var resources = CreateItemGroup(parent: project);
            CreateEmbeddedResourceFile(resourceFilePathName, resources);

            CreateImport(@"$(MSBuildBinPath)\Microsoft.CSharp.Targets", false, project);

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            using (var writer = System.Xml.XmlWriter.Create(projectPathname, xmlWriterSettings))
            {
                Document.WriteTo(writer);
            }

            Core.Log.Info("Successfully created debug project for package '{0}'", masterPackage.FullName);
            Core.Log.Info("\t{0}", projectPathname);
        }
    }
}
