// <copyright file="OSXPlistWriterOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public sealed partial class OSXPlistWriterOptionCollection: XmlWriterOptionCollection, IOSXPlistOptions
    {
        public OSXPlistWriterOptionCollection(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void InitializeDefaults (Opus.Core.DependencyNode owningNode)
        {
            var options = this as IOSXPlistOptions;
            options.CFBundleExecutable = null;
            options.CFBundleDisplayName = null;
            options.CFBundleIdentifier = null;
            options.CFBundleVersion = null;
            options.CFBundleSignature = "????";
            options.NSPrincipalClass = null;
        }
        protected override void SetDelegates (Opus.Core.DependencyNode owningNode)
        {
        }
        #endregion

        private static void AddKeyToDict(System.Xml.XmlDocument doc, System.Xml.XmlElement dict, string value)
        {
            System.Xml.XmlElement element = doc.CreateElement("key");
            System.Xml.XmlText text = doc.CreateTextNode(value);
            element.AppendChild(text);
            dict.AppendChild(element);
        }
        
        private static void AddStringToDict(System.Xml.XmlDocument doc, System.Xml.XmlElement dict, string value)
        {
            System.Xml.XmlElement element = doc.CreateElement("string");
            System.Xml.XmlText text = doc.CreateTextNode(value);
            element.AppendChild(text);
            dict.AppendChild(element);
        }

        public override void FinalizeOptions (Opus.Core.DependencyNode node)
        {
            var options = node.Module.Options as IOSXPlistOptions;
            var dependentNode = node.ExternalDependents[0];
            string primaryOutputPath = null;
            foreach (string outputPath in dependentNode.Module.Options.OutputPaths.Paths)
            {
                primaryOutputPath = outputPath;
                break;
            }

            if (null == this.OutputPaths[OutputFileFlags.XmlFile])
            {
                string contentsDir = System.IO.Directory.GetParent(primaryOutputPath).Parent.FullName;
                this.OutputPaths[OutputFileFlags.XmlFile] = System.IO.Path.Combine(contentsDir, "Info.plist");
            }

            if (null == options.CFBundleExecutable)
            {
                options.CFBundleExecutable = System.IO.Path.GetFileNameWithoutExtension(primaryOutputPath);
            }

            if (null == options.CFBundleDisplayName)
            {
                options.CFBundleDisplayName = node.UniqueModuleName;
            }

            var xmlModule = node.Module as XmlModule;
            System.Xml.XmlElement dictEl = (xmlModule as XmlUtilities.OSXPlistModule).DictElement;
            
            AddKeyToDict(xmlModule.Document, dictEl, "CFBundleDisplayName");
            AddStringToDict(xmlModule.Document, dictEl, options.CFBundleDisplayName);
            
            AddKeyToDict(xmlModule.Document, dictEl, "CFBundleExecutable");
            AddStringToDict(xmlModule.Document, dictEl, options.CFBundleExecutable);
            
            if (null != options.CFBundleIdentifier)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleIdentifier");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleIdentifier);
            }
            
            AddKeyToDict(xmlModule.Document, dictEl, "CFBundleName");
            AddStringToDict(xmlModule.Document, dictEl, options.CFBundleDisplayName);
            
            AddKeyToDict(xmlModule.Document, dictEl, "CFBundlePackageType");
            AddStringToDict(xmlModule.Document, dictEl, "APPL");
            
            AddKeyToDict(xmlModule.Document, dictEl, "CFBundleSignature");
            AddStringToDict(xmlModule.Document, dictEl, options.CFBundleSignature);
            
            if (null != options.CFBundleVersion)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleVersion");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleVersion);
            }

            if (null != options.NSPrincipalClass)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "NSPrincipalClass");
                AddStringToDict(xmlModule.Document, dictEl, options.NSPrincipalClass);
            }
        }

        // TODO: move this to the partial class
        #region IOSXPlistOptions implementation
        string IOSXPlistOptions.CFBundleExecutable 
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleExecutable");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleExecutable", value);
                this.ProcessNamedSetHandler("CFBundleExecutableSetHandler", this["CFBundleExecutable"]);
            }
        }
        string IOSXPlistOptions.CFBundleDisplayName 
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleDisplayName");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleDisplayName", value);
                this.ProcessNamedSetHandler("CFBundleDisplayNameSetHandler", this["CFBundleDisplayName"]);
            }
        }
        string IOSXPlistOptions.CFBundleIdentifier 
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleIdentifier");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleIdentifier", value);
                this.ProcessNamedSetHandler("CFBundleIdentifierClassSetHandler", this["CFBundleIdentifier"]);
            }
        }
        string IOSXPlistOptions.CFBundleVersion
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleVersion");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleVersion", value);
                this.ProcessNamedSetHandler("CFBundleVersionClassSetHandler", this["CFBundleVersion"]);
            }
        }
        string IOSXPlistOptions.CFBundleSignature
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleSignature");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleSignature", value);
                this.ProcessNamedSetHandler("CFBundleSignatureClassSetHandler", this["CFBundleSignature"]);
            }
        }
        string IOSXPlistOptions.NSPrincipalClass 
        {
            get
            {
                return this.GetReferenceTypeOption<string>("NSPrincipalClass");
            }
            set
            {
                this.SetReferenceTypeOption<string>("NSPrincipalClass", value);
                this.ProcessNamedSetHandler("NSPrincipalClassSetHandler", this["NSPrincipalClass"]);
            }
        }
        #endregion
    }
}
