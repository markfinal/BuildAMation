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
using System.Linq;
using Bam.Core.V2;
namespace Bam.Core
{
    public class PackageDefinitionFile
    {
        private bool validate;

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

        private void
        Validate()
        {
            var settings = new System.Xml.XmlReaderSettings();
            settings.ValidationType = System.Xml.ValidationType.Schema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(ValidationCallBack);
            settings.XmlResolver = new XmlResolver();

            // Create the XmlReader object.
            using (var reader = System.Xml.XmlReader.Create(this.XMLFilename, settings))
            {
                // Parse the file.
                while (reader.Read()) ;
            }
        }

        private void
        Initialize(
            string xmlFilename,
            bool validate)
        {
            this.validate = validate;
            this.XMLFilename = xmlFilename;
            this.Dependents = new Array<System.Tuple<string, string, bool?>>();
            this.BamAssemblies = new StringArray();
            this.DotNetAssemblies = new Array<DotNetAssemblyDescription>();
            this.SupportedPlatforms = EPlatform.All;
            this.Definitions = new StringArray();
            this.PackageRepositories = new StringArray();
            this.PackageRepositories.Add(this.GetPackageRepository());
            this.Description = string.Empty;
        }

        public
        PackageDefinitionFile(
            string xmlFilename,
            bool validate)
        {
            this.Initialize(xmlFilename, validate);
        }

