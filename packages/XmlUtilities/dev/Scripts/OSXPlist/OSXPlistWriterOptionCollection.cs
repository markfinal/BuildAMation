#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace XmlUtilities
{
    public sealed partial class OSXPlistWriterOptionCollection :
        XmlWriterOptionCollection,
        IOSXPlistOptions
    {
        public
        OSXPlistWriterOptionCollection(
            Bam.Core.DependencyNode owningNode) : base(owningNode)
        {}

        #region implemented abstract members of BaseOptionCollection
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
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

        private static void
        AddKeyToDict(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement dict,
            string value)
        {
            var element = doc.CreateElement("key");
            var text = doc.CreateTextNode(value);
            element.AppendChild(text);
            dict.AppendChild(element);
        }

        private static void
        AddStringToDict(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement dict,
            string value)
        {
            var element = doc.CreateElement("string");
            var text = doc.CreateTextNode(value);
            element.AppendChild(text);
            dict.AppendChild(element);
        }

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var pListDirLoc = locationMap[XmlModule.OutputDir] as Bam.Core.ScaffoldLocation;
            if (!pListDirLoc.IsValid)
            {
                pListDirLoc.SetReference(locationMap[Bam.Core.State.ModuleBuildDirLocationKey]);
            }

            base.SetNodeSpecificData (node);
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var pListFileLoc = locationMap[XmlModule.OutputFile] as Bam.Core.ScaffoldLocation;
            if (!pListFileLoc.IsValid)
            {
                pListFileLoc.SpecifyStub(locationMap[XmlModule.OutputDir], "Info.plist", Bam.Core.ScaffoldLocation.EExists.WillExist);
            }

            // TODO: move this into the Module itself
            // the plist file is relative to the main executable
            if (null == node.ExternalDependents)
            {
                throw new Bam.Core.Exception("PList generation must be dependent upon the executable associated with it");
            }

            var dependentNode = node.ExternalDependents[0];
            var exeFileLoc = dependentNode.Module.Locations[C.Application.OutputFile];

            var primaryOutputPath = exeFileLoc.GetSinglePath();

            var options = node.Module.Options as IOSXPlistOptions;
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
            var dictEl = (xmlModule as XmlUtilities.OSXPlistModule).DictElement;

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
