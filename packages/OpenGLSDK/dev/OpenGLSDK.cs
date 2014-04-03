// <copyright file="OpenGLSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>OpenGLSDK package</summary>
// <author>Mark Final</author>
namespace OpenGLSDK
{
    class OpenGL : C.ThirdPartyModule
    {
        class TargetFilter : Opus.Core.BaseTargetFilteredAttribute
        {
        }

        private static readonly TargetFilter winVCTarget;
        private static readonly TargetFilter winMingwTarget;
        private static readonly TargetFilter unixTarget;
        private static readonly TargetFilter osxTarget;

        static OpenGL()
        {
            winVCTarget = new TargetFilter();
            winVCTarget.Platform = Opus.Core.EPlatform.Windows;
            winVCTarget.ToolsetTypes = new[] { typeof(VisualC.Toolset) };

            winMingwTarget = new TargetFilter();
            winMingwTarget.Platform = Opus.Core.EPlatform.Windows;
            winMingwTarget.ToolsetTypes = new[] { typeof(Mingw.Toolset) };

            unixTarget = new TargetFilter();
            unixTarget.Platform = Opus.Core.EPlatform.Unix;

            osxTarget = new TargetFilter();
            osxTarget.Platform = Opus.Core.EPlatform.OSX;
        }

        public OpenGL()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(OpenGL_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void OpenGL_LinkerOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            // add libraries
            var libraries = new Opus.Core.StringArray();
            if (Opus.Core.TargetUtilities.MatchFilters(target, winVCTarget))
            {
                libraries.Add(@"OPENGL32.lib");
            }
            else if (Opus.Core.TargetUtilities.MatchFilters(target, winMingwTarget))
            {
                libraries.Add("-lopengl32");
            }
            else if (Opus.Core.TargetUtilities.MatchFilters(target, unixTarget))
            {
                libraries.Add("-lGL");
            }
            else if (Opus.Core.TargetUtilities.MatchFilters(target, osxTarget))
            {
                var osxLinkerOptions = module.Options as C.ILinkerOptionsOSX;
                if (null != osxLinkerOptions)
                {
                    osxLinkerOptions.Frameworks.Add("OpenGL");
                }
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported OpenGL platform");
            }

            if (libraries.Count > 0)
            {
                linkerOptions.Libraries.AddRange (libraries);
            }
        }

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependentModules = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
