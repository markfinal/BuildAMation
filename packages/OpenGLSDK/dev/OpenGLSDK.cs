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
namespace OpenGLSDK
{
    class OpenGL :
        C.ThirdPartyModule
    {
        class TargetFilter :
            Bam.Core.BaseTargetFilteredAttribute
        {}

        private static readonly TargetFilter winVCTarget;
        private static readonly TargetFilter winMingwTarget;
        private static readonly TargetFilter unixTarget;
        private static readonly TargetFilter osxTarget;

        static
        OpenGL()
        {
            winVCTarget = new TargetFilter();
            winVCTarget.Platform = Bam.Core.EPlatform.Windows;
            winVCTarget.ToolsetTypes = new[] { typeof(VisualC.Toolset) };

            winMingwTarget = new TargetFilter();
            winMingwTarget.Platform = Bam.Core.EPlatform.Windows;
            winMingwTarget.ToolsetTypes = new[] { typeof(Mingw.Toolset) };

            unixTarget = new TargetFilter();
            unixTarget.Platform = Bam.Core.EPlatform.Unix;

            osxTarget = new TargetFilter();
            osxTarget.Platform = Bam.Core.EPlatform.OSX;
        }

        public
        OpenGL()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(OpenGL_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        OpenGL_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            // add libraries
            var libraries = new Bam.Core.StringArray();
            if (Bam.Core.TargetUtilities.MatchFilters(target, winVCTarget))
            {
                libraries.Add(@"OPENGL32.lib");
            }
            else if (Bam.Core.TargetUtilities.MatchFilters(target, winMingwTarget))
            {
                libraries.Add("-lopengl32");
            }
            else if (Bam.Core.TargetUtilities.MatchFilters(target, unixTarget))
            {
                libraries.Add("-lGL");
            }
            else if (Bam.Core.TargetUtilities.MatchFilters(target, osxTarget))
            {
                var osxLinkerOptions = module.Options as C.ILinkerOptionsOSX;
                if (null != osxLinkerOptions)
                {
                    osxLinkerOptions.Frameworks.Add("OpenGL");
                }
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported OpenGL platform");
            }

            if (libraries.Count > 0)
            {
                linkerOptions.Libraries.AddRange (libraries);
            }
        }

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependentModules = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
