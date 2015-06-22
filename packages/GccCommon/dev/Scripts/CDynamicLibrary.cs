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
namespace C
{
namespace V2
{
    public partial class DynamicLibrary
    {
        partial void
        CompilerSpecificSettings(
            Bam.Core.V2.Settings settings)
        {
            // TODO: since this is compiling on non-GCC platforms, if different compilers require
            // different compiler options for a dynamic library, a partial method is not going to be the solution
            // as there will be at least two implementations of the partial method
            var gccCompiler = settings as GccCommon.V2.ICommonCompilerOptions;
            if (null == gccCompiler)
            {
                return;
            }
            gccCompiler.PositionIndependentCode = true;
        }
    }
}
    public partial class DynamicLibrary
    {
        [LocalCompilerOptionsDelegate]
        protected static void
        GccCommonDynamicLibrarySetPositionIndependentCode(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as GccCommon.ICCompilerOptions;
            if (null != compilerOptions)
            {
                compilerOptions.PositionIndependentCode = true;
            }
        }
    }
}
