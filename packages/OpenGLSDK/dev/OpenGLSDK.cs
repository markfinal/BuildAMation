// <copyright file="OpenGLSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>OpenGLSDK package</summary>
// <author>Mark Final</author>
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
