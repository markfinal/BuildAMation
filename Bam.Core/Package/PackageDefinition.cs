#region License
// Copyright (c) 2010-2017, Mark Final
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
                    throw new Exception("From {0} (line {1}, position {2}):\n{3}", args.Exception.SourceUri, args.Exception.LineNumber, args.Exception.LinePosition, args.Exception.Message);
                }

                var message = System.String.Format("Validation error: " + args.Message);
                message += System.String.Format("\nAt '" + args.Exception.SourceUri + "', line " + args.Exception.LineNumber + ", position " + args.Exception.LinePosition);

                throw new System.Xml.XmlException(message);
            }
        }

        private static string XmlNamespace
        {
            get
            {
                return "http://www.buildamation.com";
            }
        }

        private static string RelativePathToLatestSchema
        {
            get
            {
                return "./Schema/BamPackageDefinitionV1.xsd";
            }
        }

        private static System.Xml.XmlReaderSettings CommonReaderSettings
        {
            get;
            set;
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

        private string
        GetAssociatedPackageDirectoryForTests()
        {
            // package repositories have a 'package' folder and a 'tests' folder
            // if this package is from the 'tests' folder, automatically add the 'packages' folder as another place to search for packages
            var thisRepo = this.GetPackageRepository();
            if (null == thisRepo)
            {
                return null;
            }
            if (System.IO.Path.GetFileName(thisRepo) == "tests")
            {
                var associatedPackagesRepo = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(thisRepo), "packages");
                if (System.IO.Directory.Exists(associatedPackagesRepo))
                {
                    return associatedPackagesRepo;
                }
            }
            return null;
        }

        private void
        Initialize(
            string xmlFilename)
        {
            this.XMLFilename = xmlFilename;
            this.Dependents = new Array<System.Tuple<string, string, bool?>>();
            this.BamAssemblies = new Array<BamAssemblyDescription>();
            this.DotNetAssemblies = new Array<DotNetAssemblyDescription>();
            this.SupportedPlatforms = EPlatform.All;
            this.Definitions = new StringArray();
            this.PackageRepositories = new StringArray(this.GetPackageRepository());
            var associatedRepo = this.GetAssociatedPackageDirectoryForTests();
            if (null != associatedRepo)
            {
                this.PackageRepositories.AddUnique(associatedRepo);
            }
            this.Description = string.Empty;
            this.Parents = new Array<PackageDefinition>();
        }

        /// <summary>
        /// Construct a new instance, based from an existing XML filename.
        /// </summary>
        /// <param name="xmlFilename">Xml filename.</param>
        public
        PackageDefinition(
            string xmlFilename)
        {
            this.Initialize(xmlFilename);
        }

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
            var definitionName = (null != version) ? System.String.Format("{0}-{1}.xml", name, version) : name + ".xml";
            var xmlFilename = System.IO.Path.Combine(bamDirectory, definitionName);
            this.Initialize(xmlFilename);
            this.Name = name;
            this.Version = version;
            if (null != version)
            {
                this.Description = System.String.Format("A new package called {0} with version {1}", name, version);
            }
            else
            {
                this.Description = System.String.Format("A new package called {0}", name);
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
                    throw new Exception("File '{0}' cannot be written to as it is read only", this.XMLFilename);
                }
            }

            this.Dependents.Sort();

            var document = new System.Xml.XmlDocument();
            var namespaceURI = XmlNamespace;
            var packageDefinition = document.CreateElement("PackageDefinition", namespaceURI);
            {
                var xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                var schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                schemaAttribute.Value = System.String.Format("{0} {1}", namespaceURI, RelativePathToLatestSchema);
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
            // TODO: could these be marked as transient?
            // don't write out the repo that this package resides in
            packageRepos.Remove(this.GetPackageRepository());
            // nor an associated repo for tests
            var associatedRepo = this.GetAssociatedPackageDirectoryForTests();
            if (null != associatedRepo)
            {
                packageRepos.Remove(associatedRepo);
            }
            if (packageRepos.Count > 0)
            {
                var packageRootsElement = document.CreateElement("PackageRepositories", namespaceURI);
                var bamDir = this.GetBamDirectory() + System.IO.Path.DirectorySeparatorChar; // slash added to make it look like a directory
                foreach (string repo in packageRepos)
                {
                    var relativePackageRepo = RelativePathUtilities.GetPath(repo, bamDir);
                    if (OSUtilities.IsWindowsHosting)
                    {
                        // standardize on non-Windows directory separators
                        relativePackageRepo = relativePackageRepo.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
                    }

                    var rootElement = document.CreateElement("Repo", namespaceURI);
                    rootElement.SetAttribute("dir", relativePackageRepo);
                    packageRootsElement.AppendChild(rootElement);
                }

                packageDefinition.AppendChild(packageRootsElement);
            }

            if (this.Dependents.Count > 0)
            {
                var dependentsEl = document.CreateElement("Dependents", namespaceURI);
                foreach (var package in this.Dependents)
                {
                    var packageName = package.Item1;
                    var packageVersion = package.Item2;
                    var packageIsDefault = package.Item3;
                    System.Xml.XmlElement packageElement = null;

                    {
                        var node = dependentsEl.FirstChild;
                        while (node != null)
                        {
                            var attributes = node.Attributes;
                            var nameAttribute = attributes["Name"];
                            if ((null != nameAttribute) && (nameAttribute.Value == packageName))
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

            if (this.BamAssemblies.Count > 0)
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

            if (this.DotNetAssemblies.Count > 0)
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
            if (this.Definitions.Count > 0)
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
                return System.String.Format("D_PACKAGE_{0}_{1}", this.Name, this.Version.Replace('.', '_').Replace('-', '_')).ToUpper();
            }
            else
            {
                return System.String.Format("D_PACKAGE_{0}", this.Name).ToUpper();
            }
        }

        /// <summary>
        /// Read an existing XML file into the instance.
        /// </summary>
        public void
        Read()
        {
            this.ReadInternal();

            var packageDefinition = this.GetPackageDefinitionName();
            this.Definitions.AddUnique(packageDefinition);
        }

        /// <summary>
        /// Read an existing XML file into the instance.
        /// </summary>
        private void
        ReadInternal()
        {
            Log.DebugMessage("Reading package definition file: {0}", this.XMLFilename);

            // try reading the current schema version first
            if (this.ReadCurrent())
            {
                if (Graph.Instance.ForceDefinitionFileUpdate)
                {
                    Log.DebugMessage("Forced writing of package definition file '{0}'", this.XMLFilename);
                    this.Write();
                }

                return;
            }

            // this is where earlier versions would be read
            // and immediately written back to update to the latest schema

            throw new Exception("An error occurred while reading a package or package definition file '{0}' does not satisfy any of the package definition schemas", this.XMLFilename);
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

            var elName = "Repo";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == rootName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (elName != xmlReader.Name)
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}', expected '{2}'", rootName, xmlReader.Name, elName);
                }

                var dir = xmlReader.GetAttribute("dir");
                var bamDir = this.GetBamDirectory();
                var absolutePackageRepoDir = RelativePathUtilities.MakeRelativePathAbsoluteTo(dir, bamDir);
                absolutePackageRepoDir = absolutePackageRepoDir.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                this.PackageRepositories.AddUnique(absolutePackageRepoDir);
            }

            return true;
        }

        private bool
        ReadDependents(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "Dependents";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            var elName = "Package";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == rootName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (xmlReader.Name != elName)
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'. Expected '{2}'", rootName, xmlReader.Name, elName);
                }

                var name = xmlReader.GetAttribute("name");
                var version = xmlReader.GetAttribute("version");
                var isDefault = xmlReader.GetAttribute("default");

                this.Dependents.Add(new System.Tuple<string, string, bool?>(name, version, (isDefault != null) ? System.Xml.XmlConvert.ToBoolean(isDefault) as bool? : null));
            }

            foreach (var duplicateDepName in this.Dependents.GroupBy(item => item.Item1).Where(item => item.Count() > 1).Select(item => item.Key))
            {
                var numDefaults = this.Dependents.Where(item => item.Item1 == duplicateDepName).Where(item => item.Item3.HasValue && item.Item3.Value).Count();
                if (numDefaults > 1)
                {
                    throw new Exception("Package definition {0} has defined dependency {1} multiple times as default", this.XMLFilename, duplicateDepName);
                }
            }

            return true;
        }

        private bool
        ReadBamAssemblies(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "BamAssemblies";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            var elName = "BamAssembly";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == rootName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (elName != xmlReader.Name)
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}', expected '{2}'", rootName, xmlReader.Name, elName);
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
            if (rootEl != xmlReader.Name)
            {
                return false;
            }

            var elName = "DotNetAssembly";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == rootEl) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (elName != xmlReader.Name)
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}', expected '{2}'", rootEl, xmlReader.Name, elName);
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
        ReadSupportedPlatforms(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "SupportedPlatforms";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            this.SupportedPlatforms = EPlatform.Invalid;
            var elName = "Platform";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == rootName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (elName != xmlReader.Name)
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}', expected '{2}'", rootName, xmlReader.Name, elName);
                }

                var name = xmlReader.GetAttribute("name");
                switch (name)
                {
                    case "Windows":
                        this.SupportedPlatforms |= EPlatform.Windows;
                        break;

                    case "Linux":
                        this.SupportedPlatforms |= EPlatform.Linux;
                        break;

                    case "OSX":
                        this.SupportedPlatforms |= EPlatform.OSX;
                        break;

                    default:
                        throw new Exception("Unexpected platform '{0}'", name);
                }
            }

            return true;
        }

        private bool
        ReadDefinitions(
            System.Xml.XmlReader xmlReader)
        {
            var rootName = "Definitions";
            if (rootName != xmlReader.Name)
            {
                return false;
            }

            var elName = "Definition";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == rootName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (elName != xmlReader.Name)
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}', expected '{2}'", rootName, xmlReader.Name, elName);
                }

                var definition = xmlReader.GetAttribute("name");
                this.Definitions.AddUnique(definition);
            }

            return true;
        }

        /// <summary>
        /// Read the XML file using the current schema.
        /// </summary>
        /// <returns><c>true</c>, if current was  read, <c>false</c> otherwise.</returns>
        protected bool
        ReadCurrent()
        {
            try
            {
                using (var xmlReader = System.Xml.XmlReader.Create(this.XMLFilename, CommonReaderSettings))
                {
                    var rootElementName = "PackageDefinition";
                    if (!xmlReader.ReadToFollowing(rootElementName))
                    {
                        Log.DebugMessage("Root element '{0}' not found in '{1}'. Xml instance may be referencing an old schema. This file will now be upgraded to the latest schema.", rootElementName, this.XMLFilename);
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
                        else if (ReadDependents(xmlReader))
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
                        else if (ReadSupportedPlatforms(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDefinitions(xmlReader))
                        {
                            // all done
                        }
                        else if (xmlReader.Name == rootElementName)
                        {
                            // should be the end element
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                throw new Exception("Expected end of root element but found '{0}'", xmlReader.Name);
                            }
                        }
                        else
                        {
                            throw new Exception("Package definition reading code failed to recognize element with name '{0}'", xmlReader.Name);
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
                throw new Exception(formatEx, "Error while reading {0}", this.XMLFilename);
            }
            catch (System.Xml.XmlException xmlEx)
            {
                // the XML does not match the schema
                throw new Exception(xmlEx, "Error while reading {0}", this.XMLFilename);
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
        public string XMLFilename
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the package name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the package version.
        /// </summary>
        /// <value>The version.</value>
        public string Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Array of package name, package version, isdefaultversion, for each dependent of the package.
        /// </summary>
        /// <value>The dependents.</value>
        public Array<System.Tuple<string, string, bool?>> Dependents
        {
            get;
            private set;
        }

        /// <summary>
        /// Array of Bam assemblies required for this package.
        /// </summary>
        /// <value>The bam assemblies.</value>
        public Array<BamAssemblyDescription> BamAssemblies
        {
            get;
            private set;
        }

        /// <summary>
        /// Array of .NET assemblies required for this package.
        /// </summary>
        /// <value>The dot net assemblies.</value>
        public Array<DotNetAssemblyDescription> DotNetAssemblies
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or set the enumerations for the platforms supported by this package.
        /// </summary>
        /// <value>The supported platforms.</value>
        public EPlatform SupportedPlatforms
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the preprocessor definitions used for compiling the package assembly.
        /// </summary>
        /// <value>The definitions.</value>
        public StringArray Definitions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the array of repositories to search for packages in.
        /// </summary>
        /// <value>The package repositories.</value>
        public StringArray PackageRepositories
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the package.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// String representation is the XML filename
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.PackageDefinition"/>.</returns>
        public override string
        ToString()
        {
            return this.XMLFilename;
        }

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
            var matchingPackages = authenticated.Where(item => item.Name == current.Name);
            if (null != current.Version)
            {
                matchingPackages = matchingPackages.Where(item => item.Version == current.Version);
            }
            if (matchingPackages.FirstOrDefault() != null)
            {
                return;
            }

            if (!current.SupportedPlatforms.Includes(OSUtilities.CurrentOS))
            {
                throw new Exception("Package {0} is not supported on {1}", current.FullName, OSUtilities.CurrentOS.ToString());
            }

            authenticated.Add(current);
            foreach (var dependent in current.Dependents)
            {
                var depName = dependent.Item1;
                var depVersion = dependent.Item2;
                var candidates = candidatePackageDefinitions.Where(item => item.Name == depName);
                if (depVersion != null)
                {
                    candidates = candidates.Where(item => item.Version == depVersion);
                }
                var candidateCount = candidates.Count();
                if (0 == candidateCount)
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendFormat("Unable to find a candidate package with name '{0}'", depName);
                    if (null != depVersion)
                    {
                        message.AppendFormat(" and version {0}", depVersion);
                    }
                    message.AppendFormat(" required by {0}", current.XMLFilename);
                    message.AppendLine();
                    var packageRepos = new StringArray();
                    Graph.Instance.PackageRepositories.ToList().ForEach(item => packageRepos.AddUnique(item));
                    message.AppendLine("Searched in the package repositories:");
                    message.AppendLine(packageRepos.ToString("\n"));
                    throw new Exception(message.ToString());
                }
                if (candidateCount > 1)
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendFormat("There are {0} identical candidate packages with name '{1}'", candidateCount, depName);
                    if (null != depVersion)
                    {
                        message.AppendFormat(" and version {0}", depVersion);
                    }
                    message.AppendFormat(" required by {0} from the following package definition files:", current.XMLFilename);
                    message.AppendLine();
                    foreach (var candidate in candidates)
                    {
                        message.AppendFormat(candidate.XMLFilename);
                        message.AppendLine();
                    }
                    var packageRepos = new StringArray();
                    Graph.Instance.PackageRepositories.ToList().ForEach(item => packageRepos.AddUnique(item));
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
                    return System.String.Format("{0}-{1}", this.Name, this.Version);
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
                builderNames.AddUnique(System.String.Format("{0}Builder", Graph.Instance.Mode));
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

        private string
        GetBamDirectory()
        {
            // package repo/package name/bam/<definition file>.xml
            var bamDir = System.IO.Path.GetDirectoryName(this.XMLFilename);
            return bamDir;
        }

        /// <summary>
        /// Get the package directory, i.e. that containing the bam folder.
        /// </summary>
        /// <returns>The package directory.</returns>
        public string
        GetPackageDirectory()
        {
            // package repo/package name/bam/<definition file>.xml
            var packageDir = System.IO.Path.GetDirectoryName(this.GetBamDirectory());
            return packageDir;
        }

        /// <summary>
        /// Get the repository directory, if it is a formal repository structure, called 'packages'
        /// or 'tests'. If it not in a formal structure, null is returned.
        /// </summary>
        /// <returns>The package repository.</returns>
        private string
        GetPackageRepository()
        {
            // package repo/package name/bam/<definition file>.xml
            var repo = System.IO.Path.GetDirectoryName(this.GetPackageDirectory());
            if (repo.EndsWith("packages") || repo.EndsWith("tests"))
            {
                return repo;
            }
            return null;
        }

        /// <summary>
        /// Get the directory that build files are written into.
        /// </summary>
        /// <returns>The build directory.</returns>
        public string
        GetBuildDirectory()
        {
            return System.IO.Path.Combine(Graph.Instance.BuildRoot, this.FullName);
        }

        private void
        ShowDependencies(
            int depth,
            Array<PackageDefinition> visitedPackages,
            string packageFormatting)
        {
            visitedPackages.Add(this);
            foreach (var dependent in this.Dependents)
            {
                var dep = Graph.Instance.Packages.First(item => item.Name == dependent.Item1 && item.Version == dependent.Item2);
                if (visitedPackages.Contains(dep))
                {
                    continue;
                }

                var formattedName = System.String.Format("{0}{1}{2}",
                    new string(' ', depth * 4),
                    dep.FullName,
                    dependent.Item3.GetValueOrDefault(false) ? "*" : System.String.Empty);

                var repo = (dep.PackageRepositories.Count > 0) ? dep.PackageRepositories[0] : "Found in " + System.IO.Path.GetDirectoryName(dep.GetPackageDirectory());

                Log.MessageAll(packageFormatting, formattedName, repo);

                if (dep.Dependents.Count > 0)
                {
                    dep.ShowDependencies(depth + 1, visitedPackages, packageFormatting);
                }
            }
        }

        /// <summary>
        /// Show a representation of the package definition file to the console.
        /// </summary>
        public void
        Show()
        {
            var packageName = this.FullName;
            var formatString = "Definition of package '{0}'";
            int dashLength = formatString.Length - 3 + packageName.Length;
            Log.MessageAll("Definition of package '{0}'", packageName);
            Log.MessageAll(new string('-', dashLength));
            if (!string.IsNullOrEmpty(this.Description))
            {
                Log.MessageAll("Description: {0}", this.Description);
            }
            Log.MessageAll("\nSupported on: {0}", Platform.ToString(this.SupportedPlatforms, ' '));
            Log.MessageAll("\nBuildAMation assemblies:");
            foreach (var assembly in this.BamAssemblies)
            {
                var minVersionNumber = assembly.MinimumVersionNumber();
                if (null != minVersionNumber)
                {
                    Log.MessageAll("\t{0} (requires version {1})", assembly.Name, minVersionNumber);
                }
                else
                {
                    Log.MessageAll("\t{0}", assembly.Name);
                }
            }
            Log.MessageAll("\nDotNet assemblies:");
            foreach (var desc in this.DotNetAssemblies)
            {
                if (null == desc.RequiredTargetFramework)
                {
                    Log.MessageAll("\t{0}", desc.Name);
                }
                else
                {
                    Log.MessageAll("\t{0} (version {1})", desc.Name, desc.RequiredTargetFramework);
                }
            }
            if (this.Definitions.Count > 0)
            {
                Log.MessageAll("\n#defines:");
                foreach (var definition in this.Definitions)
                {
                    Log.MessageAll("\t{0}", definition);
                }
            }

            if (this.PackageRepositories.Count > 0)
            {
                Log.MessageAll("\nPackage repositories to search:");
                foreach (var repo in this.PackageRepositories)
                {
                    var absoluteRepo = RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(repo);

                    Log.MessageAll("\t'{0}'\t(absolute path '{1}')", repo, absoluteRepo);
                }
            }

            if (this.Dependents.Count > 0)
            {
                Log.MessageAll("\nDependent packages (* = default version):", packageName);
                var packageFormatting = System.String.Format("{{0, -48}} {{1, 32}}");
                Log.MessageAll(packageFormatting, "Package Name", "From Repository");
                var visitedPackages = new Array<PackageDefinition>();
                this.ShowDependencies(1, visitedPackages, packageFormatting);
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
        public PackageMetaData MetaData
        {
            get;
            set;
        }

        /// <summary>
        /// List of PackageDefinitions which have stated that this package is a dependency.
        /// </summary>
        public Array<PackageDefinition> Parents
        {
            get;
            set;
        }

        /// <summary>
        /// For each Bam assembly required by this package, validate that the version it specifies is at least
        /// that for the current version of Bam. Throw an exception if the version is insufficient.
        /// </summary>
        public void
        ValidateBamAssemblyRequirements()
        {
            foreach (var asm in this.BamAssemblies)
            {
                if (asm.Name == "Bam.Core")
                {
                    var bamVersion = Graph.Instance.ProcessState.Version;
                    if (asm.MajorVersion > bamVersion.Major ||
                        asm.MinorVersion > bamVersion.Minor ||
                        asm.PatchVersion > bamVersion.Build)
                    {
                        throw new Exception("This version of BuildAMation, v{0}, does not satisfy minimum requirements v{1}.{2}.{3}, from package {4}",
                            Graph.Instance.ProcessState.VersionString,
                            asm.MajorVersion,
                            asm.MinorVersion,
                            asm.PatchVersion,
                            this.Name);
                    }
                }
                else
                {
                    Log.DebugMessage("Unknown Bam assembly, {0}", asm.Name);
                }
            }
        }
    }
}
