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
namespace C
{
    /// <summary>
    /// C/C++ Windows application
    /// </summary>
    public class WindowsApplication : Application
    {
        [LocalCompilerOptionsDelegate]
        private static void WindowsApplicationCompilerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var compilerOptions = module.Options as ICCompilerOptions;
                compilerOptions.Defines.Add("_WINDOWS");
            }
        }

        [LocalLinkerOptionsDelegate]
        private static void WindowApplicationLinkerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var linkerOptions = module.Options as ILinkerOptions;
                linkerOptions.SubSystem = ESubsystem.Windows;
            }
        }
    }
}