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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Representation of the .xml file required by each Bam package, with methods for reading
    /// and writing them, and inspecting and retrieving data about packages.
    /// </summary>
    public class PackageDefinition
    {
        private static void
        ValidationCallBack(
            object sender,
            System.Xml.Schema.ValidationEventArgs args)
        {
            if (args.Severity == System.Xml.Schema.XmlSeverityType.Warning)
            {
                Log.DebugMessage("Warning: Matching schema not found (" + args.Exception.GetType().ToString() + "). " + args.Message);
            }
            else
            {
                if (args.Exception is System.Xml.Schema.XmlSchemaException)
                {
                    throw new Exception(
                        $"From {args.Exception.SourceUri} (line {args.Exception.LineNumber}, position {args.Exception.LinePosition}):\n{args.Exception.Message}");
                }

                var message = $"Validation error: ${args.Message}";
                message += $"\nAt '{args.Exception.SourceUri}', line {args.Exception.LineNumber}, position {args.Exception.LinePosition}";

                throw new System.Xml.XmlException(message);
            }
        }

        private static string XmlNamespace => "http://www.buildamation.com";

        private static string RelativePathToLatestSchema => "./Schema/BamPackageDefinitionV1.xsd";

        private static System.Xml.XmlReaderSettings CommonReaderSettings { get; set; }

        private System.Collections.Generic.HashSet<string> ModulesCreated = new System.Collections.Generic.HashSet<string>();

        /// <summary>
        /// Enumerated unique list of module names created from this package definition.
        /// </summary>
        public System.Collections.Generic.IEnumerable<string> CreatedModules()
        {
            foreach (var m in this.ModulesCreated)
            {
                yield return m;
            }
        }

        /// <summary>
        /// Add the name of another module to the unique list.
        /// </summary>
        /// <param name="name">String name of the module to add.</param>
        public void
        AddCreatedModule(
            string name)
        {
            lock (this.ModulesCreated)
            {
                this.ModulesCreated.Add(name);
            }
        }

        static PackageDefinition()
        {
            var xmlReaderSettings = new System.Xml.XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(XmlNamespace, System.IO.Path.Combine(Graph.Instance.ProcessState.ExecutableDirectory, RelativePathToLatestSchema));
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            xmlReaderSettings.ValidationType = System.Xml.ValidationType.Schema;
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;
            xmlReaderSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
            xmlReaderSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
            xmlReaderSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            CommonReaderSettings = xmlReaderSettings;
        }

        private void
        Validate()
        {
            // Create the XmlReader object.
            using (var reader = System.Xml.XmlReader.Create(this.XMLFilename, CommonReaderSettings))
            {
                // Parse the file.
                while (reader.Read()) ;
            }
        }

        private void
        Initialize(
            string xmlFilename,
            PackageRepository repository)
        {
            this.XMLFilename = xmlFilename;
            this.Repo = repository;

            this.Dependents = new Array<(string name, string version, bool? isDefault)>();
            this.BamAssemblies = new Array<BamAssemblyDescription>();
            this.DotNetAssemblies = new Array<DotNetAssemblyDescription>();
            this.NuGetPackages = new Array<NuGetPackageDescription>();
            this.SupportedPlatforms = EPlatform.All;
            this.Definitions = new StringArray();
            this.NamedPackageRepositories = new StringArray();
            this.PackageRepositories = new StringArray();
            this.Description = string.Empty;
            this.Parents = new Array<PackageDefinition>();
        }

        /// <summary>
        /// Construct a new instance, based from an existing XML filename.
        /// </summary>
        /// <param name="xmlFilename">Xml filename of the package's definition file</param>
        /// <param name="repository">Package repository that contains this package.</param>
        public
        PackageDefinition(
            string xmlFilename,
            PackageRepository repository) => this.Initialize(xmlFilename, repository);

        /// <summary>
        /// Create a new instance, for the specified directory, package name and version.
        /// </summary>
        /// <param name="bamDirectory">Bam directory.</param>
        /// <param name="name">Name.</param>
        /// <param name="version">Version.</param>
        public
        PackageDefinition(
            string bamDirectory,
            string name,
            string version)
        {
            var definitionName = (null != version) ? $"{name}-{version}.xml" : $"{name}.xml";
            var xmlFilename = System.IO.Path.Combine(bamDirectory, definitionName);
            this.Initialize(xmlFilename, null); // there is no repo defined for creating new packages
            this.Name = name;
            this.Version = version;
            if (null != version)
            {
                this.Description = $"A new package called {name} with version {version}";
            }
            else
            {
                this.Description = $"A new package called {name}";
            }

            // add required BuildAMation assemblies
            var bamVersion = Graph.Instance.ProcessState.Version;
            if (bamVersion.Build > 0)
            {
                this.BamAssemblies.Add(new BamAssemblyDescription("Bam.Core", bamVersion.Major, bamVersion.Minor, bamVersion.Build));
            }
            else
            {
                this.BamAssemblies.Add(new BamAssemblyDescription("Bam.Core", bamVersion.Major, bamVersion.Minor));
            }

            // add required DotNet assemblies
            {
                var systemDesc = new DotNetAssemblyDescription("System");
                var systemXmlDesc = new DotNetAssemblyDescription("System.Xml");
                var systemCoreDesc = new DotNetAssemblyDescription("System.Core");

                systemDesc.RequiredTargetFramework = "4.0.30319";
                systemXmlDesc.RequiredTargetFramework = "4.0.30319";
                systemCoreDesc.RequiredTargetFramework = "4.0.30319";

                this.DotNetAssemblies.AddRange(new[] { systemDesc, systemXmlDesc, systemCoreDesc });
            }
        }

        /// <summary>
        /// Write the latest version of the XML definition file.
        /// Always refer to the schema location as a relative path to the bam executable.
        /// Always reference the default namespace in the first element.
        /// Don't use a namespace prefix on child elements, as the default namespace covers them.
        /// </summary>
        public void
        Write()
        {
            if (System.IO.File.Exists(this.XMLFilename))
            {
                var attributes = System.IO.File.GetAttributes(this.XMLFilename);
                if (0 != (attributes & System.IO.FileAttributes.ReadOnly))
                {
                    throw new Exception($"File '{this.XMLFilename}' cannot be written to as it is read only");
                }
            }

            this.Dependents.Sort();

            var document = new System.Xml.XmlDocument();
            var namespaceURI = XmlNamespace;
            var packageDefinition = document.CreateElement("PackageDefinition", namespaceURI);
            {
                var xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                var schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                schemaAttribute.Value = $"{namespaceURI} {RelativePathToLatestSchema}";
                packageDefinition.Attributes.Append(schemaAttribute);
                packageDefinition.SetAttribute("name", this.Name);
                if (null != this.Version)
                {
                    packageDefinition.SetAttribute("version", this.Version);
                }
            }
            document.AppendChild(packageDefinition);

            // package description
            if (!string.IsNullOrEmpty(this.Description))
            {
                var descriptionElement = document.CreateElement("Description", namespaceURI);
                descriptionElement.InnerText = this.Description;
                packageDefinition.AppendChild(descriptionElement);
            }

            // package repositories
            var packageRepos = new StringArray(this.PackageRepositories);
            var namedPackageRepos = new StringArray(this.NamedPackageRepositories);
            if (packageRepos.Any() || namedPackageRepos.Any())
            {
                var packageRootsElement = document.CreateElement("PackageRepositories", namespaceURI);
                foreach (string repo in packageRepos)
                {
                    var rootElement = document.CreateElement("Repo", namespaceURI);
                    rootElement.SetAttribute("dir", repo);
                    packageRootsElement.AppendChild(rootElement);
                }
                foreach (string repo in namedPackageRepos)
                {
                    var rootElement = document.CreateElement("Repo", namespaceURI);
                    rootElement.SetAttribute("name", repo);
                    packageRootsElement.AppendChild(rootElement);
                }

                packageDefinition.AppendChild(packageRootsElement);
            }

            if (this.Dependents.Any())
            {
                var dependentsEl = document.CreateElement("Dependents", namespaceURI);
                foreach (var (packageName, packageVersion, packageIsDefault) in this.Dependents)
                {
                    System.Xml.XmlElement packageElement = null;

                    {
                        var node = dependentsEl.FirstChild;
                        while (node != null)
                        {
                            var attributes = node.Attributes;
                            var nameAttribute = attributes["Name"];
                            if ((null != nameAttribute) && (nameAttribute.Value.Equals(packageName, System.StringComparison.Ordinal)))
                            {
                                packageElement = node as System.Xml.XmlElement;
                                break;
                            }

                            node = node.NextSibling;
                        }
                    }

                    if (null == packageElement)
                    {
                        packageElement = document.CreateElement("Package", namespaceURI);
                        packageElement.SetAttribute("name", packageName);
                        if (null != packageVersion)
                        {
                            packageElement.SetAttribute("version", packageVersion);
                        }
                        if (packageIsDefault.HasValue)
                        {
                            packageElement.SetAttribute("default", packageIsDefault.Value.ToString().ToLower());
                        }
                        dependentsEl.AppendChild(packageElement);
                    }
                }
                packageDefinition.AppendChild(dependentsEl);
            }

            if (this.BamAssemblies.Any())
            {
                var requiredAssemblies = document.CreateElement("BamAssemblies", namespaceURI);
                foreach (var assembly in this.BamAssemblies)
                {
                    var assemblyElement = document.CreateElement("BamAssembly", namespaceURI);
                    assemblyElement.SetAttribute("name", assembly.Name);
                    if (assembly.MajorVersion.HasValue)
                    {
                        assemblyElement.SetAttribute("major", assembly.MajorVersion.ToString());
                        if (assembly.MinorVersion.HasValue)
                        {
                            assemblyElement.SetAttribute("minor", assembly.MinorVersion.ToString());
                            if (assembly.PatchVersion.HasValue)
                            {
                                // honour any existing BamAssembly with a specific patch version
                                assemblyElement.SetAttribute("patch", assembly.PatchVersion.ToString());
                            }
                        }
                    }
                    requiredAssemblies.AppendChild(assemblyElement);
                }
                packageDefinition.AppendChild(requiredAssemblies);
            }

            if (this.DotNetAssemblies.Any())
            {
                var requiredDotNetAssemblies = document.CreateElement("DotNetAssemblies", namespaceURI);
                foreach (var desc in this.DotNetAssemblies)
                {
                    var assemblyElement = document.CreateElement("DotNetAssembly", namespaceURI);
                    assemblyElement.SetAttribute("name", desc.Name);
                    if (null != desc.RequiredTargetFramework)
                    {
                        assemblyElement.SetAttribute("requiredTargetFramework", desc.RequiredTargetFramework);
                    }
                    requiredDotNetAssemblies.AppendChild(assemblyElement);
                }
                packageDefinition.AppendChild(requiredDotNetAssemblies);
            }

            if (this.NuGetPackages.Any())
            {
                var requiredNuGetPackages = document.CreateElement("NuGetPackages", namespaceURI);
                foreach (var desc in this.NuGetPackages)
                {
                    var nugetPackageElement = document.CreateElement("NuGetPackage", namespaceURI);
                    nugetPackageElement.SetAttribute("id", desc.Identifier);
                    nugetPackageElement.SetAttribute("version", desc.Version);
                    if (desc.Platforms.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var platformEl = document.CreateElement("Platform", namespaceURI);
                        platformEl.SetAttribute("name", "Windows");
                        nugetPackageElement.AppendChild(platformEl);
                    }
                    if (desc.Platforms.Includes(Bam.Core.EPlatform.Linux))
                    {
                        var platformEl = document.CreateElement("Platform", namespaceURI);
                        platformEl.SetAttribute("name", "Linux");
                        nugetPackageElement.AppendChild(platformEl);
                    }
                    if (desc.Platforms.Includes(Bam.Core.EPlatform.OSX))
                    {
                        var platformEl = document.CreateElement("Platform", namespaceURI);
                        platformEl.SetAttribute("name", "OSX");
                        nugetPackageElement.AppendChild(platformEl);
                    }
                    requiredNuGetPackages.AppendChild(nugetPackageElement);
                }
                packageDefinition.AppendChild(requiredNuGetPackages);
            }

            // supported platforms
            {
                var supportedPlatformsElement = document.CreateElement("SupportedPlatforms", namespaceURI);

                if (EPlatform.Windows == (this.SupportedPlatforms & EPlatform.Windows))
                {
                    var platformElement = document.CreateElement("Platform", namespaceURI);
                    platformElement.SetAttribute("name", "Windows");
                    supportedPlatformsElement.AppendChild(platformElement);
                }
                if (EPlatform.Linux == (this.SupportedPlatforms & EPlatform.Linux))
                {
                    var platformElement = document.CreateElement("Platform", namespaceURI);
                    platformElement.SetAttribute("name", "Linux");
                    supportedPlatformsElement.AppendChild(platformElement);
                }
                if (EPlatform.OSX == (this.SupportedPlatforms & EPlatform.OSX))
                {
                    var platformElement = document.CreateElement("Platform", namespaceURI);
                    platformElement.SetAttribute("name", "OSX");
                    supportedPlatformsElement.AppendChild(platformElement);
                }

                packageDefinition.AppendChild(supportedPlatformsElement);
            }

            // definitions
            this.Definitions.Remove(this.GetPackageDefinitionName());
            if (this.Definitions.Any())
            {
                var definitionsElement = document.CreateElement("Definitions", namespaceURI);

                foreach (string define in this.Definitions)
                {
                    var defineElement = document.CreateElement("Definition", namespaceURI);
                    defineElement.SetAttribute("name", define);
                    definitionsElement.AppendChild(defineElement);
                }

                packageDefinition.AppendChild(definitionsElement);
            }

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;
            xmlWriterSettings.NewLineChars = "\n";
            xmlWriterSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            xmlWriterSettings.Encoding = new System.Text.UTF8Encoding(false);

            using (var xmlWriter = System.Xml.XmlWriter.Create(this.XMLFilename, xmlWriterSettings))
            {
                document.WriteTo(xmlWriter);
                xmlWriter.WriteWhitespace(xmlWriterSettings.NewLineChars);
            }

            this.Validate();
        }

        private string
        GetPackageDefinitionName()
        {
            if (null != this.Version)
            {
                var processedVersion = this.Version.Replace('.', '_').Replace('-', '_');
                return $"D_PACKAGE_{this.Name}_{processedVersion}".ToUpper();
            }
            else
            {
                return $"D_PACKAGE_{this.Name}".ToUpper();
            }
        }

        /// <summary>
        /// Read an existing XML file into the instance.
        /// This is not treated as a master package, so any 'default' dependents
        /// are not honoured.
        /// </summary>
        public void
        Read()
        {
            this.ReadInternal();

            var packageDefinition = this.GetPackageDefinitionName();
            this.Definitions.AddUnique(packageDefinition);
        }

        /// <summary>
        /// Re-read an existing XML file into the instance, treating it specially as
        /// the master package, which means that 'default' dependents are honoured.
        /// </summary>
        public void
        ReReadAsMaster()
        {
            this.Initialize(this.XMLFilename, this.Repo);
            this.ReadInternal(isMaster: true);

            var packageDefinition = this.GetPackageDefinitionName();
            this.Definitions.AddUnique(packageDefinition);
        }

        /// <summary>
        /// Read an existing XML file into the instance.
        /// </summary>
        private void
        ReadInternal(
            bool isMaster = false)
        {
            Log.DebugMessage($"Reading package definition file: {this.XMLFilename}");

            // try reading the current schema version first
            if (this.ReadCurrent(isMaster))
            {
                if (Graph.Instance.ForceDefinitionFileUpdate)
                {
                    if (Graph.Instance.UpdateBamAssemblyVersions)
                    {
                        var bamVersion = Graph.Instance.ProcessState.Version;
                        var newAssemblyList = new Array<BamAssemblyDescription>();
                        foreach (var asm in this.BamAssemblies)
                        {
                            newAssemblyList.Add(
                                new BamAssemblyDescription(
                                    asm.Name,
                                    bamVersion.Major,
                                    bamVersion.Minor,
                                    bamVersion.Build
                                )
                           );
                        }
                        this.BamAssemblies = newAssemblyList;
                    }
                    Log.DebugMessage($"Forced writing of package definition file '{this.XMLFilename}'");
                    this.Write();
                }

                return;
            }

            // this is where earlier versions would be read
            // and immediately written back to update to the latest schema

            throw new Exception($"An error occurred while reading a package or package definition file '{this.XMLFilename}' does not satisfy any of the package definition schemas");
        }

        private bool
        ReadDescription(
            System.Xml.XmlReader xmlReader)
        {
            var descriptionElementName = "Description";
            if (descriptionElementName != xmlReader.Name)
            {
                return false;
            }

            var message = xmlReader.ReadString();
            this.Description = message;

            return true;
        }

        private bool
        ReadPackageRepositories(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "PackageRepositories";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            var elName1 = "Repo";
            var elName2 = "NamedRepo";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootName, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (elName1.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    var dir = xmlReader.GetAttribute("dir");
                    this.PackageRepositories.AddUnique(dir);
                    continue;
                }
                else if (elName2.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    var name = xmlReader.GetAttribute("name");
                    this.NamedPackageRepositories.AddUnique(name);
                    continue;
                }

                throw new Exception(
                    $"Unexpected child element of '{rootName}'. Found '{xmlReader.Name}', expected '{elName1}' or '{elName2}'"
                );
            }

            return true;
        }

        private bool
        ReadDependents(
            System.Xml.XmlReader xmlReader,
            bool isMaster)
        {
            var rootName = "Dependents";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            var elName = "Package";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootName, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!xmlReader.Name.Equals(elName, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootName}'. Found '{xmlReader.Name}'. Expected '{elName}'");
                }

                var name = xmlReader.GetAttribute("name");
                var version = xmlReader.GetAttribute("version");
                var isDefault = xmlReader.GetAttribute("default");

                this.Dependents.Add(
                    (
                        name,
                        version,
                        (isMaster && (isDefault != null)) ? System.Xml.XmlConvert.ToBoolean(isDefault) as bool? : null
                    )
                );
            }

            foreach (var duplicateDepName in this.Dependents.GroupBy(item => item.name).Where(item => item.Count() > 1).Select(item => item.Key))
            {
                var numDefaults = this.Dependents.Where(item => item.name.Equals(duplicateDepName, System.StringComparison.Ordinal)).Where(item => item.isDefault.HasValue && item.isDefault.Value).Count();
                if (numDefaults > 1)
                {
                    throw new Exception($"Package definition {this.XMLFilename} has defined dependency {duplicateDepName} multiple times as default");
                }
            }

            return true;
        }

        private bool
        ReadBamAssemblies(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "BamAssemblies";
            if (!rootName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
            {
                return false;
            }

            var elName = "BamAssembly";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootName, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!elName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootName}'. Found '{xmlReader.Name}', expected '{elName}'");
                }

                var assemblyName = xmlReader.GetAttribute("name");

                var bamVersion = Graph.Instance.ProcessState.Version;
                var majorVersion = xmlReader.GetAttribute("major");
                if (!System.String.IsNullOrEmpty(majorVersion))
                {
                    var major = System.Convert.ToInt32(majorVersion);
                    var minorVersion = xmlReader.GetAttribute("minor");
                    if (!System.String.IsNullOrEmpty(minorVersion))
                    {
                        var minor = System.Convert.ToInt32(minorVersion);
                        var patchVersion = xmlReader.GetAttribute("patch");
                        if (!System.String.IsNullOrEmpty(patchVersion))
                        {
                            var patch = System.Convert.ToInt32(patchVersion);
                            this.BamAssemblies.AddUnique(new BamAssemblyDescription(assemblyName, major, minor, patch));
                        }
                        else
                        {
                            this.BamAssemblies.AddUnique(new BamAssemblyDescription(assemblyName, major, minor));
                        }
                    }
                    else
                    {
                        this.BamAssemblies.AddUnique(new BamAssemblyDescription(assemblyName, major, bamVersion.Minor));
                    }
                }
                else
                {
                    this.BamAssemblies.AddUnique(new BamAssemblyDescription(assemblyName, bamVersion.Major, bamVersion.Minor));
                }
            }

            return true;
        }

        private bool
        ReadDotNetAssemblies(
            System.Xml.XmlReader xmlReader)
        {
            var rootEl = "DotNetAssemblies";
            if (!rootEl.Equals(xmlReader.Name, System.StringComparison.Ordinal))
            {
                return false;
            }

            var elName = "DotNetAssembly";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootEl, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!elName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootEl}'. Found '{xmlReader.Name}', expected '{elName}'");
                }

                var assemblyName = xmlReader.GetAttribute("name");
                var targetFramework = xmlReader.GetAttribute("requiredTargetFramework");

                var desc = new DotNetAssemblyDescription(assemblyName);
                if (null != targetFramework)
                {
                    desc.RequiredTargetFramework = targetFramework;
                }

                this.DotNetAssemblies.AddUnique(desc);
            }

            return true;
        }

        private bool
        ReadNuGetPackages(
            System.Xml.XmlReader xmlReader)
        {
            var rootEl = "NuGetPackages";
            if (!rootEl.Equals(xmlReader.Name, System.StringComparison.Ordinal))
            {
                return false;
            }

            var elName = "NuGetPackage";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootEl, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!elName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootEl}'. Found '{xmlReader.Name}', expected '{elName}'");
                }

                var nugetIdentifier = xmlReader.GetAttribute("id");
                var nugetVersion = xmlReader.GetAttribute("version");
                var platforms = this.TranslatePlatformElements(xmlReader, elName);

                var desc = new NuGetPackageDescription(nugetIdentifier, nugetVersion, platforms);

                this.NuGetPackages.AddUnique(desc);
            }

            return true;
        }

        private EPlatform
        TranslatePlatformElements(
            System.Xml.XmlReader xmlReader,
            string rootName)
        {
            EPlatform platforms = EPlatform.Invalid;
            var elName = "Platform";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootName, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!elName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootName}'. Found '{xmlReader.Name}', expected '{elName}'");
                }

                var name = xmlReader.GetAttribute("name");
                platforms |= Platform.FromString(name);
            }

            return platforms;
        }

        private bool
        ReadSupportedPlatforms(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "SupportedPlatforms";
            if (!rootName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
            {
                return false;
            }

            this.SupportedPlatforms = this.TranslatePlatformElements(xmlReader, rootName);

            return true;
        }

        private bool
        ReadDefinitions(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "Definitions";
            if (!rootName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
            {
                return false;
            }

            var elName = "Definition";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootName, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!elName.Equals(xmlReader.Name, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootName}'. Found '{xmlReader.Name}', expected '{elName}'");
                }

                var definition = xmlReader.GetAttribute("name");
                this.Definitions.AddUnique(definition);
            }

            return true;
        }

        private bool
        ReadSources(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "Sources";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            var extractto = xmlReader.GetAttribute("extractto");

            var elName = "Source";
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(rootName, System.StringComparison.Ordinal) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (!xmlReader.Name.Equals(elName, System.StringComparison.Ordinal))
                {
                    throw new Exception($"Unexpected child element of '{rootName}'. Found '{xmlReader.Name}'. Expected '{elName}'");
                }

                var platform = xmlReader.GetAttribute("platform");
                if (null != platform)
                {
                    var platformEnum = Platform.FromString(platform);
                    if (!OSUtilities.IsCurrentPlatformSupported(platformEnum))
                    {
                        // skip Source elements not applicable to the current platform
                        continue;
                    }
                }

                var type = xmlReader.GetAttribute("type");
                var path = xmlReader.GetAttribute("path");
                var subdir = xmlReader.GetAttribute("subdir");

                if (null == this.Sources)
                {
                    this.Sources = new Array<PackageSource>();
                }
                this.Sources.Add(
                    new PackageSource(
                        this.Name,
                        this.Version,
                        type,
                        path,
                        subdir,
                        extractto
                    )
                );
            }

            return true;
        }

        private bool
        ReadCurrent(
            bool isMaster)
        {
            try
            {
                using (var xmlReader = System.Xml.XmlReader.Create(this.XMLFilename, CommonReaderSettings))
                {
                    var rootElementName = "PackageDefinition";
                    if (!xmlReader.ReadToFollowing(rootElementName))
                    {
                        Log.DebugMessage($"Root element '{rootElementName}' not found in '{this.XMLFilename}'. Xml instance may be referencing an old schema. This file will now be upgraded to the latest schema.");
                        return false;
                    }

                    this.Name = xmlReader.GetAttribute("name");
                    this.Version = xmlReader.GetAttribute("version");

                    while (xmlReader.Read())
                    {
                        if (ReadDescription(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadPackageRepositories(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDependents(xmlReader, isMaster))
                        {
                            // all done
                        }
                        else if (ReadBamAssemblies(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDotNetAssemblies(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadNuGetPackages(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadSupportedPlatforms(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDefinitions(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadSources(xmlReader))
                        {
                            // all done
                        }
                        else if (xmlReader.Name.Equals(rootElementName, System.StringComparison.Ordinal))
                        {
                            // should be the end element
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                throw new Exception($"Expected end of root element but found '{xmlReader.Name}'");
                            }
                        }
                        else
                        {
                            throw new Exception($"Package definition reading code failed to recognize element with name '{xmlReader.Name}'");
                        }
                    }

                    if (!xmlReader.EOF)
                    {
                        throw new Exception("Failed to read all of file");
                    }
                }
            }
            catch (Exception ex)
            {
                // something in Bam went wrong
                throw ex;
            }
            catch (System.FormatException formatEx)
            {
                // format of the XML is wrong
                throw new Exception(formatEx, $"Error while reading {this.XMLFilename}");
            }
            catch (System.Xml.XmlException xmlEx)
            {
                // the XML does not match the schema
                throw new Exception(xmlEx, $"Error while reading {this.XMLFilename}");
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the XML filename associated with the package.
        /// </summary>
        /// <value>The XML filename.</value>
        public string XMLFilename { get; private set; }

        /// <summary>
        /// Get the package name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Get the package version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; private set; }

        /// <summary>
        /// Array of package name, package version, isdefaultversion, for each dependent of the package.
        /// </summary>
        /// <value>The dependents.</value>
        public Array<(string name, string version, bool? isDefault)> Dependents { get; private set; }

        /// <summary>
        /// Array of Bam assemblies required for this package.
        /// </summary>
        /// <value>The bam assemblies.</value>
        public Array<BamAssemblyDescription> BamAssemblies { get; private set; }

        /// <summary>
        /// Array of .NET assemblies required for this package.
        /// </summary>
        /// <value>The dot net assemblies.</value>
        public Array<DotNetAssemblyDescription> DotNetAssemblies { get; private set; }

        /// <summary>
        /// Array of NuGet packages required for this package.
        /// </summary>
        /// <value>The NuGet packages.</value>
        public Array<NuGetPackageDescription> NuGetPackages { get; private set; }

        /// <summary>
        /// Get or set the enumerations for the platforms supported by this package.
        /// </summary>
        /// <value>The supported platforms.</value>
        public EPlatform SupportedPlatforms { get; set; }

        /// <summary>
        /// Get or set the preprocessor definitions used for compiling the package assembly.
        /// </summary>
        /// <value>The definitions.</value>
        public StringArray Definitions { get; set; }

        /// <summary>
        /// Gets or sets the array of repositories to search for packages in.
        /// NOTE: These are EXTRA repositories to search.
        /// </summary>
        /// <value>The package repositories.</value>
        public StringArray PackageRepositories { get; set; }

        /// <summary>
        /// Gets or sets the array of named repositories to search for packages in.
        /// The User Configuration "Repository:SearchDirs" is used to specify the directories
        /// in which to find the named repositories.
        /// NOTE: These are EXTRA repositories to search.
        /// </summary>
        /// <value>The named package repositories.</value>
        public StringArray NamedPackageRepositories { get; set; }

        /// <summary>
        /// PackageRepository containing this package.
        /// </summary>
        public PackageRepository Repo { get; private set; }

        /// <summary>
        /// Gets or sets the description of the package.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the array of sources of the package.
        /// </summary>
        public Array<PackageSource> Sources { get; private set; }

        /// <summary>
        /// String representation is the XML filename
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.PackageDefinition"/>.</returns>
        public override string
        ToString() => this.XMLFilename;

        /// <summary>
        /// Based on the dependents lists in the XML file, resolve all dependents by reading more
        /// package definition files.
        /// </summary>
        /// <param name="current">Current.</param>
        /// <param name="authenticated">Authenticated.</param>
        /// <param name="candidatePackageDefinitions">Candidate package definitions.</param>
        public static void
        ResolveDependencies(
            PackageDefinition current,
            Array<PackageDefinition> authenticated,
            Array<PackageDefinition> candidatePackageDefinitions)
        {
            var matchingPackages = authenticated.Where(item => item.Name.Equals(current.Name, System.StringComparison.Ordinal));
            if (null != current.Version)
            {
                matchingPackages = matchingPackages.Where(item => item.Version.Equals(current.Version, System.StringComparison.Ordinal));
            }
            if (matchingPackages.FirstOrDefault() != null)
            {
                return;
            }

            if (!current.SupportedPlatforms.Includes(OSUtilities.CurrentOS))
            {
                throw new Exception($"Package {current.FullName} is not supported on {OSUtilities.CurrentOS.ToString()}");
            }

            authenticated.Add(current);
            foreach (var (depName, depVersion, depIsDefault) in current.Dependents)
            {
                var candidates = candidatePackageDefinitions.Where(item => item.Name.Equals(depName, System.StringComparison.Ordinal));
                if (depVersion != null)
                {
                    // the item.Version check is in case the candidates list has both versioned and an unversioned package
                    candidates = candidates.Where(item => item.Version != null && item.Version.Equals(depVersion, System.StringComparison.Ordinal));
                }
                var candidateCount = candidates.Count();
                if (0 == candidateCount)
                {
                    var message = new System.Text.StringBuilder();
                    message.Append($"Unable to find a candidate package with name '{depName}'");
                    if (null != depVersion)
                    {
                        message.Append($" and version {depVersion}");
                    }
                    message.AppendLine($" required by {current.XMLFilename}");
                    var packageRepos = new StringArray();
                    Graph.Instance.PackageRepositories.ToList().ForEach(item => packageRepos.AddUnique(item.RootPath));
                    message.AppendLine("Searched in the package repositories:");
                    message.AppendLine(packageRepos.ToString("\n"));
                    throw new Exception(message.ToString());
                }
                if (candidateCount > 1)
                {
                    var message = new System.Text.StringBuilder();
                    message.Append($"There are {candidateCount} identical candidate packages with name '{depName}'");
                    if (null != depVersion)
                    {
                        message.Append($" and version {depVersion}");
                    }
                    message.AppendLine($" required by {current.XMLFilename} from the following package definition files:");
                    foreach (var candidate in candidates)
                    {
                        message.AppendLine(candidate.XMLFilename);
                    }
                    var packageRepos = new StringArray();
                    Graph.Instance.PackageRepositories.ToList().ForEach(item => packageRepos.AddUnique(item.RootPath));
                    message.AppendLine("Found in the package repositories:");
                    message.AppendLine(packageRepos.ToString("\n"));
                    throw new Exception(message.ToString());
                }

                candidates.First().Parents.AddUnique(current);

                ResolveDependencies(candidates.First(), authenticated, candidatePackageDefinitions);
            }
        }

        /// <summary>
        /// Get the full name of the package, which is Name-Version, or just Name if there is no version.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName
        {
            get
            {
                if (null == this.Version)
                {
                    return this.Name;
                }
                else
                {
                    return $"{this.Name}-{this.Version}";
                }
            }
        }

        /// <summary>
        /// Get the array of files to compile from all packages.
        /// </summary>
        /// <returns>The script files.</returns>
        /// <param name="allBuilders">If set to <c>true</c> all builders.</param>
        public StringArray
        GetScriptFiles(
            bool allBuilders = false)
        {
            var bamDir = this.GetBamDirectory();
            var scriptDir = System.IO.Path.Combine(bamDir, PackageUtilities.ScriptsSubFolder);
            var scripts = new StringArray(System.IO.Directory.GetFiles(scriptDir, "*.cs", System.IO.SearchOption.AllDirectories));

            var builderNames = new StringArray();
            if (allBuilders)
            {
                foreach (var package in Graph.Instance.Packages)
                {
                    if (!BuildModeUtilities.IsBuildModePackage(package.Name))
                    {
                        continue;
                    }
                    builderNames.Add(package.Name);
                }
            }
            else
            {
                builderNames.AddUnique($"{Graph.Instance.Mode}Builder");
            }
            foreach (var builderName in builderNames)
            {
                var builderScriptDir = System.IO.Path.Combine(bamDir, builderName);
                if (System.IO.Directory.Exists(builderScriptDir))
                {
                    scripts.AddRange(System.IO.Directory.GetFiles(builderScriptDir, "*.cs", System.IO.SearchOption.AllDirectories));
                }
            }
            return scripts;
        }

        /// <summary>
        /// Get the pathname of the debug project file that could be written.
        /// </summary>
        /// <returns>The debug package project pathname.</returns>
        public string
        GetDebugPackageProjectPathname()
        {
            var projectDir = System.IO.Path.Combine(this.GetPackageDirectory(), "PackageDebug");
            var projectPathname = System.IO.Path.Combine(projectDir, this.FullName) + "-bam.csproj";
            return projectPathname;
        }

        // package repo/package name/bam/<definition file>.xml
        private string
        GetBamDirectory() => System.IO.Path.GetDirectoryName(this.XMLFilename);

        /// <summary>
        /// Get the package directory, i.e. that containing the bam folder.
        /// </summary>
        /// <returns>The package directory.</returns>
        public string
        GetPackageDirectory() => System.IO.Path.GetDirectoryName(this.GetBamDirectory());

        /// <summary>
        /// Get the directory that build files are written into.
        /// </summary>
        /// <returns>The build directory.</returns>
        public string
        GetBuildDirectory() => System.IO.Path.Combine(Graph.Instance.BuildRoot, this.FullName);

        private void
        ShowDependencies(
            int depth,
            Array<PackageDefinition> visitedPackages,
            string packageFormatting)
        {
            visitedPackages.Add(this);
            foreach (var (depName, depVersion, depIsDefault) in this.Dependents)
            {
                var dep = Graph.Instance.Packages.First(item =>
                    System.String.Equals(item.Name, depName, System.StringComparison.Ordinal) &&
                    System.String.Equals(item.Version, depVersion, System.StringComparison.Ordinal)
                );
                if (visitedPackages.Contains(dep))
                {
                    continue;
                }

                var indent = new string(' ', depth * 4);
                var isDefault = depIsDefault.GetValueOrDefault(false) ? "*" : System.String.Empty;
                var formattedName = $"{indent}{dep.FullName}{isDefault}";
                var repo = dep.PackageRepositories.Any() ? dep.PackageRepositories[0] : "Found in " + System.IO.Path.GetDirectoryName(dep.GetPackageDirectory());

                Log.MessageAll(packageFormatting, formattedName, repo);

                if (dep.Dependents.Any())
                {
                    dep.ShowDependencies(depth + 1, visitedPackages, packageFormatting);
                }
            }
        }

        private void
        DumpTreeInternal(
            PackageTreeNode node,
            int depth,
            System.Collections.Generic.Dictionary<PackageTreeNode, int> encountered,
            Array<PackageTreeNode> displayed)
        {
            if (!encountered.ContainsKey(node))
            {
                encountered.Add(node, depth);
            }
            foreach (var child in node.Children)
            {
                if (!encountered.ContainsKey(child))
                {
                    encountered.Add(child, depth + 1);
                }
            }

            var indent = new string('\t', depth);
            if (null != node.Definition)
            {
                Log.MessageAll($"{indent}{node.Definition.FullName}");
            }
            else
            {
                Log.MessageAll($"{indent}{node.Name}-{node.Version} ***** undiscovered *****");
            }
            if (encountered[node] < depth)
            {
                return;
            }
            if (displayed.Contains(node))
            {
                return;
            }
            else
            {
                displayed.Add(node);
            }
            foreach (var child in node.Children)
            {
                this.DumpTreeInternal(child, depth + 1, encountered, displayed);
            }
        }

        /// <summary>
        /// Show a representation of the package definition file to the console.
        /// </summary>
        public void
        Show(
            PackageTreeNode rootNode)
        {
            var packageName = this.FullName;
            var formatString = "Definition of package ''";
            int dashLength = formatString.Length + packageName.Length;
            Log.MessageAll($"Definition of package '{packageName}'");
            Log.MessageAll(new string('-', dashLength));
            if (!string.IsNullOrEmpty(this.Description))
            {
                Log.MessageAll($"Description: {this.Description}");
            }
            Log.MessageAll($"\nSupported on: {Platform.ToString(this.SupportedPlatforms, ' ')}");
            Log.MessageAll("\nBuildAMation assemblies:");
            foreach (var assembly in this.BamAssemblies)
            {
                var minVersionNumber = assembly.MinimumVersionNumber();
                if (null != minVersionNumber)
                {
                    Log.MessageAll($"\t{assembly.Name} (requires version {minVersionNumber})");
                }
                else
                {
                    Log.MessageAll($"\t{assembly.Name}");
                }
            }
            Log.MessageAll("\nDotNet assemblies:");
            foreach (var desc in this.DotNetAssemblies)
            {
                if (null == desc.RequiredTargetFramework)
                {
                    Log.MessageAll($"\t{desc.Name}");
                }
                else
                {
                    Log.MessageAll($"\t{desc.Name} (version {desc.RequiredTargetFramework})");
                }
            }
            if (null != this.Sources && this.Sources.Any())
            {
                Log.MessageAll("\nSource archives:");
                foreach (var source in this.Sources)
                {
                    Log.MessageAll($"\t{source.ArchivePath}");
                }
            }
            if (this.Definitions.Any())
            {
                Log.MessageAll("\n#defines:");
                foreach (var definition in this.Definitions)
                {
                    Log.MessageAll($"\t{definition}");
                }
            }

            if (this.PackageRepositories.Any())
            {
                Log.MessageAll("\nPackage repositories to search:");
                foreach (var repo in this.PackageRepositories)
                {
                    var absoluteRepo = RelativePathUtilities.ConvertRelativePathToAbsolute(
                        Graph.Instance.ProcessState.WorkingDirectory,
                        repo
                    );

                    Log.MessageAll($"\t'{repo}'\t(absolute path '{absoluteRepo}')");
                }
            }

            if (this.NamedPackageRepositories.Any())
            {
                Log.MessageAll("\nNamed package repositories to search (via search paths set by user configuration):");
                foreach (var repo in this.NamedPackageRepositories)
                {
                    Log.MessageAll($"\t'{repo}'");
                }
            }

            Log.MessageAll("\nPackage repositories that the system is aware of (may be more than during builds):");
            foreach (var repo in Graph.Instance.PackageRepositories)
            {
                Log.MessageAll($"\t{repo.ToString()}");
            }

            if (this.Dependents.Any())
            {
                Log.MessageAll("\nDependent packages (* = default version):");
                var packageFormatting = System.String.Format("{{0, -48}} {{1, 32}}");
                Log.MessageAll(packageFormatting, "Package Name", "From Repository");
                var visitedPackages = new Array<PackageDefinition>();
                this.ShowDependencies(1, visitedPackages, packageFormatting);
                Log.MessageAll("\n-----");
                var encountered = new System.Collections.Generic.Dictionary<PackageTreeNode, int>();
                var displayed = new Array<PackageTreeNode>();
                this.DumpTreeInternal(rootNode, 0, encountered, displayed);
            }
            else
            {
                Log.MessageAll("\nNo dependent packages", packageName);
            }
        }

        /// <summary>
        /// Get or set metadata associated with the package.
        /// </summary>
        /// <value>The meta data.</value>
        public PackageMetaData MetaData { get; set; }

        /// <summary>
        /// List of PackageDefinitions which have stated that this package is a dependency.
        /// </summary>
        public Array<PackageDefinition> Parents { get; set; }

        /// <summary>
        /// For each Bam assembly required by this package, validate that the version it specifies is at least
        /// that for the current version of Bam. Throw an exception if the version is insufficient.
        /// </summary>
        public void
        ValidateBamAssemblyRequirements()
        {
            foreach (var asm in this.BamAssemblies)
            {
                if (asm.Name.Equals("Bam.Core", System.StringComparison.Ordinal))
                {
                    var bamVersion = Graph.Instance.ProcessState.Version;
                    if (asm.MajorVersion <= bamVersion.Major)
                    {
                        return;
                    }
                    if (asm.MinorVersion <= bamVersion.Minor)
                    {
                        return;
                    }
                    if (asm.PatchVersion <= bamVersion.Build)
                    {
                        return;
                    }
                    throw new Exception(
                        $"This version of BuildAMation, v{Graph.Instance.ProcessState.VersionString}, does not satisfy minimum requirements v{asm.MajorVersion}.{asm.MinorVersion}.{asm.PatchVersion}, from package {this.Name}"
                    );
                }
                else
                {
                    Log.DebugMessage($"Unknown Bam assembly, {asm.Name}");
                }
            }
        }
    }
}
