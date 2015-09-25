#region License
// Copyright (c) 2010-2015, Mark Final
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
using Bam.Core;
namespace C
{
    public class DynamicLibrary :
        ConsoleApplication,
        IDynamicLibrary
    {
        static public Bam.Core.FileKey ImportLibraryKey = Bam.Core.FileKey.Generate("Import Library File");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths[Key] = Bam.Core.TokenizedString.Create("$(packagebuilddir)/$(moduleoutputdir)/$(dynamicprefix)$(OutputName)$(dynamicext)", this);
            this.Macros.Add("LinkOutput", this.GeneratedPaths[Key]);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.RegisterGeneratedFile(ImportLibraryKey, Bam.Core.TokenizedString.Create("$(packagebuilddir)/$(moduleoutputdir)/$(libprefix)$(OutputName)$(libext)", this));
            }

            this.PrivatePatch(settings =>
            {
                var linker = settings as C.ICommonLinkerSettings;
                if (null != linker)
                {
                    linker.OutputType = ELinkerOutput.DynamicLibrary;
                }

                var osxLinker = settings as C.ILinkerSettingsOSX;
                if (null != osxLinker)
                {
                    osxLinker.InstallName = Bam.Core.TokenizedString.Create("@executable_path/@filename($(LinkOutput))", this);
                }
            });
        }

        // TODO: what is this used for?
        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> Source
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module>(this.sourceModules.ToArray());
            }
        }

        public override CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateCSourceContainer(wildcardPath, macroModuleOverride, filter);
            collection.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
                (collection.Tool as C.CompilerTool).CompileAsShared(settings);
            });
            return collection;
        }

        public void
        CompileAgainstPublicly<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source module argument must be passed to {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, this.ToString());
            }

            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
                this.UsePublicPatches(dependent);
            }
        }
    }
}
