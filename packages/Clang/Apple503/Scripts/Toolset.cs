#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace Clang
{
    public sealed class Toolset :
        ClangCommon.Toolset,
        XcodeBuilder.IXcodeDetails
    {
        public
        Toolset() : base()
        {
            this.toolConfig[typeof(C.ICompilerTool)]       = new Bam.Core.ToolAndOptionType(new ClangCommon.CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)]    = new Bam.Core.ToolAndOptionType(new ClangCommon.CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCCompilerTool)]   = new Bam.Core.ToolAndOptionType(new ClangCommon.CCompiler(this), typeof(ClangCommon.ObjCCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new ClangCommon.CxxCompiler(this), typeof(ClangCommon.ObjCxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)]         = new Bam.Core.ToolAndOptionType(new Linker(this), typeof(GccCommon.LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)]       = new Bam.Core.ToolAndOptionType(new GccCommon.Archiver(this), typeof(GccCommon.ArchiverOptionCollection));
            this.toolConfig[typeof(C.IPosixSharedLibrarySymlinksTool)] =
                new Bam.Core.ToolAndOptionType(new GccCommon.PosixSharedLibrarySymlinksTool(this), typeof(GccCommon.PosixSharedLibrarySymlinksOptionCollection));
        }

        protected override string
        SpecificVersion(
            Bam.Core.BaseTarget baseTarget)
        {
            return "Apple503";
        }

        protected override string
        SpecificInstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            return @"/usr/bin";
        }

        #region IXcodeDetails implementation

        XcodeBuilder.EXcodeVersion XcodeBuilder.IXcodeDetails.SupportedVersion
        {
            get
            {
                return XcodeBuilder.EXcodeVersion.V5dot1;
            }
        }

        #endregion
    }
}
