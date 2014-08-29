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
#endregion // License
namespace VisualCCommon
{
    /// <summary>
    /// Managed C++ object file
    /// </summary>
    public class ManagedCxxObjectFile :
        C.Cxx.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.CLR;
        }
    }

    /// <summary>
    /// Pure Managed C++ object file
    /// </summary>
    public class PureManagedCxxObjectFile :
        C.Cxx.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.PureCLR;
        }
    }

    /// <summary>
    /// Safe Managed C++ object file
    /// </summary>
    public class SafeManagedCxxObjectFile :
        C.Cxx.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.SafeCLR;
        }
    }

    /// <summary>
    /// Old Syntax Managed C++ object file
    /// </summary>
    public class OldSyntaxManagedCxxObjectFile :
        C.Cxx.ObjectFile
    {
        [C.LocalCompilerOptionsDelegate]
        private static void
        ManagedCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.CompileAsManaged = EManagedCompilation.OldSyntaxCLR;
        }
    }
}
