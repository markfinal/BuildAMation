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
    /// Base class for a collection of homogenous object files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    [System.Obsolete("Please use CCompilableModuleCollection instead", true)]
    abstract class CCompilableModuleContainer<ChildModuleType> :
        CCompilableModuleCollection<ChildModuleType>
        where ChildModuleType : Bam.Core.Module, Bam.Core.IChildModule, IRequiresSourceModule, new()
    { }

    /// <summary>
    /// Base class for a collection of homogenous object files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    abstract class CCompilableModuleCollection<ChildModuleType> :
        CModuleCollection<ChildModuleType>
        where ChildModuleType : Bam.Core.Module, Bam.Core.IChildModule, IRequiresSourceModule, new()
    {
        /// <summary>
        /// Add an object file from an existing SourceFile module.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="sourceModule">Source module.</param>
        /// <param name="preInitDlg">Pre init dlg.</param>
        virtual public ChildModuleType
        AddFile(
            SourceFile sourceModule,
            Bam.Core.Module.PreInitDelegate<ChildModuleType> preInitDlg = null)
        {
            var child = Bam.Core.Module.Create<ChildModuleType>(this, preInitCallback: preInitDlg);
            var requiresSource = child as IRequiresSourceModule;
            if (null == requiresSource)
            {
                throw new Bam.Core.Exception(
                    $"Module type {typeof(ChildModuleType).FullName} does not implement the interface {typeof(IRequiresSourceModule).FullName}"
                );
            }
            requiresSource.Source = sourceModule;
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        /// <summary>
        /// Get the Compiler tool
        /// </summary>
        public CompilerTool Compiler => this.Tool as CompilerTool;

        /// <summary>
        /// Set the warnings suppression delegate.
        /// </summary>
        /// <param name="suppressor"></param>
        public void
        SuppressWarningsDelegate(
            SuppressWarningsDelegate suppressor) => suppressor?.Execute(this);

        private SourceFileType
        CreateSourceFile<SourceFileType>(
            string path,
            Bam.Core.Module macroModuleOverride,
            bool verbatim)
            where SourceFileType : Bam.Core.Module, Bam.Core.IInputPath, new()
        {
            // explicitly make a source file
            var sourceFile = Bam.Core.Module.Create<SourceFileType>(postInitCallback: (module) =>
            {
                if (verbatim)
                {
                    (module as SourceFileType).InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
                }
                else
                {
                    var macroModule = macroModuleOverride ?? this;
                    (module as SourceFileType).InputPath = macroModule.CreateTokenizedString(path);
                }
            });
            return sourceFile;
        }

        private SourceFileType
        CreateSourceFile<SourceFileType>(
            Bam.Core.TokenizedString path)
            where SourceFileType : Bam.Core.Module, Bam.Core.IInputPath, new()
        {
            // explicitly make a source file
            var sourceFile = Bam.Core.Module.Create<SourceFileType>(postInitCallback: (module) =>
            {
                (module as SourceFileType).InputPath = path;
            });
            return sourceFile;
        }

        /// /copydoc C.CModuleCollection.AddFile
        public override ChildModuleType
        AddFile(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            if (path.Contains('*'))
            {
                throw new Bam.Core.Exception(
                    $"Single path '{path}' cannot contain a wildcard character. Use AddFiles instead of AddFile"
                );
            }
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);

            var requiresSourceModule = child as IRequiresSourceModule;
            requiresSourceModule.Source = this.CreateSourceFile<SourceFile>(path, macroModuleOverride, verbatim);

            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        /// <summary>
        /// Add a single object file, given the source path, to the collection. Path must resolve to a single file.
        /// </summary>
        /// <returns>The object file module, in order to manage patches.</returns>
        /// <param name="path">Path.</param>
        public ChildModuleType
        AddFile(
            Bam.Core.TokenizedString path)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);

            var requiresSourceModule = child as IRequiresSourceModule;
            requiresSourceModule.Source = this.CreateSourceFile<SourceFile>(path);

            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        /// <summary>
        /// Take a collection of source, and clone each of its children and embed them into a collection of the same type.
        /// This is a mechanism for essentially embedding the object files that would be in a static library into a dynamic
        /// library in a cross-platform way.
        /// In the clone, private patches are copied both from the collection, and also from each child in turn.
        /// No use of any public patches is made here.
        /// </summary>
        /// <param name="otherSource">The collection of object files to embed into the current collection.</param>
        public void
        ExtendWith(
            CModuleCollection<ChildModuleType> otherSource)
        {
            foreach (var child in otherSource.Children)
            {
                var clonedChild = Bam.Core.Module.CloneWithPrivatePatches(child, this);

                // attach the cloned object file into the collection so parentage is clear for macros
                (clonedChild as Bam.Core.IChildModule).Parent = this;
                this.children.Add(clonedChild);
                this.DependsOn(clonedChild);

                // source might be a buildable module (derived from C.SourceFile), or non-buildable module (C.SourceFile), or just a path
                var clonedChildRequiresSource = clonedChild as IRequiresSourceModule;
                var sourceOfChild = (child as IRequiresSourceModule).Source;
                if (sourceOfChild is Bam.Core.ICloneModule sourceOfChildIsCloned)
                {
                    sourceOfChildIsCloned.Clone(this, (newModule) =>
                    {
                        // associate the cloned source, to the cloned object file
                        // might need to happen prior to type-specific post-cloning ops
                        clonedChildRequiresSource.Source = newModule as SourceFile;
                    });
                }
                else
                {
                    clonedChildRequiresSource.Source = sourceOfChild;
                }
            }
        }

        /// <summary>
        /// Compile this source collection against the SDK module typed.
        /// </summary>
        /// <typeparam name="SDKModule">Type of the SDK module to compile against</typeparam>
        public void
        CompileAgainstSDK<SDKModule>() where SDKModule : SDKTemplate, new()
        {
            var sdk = Bam.Core.Graph.Instance.FindReferencedModule<SDKModule>();
            if (null == sdk)
            {
                return;
            }
            this.DependsOn(sdk);
            this.UsePublicPatches(sdk);
            // TODO: this isn't right - you wouldn't include the interface to all library modules from the SDK
            foreach (var module in sdk.LibraryFilter())
            {
                if (module is IDynamicLibrary dynamicLibrary)
                {
                    foreach (var interfaceDep in dynamicLibrary.InterfaceDependencies)
                    {
                        this.DependsOn(interfaceDep);
                        this.UsePublicPatches(interfaceDep);
                    }
                }
            }
        }
    }
}
