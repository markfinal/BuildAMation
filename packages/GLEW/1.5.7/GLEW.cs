#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace GLEW
{
    sealed class GLEWStaticV2 :
        C.V2.StaticLibrary
    {
        public GLEWStaticV2()
        {
            this.Macros.Add("GLEWRootDir", Bam.Core.V2.TokenizedString.Create("$(pkgroot)/glew-1.5.7", this));
        }

        private Bam.Core.V2.Module.PublicPatchDelegate exported = (settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                if (null != compiler)
                {
                    compiler.PreprocessorDefines.Add("GLEW_STATIC");
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(GLEWRootDir)/include", appliedTo));
                }
            };

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(GLEWRootDir)/src/glew.c", macroModuleOverride:this);
            source.PrivatePatch(settings => this.exported(settings, this));

            this.PublicPatch((settings, appliedTo) => this.exported(settings, this));

            this.CompileAgainst<OpenGLSDK.OpenGLV2>(source);
            this.CompileAgainst<WindowsSDK.WindowsSDKV2>(source);
        }
    }

    // Define module classes here
    class GLEWStatic :
        C.StaticLibrary
    {
        public
        GLEWStatic()
        {
            var glewDir = this.PackageLocation.SubDirectory("glew-1.5.7");
            var includeDir = glewDir.SubDirectory("include");
            var GLDir = includeDir.SubDirectory("GL");
            this.headerFiles.Include(GLDir, "*.h");
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var glewDir = this.PackageLocation.SubDirectory("glew-1.5.7");
                var sourceDir = glewDir.SubDirectory("src");
                this.Include(sourceDir, "glew.c");
                //this.Add(new C.ObjectFile(new Bam.Core.File(@"glew-1.5.7/src/glewinfo.c")));
                //this.Add(new C.ObjectFile(new Bam.Core.File(@"glew-1.5.7/src/visualinfo.c")));

                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(GLEW_IncludePathAndStaticDefine);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(GLEW_VCWarningLevel);
                //this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(GLEW_VCSecurity);
            }

            void
            GLEW_VCSecurity(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (module.Options is VisualCCommon.ICCompilerOptions)
                {
                    var compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }

            void
            GLEW_VCWarningLevel(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as VisualCCommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                }
            }

            [C.ExportCompilerOptionsDelegate]
            void
            GLEW_IncludePathAndStaticDefine(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                var glewDir = this.PackageLocation.SubDirectory("glew-1.5.7");
                compilerOptions.IncludePaths.Include(glewDir, "include");
                compilerOptions.Defines.Add("GLEW_STATIC");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(OpenGLSDK.OpenGL)
        );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );
    }
}
