﻿// <copyright file="XmlPackageDependencyDiscovery.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class XmlPackageDependencyDiscovery
    {
        static void xmlReaderSettings_ValidationEventHandler(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            string message = System.String.Format("Xml  '{0}' in '{1}' at line {2}", e.Message, e.Exception.SourceUri, e.Exception.LineNumber);
            throw new Exception(message, e.Exception);
        }
        
        public static void Execute(PackageInformation package)
        {
            string dependencyFile = package.DependencyFile;
            if (!System.IO.File.Exists(dependencyFile))
            {
                throw new Exception(System.String.Format("Dependency file '{0}' does not exist", dependencyFile));
            }
            System.Xml.XmlReaderSettings xmlReaderSettings = new System.Xml.XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(null, State.OpusPackageDependencySchemaPathName);
            xmlReaderSettings.ValidationType = System.Xml.ValidationType.Schema;
            xmlReaderSettings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(xmlReaderSettings_ValidationEventHandler);
            using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(dependencyFile, xmlReaderSettings))
            {
                xmlReader.ReadToFollowing("RequiredPackages");
                while (xmlReader.Read())
                {
                    if ("Package" == xmlReader.Name)
                    {
                        string packageName = xmlReader.GetAttribute("Name");
                        string packageVersion = xmlReader.GetAttribute("Version");
                        
                        Core.PackageInformation packageInfo = Core.PackageInformation.FindPackage(packageName, packageVersion);
                        if (null == packageInfo)
                        {
                            throw new Exception(System.String.Format("Package '{0}-{1}' not found in any package root", packageName, packageVersion), false);
                        }
                        
                        PackageInformationCollection collection = State.PackageInfo;
                        if (!collection.Contains(packageInfo))
                        {
                            collection.Add(packageInfo);
                        }
                                                
                        Execute(packageInfo);
                    }
                }
            }
        }
    }
}