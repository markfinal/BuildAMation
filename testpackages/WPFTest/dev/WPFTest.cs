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
namespace WPFTest
{
    // Define module classes here
    class WPFExecutable :
        CSharp.WindowsExecutable
    {
        public
        WPFExecutable()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.applicationDefinition = Bam.Core.FileLocation.Get(sourceDir, "App.xaml");
            this.page = Bam.Core.FileLocation.Get(sourceDir, "MainWindow.xaml");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(WPFExecutable_UpdateOptions);
        }

        void
        WPFExecutable_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as CSharp.IOptions;
            options.References.Add("System.dll");
            options.References.Add("System.Data.dll");
            options.References.Add("System.Xaml.dll");
            options.References.Add("WindowsBase.dll");
            options.References.Add("PresentationCore.dll");
            options.References.Add("PresentationFramework.dll");
        }

        [CSharp.ApplicationDefinition]
        Bam.Core.Location applicationDefinition;

        [CSharp.Pages]
        Bam.Core.Location page;
    }
}
