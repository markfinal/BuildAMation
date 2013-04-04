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
            options.CFBundleName = null;
            options.CFBundleDisplayName = null;
            options.CFBundleIdentifier = null;
            options.CFBundleVersion = null;
            options.CFBundleSignature = "????";
            options.CFBundleExecutable = null;
            // TODO: CFBundleDocumentTypes
            options.CFBundleShortVersionString = null;
            options.LSMinimumSystemVersion = null;
            options.NSHumanReadableCopyright = null;
            options.NSMainNibFile = null;
            options.NSPrincipalClass = null;
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

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var options = node.Module.Options as IOSXPlistOptions;
            string primaryOutputPath = null;
            // the plist file is relative to the main executable
            if (null != node.ExternalDependents)
            {
                var dependentNode = node.ExternalDependents[0];
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
            }
            else
            {
                primaryOutputPath = "Undefined";
            }

            // some other defaults
            if (null == options.CFBundleName)
            {
                options.CFBundleName = node.UniqueModuleName;
            }

            if (null == options.CFBundleExecutable)
            {
                options.CFBundleExecutable = System.IO.Path.GetFileNameWithoutExtension(primaryOutputPath);
            }

            if (null == options.CFBundleDisplayName)
            {
                options.CFBundleDisplayName = node.UniqueModuleName;
            }

            // now generate the XML document
            var xmlModule = node.Module as XmlModule;
            System.Xml.XmlElement dictEl = (xmlModule as XmlUtilities.OSXPlistModule).DictElement;

            if (null != options.CFBundleName)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleName");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleName);
            }

            if (null != options.CFBundleDisplayName)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleDisplayName");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleDisplayName);
            }

            if (null != options.CFBundleIdentifier)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleIdentifier");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleIdentifier);
            }

            if (null != options.CFBundleVersion)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleVersion");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleVersion);
            }

            AddKeyToDict(xmlModule.Document, dictEl, "CFBundlePackageType");
            AddStringToDict(xmlModule.Document, dictEl, "APPL");

            if (null != options.CFBundleSignature)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleSignature");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleSignature);
            }

            if (null != options.CFBundleExecutable)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleExecutable");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleExecutable);
            }

            if (null != options.CFBundleShortVersionString)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "CFBundleShortVersionString");
                AddStringToDict(xmlModule.Document, dictEl, options.CFBundleShortVersionString);
            }

            if (null != options.LSMinimumSystemVersion)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "LSMinimumSystemVersion");
                AddStringToDict(xmlModule.Document, dictEl, options.LSMinimumSystemVersion);
            }

            if (null != options.NSHumanReadableCopyright)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "NSHumanReadableCopyright");
                AddStringToDict(xmlModule.Document, dictEl, options.NSHumanReadableCopyright);
            }

            if (null != options.NSMainNibFile)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "NSMainNibFile");
                AddStringToDict(xmlModule.Document, dictEl, options.NSMainNibFile);
            }

            if (null != options.NSPrincipalClass)
            {
                AddKeyToDict(xmlModule.Document, dictEl, "NSPrincipalClass");
                AddStringToDict(xmlModule.Document, dictEl, options.NSPrincipalClass);
            }
        }
    }
}
