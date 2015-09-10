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
namespace Bam.Core
{
    public class PackageDefinitionFile
    {
        private static readonly string xmlNamespace = "Opus";

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
                // NEW style definition files
                if (args.Exception is System.Xml.Schema.XmlSchemaException)
                {
                    throw new Exception("From {0} (line {1}, position {2}):\n{3}", args.Exception.SourceUri, args.Exception.LineNumber, args.Exception.LinePosition, args.Exception.Message);
                }

                // OLD style definition files
                if (args.Exception.SourceUri.Contains(".xsd"))
                {
                    var schemaUri = new System.Uri(State.PackageDefinitionSchemaPathV2);
                    if (schemaUri.AbsolutePath != args.Exception.SourceUri)
                    {
                        throw new Exception("Duplicate schemas exist! From package definition file '{0}', and from the build '{1}'", args.Exception.SourceUri, schemaUri.AbsolutePath);
                    }
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

        public
        PackageDefinitionFile(
            string xmlFilename,
            bool validate)
        {
            this.validate = validate;
            this.XMLFilename = xmlFilename;
#if true
            this.Dependents = new Array<System.Tuple<string, string, bool?>>();
#else
            this.PackageIdentifiers = new PackageIdentifierCollection();
#endif
            this.BamAssemblies = new StringArray();
            this.DotNetAssemblies = new Array<DotNetAssemblyDescription>();
            this.SupportedPlatforms = EPlatform.All;
            this.Definitions = new StringArray();
            this.PackageRepositories = new StringArray();
            // package repo/package name/bam/<definition file>.xml
            this.PackageRepositories.Add(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(xmlFilename))));
            this.Description = string.Empty;
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

#if true
#else
            this.PackageIdentifiers.Sort();
#endif

