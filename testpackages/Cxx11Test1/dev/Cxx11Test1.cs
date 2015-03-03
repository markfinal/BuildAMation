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
[assembly:Bam.Core.GlobalOptionCollectionOverride(typeof(Cxx11Test1.GlobalSettings))]

namespace Cxx11Test1
{
    class GlobalSettings : Bam.Core.IGlobalOptionCollectionOverride
    {
        #region IGlobalOptionCollectionOverride implementation
        void
        Bam.Core.IGlobalOptionCollectionOverride.OverrideOptions(
            Bam.Core.BaseOptionCollection optionCollection,
            Bam.Core.Target target)
        {
            var cOptions = optionCollection as C.ICCompilerOptions;
            if (null != cOptions)
            {
                var cxxOptions = optionCollection as C.ICxxCompilerOptions;
                if (null != cxxOptions)
                {
                    // enable exceptions (some flavours of STL need it)
                    cxxOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;

                    // default C++ mode to C++11
                    // note this is on the C compiler options, but only enabled when C++
                    // options are in use
                    cOptions.LanguageStandard = C.ELanguageStandard.Cxx11;
                }
            }
        }
        #endregion
    }

    class TestProg : C.Application
    {
        class Source : C.Cxx.ObjectFileCollection
        {
            public Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows)]
        Bam.Core.TypeArray windowsDeps = new Bam.Core.TypeArray() {
            typeof(WindowsSDK.WindowsSDK)
        };
    }
}
