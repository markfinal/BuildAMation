// <copyright file="PackageDependencyXmlFile.cs" company="Mark Final">
//  Opus.Core
// </copyright>
// <summary>Opus package dependency XML file</summary>
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
                if (ofObjectToReturn == typeof(System.IO.Stream))
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(absoluteUri.LocalPath);
                    return reader.BaseStream;
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            else
            {
                if ("http://code.google.com/p/opus/" == absoluteUri.ToString())
                {
                    if (ofObjectToReturn == typeof(System.IO.Stream))
                    {
                        System.IO.StreamReader reader = new System.IO.StreamReader(State.OpusPackageDependencySchemaPathNameV2);
                        return reader.BaseStream;
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }

            throw new System.NotImplementedException();
        }
    }

    public class PackageDependencyXmlFile
    {
        private PackageInformationCollection packages;
        private string xmlFilename;
        private string schemaFilename;
        private bool validate;
        
        private static void ValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
#if true
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
#else
            if (args.Severity == System.Xml.Schema.XmlSeverityType.Warning)
            {
                Log.Info("\tWarning: Matching schema not found (" + args.Exception.GetType().ToString() + ").  No validation occurred." + args.Message);
            }
            else
            {
                Log.Info("\tValidation error: " + args.Message);
            }
            Log.Info("\tAt '" + args.Exception.SourceSchemaObject + "', line " + args.Exception.LineNumber + ", position " + args.Exception.LinePosition);
#endif
        }
        
        private void Validate()
        {
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
            settings.ValidationType = System.Xml.ValidationType.Schema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema;
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
        
        public PackageDependencyXmlFile(string xmlFilename, string schemaFilename, bool validate)
        {
            this.validate = validate;
            this.xmlFilename = xmlFilename;
            this.schemaFilename = schemaFilename;
            this.packages = new PackageInformationCollection();
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

            this.packages.Sort();

            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            //document.XmlResolver = new MyXmlResolver();
#if true
            string targetNamespace = "Opus";
            string namespaceURI = "http://code.google.com/p/opus/";
            System.Xml.XmlElement packageDefinition = document.CreateElement(targetNamespace, "PackageDefinition", namespaceURI);
            {
                string xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                System.Xml.XmlAttribute schemaAttribute = document.CreateAttribute("xsi", "schemaLocation", xmlns);
                schemaAttribute.Value = State.OpusPackageDependencySchemaPathNameV2;
                packageDefinition.Attributes.Append(schemaAttribute);
            }
            document.AppendChild(packageDefinition);

            System.Xml.XmlElement requiredPackages = document.CreateElement(targetNamespace, "RequiredPackages", namespaceURI);
            foreach (PackageInformation package in this.packages)
            {
                System.Xml.XmlElement packageElement = document.CreateElement(targetNamespace, "Package", namespaceURI);
                packageElement.SetAttribute("Name", package.Name);
                packageElement.SetAttribute("Version", package.Version);
                // TODO: Platform!
                requiredPackages.AppendChild(packageElement);
            }
            packageDefinition.AppendChild(requiredPackages);
#else
            document.Schemas.Add("noNamespaceSchemaLocation", schemaFilename);
            System.Xml.XmlElement requiredPackages = document.CreateElement("RequiredPackages");
            {
                string xmlns = "http://www.w3.org/2001/XMLSchema-instance";
                System.Xml.XmlAttribute schemaAttribute = document.CreateAttribute("xsi", "noNamespaceSchemaLocation", xmlns);
                schemaAttribute.Value = schemaFilename;
                requiredPackages.Attributes.Append(schemaAttribute);
            }
            document.AppendChild(requiredPackages);

            foreach (PackageInformation package in this.packages)
            {
                System.Xml.XmlElement packageElement = document.CreateElement("Package");
                packageElement.SetAttribute("Name", package.Name);
                packageElement.SetAttribute("Version", package.Version);
                requiredPackages.AppendChild(packageElement);
            }
#endif

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
            if (this.validate)
            {
                xmlReaderSettings.ValidationType = System.Xml.ValidationType.Schema;
            }
            xmlReaderSettings.Schemas = new System.Xml.Schema.XmlSchemaSet();
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

            System.Xml.XmlException readerException = null;

            // read the current version first
            try
            {
                this.ReadCurrent(xmlReaderSettings);
                return;
            }
            catch (System.Xml.XmlException exception)
            {
                readerException = exception;
            }

            // fall back on earlier versions
            // these will re-write out the definition file with the latest schema
            try
            {
                this.ReadV1(xmlReaderSettings);
                return;
            }
            catch (System.Xml.XmlException exception)
            {
                readerException = exception;
            }

            throw new Exception(System.String.Format("Package definition file '{0}' does not satisfy any of the Opus schemas", this.xmlFilename), readerException);
        }

        protected void ReadCurrent(System.Xml.XmlReaderSettings readerSettings)
        {
            System.Xml.XmlReaderSettings settings = readerSettings.Clone();
            settings.Schemas.Add(null, State.OpusPackageDependencySchemaPathNameV2);

            using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(this.xmlFilename, settings))
            {
                if (!xmlReader.ReadToFollowing("Opus:PackageDefinition"))
                {
                    throw new System.Xml.XmlException("'Opus:PackageDefinition' root element not found");
                }

                while (xmlReader.Read())
                {
                    if (System.Xml.XmlNodeType.Whitespace == xmlReader.NodeType)
                    {
                        continue;
                    }

                    if (System.Xml.XmlNodeType.Element == xmlReader.NodeType)
                    {
                    }

                    Log.MessageAll("NodeType '{0}', ns '{1}', '{2}'", xmlReader.NodeType.ToString(), xmlReader.NamespaceURI, xmlReader.Name);
                }

                if (!xmlReader.EOF)
                {
                    throw new System.Xml.XmlException("Failed to read all of file");
                }
            }
        }

        public void ReadV1(System.Xml.XmlReaderSettings readerSettings)
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

                                            PackageInformation package = PackageInformation.FindPackage(packageName, packageVersion);
                                            if (null == package)
                                            {
                                                PackageInformation p = new PackageInformation(packageName, packageVersion);
                                                this.packages.Add(p);
                                            }
                                            else
                                            {
                                                this.packages.Add(package);
                                            }

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

            // now write the file out using the most recent schema
            this.Write();
        }

        private PackageInformation GetPackageDetails(string packageName, string packageVersion)
        {
            PackageInformation package = PackageInformation.FindPackage(packageName, packageVersion);
            if (null == package)
            {
                string message = System.String.Format("Unable to locate package '{0}-{1}' in package roots\n", packageName, packageVersion);
                StringArray packageRoots = State.PackageRoots;
                foreach (string packageRoot in packageRoots)
                {
                    message = System.String.Concat(message, System.String.Format("\t{0}\n", packageRoot));
                }
                throw new Exception(System.String.Format(message), false);
            }

            return package;
        }

        public bool UpdatePackage(string packageName, string packageVersion)
        {
            PackageInformation package = this.Packages[packageName];
            if (null == package)
            {
                this.AddRequiredPackage(packageName, packageVersion);
                return false;
            }

            string oldVersion = package.Version;
            if (oldVersion == packageVersion)
            {
                Log.Info("Package '{0}' is already set to version '{1}'", packageName, oldVersion);
                return false;
            }

            PackageInformation packageToChangeTo = GetPackageDetails(packageName, packageVersion);
            this.packages.Remove(package);
            this.packages.Add(packageToChangeTo);

            Log.Info("Changed package '{0}' from version '{1}' to '{2}'", packageName, oldVersion, packageVersion);

            return true;
        }

        public void AddRequiredPackage(string packageName, string packageVersion)
        {
            PackageInformation package = GetPackageDetails(packageName, packageVersion);
            if (this.packages.Contains(package))
            {
                throw new Exception(System.String.Format("Package '{0}' is already included in the dependency file", package.FullName), false);
            }
            else
            {
                this.packages.Add(package);
            }

            Log.Info("Added dependency '{0}' from root '{1}'", package.FullName, package.Root);
        }

        public void RemovePackage(string packageName, string packageVersion)
        {
            PackageInformation package = this.packages[packageName];
            if (null == package)
            {
                throw new Exception(System.String.Format("Package '{0}' was not found in the collection", packageName), false);
            }
            if (package.Version != packageVersion)
            {
                throw new Exception(System.String.Format("Package '{0}' has version '{1}' in the collection, but version '{2}' is requested for removal", packageName, package.Version, packageVersion), false);
            }

            this.packages.Remove(package);

            Log.Info("Removed dependency '{0}' from root '{1}'", package.FullName, package.Root);
        }

        public PackageInformationCollection Packages
        {
            get
            {
                return this.packages;
            }
        }
    }
}