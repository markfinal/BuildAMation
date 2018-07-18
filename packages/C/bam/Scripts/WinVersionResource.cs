#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace C
{
    /// <summary>
    /// Windows binary version resource file
    /// </summary>
    [Bam.Core.EvaluationRequired(true)]
    public class WinVersionResource :
        SourceFile
    {
#if BAM_V2
        public const string HashFileKey = "Hash of version resource contents";
#else
        private static Bam.Core.PathKey HashFileKey = Bam.Core.PathKey.Generate("Hash of version resource contents");
#endif

        private delegate int GetHashFn(string inPath);

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(
                HashFileKey,
                this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/@filename($(0)).hash", this.InputPath)
            );
        }

        public ConsoleApplication BinaryModule
        {
            get;
            set;
        }

        private string
        Contents
        {
            get
            {
                var binaryModule = this.BinaryModule;
                var binaryMajorVersion = binaryModule.Macros["MajorVersion"].ToString();
                var getMinorVersion = binaryModule.CreateTokenizedString("#valid($(MinorVersion),0)");
                if (!getMinorVersion.IsParsed)
                {
                    getMinorVersion.Parse();
                }
                var binaryMinorVersion = getMinorVersion.ToString();
                var getPatchVersion = binaryModule.CreateTokenizedString("#valid($(PatchVersion),0)");
                if (!getPatchVersion.IsParsed)
                {
                    getPatchVersion.Parse();
                }
                var binaryPatchVersion = getPatchVersion.ToString();

                var productDefinition = Bam.Core.Graph.Instance.ProductDefinition;

                var contents = new System.Text.StringBuilder();
                contents.AppendFormat("// Version resource for {0}, automatically generated by BuildAMation", binaryModule.Macros["modulename"].ToString());
                contents.AppendLine();
                contents.AppendLine("#include \"winver.h\"");
                contents.AppendLine("VS_VERSION_INFO VERSIONINFO");
                // note that these are comma separated
                contents.AppendFormat("FILEVERSION {0},{1},{2}", binaryMajorVersion, binaryMinorVersion, binaryPatchVersion);
                contents.AppendLine();
                if (null != productDefinition)
                {
                    contents.AppendFormat("PRODUCTVERSION {0},{1},{2}",
                        productDefinition.MajorVersion.HasValue ? productDefinition.MajorVersion.Value : 0,
                        productDefinition.MinorVersion.HasValue ? productDefinition.MinorVersion.Value : 0,
                        productDefinition.PatchVersion.HasValue ? productDefinition.PatchVersion.Value : 0);
                    contents.AppendLine();
                }
                contents.AppendLine("FILEFLAGSMASK VS_FFI_FILEFLAGSMASK");
                string flags = "";
                if (this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug)
                {
                    if (flags.Length > 0)
                    {
                        flags += "|";
                    }
                    flags += "VS_FF_DEBUG";
                }
                if (null != productDefinition)
                {
                    if (productDefinition.IsPrerelease)
                    {
                        if (flags.Length > 0)
                        {
                            flags += "|";
                        }
                        flags += "VS_FF_PRERELEASE";
                    }
                }
                if (!string.IsNullOrEmpty(flags))
                {
                    contents.AppendFormat("FILEFLAGS ({0})", flags);
                    contents.AppendLine();
                }
                contents.AppendLine("FILEOS VOS_NT_WINDOWS32"); // TODO: is there a 64-bit?
                if (binaryModule is DynamicLibrary || binaryModule is Cxx.DynamicLibrary)
                {
                    contents.AppendLine("FILETYPE VFT_DLL");
                }
                else
                {
                    contents.AppendLine("FILETYPE VFT_APP");
                }

                // use the current machine's configuration to determine the default
                // language,codepage pair supported by the binary
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                var codepage = System.Text.Encoding.Default.WindowsCodePage;

                contents.AppendLine("FILESUBTYPE VFT2_UNKNOWN");
                contents.AppendLine("BEGIN");
                contents.AppendLine("\tBLOCK \"StringFileInfo\"");
                contents.AppendLine("\tBEGIN");
                contents.AppendFormat("\t\tBLOCK \"{0:X4}{1:X4}\"", culture.LCID, codepage);
                contents.AppendLine();
                contents.AppendLine("\t\tBEGIN");
                var fileDescription = binaryModule.CreateTokenizedString("#valid($(Description),$(modulename))");
                if (!fileDescription.IsParsed)
                {
                    fileDescription.Parse();
                }
                contents.AppendFormat("\t\t\tVALUE \"FileDescription\", \"{0}\"", fileDescription.ToString());
                contents.AppendLine();
                contents.AppendFormat("\t\t\tVALUE \"FileVersion\", \"{0}.{1}.{2}\"", binaryMajorVersion, binaryMinorVersion, binaryPatchVersion);
                contents.AppendLine();
                contents.AppendFormat("\t\t\tVALUE \"InternalName\", \"{0}\"", binaryModule.Macros["modulename"].ToString());
                contents.AppendLine();
#if BAM_V2
                contents.AppendFormat("\t\t\tVALUE \"OriginalFilename\", \"{0}\"", System.IO.Path.GetFileName(binaryModule.GeneratedPaths[ConsoleApplication.ExecutableKey].ToString()));
#else
                contents.AppendFormat("\t\t\tVALUE \"OriginalFilename\", \"{0}\"", System.IO.Path.GetFileName(binaryModule.GeneratedPaths[ConsoleApplication.Key].ToString()));
#endif
                contents.AppendLine();
                if (null != productDefinition)
                {
                    contents.AppendFormat("\t\t\tVALUE \"ProductName\", \"{0}\"", productDefinition.Name);
                    contents.AppendLine();
                    contents.AppendFormat("\t\t\tVALUE \"ProductVersion\", \"{0}.{1}.{2}\"",
                        productDefinition.MajorVersion.HasValue ? productDefinition.MajorVersion.Value : 0,
                        productDefinition.MinorVersion.HasValue ? productDefinition.MinorVersion.Value : 0,
                        productDefinition.PatchVersion.HasValue ? productDefinition.PatchVersion.Value : 0);
                    contents.AppendLine();
                    contents.AppendFormat("\t\t\tVALUE \"LegalCopyright\", \"{0}\"", productDefinition.CopyrightNotice);
                    contents.AppendLine();
                    contents.AppendFormat("\t\t\tVALUE \"CompanyName\", \"{0}\"", productDefinition.CompanyName);
                    contents.AppendLine();
                }
                contents.AppendLine("\t\tEND");
                contents.AppendLine("\tEND");
                contents.AppendLine("\tBLOCK \"VarFileInfo\"");
                contents.AppendLine("\tBEGIN");
                contents.AppendFormat("\t\tVALUE \"Translation\", 0x{0:X4}, {1}", culture.LCID, codepage);
                contents.AppendLine();
                contents.AppendLine("\tEND");
                contents.AppendLine("END");

                return contents.ToString();
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            base.ExecuteInternal(context);

            var rcPath = this.InputPath.ToString();
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(rcPath)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(rcPath));
            }

            using (System.IO.TextWriter writer = new System.IO.StreamWriter(rcPath))
            {
                writer.NewLine = "\n";
                writer.WriteLine(this.Contents);
            }
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
#if BAM_V2
            var outputPath = this.GeneratedPaths[SourceFileKey].ToString();
#else
            var outputPath = this.GeneratedPaths[Key].ToString();
#endif
            if (!System.IO.File.Exists(outputPath))
            {
#if BAM_V2
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[SourceFileKey]);
#else
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
#endif
            }
            // have the contents changed since last time?
            var writeHashFile = true;
            var currentContentsHash = this.Contents.GetHashCode();
            var hashFilePath = this.GeneratedPaths[HashFileKey].ToString();
            if (System.IO.File.Exists(hashFilePath))
            {
                GetHashFn getHash = inPath =>
                {
                    int hash = 0;
                    using (System.IO.TextReader readFile = new System.IO.StreamReader(inPath))
                    {
                        var contents = readFile.ReadToEnd();
                        hash = System.Convert.ToInt32(contents);
                    }
                    return hash;
                };
                var oldHash = getHash(hashFilePath);
                if (oldHash == currentContentsHash)
                {
                    writeHashFile = false;
                }
                else
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
#if BAM_V2
                        this.GeneratedPaths[SourceFileKey],
#else
                        this.GeneratedPaths[Key],
#endif
                        this.GeneratedPaths[HashFileKey]
                    );
                }
            }
            if (writeHashFile)
            {
                var destDir = System.IO.Path.GetDirectoryName(hashFilePath);
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destDir);
                using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(hashFilePath))
                {
                    writeFile.NewLine = "\n";
                    writeFile.Write(currentContentsHash);
                }
            }
        }
    }
}
