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

        private static bool
        AreTextFilesIdentical(
            string targetPath,
            string tempPath)
        {
            var targetSize = new System.IO.FileInfo(targetPath).Length;
            var tempSize = new System.IO.FileInfo(targetPath).Length;
            if (targetSize != tempSize)
            {
                return false;
            }
            using (System.IO.TextReader targetReader = new System.IO.StreamReader(targetPath))
            {
                using (System.IO.TextReader tempReader = new System.IO.StreamReader(tempPath))
                {
                    var targetContents = targetReader.ReadToEnd();
                    var tempContents = tempReader.ReadToEnd();
                    if (0 != System.String.Compare(targetContents, tempContents, false))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void
        WriteXMLIfDifferent(
            string targetPath,
            System.Xml.XmlWriterSettings settings,
            System.Xml.XmlDocument document)
        {
            var targetExists = System.IO.File.Exists(targetPath);
            var writePath = targetExists ? System.IO.Path.GetTempFileName() : targetPath;
            using (var xmlwriter = System.Xml.XmlWriter.Create(writePath, settings))
            {
                document.WriteTo(xmlwriter);
            }
            Bam.Core.Log.DebugMessage(PrettyPrintXMLDoc(document));
            if (targetExists && !AreTextFilesIdentical(targetPath, writePath))
            {
                Bam.Core.Log.DebugMessage("\tXML has changed, moving {0} to {1}", writePath, targetPath);
                System.IO.File.Delete(targetPath);
                System.IO.File.Move(writePath, targetPath);
            }
        }

        private static void
        WriteSolutionFileIfDifferent(
            string targetPath,
            System.Text.StringBuilder contents)
        {
            var targetExists = System.IO.File.Exists(targetPath);
            var writePath = targetExists ? System.IO.Path.GetTempFileName() : targetPath;
            using (var writer = new System.IO.StreamWriter(writePath))
            {
                writer.Write(contents);
            }
            Bam.Core.Log.DebugMessage(contents.ToString());
            if (targetExists && !AreTextFilesIdentical(targetPath, writePath))
            {
                Bam.Core.Log.DebugMessage("\tText has changed, moving {0} to {1}", writePath, targetPath);
                System.IO.File.Delete(targetPath);
                System.IO.File.Move(writePath, targetPath);
            }
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
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(projectPathDir);

                WriteXMLIfDifferent(project.ProjectPath, xmlWriterSettings, project.Serialize());
                WriteXMLIfDifferent(project.ProjectPath + ".filters", xmlWriterSettings, project.Filter.Serialize());
                if (project.Module is C.ConsoleApplication && (project.Module as C.ConsoleApplication).WorkingDirectory != null)
                {
                    WriteXMLIfDifferent(project.ProjectPath + ".user", xmlWriterSettings, project.SerializeUserSettings());
                }
            }

            var solutionPath = Bam.Core.TokenizedString.Create("$(buildroot)/$(masterpackagename).sln", null).Parse();
            WriteSolutionFileIfDifferent(solutionPath, solution.Serialize(solutionPath));

            Bam.Core.Log.Info("Successfully created Visual Studio solution for package '{0}'\n\t{1}", graph.MasterPackage.Name, solutionPath);
        }

        Bam.Core.TokenizedString
        Bam.Core.IBuildModeMetaData.ModuleOutputDirectory(
            Bam.Core.Module currentModule,
            Bam.Core.Module encapsulatingModule)
        {
            var outputDir = System.IO.Path.Combine(encapsulatingModule.GetType().Name, currentModule.BuildEnvironment.Configuration.ToString());
            var moduleSubDir = currentModule.CustomOutputSubDirectory;
            if (null != moduleSubDir)
            {
                outputDir = System.IO.Path.Combine(outputDir, moduleSubDir);
            }
            return Bam.Core.TokenizedString.CreateVerbatim(outputDir);
        }

        bool Bam.Core.IBuildModeMetaData.PublishBesideExecutable
        {
            get
            {
                return true;
            }
        }

        bool Bam.Core.IBuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles
        {
            get
            {
                return true;
            }
        }
    }
}