        public
        PackageDefinitionFile(
            string bamDirectory,
            string name,
            string version)
        {
            var definitionName = (null != version) ? System.String.Format("{0}-{1}.xml", name, version) : name + ".xml";
            var xmlFilename = System.IO.Path.Combine(bamDirectory, definitionName);
            this.Initialize(xmlFilename, validate);
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
            this.BamAssemblies.Add("Bam.Core");

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
            var namespaceURI = "http://www.buildamation.com";
            var packageDefinition = document.CreateElement("PackageDefinition", namespaceURI);
            {
                var xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                var schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                var mostRecentSchemaRelativePath = State.PackageDefinitionSchemaRelativePath;
                schemaAttribute.Value = System.String.Format("{0} {1}", namespaceURI, mostRecentSchemaRelativePath);
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

            // package repositories - don't write out the repo that this package resides in
            var packageRepos = new StringArray(this.PackageRepositories);
            packageRepos.Remove(this.GetPackageRepository());
            if (packageRepos.Count > 0)
            {
                var packageRootsElement = document.CreateElement("PackageRepositories", namespaceURI);
                var bamDir = this.GetBamDirectory() + System.IO.Path.DirectorySeparatorChar; // slash added to make it look like a directory
                foreach (string repo in packageRepos)
                {
                    var relativePackageRepo = Core.RelativePathUtilities.GetPath(repo, bamDir);
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
                foreach (var assemblyName in this.BamAssemblies)
                {
                    var assemblyElement = document.CreateElement("BamAssembly", namespaceURI);
                    assemblyElement.SetAttribute("name", assemblyName);
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

            if (this.validate)
            {
                this.Validate();
            }
        }

        public void
        Read(
            bool validateSchemaLocation)
        {
            this.Read(validateSchemaLocation, true);

            string packageDefinition = null;
            if (null != this.Version)
            {
                packageDefinition = System.String.Format("D_PACKAGE_{0}_{1}", this.Name, this.Version.Replace('.', '_').Replace('-', '_')).ToUpper();
            }
            else
            {
                packageDefinition = System.String.Format("D_PACKAGE_{0}", this.Name).ToUpper();
            }
            this.Definitions.AddUnique(packageDefinition);
        }

        public void
        Read(
            bool validateSchemaLocation,
            bool validatePackageLocations)
        {
            var xmlReaderSettings = new System.Xml.XmlReaderSettings();
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            if (this.validate)
            {
                xmlReaderSettings.ValidationType = System.Xml.ValidationType.Schema;
            }
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;
            xmlReaderSettings.XmlResolver = new XmlResolver();

            // try reading the current schema version first
            if (this.ReadCurrent(xmlReaderSettings, validateSchemaLocation, validatePackageLocations))
            {
                if (State.ForceDefinitionFileUpdate)
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
                var absolutePackageRepoDir = Core.RelativePathUtilities.MakeRelativePathAbsoluteTo(dir, bamDir);
                this.PackageRepositories.Add(absolutePackageRepoDir);
            }

            return true;
        }

        private bool
        ReadDependents(
            System.Xml.XmlReader xmlReader,
            bool validatePackageLocations)
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

                var assembly = xmlReader.GetAttribute("name");
                this.BamAssemblies.AddUnique(assembly);
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

        protected bool
        ReadCurrent(
            System.Xml.XmlReaderSettings readerSettings,
            bool validateSchemaLocation,
            bool validatePackageLocations)
        {
            try
            {
                var settings = readerSettings.Clone();
                if (this.validate)
                {
                    if (validateSchemaLocation)
                    {
                        settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                    }
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                }

                using (var xmlReader = System.Xml.XmlReader.Create(this.XMLFilename, settings))
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
                        else if (ReadDependents(xmlReader, validatePackageLocations))
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
            catch (System.FormatException ex)
            {
                // format of the XML is wrong
                throw ex;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        public string XMLFilename
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public Array<System.Tuple<string, string, bool?>> Dependents
        {
            get;
            private set;
        }

        public StringArray BamAssemblies
        {
            get;
            private set;
        }

        public Array<DotNetAssemblyDescription> DotNetAssemblies
        {
            get;
            private set;
        }

        public EPlatform SupportedPlatforms
        {
            get;
            set;
        }

        public StringArray Definitions
        {
            get;
            set;
        }

        public StringArray PackageRepositories
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public override string
        ToString()
        {
            return this.XMLFilename;
        }

        public static void
        ResolveDependencies(
            PackageDefinitionFile current,
            Array<PackageDefinitionFile> authenticated,
            Array<PackageDefinitionFile> candidatePackageDefinitions)
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
                    message.AppendLine();
                    var packageRepos = new StringArray();
                    State.PackageRepositories.ToList().ForEach(item => packageRepos.AddUnique(item));
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
                    message.AppendLine(" from the following package definition files:");
                    foreach (var candidate in candidates)
                    {
                        message.AppendFormat(candidate.XMLFilename);
                        message.AppendLine();
                    }
                    var packageRepos = new StringArray();
                    State.PackageRepositories.ToList().ForEach(item => packageRepos.AddUnique(item));
                    message.AppendLine("Found in the package repositories:");
                    message.AppendLine(packageRepos.ToString("\n"));
                    throw new Exception(message.ToString());
                }

                ResolveDependencies(candidates.ElementAt(0), authenticated, candidatePackageDefinitions);
            }
        }

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
                foreach (var package in V2.Graph.Instance.Packages)
                {
                    if (!BuilderUtilities.IsBuilderPackage(package.Name))
                    {
                        continue;
                    }
                    builderNames.Add(package.Name);
                }
            }
            else
            {
                builderNames.AddUnique(System.String.Format("{0}Builder", State.BuildMode));
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

        public string
        GetDebugPackageProjectPathname()
        {
            var projectDir = System.IO.Path.Combine(this.GetPackageDirectory(), "PackageDebug");
            var projectPathname = System.IO.Path.Combine(projectDir, this.FullName) + ".csproj";
            return projectPathname;
        }

        private string
        GetBamDirectory()
        {
            // package repo/package name/bam/<definition file>.xml
            var bamDir = System.IO.Path.GetDirectoryName(this.XMLFilename);
            return bamDir;
        }

        public string
        GetPackageDirectory()
        {
            // package repo/package name/bam/<definition file>.xml
            var packageDir = System.IO.Path.GetDirectoryName(this.GetBamDirectory());
            return packageDir;
        }

        private string
        GetPackageRepository()
        {
            // package repo/package name/bam/<definition file>.xml
            var repo = System.IO.Path.GetDirectoryName(this.GetPackageDirectory());
            return repo;
        }

        public string
        GetBuildDirectory()
        {
            return System.IO.Path.Combine(State.BuildRoot, this.FullName);
        }

        private void
        ShowDependencies(
            int depth,
            Array<PackageDefinitionFile> visitedPackages)
        {
            visitedPackages.Add(this);
            foreach (var dependent in this.Dependents)
            {
                var dep = V2.Graph.Instance.Packages.Where(item => item.Name == dependent.Item1 && item.Version == dependent.Item2).ElementAt(0);
                if (visitedPackages.Contains(dep))
                {
                    continue;
                }

                Log.MessageAll("{0}{1}{2} (repo: '{3}')",
                    new string('\t', depth),
                    dep.FullName,
                    dependent.Item3.GetValueOrDefault(false) ? "*" : System.String.Empty,
                    dep.PackageRepositories[0]);

                if (dep.Dependents.Count > 0)
                {
                    dep.ShowDependencies(depth + 1, visitedPackages);
                }
            }
        }

        public void
        Show()
        {
            var packageName = this.FullName;
            var formatString = "Definition of package '{0}'";
            int dashLength = formatString.Length - 3 + packageName.Length;
            Core.Log.MessageAll("Definition of package '{0}'", packageName);
            Core.Log.MessageAll(new string('-', dashLength));
            if (!string.IsNullOrEmpty(this.Description))
            {
                Core.Log.MessageAll("Description: {0}", this.Description);
            }
            Core.Log.MessageAll("\nSupported on: {0}", Core.Platform.ToString(this.SupportedPlatforms, ' '));
            Core.Log.MessageAll("\nBuildAMation assemblies:");
            foreach (var assembly in this.BamAssemblies)
            {
                Core.Log.MessageAll("\t{0}", assembly);
            }
            Core.Log.MessageAll("\nDotNet assemblies:");
            foreach (var desc in this.DotNetAssemblies)
            {
                if (null == desc.RequiredTargetFramework)
                {
                    Core.Log.MessageAll("\t{0}", desc.Name);
                }
                else
                {
                    Core.Log.MessageAll("\t{0} (version {1})", desc.Name, desc.RequiredTargetFramework);
                }
            }
            if (this.Definitions.Count > 0)
            {
                Core.Log.MessageAll("\n#defines:");
                foreach (var definition in this.Definitions)
                {
                    Core.Log.MessageAll("\t{0}", definition);
                }
            }

            if (this.PackageRepositories.Count > 0)
            {
                Core.Log.MessageAll("\nExtra package repositories to search:");
                foreach (var repo in this.PackageRepositories)
                {
                    var absoluteRepo = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(repo);

                    Core.Log.MessageAll("\t'{0}'\t(absolute path '{1}')", repo, absoluteRepo);
                }
            }

            if (this.Dependents.Count > 0)
            {
                Core.Log.MessageAll("\nDependent packages (* = default version):", packageName);
                var visitedPackages = new Array<PackageDefinitionFile>();
                this.ShowDependencies(1, visitedPackages);
            }
            else
            {
                Core.Log.MessageAll("\nNo dependent packages", packageName);
            }
        }
    }
}
