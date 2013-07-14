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

        static OpenGL()
        {
            winVCTarget = new TargetFilter();
            winVCTarget.Platform = Opus.Core.EPlatform.Windows;
            winVCTarget.ToolsetTypes = new[] { typeof(VisualC.Toolset) };

            winMingwTarget = new TargetFilter();
            winMingwTarget.Platform = Opus.Core.EPlatform.Windows;
            winMingwTarget.ToolsetTypes = new[] { typeof(Mingw.Toolset) };
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
            Opus.Core.StringArray libraries = new Opus.Core.StringArray();
            if (Opus.Core.TargetUtilities.MatchFilters(target, winVCTarget))
            {
                libraries.Add(@"OPENGL32.lib");
            }
            else if (Opus.Core.TargetUtilities.MatchFilters(target, winMingwTarget))
            {
                libraries.Add("-lopengl32");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported OpenGL platform");
            }
            linkerOptions.Libraries.AddRange(libraries);
        }

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependentModules = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
