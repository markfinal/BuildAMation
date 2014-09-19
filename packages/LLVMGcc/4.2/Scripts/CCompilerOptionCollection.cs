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
namespace LLVMGcc
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        GccCommon.CCompilerOptionCollection,
        ICCompilerOptions,
        C.ICCompilerOptionsOSX
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;

            // use C99 by default with llvm-gcc
            if (!(this is C.ICxxCompilerOptions))
            {
                (this as C.ICCompilerOptions).LanguageStandard = C.ELanguageStandard.C99;
            }

            var options = this as C.ICCompilerOptionsOSX;
            options.FrameworkSearchDirectories = new Bam.Core.DirectoryCollection();
            options.SDKVersion = "10.8";
        }
    }
}
