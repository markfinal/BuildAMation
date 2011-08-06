// <copyright file="PackageDefinitionFile.cs" company="Mark Final">
//  Opus.Core
// </copyright>
// <summary>Opus package definition XML file</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    internal class MyXmlResolver : System.Xml.XmlResolver
    {
        internal MyXmlResolver()
            : base()
        {
        }

        public override System.Net.ICredentials Credentials
        {
            set { throw new System.NotImplementedException(); }
        }

        public override object GetEntity(System.Uri absoluteUri, string role, System.Type ofObjectToReturn)
        {
            if (absoluteUri.IsFile)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(absoluteUri.LocalPath);
                return reader.BaseStream;
            }
            else
            {
                if ("http://code.google.com/p/opus" == absoluteUri.ToString())
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(State.OpusPackageDependencySchemaPathNameV2);
                    return reader.BaseStream;
                }

                throw new System.Xml.XmlException(System.String.Format("Did not understand non-file URI '{0}'", absoluteUri.ToString()));
            }
        }
    }

    public class PackageDefinitionFile
    {
        private Array<PackageIdentifier> packageIds;
        private string xmlFilename;
        private bool validate;
        
        private static void ValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
            string message = null;
            if (args.Severity == System.Xml.Schema.XmlSeverityType.Warning)
            {
                message += System.String.Format("\tWarning: Matching schema not found (" + args.Exception.GetType().ToString() + ").  No validation occurred." + args.Message);
            }
            else
            {
                message += System.String.Format("\tValidation error: " + args.Message);
            }
            message += System.String.Format("\nAt '" + args.Exception.SourceUri + "', line " + args.Exception.LineNumber + ", position " + args.Exception.LinePosition);

            throw new System.Xml.XmlException(message);
        }
        
        private void Validate()
        {
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
            settings.ValidationType = System.Xml.ValidationType.Schema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(ValidationCallBack);
            settings.XmlResolver = new MyXmlResolver();
            settings.Schemas.Add(null, State.OpusPackageDependencySchemaPathNameV2);
    
            // Create the XmlReader object.
            System.Xml.XmlReader reader = System.Xml.XmlReader.Create(this.xmlFilename, settings);
    
            // Parse the file. 
            while (reader.Read());
        }
        
        public PackageDefinitionFile(string xmlFilename, bool validate)
        {
            this.validate = validate;
            this.xmlFilename = xmlFilename;
            this.packageIds = new Array<PackageIdentifier>();
        }

        public void Write()
        {
            if (System.IO.File.Exists(this.xmlFilename))
            {
                System.IO.FileAttributes attributes = System.IO.File.GetAttributes(this.xmlFilename);
                if (0 != (attributes & System.IO.FileAttributes.ReadOnly))
                {
                    throw new Exception(System.String.Format("File '{0}' cannot be written to as it is read only", this.xmlFilename), false);
                }
            }

            this.packageIds.Sort();

            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            string targetNamespace = "Opus";
            string namespaceURI = "http://code.google.com/p/opus";
            System.Xml.XmlElement packageDefinition = document.CreateElement(targetNamespace, "PackageDefinition", namespaceURI);
            {
                string xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                System.Xml.XmlAttribute schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                var schemaPathUri = new System.Uri(State.OpusPackageDependencySchemaPathNameV2);
                schemaAttribute.Value = System.String.Format("{0} {1}", namespaceURI, schemaPathUri.AbsoluteUri);
                packageDefinition.Attributes.Append(schemaAttribute);
            }
            document.AppendChild(packageDefinition);

            if (this.packageIds.Count > 0)
            {
                System.Xml.XmlElement requiredPackages = document.CreateElement(targetNamespace, "RequiredPackages", namespaceURI);
                foreach (PackageIdentifier package in this.packageIds)
                {
                    System.Xml.XmlElement packageElement = document.CreateElement(targetNamespace, "Package", namespaceURI);
                    packageElement.SetAttribute("Name", package.Name);
                    {
                        System.Xml.XmlElement packageVersionElement = document.CreateElement(targetNamespace, "Version", namespaceURI);
                        packageVersionElement.SetAttribute("Id", package.Version);
                        //packageVersionElement.SetAttribute("Condition", "'$(Platform)' == 'win*' || '$(Platform)' == 'unix*'");
                        packageElement.AppendChild(packageVersionElement);
                    }
                    requiredPackages.AppendChild(packageElement);
                }
                packageDefinition.AppendChild(requiredPackages);
            }

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;
            xmlWriterSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;

            using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(this.xmlFilename, xmlWriterSettings))
            {
                document.WriteTo(xmlWriter);
            }

            if (this.validate)
            {
                this.Validate();
            }
        }

        public void Read()
        {
            System.Xml.XmlReaderSettings xmlReaderSettings = new System.Xml.XmlReaderSettings();
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            if (this.validate)
            {
                xmlReaderSettings.ValidationType = System.Xml.ValidationType.Schema;
                xmlReaderSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
                xmlReaderSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            }
            xmlReaderSettings.Schemas = new System.Xml.Schema.XmlSchemaSet();
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

            // try reading the current schema version first
            if (this.ReadCurrent(xmlReaderSettings))
            {
                return;
            }

            // fall back on earlier versions of the schema
            if (this.ReadV1(xmlReaderSettings))
            {
                // now write the file out using the curent schema
                this.Write();

                Log.Info("Converted package definition file '{0}' to the current schema", this.xmlFilename);

                return;
            }

            throw new Exception(System.String.Format("An error occurred while reading a package or package definition file '{0}' does not satisfy any of the Opus schemas", this.xmlFilename));
        }

        protected void InterpretConditionValue(string condition)
        {
            // unless you apply the pattern, remove the match, apply another pattern, etc.?

            //string filter = "'$(Platform)'";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)')";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s)";
            //string pattern = @"^('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')";
            //string pattern = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')\s([\|]{2})";
            //string pattern2 = @"^\s([\|]{2})\s";
            string[] pattern = new string[2];
            pattern[0] = @"('\$\(([A-Za-z0-9]+)\)'\s([=]{2})\s'([A-za-z0-9\*]+)')";
            pattern[1] = @"([\|]{2})";
            System.Text.RegularExpressions.RegexOptions expOptions =
                System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace |
                System.Text.RegularExpressions.RegexOptions.Singleline;

            int position = 0;
            int patternIndex = 0;
            for (; ; )
            {
                Core.Log.MessageAll("Searching from pos {0} with pattern index {1}", position, patternIndex);
                if (position > condition.Length)
                {
                    Core.Log.MessageAll("End of string");
                    break;
                }

                System.Text.RegularExpressions.Regex exp = new System.Text.RegularExpressions.Regex(pattern[patternIndex], expOptions);
                System.Text.RegularExpressions.MatchCollection matches = exp.Matches(condition, position);
                if (0 == matches.Count)
                {
                    break;
                }

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Length > 0)
                    {
                        Core.Log.MessageAll("Match '{0}'", match.Value);
                        foreach (System.Text.RegularExpressions.Group group in match.Groups)
                        {
                            Core.Log.MessageAll("\tg '{0}' @ {1}", group.Value, group.Index);
                        }
                        position += match.Length + 1;
                        ++patternIndex;
                        patternIndex = patternIndex % 2;

                        break;
                    }
                }
            }
            Core.Log.MessageAll("End of matches");
        }

        protected bool ReadCurrent(System.Xml.XmlReaderSettings readerSettings)
        {
            try
            {
                System.Xml.XmlReaderSettings settings = readerSettings.Clone();
                settings.Schemas.Add(null, State.OpusPackageDependencySchemaPathNameV2);
                if (this.validate)
                {
                    settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                }

                using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(this.xmlFilename, settings))
                {
                    string rootElementName = "Opus:PackageDefinition";
                    if (!xmlReader.ReadToFollowing(rootElementName))
                    {
                        Log.DebugMessage("Root element '{0}' not found in '{1}'. Xml instance may be referencing an old schema and will be upgraded.", rootElementName, this.xmlFilename);
                        return false;
                    }

                    string requiredPackagesElementName = "Opus:RequiredPackages";
                    while (xmlReader.Read())
                    {
                        if (xmlReader.Name == requiredPackagesElementName)
                        {
                            string packageElementName = "Opus:Package";
                            while (xmlReader.Read())
                            {
                                if ((xmlReader.Name == requiredPackagesElementName) &&
                                    (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement))
                                {
                                    break;
                                }

                                if (xmlReader.Name == packageElementName)
                                {
                                    string packageNameAttribute = "Name";
                                    if (!xmlReader.MoveToAttribute(packageNameAttribute))
                                    {
                                        throw new System.Xml.XmlException("Required attribute 'Name' of 'Package' node missing");
                                    }
                                    string packageName = xmlReader.Value;

                                    string packageVersionElementName = "Opus:Version";
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
                                                string packageVersionIdAttribute = "Id";
                                                if (!xmlReader.MoveToAttribute(packageVersionIdAttribute))
                                                {
                                                    throw new System.Xml.XmlException("Required 'Id' attribute of 'Version' node missing");
                                                }
                                                string packageVersion = xmlReader.Value;

                                                string packageConditionAttribute = "Condition";
                                                if (xmlReader.MoveToAttribute(packageConditionAttribute))
                                                {
                                                    string conditionValue = xmlReader.Value;
                                                    this.InterpretConditionValue(conditionValue);
                                                }

                                                PackageIdentifier id = new PackageIdentifier(packageName, packageVersion);
                                                this.packageIds.Add(id);
                                            }
                                        }
                                        else
                                        {
                                            throw new System.Xml.XmlException("Unexpected element");
                                        }
                                    }
                                }
                            }
                        }
                        else if (xmlReader.Name == rootElementName)
                        {
                            // should be the end element
                            if (xmlReader.NodeType != System.Xml.XmlNodeType.EndElement)
                            {
                                throw new System.Xml.XmlException(System.String.Format("Expected end of root element but found '{0}'", xmlReader.Name));
                            }
                        }
                        else
                        {
                            throw new System.Xml.XmlException(System.String.Format("Unknown element name '{0}'", xmlReader.Name));
                        }
                    }

                    if (!xmlReader.EOF)
                    {
                        throw new System.Xml.XmlException("Failed to read all of file");
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        public bool ReadV1(System.Xml.XmlReaderSettings readerSettings)
        {
            try
            {
                System.Xml.XmlReaderSettings settings = readerSettings.Clone();
                settings.Schemas.Add(null, State.OpusPackageDependencySchemaPathName);

                using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(this.xmlFilename, settings))
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
                                                string packageName = xmlReader.Value;
                                                xmlReader.MoveToAttribute("Version");
                                                string packageVersion = xmlReader.Value;

                                                PackageIdentifier id = new PackageIdentifier(packageName, packageVersion);
                                                this.packageIds.Add(id);

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
                Log.MessageAll("Blah: '{0}'", ex.Message);
                return false;
            }

            return true;
        }

        public bool UpdatePackage(PackageIdentifier idToChangeTo)
        {
            PackageIdentifier idToRemove = null;
            foreach (PackageIdentifier id in this.packageIds)
            {
                if (id.MatchName(idToChangeTo, false))
                {
                    if (0 == id.MatchVersion(idToChangeTo, false))
                    {
                        Log.Info("Package '{0}' is already set to version '{1}'", id.Name, id.Version);
                        return false;
                    }
                    else
                    {
                        idToRemove = id;
                    }
                }
            }

            if (null != idToRemove)
            {
                this.packageIds.Remove(idToRemove);
                Log.Info("Changed package '{0}' from version '{1}' to '{2}'", idToChangeTo.Name, idToRemove.Version, idToChangeTo.Version);
            }
            else
            {
                Log.Info("Added dependency '{0}' from root '{1}'", idToChangeTo.ToString(), idToChangeTo.Root);
            }

            this.packageIds.Add(idToChangeTo);

            return true;
        }

        public void AddRequiredPackage(PackageIdentifier idToAdd)
        {
            foreach (PackageIdentifier id in this.packageIds)
            {
                if (id.Match(idToAdd, false))
                {
                    throw new Exception(System.String.Format("Package '{0}' is already included in the dependency file", id.ToString()), false);
                }
            }

            this.packageIds.Add(idToAdd);
            Log.Info("Added dependency '{0}' from root '{1}'", idToAdd.ToString(), idToAdd.Root);
        }

        public bool RemovePackage(PackageIdentifier idToRemove)
        {
            PackageIdentifier idToRemoveReally = null;
            foreach (PackageIdentifier id in this.packageIds)
            {
                if (id.Match(idToRemove, false))
                {
                    idToRemoveReally = id;
                    break;
                }
            }

            if (null != idToRemoveReally)
            {
                this.packageIds.Remove(idToRemoveReally);
                Log.Info("Removed dependency '{0}' from root '{1}'", idToRemove.ToString(), idToRemove.Root);
                return true;
            }
            else
            {
                Log.Info("Could not find reference to '{0}' to remove", idToRemove.ToString());
                return false;
            }
        }

        public Array<PackageIdentifier> PackageIdentifiers
        {
            get
            {
                return this.packageIds;
            }
        }
    }
}