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
namespace Gcc
{
    public sealed class Toolset :
        GccCommon.Toolset
    {
        public
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)]       = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)]    = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCCompilerTool)]   = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(GccCommon.ObjCCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(GccCommon.ObjCxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)]         = new Bam.Core.ToolAndOptionType(new Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)]       = new Bam.Core.ToolAndOptionType(new GccCommon.Archiver(this), typeof(ArchiverOptionCollection));
        }
    }
}
