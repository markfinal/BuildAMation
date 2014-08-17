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
    /// C/C++ dynamic library
    /// </summary>
    public partial class DynamicLibrary :
        Application,
        Bam.Core.IPostActionModules
    {
        public static readonly Bam.Core.LocationKey ImportLibraryFile = new Bam.Core.LocationKey("ImportLibraryFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey ImportLibraryDir = new Bam.Core.LocationKey("ImportLibraryDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        protected
        DynamicLibrary()
        {
            this.PostActionModuleTypes = new Bam.Core.TypeArray();
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                this.PostActionModuleTypes.Add(typeof(PosixSharedLibrarySymlinks));
            }
        }

        [LocalCompilerOptionsDelegate]
        protected static void
        DynamicLibrarySetOpusDLLBuildPreprocessorDefine(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add("D_OPUS_DYNAMICLIBRARY_BUILD");
        }

        [LocalLinkerOptionsDelegate]
        protected static void
        DynamicLibraryEnableDLL(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.DynamicLibrary;
            linkerOptions.DynamicLibrary = true;

            if (module.Options is ILinkerOptionsOSX)
            {
                (module.Options as ILinkerOptionsOSX).SuppressReadOnlyRelocations = true;
            }
        }

        public Bam.Core.TypeArray PostActionModuleTypes
        {
            get;
            private set;
        }

        #region IPostActionModules Members

        Bam.Core.TypeArray
        Bam.Core.IPostActionModules.GetPostActionModuleTypes(
            Bam.Core.BaseTarget target)
        {
            return this.PostActionModuleTypes;
        }

        #endregion
    }
}
