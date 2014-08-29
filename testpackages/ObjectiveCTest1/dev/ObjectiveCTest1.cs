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
namespace ObjectiveCTest1
{
    class Program :
        C.Application
    {
        public
        Program()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                var link = module.Options as C.ILinkerOptionsOSX;
                if (null != link)
                {
                    link.Frameworks.Add("Cocoa");
                }
            };
        }

        class Source :
            C.ObjC.ObjectFileCollection
        {
            public
            Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.m");
                this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                    var compileOptions = module.Options as C.ICCompilerOptions;
                    if (null != compileOptions)
                    {
                        if (target.HasPlatform(Bam.Core.EPlatform.Unix))
                        {
                            compileOptions.IncludePaths.Add("/usr/include/GNUstep");
                        }
                    }
                };
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();
    }
}
