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
using System.Linq;
namespace C
{
    /// <summary>
    /// Module specifically for symbolic links for SONames
    /// </summary>
    class SONameSymbolicLink :
        SharedObjectSymbolicLink
    { }

    /// <summary>
    /// Module specifically for symbolic links for LinkerNames
    /// </summary>
    class LinkerNameSymbolicLink :
        SharedObjectSymbolicLink
    { }

    /// <summary>
    /// Module for the symbolic links for shared objects
    /// </summary>
    abstract class SharedObjectSymbolicLink :
        Bam.Core.Module
    {
        /// <summary>
        /// Pathkey for this module
        /// </summary>
        public const string SOSymLinkKey = "Shared object symbolic link";
        private ConsoleApplication sharedObject;

        /// <summary>
        /// Query whether the module has the C.Prebuilt attribute assigned to it.
        /// </summary>
        public bool IsPrebuilt { get; private set; }

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<SharedObjectSymbolicLinkTool>();
            this.IsPrebuilt = (this.GetType().GetCustomAttributes(typeof(PrebuiltAttribute), true).Length > 0);

            this.RegisterGeneratedFile(
                SOSymLinkKey,
                this.CreateTokenizedString("@dir($(0))/$(1)",
                                            this.SharedObject.GeneratedPaths[ConsoleApplication.ExecutableKey],
                                            this.Macros["SymlinkFilename"]),
                true
            );
        }

        /// <summary>
        /// Get or set the shared object.
        /// </summary>
        public ConsoleApplication
        SharedObject
        {
            get
            {
                return this.sharedObject;
            }
            set
            {
                this.sharedObject = value;
                this.DependsOn(value);

                // ensure that the symlink is called the same as what it is linking to
                this.Macros[Bam.Core.ModuleMacroNames.OutputName] = value.Macros[Bam.Core.ModuleMacroNames.OutputName];
            }
        }

        /// <summary>
        /// Execute the build for this module
        /// </summary>
        /// <param name="context"></param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (this.IsPrebuilt)
            {
                return;
            }
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(
                        this,
                        isDependencyOfAll: true
                    );
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Determine if this module needs updating
        /// </summary>
        protected override void
        EvaluateInternal()
        {
#if D_NUGET_MONO_POSIX_NETSTANDARD
            if (this.IsPrebuilt)
            {
                return;
            }
            this.ReasonToExecute = null;
            var symlinkPath = this.GeneratedPaths[SOSymLinkKey].ToString();
            var symlinkInfo = new Mono.Unix.UnixSymbolicLinkInfo(symlinkPath);
            if (!symlinkInfo.Exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[SOSymLinkKey]);
                return;
            }
            try
            {
                var targetPath = symlinkInfo.ContentsPath;
                if (targetPath != System.IO.Path.GetFileName(this.SharedObject.GeneratedPaths[ConsoleApplication.ExecutableKey].ToString()))
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                        this.GeneratedPaths[SOSymLinkKey],
                        this.SharedObject.Macros[this.GeneratedPaths["SymlinkUsage"].ToString()]
                    );
                    return;
                }
            }
            catch (System.ArgumentException)
            {
                throw new Bam.Core.Exception($"'{symlinkPath}' is not a symbolic link");
            }
#else
            throw new System.NotSupportedException("Symbolic links not supported for shared objects on this platform");
#endif
        }

        /// <summary>
        /// /copydoc Bam.Core.Module.InputModulePaths
        /// </summary>
        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module module, string pathKey)> InputModulePaths
        {
            get
            {
                System.Diagnostics.Debug.Assert(1 == this.Dependents.Count());
                yield return (this.Dependents.First(), C.DynamicLibrary.ExecutableKey);
            }
        }

        /// <summary>
        /// Set the working directory for this Module
        /// </summary>
        public override Bam.Core.TokenizedString WorkingDirectory => this.OutputDirectories.First(); // since ln needs this for relative paths

        /// <summary>
        /// Modify the root of the output symbolic link path.
        /// </summary>
        /// <param name="newRoot">New root directory.</param>
        public void
        ChangeSymbolicLinkRootPath(
            Bam.Core.TokenizedString newRoot
        )
        {
            this.RegisterGeneratedFile(
                SOSymLinkKey,
                this.CreateTokenizedString("$(0)/$(1)",
                                           newRoot,
                                           this.Macros["SymlinkFilename"]),
                true
            );
        }
    }
}
