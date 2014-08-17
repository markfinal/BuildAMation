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
namespace CSharpTest1
{
    // Define module classes here
    class SimpleLibrary :
        CSharp.Library
    {
        public
        SimpleLibrary()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source.Include(sourceDir, "simpletest.cs");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.FileCollection source = new Bam.Core.FileCollection();

#if OPUSPACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(CSharp.Assembly.OutputFile));
#endif
    }

    class SimpleExecutable :
        CSharp.Executable
    {
        public
        SimpleExecutable()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source = Bam.Core.FileLocation.Get(sourceDir, "simpleexecutable.cs");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.Location source;
    }

    class SimpleWindowExecutable :
        CSharp.WindowsExecutable
    {
        public
        SimpleWindowExecutable()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source = Bam.Core.FileLocation.Get(sourceDir, "simplewindowsexecutable.cs");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.Location source;
    }

    // disabled on Unix builds, as it fails: CS0016
    // Could not write to file `SimpleModule', cause: fileName '<blah>' must not include a path.
    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.NotUnix)]
    class SimpleModule :
        CSharp.Module
    {
        public
        SimpleModule()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source = Bam.Core.FileLocation.Get(sourceDir, "simplemodule.cs");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.Location source;
    }

    class Executable2 :
        CSharp.Executable
    {
        public
        Executable2()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source = Bam.Core.FileLocation.Get(sourceDir, "executable2.cs");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.Location source;

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(typeof(SimpleLibrary));

#if OPUSPACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(CSharp.Assembly.OutputFile));
#endif
    }

#if OPUSPACKAGE_PUBLISHER_DEV
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(Executable2);
    }
#endif

    class ExecutableReferences :
        CSharp.Executable
    {
        public
        ExecutableReferences()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.source = Bam.Core.FileLocation.Get(sourceDir, "executablexml.cs");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(ExecutableReferences_UpdateOptions);
        }

        void
        ExecutableReferences_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as CSharp.IOptions;
            options.References.Add("System.Xml.dll");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.Location source;
    }
}
