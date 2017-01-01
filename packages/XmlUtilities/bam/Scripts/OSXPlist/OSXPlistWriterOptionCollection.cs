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