            var document = new System.Xml.XmlDocument();
            var namespaceURI = "http://www.buildamation.com";
            var packageDefinition = document.CreateElement("PackageDefinition", namespaceURI);
            {
                var xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                var schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                var mostRecentSchemaRelativePath = State.PackageDefinitionSchemaRelativePathNameV3;
                schemaAttribute.Value = System.String.Format("{0} {1}", namespaceURI, mostRecentSchemaRelativePath);
                packageDefinition.Attributes.Append(schemaAttribute);
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
            if (this.PackageRepositories.Count > 0)
            {
                var packageRootsElement = document.CreateElement("PackageRoots", namespaceURI);

                foreach (string rootPath in this.PackageRepositories)
                {
                    var rootElement = document.CreateElement("RootDirectory", namespaceURI);
                    rootElement.SetAttribute("Path", rootPath);
                    packageRootsElement.AppendChild(rootElement);
                }

                packageDefinition.AppendChild(packageRootsElement);
            }

#if true
#else
            if (this.PackageIdentifiers.Count > 0)
            {
                var requiredPackages = document.CreateElement("RequiredPackages", namespaceURI);
                foreach (var package in this.PackageIdentifiers)
                {
                    System.Xml.XmlElement packageElement = null;

                    {
                        var node = requiredPackages.FirstChild;
                        while (node != null)
                        {
                            var attributes = node.Attributes;
                            var nameAttribute = attributes["Name"];
                            if ((null != nameAttribute) && (nameAttribute.Value == package.Name))
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
                        packageElement.SetAttribute("Name", package.Name);
                        requiredPackages.AppendChild(packageElement);
                    }
                    {
                        var packageVersionElement = document.CreateElement("Version", namespaceURI);
                        packageVersionElement.SetAttribute("Id", package.Version);

                        var platformFilter = package.PlatformFilter;
                        if (platformFilter == EPlatform.Invalid)
                        {
                            throw new Exception("Dependent '{0}' requires at least one platform filter", package.ToString());
                        }

                        if (platformFilter != EPlatform.All)
                        {
                            var platformFilterString = Platform.ToString(platformFilter, ' ');
                            var split = platformFilterString.Split(' ');
                            string conditionText = null;
                            foreach (var platform in split)
                            {
                                if (null != conditionText)
                                {
                                    conditionText += " || ";
                                }
                                conditionText += System.String.Format("'$(Platform)' == '{0}'", platform);
                            }

                            packageVersionElement.SetAttribute("Condition", conditionText);
                        }

                        if (package.IsDefaultVersion)
                        {
                            packageVersionElement.SetAttribute("Default", "true");
                        }

                        packageElement.AppendChild(packageVersionElement);
                    }
                }
                packageDefinition.AppendChild(requiredPackages);
            }
#endif

            if (this.BamAssemblies.Count > 0)
            {
                var requiredAssemblies = document.CreateElement("RequiredBamAssemblies", namespaceURI);
                foreach (var assemblyName in this.BamAssemblies)
                {
                    var assemblyElement = document.CreateElement("BamAssembly", namespaceURI);
                    assemblyElement.SetAttribute("Name", assemblyName);
                    requiredAssemblies.AppendChild(assemblyElement);
                }
                packageDefinition.AppendChild(requiredAssemblies);
            }

            if (this.DotNetAssemblies.Count > 0)
            {
                var requiredDotNetAssemblies = document.CreateElement("RequiredDotNetAssemblies", namespaceURI);
                foreach (var desc in this.DotNetAssemblies)
                {
                    var assemblyElement = document.CreateElement("DotNetAssembly", namespaceURI);
                    assemblyElement.SetAttribute("Name", desc.Name);
                    if (null != desc.RequiredTargetFramework)
                    {
                        assemblyElement.SetAttribute("RequiredTargetFramework", desc.RequiredTargetFramework);
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
                    platformElement.SetAttribute("Name", "Windows");
                    supportedPlatformsElement.AppendChild(platformElement);
                }
                if (EPlatform.Unix == (this.SupportedPlatforms & EPlatform.Unix))
                {
                    var platformElement = document.CreateElement("Platform", namespaceURI);
                    platformElement.SetAttribute("Name", "Unix");
                    supportedPlatformsElement.AppendChild(platformElement);
                }
                if (EPlatform.OSX == (this.SupportedPlatforms & EPlatform.OSX))
                {
                    var platformElement = document.CreateElement("Platform", namespaceURI);
                    platformElement.SetAttribute("Name", "OSX");
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
                    defineElement.SetAttribute("Name", define);
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
            this.Definitions.Add(packageDefinition);
            Log.DebugMessage("Package define: {0}", packageDefinition);
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

            // fall back on earlier versions of the schema
            if (this.ReadV2(xmlReaderSettings, validateSchemaLocation, validatePackageLocations))
            {
                // now write the file out using the current schema
                this.Write();
                Log.MessageAll("Package definition file '{0}' converted to use the latest schema", this.XMLFilename);
                return;
            }

            if (this.ReadV1(xmlReaderSettings))
            {
                // now write the file out using the current schema
                this.Write();
                Log.MessageAll("Package definition file '{0}' converted to use the latest schema", this.XMLFilename);
                return;
            }

            throw new Exception("An error occurred while reading a package or package definition file '{0}' does not satisfy any of the package definition schemas", this.XMLFilename);
        }

        protected EPlatform
        InterpretConditionValue(
            string condition)
        {
            // unless you apply the pattern, remove the match, apply another pattern, etc.?

            //string filter = "'$(Platform)'";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)')";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s)";
            //string pattern = @"^('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')\s([\|]{2})";
            //string pattern2 = @"^\s([\|]{2})\s";
            var pattern = new string[2];
            pattern[0] = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')";
            pattern[1] = @"([\|]{2})";
            System.Text.RegularExpressions.RegexOptions expOptions =
                System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace |
                System.Text.RegularExpressions.RegexOptions.Singleline;

            var supportedPlatforms = EPlatform.Invalid;

            int position = 0;
            int patternIndex = 0;
            for (; ; )
            {
                Core.Log.DebugMessage("Searching from pos {0} with pattern index {1}", position, patternIndex);
                if (position > condition.Length)
                {
                    Core.Log.DebugMessage("End of string");
                    break;
                }

                var exp = new System.Text.RegularExpressions.Regex(pattern[patternIndex], expOptions);
                var matches = exp.Matches(condition, position);
                if (0 == matches.Count)
                {
                    break;
                }

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Length > 0)
                    {
                        Core.Log.DebugMessage("Match '{0}'", match.Value);
                        foreach (System.Text.RegularExpressions.Group group in match.Groups)
                        {
                            Core.Log.DebugMessage("\tg '{0}' @ {1}", group.Value, group.Index);
                        }

                        if (0 == patternIndex)
                        {
                            if (match.Groups.Count != 5)
                            {
                                throw new Exception("Expected format to be '$(Platform) == '...''. Instead found '{0}'", match.Value);
                            }

                            if (match.Groups[2].Value != "Platform")
                            {
                                throw new Exception("Expected 'Platform/ in '{0}'", match.Value);
                            }

                            if (match.Groups[3].Value != "==")
                            {
                                throw new Exception("Only supporting equivalence == in '{0}'", match.Value);
                            }

                            var platform = match.Groups[4].Value;
                            var ePlatform = Platform.FromString(platform);
                            if (ePlatform == EPlatform.Invalid)
                            {
                                throw new Exception("Unrecognize platform '{0}'", platform);
                            }

                            supportedPlatforms |= ePlatform;
                        }
                        else if (1 == patternIndex)
                        {
                            if (match.Groups.Count != 2)
                            {
                                throw new Exception("Expected format to be '... || ...'. Instead found '{0}'", match.Value);
                            }

                            if (match.Groups[1].Value != "||")
                            {
                                throw new Exception("Only supporting Boolean OR || in '{0}'", match.Value);
                            }
                        }

                        position += match.Length + 1;
                        ++patternIndex;
                        patternIndex = patternIndex % 2;

                        break;
                    }
                }
            }
            Core.Log.DebugMessage("End of matches");

            return supportedPlatforms;
        }

        private bool
        ReadDescriptionV3(
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
                var packageDirectory = System.IO.Path.GetDirectoryName(this.XMLFilename);
                var absolutePackageRepoDir = Core.RelativePathUtilities.MakeRelativePathAbsoluteTo(dir, packageDirectory);
                this.PackageRepositories.Add(absolutePackageRepoDir);
            }

            return true;
        }

        private bool
        ReadPackageRootsV3(
            System.Xml.XmlReader xmlReader)
        {
            var packageRootsElementName = "PackageRoots";
            if (packageRootsElementName != xmlReader.Name)
            {
                return false;
            }

            var rootDirElementName = "RootDirectory";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == packageRootsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (rootDirElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var pathAttribute = "Path";
                        if (!xmlReader.MoveToAttribute(pathAttribute))
                        {
                            throw new Exception("Required '{0}' attribute of '{1}' node missing", pathAttribute, rootDirElementName);
                        }

                        var path = xmlReader.Value;
                        this.PackageRepositories.Add(path);

                        var packageDirectory = System.IO.Path.GetDirectoryName(this.XMLFilename);
                        var absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteTo(path, packageDirectory);
                        Core.State.PackageRoots.Add(Core.DirectoryLocation.Get(absolutePackageRoot));
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", packageRootsElementName, xmlReader.Name);
                }
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
        ReadRequiredPackagesV3(
            System.Xml.XmlReader xmlReader,
            bool validatePackageLocations)
        {
            var requiredPackagesElementName = "RequiredPackages";
            if (requiredPackagesElementName != xmlReader.Name)
            {
                return false;
            }

            var packageElementName = "Package";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredPackagesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (xmlReader.Name == packageElementName)
                {
                    var packageNameAttribute = "Name";
                    if (!xmlReader.MoveToAttribute(packageNameAttribute))
                    {
                        throw new Exception("Required attribute 'Name' of 'Package' node missing");
                    }
                    var packageName = xmlReader.Value;

                    var packageVersionElementName = "Version";
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.Name == packageElementName) &&
                            (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                        {
                            break;
                        }

                        if (xmlReader.Name == packageVersionElementName)
                        {
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                var packageVersionIdAttribute = "Id";
                                if (!xmlReader.MoveToAttribute(packageVersionIdAttribute))
                                {
                                    throw new Exception("Required 'Id' attribute of 'Version' node missing");
                                }
                                var packageVersion = xmlReader.Value;

#if true
#else
                                var platformFilter = EPlatform.All;
                                var packageConditionAttribute = "Condition";
                                if (xmlReader.MoveToAttribute(packageConditionAttribute))
                                {
                                    var conditionValue = xmlReader.Value;
                                    platformFilter = this.InterpretConditionValue(conditionValue);
                                }

                                var isDefaultVersion = false;
                                var packageVersionDefaultAttribute = "Default";
                                if (xmlReader.MoveToAttribute(packageVersionDefaultAttribute))
                                {
                                    var isDefault = System.Xml.XmlConvert.ToBoolean(xmlReader.Value);
                                    isDefaultVersion = isDefault;
                                }

                                var id = new PackageIdentifier(packageName, packageVersion, validatePackageLocations);
                                id.PlatformFilter = platformFilter;
                                id.IsDefaultVersion = isDefaultVersion;
                                this.PackageIdentifiers.Add(id);
#endif
                            }
                        }
                        else
                        {
                            throw new Exception("Unexpected element");
                        }
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", requiredPackagesElementName, xmlReader.Name);
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
        ReadRequiredBamAssembliesV3(
            System.Xml.XmlReader xmlReader)
        {
            var requiredBamAssembliesElementName = "RequiredBamAssemblies";
            if (requiredBamAssembliesElementName != xmlReader.Name)
            {
                return false;
            }

            var opusAssemblyElementName = "BamAssembly";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredBamAssembliesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (opusAssemblyElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var assemblyNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(assemblyNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of 'BamAssembly' node missing");
                        }
                        var assemblyName = xmlReader.Value;

                        // conversion from Opus to BuildAMation
                        if (assemblyName.StartsWith("Opus."))
                        {
                            assemblyName = assemblyName.Replace("Opus.", "Bam.");
                        }

                        this.BamAssemblies.Add(assemblyName);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", requiredBamAssembliesElementName, xmlReader.Name);
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
        ReadRequiredDotNetAssembliesV3(
            System.Xml.XmlReader xmlReader)
        {
            var requiredDotNetAssembliesElementName = "RequiredDotNetAssemblies";
            if (requiredDotNetAssembliesElementName != xmlReader.Name)
            {
                return false;
            }

            var dotNetAssemblyElementName = "DotNetAssembly";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredDotNetAssembliesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (dotNetAssemblyElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var assemblyNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(assemblyNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:DotNetAssembly' node missing", xmlNamespace);
                        }
                        var assemblyName = xmlReader.Value;

                        var desc = new DotNetAssemblyDescription(assemblyName);

                        var assemblyRequiredTargetFrameworkNameAttribute = "RequiredTargetFramework";
                        if (xmlReader.MoveToAttribute(assemblyRequiredTargetFrameworkNameAttribute))
                        {
                            desc.RequiredTargetFramework = xmlReader.Value;
                        }

                        this.DotNetAssemblies.Add(desc);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", requiredDotNetAssembliesElementName, xmlReader.Name);
                }
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
                        this.SupportedPlatforms |= EPlatform.Unix;
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
        ReadSupportedPlatformsV3(
            System.Xml.XmlReader xmlReader)
        {
            var supportedPlatformsElementName = "SupportedPlatforms";
            if (supportedPlatformsElementName != xmlReader.Name)
            {
                return false;
            }

            this.SupportedPlatforms = EPlatform.Invalid;
            var platformElementName = "Platform";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == supportedPlatformsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (platformElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var platformNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(platformNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:Platform' node missing", xmlNamespace);
                        }

                        var platformName = xmlReader.Value;
                        if ("Windows" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.Windows;
                        }
                        if ("Unix" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.Unix;
                        }
                        if ("OSX" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.OSX;
                        }
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", supportedPlatformsElementName, xmlReader.Name);
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

        private bool
        ReadDefinitionsV3(
            System.Xml.XmlReader xmlReader)
        {
            var definitionsElementName = "Definitions";
            if (definitionsElementName != xmlReader.Name)
            {
                return false;
            }

            var defineElementName = "Definition";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == definitionsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (defineElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var defineNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(defineNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:Definition' node missing", xmlNamespace);
                        }

                        var definition = xmlReader.Value;

                        this.Definitions.Add(definition);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", definitionsElementName, xmlReader.Name);
                }
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
                        if (ReadDescriptionV3(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadPackageRepositories(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadPackageRootsV3(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDependents(xmlReader, validatePackageLocations))
                        {
                            // all done
                        }
                        else if (ReadRequiredPackagesV3(xmlReader, validatePackageLocations))
                        {
                            // all done
                        }
                        else if (ReadBamAssemblies(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadRequiredBamAssembliesV3(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDotNetAssemblies(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadRequiredDotNetAssembliesV3(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadSupportedPlatforms(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadSupportedPlatformsV3(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDefinitions(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDefinitionsV3(xmlReader))
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
                throw ex;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        private bool
        ReadPackageRootsV2(
            System.Xml.XmlReader xmlReader)
        {
            var packageRootsElementName = System.String.Format("{0}:PackageRoots", xmlNamespace);
            if (packageRootsElementName != xmlReader.Name)
            {
                return false;
            }

            var rootDirElementName = System.String.Format("{0}:RootDirectory", xmlNamespace);
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == packageRootsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (rootDirElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var pathAttribute = "Path";
                        if (!xmlReader.MoveToAttribute(pathAttribute))
                        {
                            throw new Exception("Required '{0}' attribute of '{1}' node missing", pathAttribute, pathAttribute, rootDirElementName);
                        }

                        var path = xmlReader.Value;
                        this.PackageRepositories.Add(path);

                        var absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(path);
                        Core.State.PackageRoots.Add(Core.DirectoryLocation.Get(absolutePackageRoot));
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", packageRootsElementName, xmlReader.Name);
                }
            }

            return true;
        }

        private bool
        ReadRequiredPackagesV2(
            System.Xml.XmlReader xmlReader,
            bool validatePackageLocations)
        {
            var requiredPackagesElementName = System.String.Format("{0}:RequiredPackages", xmlNamespace);
            if (requiredPackagesElementName != xmlReader.Name)
            {
                return false;
            }

            var packageElementName = System.String.Format("{0}:Package", xmlNamespace);
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredPackagesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (xmlReader.Name == packageElementName)
                {
                    var packageNameAttribute = "Name";
                    if (!xmlReader.MoveToAttribute(packageNameAttribute))
                    {
                        throw new Exception("Required attribute 'Name' of 'Package' node missing");
                    }
                    var packageName = xmlReader.Value;

                    var packageVersionElementName = System.String.Format("{0}:Version", xmlNamespace);
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.Name == packageElementName) &&
                            (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                        {
                            break;
                        }

                        if (xmlReader.Name == packageVersionElementName)
                        {
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                var packageVersionIdAttribute = "Id";
                                if (!xmlReader.MoveToAttribute(packageVersionIdAttribute))
                                {
                                    throw new Exception("Required 'Id' attribute of 'Version' node missing");
                                }
                                var packageVersion = xmlReader.Value;

#if true
#else
                                var platformFilter = EPlatform.All;
                                var packageConditionAttribute = "Condition";
                                if (xmlReader.MoveToAttribute(packageConditionAttribute))
                                {
                                    var conditionValue = xmlReader.Value;
                                    platformFilter = this.InterpretConditionValue(conditionValue);
                                }

                                var isDefaultVersion = false;
                                var packageVersionDefaultAttribute = "Default";
                                if (xmlReader.MoveToAttribute(packageVersionDefaultAttribute))
                                {
                                    var isDefault = System.Xml.XmlConvert.ToBoolean(xmlReader.Value);
                                    isDefaultVersion = isDefault;
                                }

                                var id = new PackageIdentifier(packageName, packageVersion, validatePackageLocations);
                                id.PlatformFilter = platformFilter;
                                id.IsDefaultVersion = isDefaultVersion;
                                this.PackageIdentifiers.Add(id);
#endif
                            }
                        }
                        else
                        {
                            throw new Exception("Unexpected element");
                        }
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", requiredPackagesElementName, xmlReader.Name);
                }
            }

            return true;
        }

        private bool
        ReadRequiredOpusAssembliesV2(
            System.Xml.XmlReader xmlReader)
        {
            var requiredOpusAssembliesElementName = System.String.Format("{0}:RequiredOpusAssemblies", xmlNamespace);
            if (requiredOpusAssembliesElementName != xmlReader.Name)
            {
                return false;
            }

            var opusAssemblyElementName = System.String.Format("{0}:OpusAssembly", xmlNamespace);
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredOpusAssembliesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (opusAssemblyElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var assemblyNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(assemblyNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:OpusAssembly' node missing", xmlNamespace);
                        }
                        var assemblyName = xmlReader.Value;

                        // conversion from Opus to BuildAMation
                        if (assemblyName.StartsWith("Opus."))
                        {
                            assemblyName = assemblyName.Replace("Opus.", "Bam.");
                        }

                        this.BamAssemblies.Add(assemblyName);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", requiredOpusAssembliesElementName, xmlReader.Name);
                }
            }

            return true;
        }

        private bool
        ReadRequiredDotNetAssembliesV2(
            System.Xml.XmlReader xmlReader)
        {
            var requiredDotNetAssembliesElementName = System.String.Format("{0}:RequiredDotNetAssemblies", xmlNamespace);
            if (requiredDotNetAssembliesElementName != xmlReader.Name)
            {
                return false;
            }

            var dotNetAssemblyElementName = System.String.Format("{0}:DotNetAssembly", xmlNamespace);
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == requiredDotNetAssembliesElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (dotNetAssemblyElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var assemblyNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(assemblyNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:DotNetAssembly' node missing", xmlNamespace);
                        }
                        var assemblyName = xmlReader.Value;

                        var desc = new DotNetAssemblyDescription(assemblyName);

                        var assemblyRequiredTargetFrameworkNameAttribute = "RequiredTargetFramework";
                        if (xmlReader.MoveToAttribute(assemblyRequiredTargetFrameworkNameAttribute))
                        {
                            desc.RequiredTargetFramework = xmlReader.Value;
                        }

                        this.DotNetAssemblies.Add(desc);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", requiredDotNetAssembliesElementName, xmlReader.Name);
                }
            }

            return true;
        }

        private bool
        ReadSupportedPlatformsV2(
            System.Xml.XmlReader xmlReader)
        {
            var supportedPlatformsElementName = System.String.Format("{0}:SupportedPlatforms", xmlNamespace);
            if (supportedPlatformsElementName != xmlReader.Name)
            {
                return false;
            }

            this.SupportedPlatforms = EPlatform.Invalid;
            var platformElementName = System.String.Format("{0}:Platform", xmlNamespace);
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == supportedPlatformsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (platformElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var platformNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(platformNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:Platform' node missing", xmlNamespace);
                        }

                        var platformName = xmlReader.Value;
                        if ("Windows" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.Windows;
                        }
                        if ("Unix" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.Unix;
                        }
                        if ("OSX" == platformName)
                        {
                            this.SupportedPlatforms |= EPlatform.OSX;
                        }
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", supportedPlatformsElementName, xmlReader.Name);
                }
            }

            return true;
        }

        private bool
        ReadDefinitionsV2(
            System.Xml.XmlReader xmlReader)
        {
            var definitionsElementName = System.String.Format("{0}:Definitions", xmlNamespace);
            if (definitionsElementName != xmlReader.Name)
            {
                return false;
            }

            var defineElementName = System.String.Format("{0}:Definition", xmlNamespace);
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == definitionsElementName) &&
                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                {
                    break;
                }

                if (defineElementName == xmlReader.Name)
                {
                    if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        var defineNameAttribute = "Name";
                        if (!xmlReader.MoveToAttribute(defineNameAttribute))
                        {
                            throw new Exception("Required 'Name' attribute of '{0}:Definition' node missing", xmlNamespace);
                        }

                        var definition = xmlReader.Value;

                        this.Definitions.Add(definition);
                    }
                }
                else
                {
                    throw new Exception("Unexpected child element of '{0}'. Found '{1}'", definitionsElementName, xmlReader.Name);
                }
            }

            return true;
        }

        protected bool
        ReadV2(
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
                    string rootElementName = System.String.Format("{0}:PackageDefinition", xmlNamespace);
                    if (!xmlReader.ReadToFollowing(rootElementName))
                    {
                        Log.DebugMessage("Root element '{0}' not found in '{1}'. Xml instance may be referencing an old {2} schema. This file will now be upgraded to the latest schema.", rootElementName, this.XMLFilename, xmlNamespace);
                        return false;
                    }

                    while (xmlReader.Read())
                    {
                        if (ReadPackageRootsV2(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadRequiredPackagesV2(xmlReader, validatePackageLocations))
                        {
                            // all done
                        }
                        else if (ReadRequiredOpusAssembliesV2(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadRequiredDotNetAssembliesV2(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadSupportedPlatformsV2(xmlReader))
                        {
                            // all done
                        }
                        else if (ReadDefinitionsV2(xmlReader))
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
                throw ex;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        public bool
        ReadV1(
            System.Xml.XmlReaderSettings readerSettings)
        {
            try
            {
                var settings = readerSettings.Clone();
                settings.Schemas.Add(null, State.PackageDefinitionSchemaPath);

                using (var xmlReader = System.Xml.XmlReader.Create(this.XMLFilename, settings))
                {
                    while (xmlReader.Read())
                    {
                        if (System.Xml.XmlNodeType.XmlDeclaration == xmlReader.NodeType)
                        {
                            // do nothing
                        }
                        else if (System.Xml.XmlNodeType.Whitespace == xmlReader.NodeType)
                        {
                            // do nothing
                        }
                        else if (System.Xml.XmlNodeType.Element == xmlReader.NodeType)
                        {
                            if ("RequiredPackages" == xmlReader.Name)
                            {
                                while (xmlReader.Read())
                                {
                                    if (System.Xml.XmlNodeType.Whitespace == xmlReader.NodeType)
                                    {
                                        // do nothing
                                    }
                                    else if (System.Xml.XmlNodeType.EndElement == xmlReader.NodeType)
                                    {
                                        if ("RequiredPackages" == xmlReader.Name)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            throw new Exception("Unexpected EndElement");
                                        }
                                    }
                                    else if (System.Xml.XmlNodeType.Element == xmlReader.NodeType)
                                    {
                                        if ("Package" == xmlReader.Name)
                                        {
                                            if (xmlReader.HasAttributes)
                                            {
                                                xmlReader.MoveToAttribute("Name");
                                                var packageName = xmlReader.Value;
                                                xmlReader.MoveToAttribute("Version");
                                                var packageVersion = xmlReader.Value;

#if true
#else
                                                var id = new PackageIdentifier(packageName, packageVersion);
                                                id.PlatformFilter = EPlatform.All;
                                                this.PackageIdentifiers.Add(id);
#endif

                                                xmlReader.MoveToElement();
                                            }
                                            else
                                            {
                                                throw new Exception("Package element has no attributes");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!xmlReader.EOF)
                    {
                        throw new System.Xml.XmlException("Failed to read all of file");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.MessageAll("Package Definition Read of Schema v1 failed: '{0}'", ex.Message);
                return false;
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

                this.DotNetAssemblies.AddRange(new [] { systemDesc, systemXmlDesc, systemCoreDesc });
            }

            // supported on all platforms
            this.SupportedPlatforms = EPlatform.All;

            return true;
        }

#if true
#else
        public void
        AddRequiredPackage(
            PackageIdentifier idToAdd)
        {
            foreach (var id in this.PackageIdentifiers)
            {
                if (id.Match(idToAdd, false))
                {
                    throw new Exception("Package '{0}' is already included in the dependency file", id.ToString());
                }
            }

            this.PackageIdentifiers.Add(idToAdd);
            Log.Info("Added dependency '{0}' from root '{1}'", idToAdd.ToString(), idToAdd.Root.GetSingleRawPath());
        }
#endif

#if true
#else
        public bool
        RemovePackage(
            PackageIdentifier idToRemove)
        {
            PackageIdentifier idToRemoveReally = null;
            foreach (var id in this.PackageIdentifiers)
            {
                if (id.Match(idToRemove, false))
                {
                    idToRemoveReally = id;
                    break;
                }
            }

            if (null != idToRemoveReally)
            {
                this.PackageIdentifiers.Remove(idToRemoveReally);
                var root = idToRemove.Root;
                if (root != null)
                {
                    Log.Info("Removed dependency '{0}' from root '{1}'", idToRemove.ToString(), root.GetSingleRawPath());
                }
                else
                {
                    Log.Info("Removed dependency '{0}' from undefined root", idToRemove.ToString());
                }
                return true;
            }
            else
            {
                Log.Info("Could not find reference to package '{0}' to remove", idToRemove.ToString());
                return false;
            }
        }
#endif

#if true
#else
        public Array<PackageIdentifier>
        RecursiveDependentIdentifiers()
        {
            var ids = new Array<PackageIdentifier>();
            foreach (var id in this.PackageIdentifiers)
            {
                ids.AddUnique(id);
                var definition = id.Definition;
                if (null == definition)
                {
                    continue;
                }
                ids.AddRangeUnique(definition.RecursiveDependentIdentifiers());
            }
            return ids;
        }
#endif

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

#if true
        public Array<System.Tuple<string, string, bool?>> Dependents
        {
            get;
            private set;
        }
#else
        public PackageIdentifierCollection PackageIdentifiers
        {
            get;
            private set;
        }
#endif

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
                    State.PackageRoots.ToList().ForEach(item => packageRepos.AddUnique(item.AbsolutePath));
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
                    message.AppendLine();
                    foreach (var candidate in candidates)
                    {
                        message.AppendFormat(candidate.FullName);
                        message.AppendLine();
                    }
                    var packageRepos = new StringArray();
                    State.PackageRoots.ToList().ForEach(item => packageRepos.AddUnique(item.AbsolutePath));
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
            var bamDir = System.IO.Path.GetDirectoryName(this.XMLFilename);
            var scriptDir = System.IO.Path.Combine(bamDir, "Scripts");
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
                builderNames.AddUnique(System.String.Format("{0}Builder", State.BuilderName));
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
            var projectDir = System.IO.Path.Combine(this.GetPackageDirectory(), "BamProject");
            var projectPathname = System.IO.Path.Combine(projectDir, this.FullName) + ".csproj";
            return projectPathname;
        }

        public string
        GetPackageDirectory()
        {
            var packageDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(this.XMLFilename));
            return packageDir;
        }
    }
}
