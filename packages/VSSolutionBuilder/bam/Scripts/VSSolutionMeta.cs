#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace VSSolutionBuilder
{
    // although this is project related data, it needs to be named after the builder, VSSolution
    public class VSSolutionMeta :
        Bam.Core.IBuildModeMetaData
    {
        public static void
        PreExecution()
        {
            var graph = Bam.Core.Graph.Instance;
            graph.MetaData = new VSSolution();
        }

        private static string
        PrettyPrintXMLDoc(
            System.Xml.XmlDocument document)
        {
            var content = new System.Text.StringBuilder();
            var settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                NewLineChars = System.Environment.NewLine,
                Encoding = new System.Text.UTF8Encoding(false) // no BOM
            };
            using (var writer = System.Xml.XmlWriter.Create(content, settings))
            {
                document.Save(writer);
            }
            return content.ToString();
        }

        public static void
        PostExecution()
        {
            var graph = Bam.Core.Graph.Instance;
            var solution = graph.MetaData as VSSolution;
            if (0 == solution.Projects.Count())
            {
                throw new Bam.Core.Exception("No projects were generated");
            }

            var xmlWriterSettings = new System.Xml.XmlWriterSettings
                {
                    OmitXmlDeclaration = false,
                    Encoding = new System.Text.UTF8Encoding(false), // no BOM (Byte Ordering Mark)
                    NewLineChars = System.Environment.NewLine,
                    Indent = true
                };

            foreach (var project in solution.Projects)
            {
                var projectPathDir = System.IO.Path.GetDirectoryName(project.ProjectPath);
                if (!System.IO.Directory.Exists(projectPathDir))
                {
                    System.IO.Directory.CreateDirectory(projectPathDir);
                }

                var projectXML = project.Serialize();
                using (var xmlwriter = System.Xml.XmlWriter.Create(project.ProjectPath, xmlWriterSettings))
                {
                    projectXML.WriteTo(xmlwriter);
                }
                Bam.Core.Log.DebugMessage(PrettyPrintXMLDoc(projectXML));

                var projectFilterXML = project.Filter.Serialize();
                using (var xmlwriter = System.Xml.XmlWriter.Create(project.ProjectPath + ".filters", xmlWriterSettings))
                {
                    projectFilterXML.WriteTo(xmlwriter);
                }
                Bam.Core.Log.DebugMessage(PrettyPrintXMLDoc(projectFilterXML));
            }

            var solutionPath = Bam.Core.TokenizedString.Create("$(buildroot)/$(masterpackagename).sln", null).Parse();
            var solutionContents = solution.Serialize();
            using (var writer = new System.IO.StreamWriter(solutionPath))
            {
                writer.Write(solutionContents);
            }
            Bam.Core.Log.DebugMessage(solutionContents.ToString());

            Bam.Core.Log.Info("Successfully created Visual Studio solution file for package '{0}'\n\t{1}", graph.MasterPackage.Name, solutionPath);
        }

        Bam.Core.TokenizedString
        Bam.Core.IBuildModeMetaData.ModuleOutputDirectory(
            Bam.Core.Module currentModule,
            Bam.Core.Module encapsulatingModule)
        {
            return Bam.Core.TokenizedString.CreateVerbatim(System.IO.Path.Combine(encapsulatingModule.GetType().Name, currentModule.BuildEnvironment.Configuration.ToString()));
        }

        bool Bam.Core.IBuildModeMetaData.PublishBesideExecutable
        {
            get
            {
                return true;
            }
        }
    }
}
