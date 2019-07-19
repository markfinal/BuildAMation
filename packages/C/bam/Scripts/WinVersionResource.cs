#region License
// Copyright (c) 2010-2019, Mark Final
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
        /// <summary>
        /// Path key for this module
        /// </summary>
        public const string HashFileKey = "Hash of version resource contents";

        /// <summary>
        /// Initialize this module
        /// </summary>
        /// <param name="parent">from this parent module</param>
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

        /// <summary>
        /// Get or set the binary module associated with this resource
        /// </summary>
        public ConsoleApplication BinaryModule { get; set; }

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
                contents.AppendLine($"// Version resource for {binaryModule.Macros["modulename"].ToString()}, automatically generated by BuildAMation");
                contents.AppendLine("#include \"winver.h\"");
                contents.AppendLine("VS_VERSION_INFO VERSIONINFO");
                // note that these are comma separated
                contents.AppendLine($"FILEVERSION {binaryMajorVersion},{binaryMinorVersion},{binaryPatchVersion}");
                if (null != productDefinition)
                {
                    contents.AppendLine($"PRODUCTVERSION {productDefinition.MajorVersion ?? 0},{productDefinition.MinorVersion ?? 0},{productDefinition.PatchVersion ?? 0}");
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
                    contents.AppendLine($"FILEFLAGS ({flags})");
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
                contents.AppendLine($"\t\tBLOCK \"{culture.LCID:X4}{codepage:X4}\"");
                contents.AppendLine("\t\tBEGIN");
                var fileDescription = binaryModule.CreateTokenizedString("#valid($(Description),$(modulename))");
                if (!fileDescription.IsParsed)
                {
                    fileDescription.Parse();
                }
                contents.AppendLine($"\t\t\tVALUE \"FileDescription\", \"{fileDescription.ToString()}\"");
                contents.AppendLine($"\t\t\tVALUE \"FileVersion\", \"{binaryMajorVersion}.{binaryMinorVersion}.{binaryPatchVersion}\"");
                contents.AppendLine($"\t\t\tVALUE \"InternalName\", \"{binaryModule.Macros["modulename"].ToString()}\"");
                contents.AppendLine($"\t\t\tVALUE \"OriginalFilename\", \"{System.IO.Path.GetFileName(binaryModule.GeneratedPaths[ConsoleApplication.ExecutableKey].ToString())}\"");
                if (null != productDefinition)
                {
                    contents.AppendLine($"\t\t\tVALUE \"ProductName\", \"{productDefinition.Name}\"");
                    contents.AppendLine($"\t\t\tVALUE \"ProductVersion\", \"{productDefinition.MajorVersion ?? 0}.{productDefinition.MinorVersion ?? 0}.{productDefinition.PatchVersion ?? 0}\"");
                    contents.AppendLine($"\t\t\tVALUE \"LegalCopyright\", \"{productDefinition.CopyrightNotice}\"");
                    contents.AppendLine($"\t\t\tVALUE \"CompanyName\", \"{productDefinition.CompanyName}\"");
                }
                contents.AppendLine("\t\tEND");
                contents.AppendLine("\tEND");
                contents.AppendLine("\tBLOCK \"VarFileInfo\"");
                contents.AppendLine("\tBEGIN");
                contents.AppendLine($"\t\tVALUE \"Translation\", 0x{culture.LCID:X4}, {codepage}");
                contents.AppendLine("\tEND");
                contents.AppendLine("END");

                return contents.ToString();
            }
        }

        /// <summary>
        /// Execute the build on this module
        /// </summary>
        /// <param name="context">in this context</param>
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

        /// <summary>
        /// Determine if this module needs updating
        /// </summary>
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[SourceFileKey].ToString();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[SourceFileKey]);
            }
            // have the contents changed since last time?
            var hashFilePath = this.GeneratedPaths[HashFileKey].ToString();
            var hashCompare = Bam.Core.Hash.CompareAndUpdateHashFile(
                hashFilePath,
                this.Contents
            );
            switch (hashCompare)
            {
                case Bam.Core.Hash.EHashCompareResult.HashesAreDifferent:
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                        this.GeneratedPaths[SourceFileKey],
                        this.GeneratedPaths[HashFileKey]
                    );
                    break;

                case Bam.Core.Hash.EHashCompareResult.HashFileDoesNotExist:
                case Bam.Core.Hash.EHashCompareResult.HashesAreIdentical:
                    break;
            }
        }
    }
}
