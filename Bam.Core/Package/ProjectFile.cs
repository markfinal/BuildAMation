#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace Bam.Core
{
    /// <summary>
    /// Generate a .csproj file (new style) for a script assembly.
    /// </summary>
    public sealed class ProjectFile
    {
        private System.Xml.XmlDocument Document = new System.Xml.XmlDocument();
        private readonly string CsProjPath;
        private readonly System.Xml.XmlElement Root;

        private System.Xml.XmlElement
        CreateElement(
            string name,
            System.Xml.XmlDocument parent = null)
        {
            var element = this.Document.CreateElement(name);
            if (null != parent)
            {
                parent.AppendChild(element);
            }
            return element;
        }

        private System.Xml.XmlElement
        CreateElement(
            string name,
            string condition = null,
            string value = null,
            System.Xml.XmlElement parent = null)
        {
            var element = this.Document.CreateElement(name);
            if (null != parent)
            {
                parent.AppendChild(element);
            }
            if (null != condition)
            {
                this.CreateAttribute("Condition", condition, element);
            }
            if (null != value)
            {
                element.InnerText = value;
            }
            return element;
        }

        private void
        CreateAttribute(
            string name,
            string value,
            System.Xml.XmlElement parent) => parent.SetAttribute(name, value);

        private System.Xml.XmlElement
        CreatePropertyGroup(
            string condition = null,
            System.Xml.XmlElement parent = null) => this.CreateElement("PropertyGroup", condition: condition, parent: parent);

        private System.Xml.XmlElement
        CreateItemGroup(
            string condition = null,
            System.Xml.XmlElement parent = null) => this.CreateElement("ItemGroup", condition: condition, parent: parent);

        private void
        CreateCompilableSourceFile(
            string include,
            PackageDefinition packageDefinition,
            System.Xml.XmlElement parent)
        {
            var source = this.CreateElement("Compile", parent: parent);
            this.CreateAttribute("Include", include, source);
            if (null != packageDefinition)
            {
                var linkPath = include.Replace(packageDefinition.GetPackageDirectory(), packageDefinition.FullName);
                linkPath = linkPath.Replace(PackageUtilities.BamSubFolder + System.IO.Path.DirectorySeparatorChar, string.Empty);
                this.CreateElement("Link", parent: source, value: linkPath);
            }
        }

        private void
        CreateOtherSourceFile(
            string include,
            PackageDefinition packageDefinition,
            System.Xml.XmlElement parent)
        {
            var source = CreateElement("None", parent: parent);
            this.CreateAttribute("Include", include, source);
            var linkPath = include.Replace(packageDefinition.GetPackageDirectory(), packageDefinition.FullName);
            linkPath = linkPath.Replace(PackageUtilities.BamSubFolder + System.IO.Path.DirectorySeparatorChar, string.Empty);
            this.CreateElement("Link", parent: source, value: linkPath);
        }

        private void
        CreateReference(
            string include,
            System.Xml.XmlElement parent,
            bool copyLocal,
            string hintpath = null,
            string targetframework = null)
        {
            var reference = this.CreateElement("Reference", parent: parent);
            this.CreateAttribute("Include", include, reference);
            this.CreateElement("Private", value: copyLocal ? "True" : "False", parent: reference);
            if (null != hintpath)
            {
                this.CreateElement("HintPath", value: hintpath, parent: reference);
            }
            if (null != targetframework)
            {
                this.CreateElement("RequiredTargetFramework", value: targetframework, parent: reference);
            }
        }

        private void
        CreateNugetReference(
            string package,
            string version,
            System.Xml.XmlElement parent)
        {
            var packageReference = this.CreateElement("PackageReference", parent: parent);
            this.CreateAttribute("Include", package, packageReference);
            this.CreateAttribute("Version", version, packageReference);
        }

        private void
        CreateEmbeddedResourceFile(
            string include,
            System.Xml.XmlElement parent)
        {
            var source = this.CreateElement("EmbeddedResource", parent: parent);
            this.CreateAttribute("Include", include, source);
        }

        private static string
        GetPreprocessorDefines()
        {
            var allDefines = new Core.StringArray();
            allDefines.Add("DEBUG");
            allDefines.Add("TRACE");
            allDefines.Add(Core.PackageUtilities.VersionDefineForCompiler);
            allDefines.Add(Core.PackageUtilities.HostPlatformDefineForCompiler);
            allDefines.AddRange(Core.Features.PreprocessorDefines);
            var nugetSet = new System.Collections.Generic.SortedSet<string>();
            // custom definitions from all the packages in the compilation
            foreach (var package in Core.Graph.Instance.Packages)
            {
                allDefines.AddRange(package.Definitions);
                foreach (var nuget in package.NuGetPackages)
                {
                    if (!nuget.Platforms.Includes(Bam.Core.OSUtilities.CurrentPlatform))
                    {
                        continue;
                    }
                    nugetSet.Add(nuget.Identifier);
                }
            }
            // custom definitions for all the NuGet packages in the compilation
            foreach (var nugetIdentifier in nugetSet)
            {
                allDefines.Add(System.String.Format("D_NUGET_{0}", nugetIdentifier.ToUpper().Replace('.', '_').Replace('-', '_')));
            }

            allDefines.Sort();

            return allDefines.ToString(';');
        }

        /// <summary>
        /// Generate an instance of a project file.
        /// </summary>
        /// <param name="isExecutable">true if the output is an executable, rather than a dynamic library assembly.</param>
        /// <param name="csprojPath">Path of the .csproj to write.</param>
        /// <param name="additionalNuGetReferences">Optional: list of name-version tuples for additional NuGet references.</param>
        public ProjectFile(
            bool isExecutable,
            string csprojPath,
            System.Collections.Generic.List<(string package,string version)> additionalNuGetReferences = null)
        {
            this.CsProjPath = csprojPath;

            this.Document.AppendChild(Document.CreateComment("Automatically generated by BuildAMation"));
            this.Root = this.CreateElement("Project", Document);
            this.CreateAttribute("Sdk", "Microsoft.NET.Sdk", this.Root);

            var generalProperties = this.CreatePropertyGroup(parent: this.Root);
            this.CreateElement("TargetFramework", parent: generalProperties, value: "netcoreapp2.1"); // cannot use netstandard2.0 here
            this.CreateElement("EnableDefaultItems", parent: generalProperties, value: "false");
            if (isExecutable)
            {
                this.CreateElement("OutputType", parent: generalProperties, value: "Exe");
            }
            this.CreateElement("LangVersion", parent: generalProperties, value: "latest");

            var debugProperties = this.CreatePropertyGroup(condition: @"'$(Configuration)|$(Platform)'=='Debug|AnyCPU'", parent: this.Root);
            this.CreateElement("DefineConstants", parent: debugProperties, value: GetPreprocessorDefines());
            this.CreateElement("NoWarn", parent: debugProperties);
            this.CreateElement("TreatWarningsAsErrors", parent: debugProperties, value: "true");
            this.CreateElement("WarningsAsErrors", parent: debugProperties);
            this.CreateElement("PlatformTarget", parent: debugProperties, value: "AnyCPU");

            var releaseProperties = this.CreatePropertyGroup(condition: @"'$(Configuration)|$(Platform)'=='Release|AnyCPU'", parent: this.Root);
            this.CreateElement("DefineConstants", parent: releaseProperties, value: GetPreprocessorDefines());
            this.CreateElement("NoWarn", parent: releaseProperties);
            this.CreateElement("TreatWarningsAsErrors", parent: releaseProperties, value: "true");
            this.CreateElement("WarningsAsErrors", parent: releaseProperties);
            this.CreateElement("PlatformTarget", parent: releaseProperties, value: "AnyCPU");

            var nugetReferences = this.CreateItemGroup(parent: this.Root);
            foreach (var package in Graph.Instance.Packages)
            {
                var packageSource = this.CreateItemGroup(parent: this.Root);

                this.CreateOtherSourceFile(package.XMLFilename, package, packageSource);
                foreach (var script in package.GetScriptFiles(allBuilders: true))
                {
                    this.CreateCompilableSourceFile(script, package, packageSource);
                }

                foreach (var nuget in package.NuGetPackages)
                {
                    if (!nuget.Platforms.Includes(Bam.Core.OSUtilities.CurrentPlatform))
                    {
                        continue;
                    }
                    this.CreateNugetReference(nuget.Identifier, nuget.Version, nugetReferences);
                }
            }
            if (null != additionalNuGetReferences)
            {
                foreach (var nuget in additionalNuGetReferences)
                {
                    this.CreateNugetReference(nuget.package, nuget.version, nugetReferences);
                }
            }

            var masterPackage = Graph.Instance.MasterPackage;
            var references = this.CreateItemGroup(parent: this.Root);
            foreach (var assembly in masterPackage.BamAssemblies)
            {
                var assemblyPath = System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, assembly.Name) + ".dll";
                this.CreateReference(assembly.Name, references, isExecutable, hintpath: assemblyPath);
            }
        }

        /// <summary>
        /// Write the .csproj to disk.
        /// </summary>
        public void
        Write()
        {
            var projectDir = System.IO.Path.GetDirectoryName(this.CsProjPath);
            IOWrapper.CreateDirectoryIfNotExists(projectDir);

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            using (var writer = System.Xml.XmlWriter.Create(this.CsProjPath, xmlWriterSettings))
            {
                this.Document.WriteTo(writer);
            }
        }

        /// <summary>
        /// Specify the entry point used by the assembly.
        /// The .cs file will only write if it does not exist, in order to honour any local edits.
        /// </summary>
        /// <param name="filename">Path of the .cs file to write for the entry point.</param>
        /// <param name="writer">Action that writes the .csproj to disk.</param>
        public void
        AddEntryPoint(
            string filename,
            System.Action<string> writer)
        {
            var projectDir = System.IO.Path.GetDirectoryName(this.CsProjPath);
            IOWrapper.CreateDirectoryIfNotExists(projectDir);

            var filePath = System.IO.Path.Combine(projectDir, filename);
            if (!System.IO.File.Exists(filePath))
            {
                writer(filePath);
            }

            var mainSource = CreateItemGroup(parent: this.Root);
            CreateCompilableSourceFile(filename, null, mainSource);
        }

        /// <summary>
        /// Specify the launch settings used by the VisualStudio project while debugging.
        /// </summary>
        /// <param name="filename">Path of the .cs file to write for the launch settings.</param>
        /// <param name="projectPath">Path of the debug project being written.</param>
        /// <param name="commandLineArgs">Enumerable of command line arguments passed when the debug project was created.</param>
        /// <param name="writer">Action that writes the json file to disk.</param>
        public void
        AddLaunchSettings(
            string filename,
            string projectPath,
            System.Collections.Generic.IEnumerable<string> commandLineArgs,
            System.Action<string, string, string> writer)
        {
            var projectDir = System.IO.Path.GetDirectoryName(this.CsProjPath);
            IOWrapper.CreateDirectoryIfNotExists(projectDir);

            var filePath = System.IO.Path.Combine(projectDir, filename);
            IOWrapper.CreateDirectoryIfNotExists(System.IO.Path.GetDirectoryName(filePath));
            writer(filePath, System.IO.Path.GetFileNameWithoutExtension(projectPath), string.Join(' ', commandLineArgs));
        }

        /// <summary>
        /// Add an embedded resource to the project file.
        /// </summary>
        /// <param name="writer">Action that writes the resource to disk.</param>
        public void
        AddEmbeddedResource(
            System.Func<string,string> writer)
        {
            var resourcePath = writer(this.CsProjPath);
            var resources = CreateItemGroup(parent: this.Root);
            this.CreateEmbeddedResourceFile(resourcePath, resources);
        }
    }
}
