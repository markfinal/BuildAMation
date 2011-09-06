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
            winVCTarget.Toolchains = new string[] { "visualc" };

            winMingwTarget = new TargetFilter();
            winMingwTarget.Platform = Opus.Core.EPlatform.Windows;
            winMingwTarget.Toolchains = new string[] { "mingw" };
        }

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            Opus.Core.StringArray libraries = new Opus.Core.StringArray();

            if (target.MatchFilters(winVCTarget))
            {
                libraries.Add(@"OPENGL32.lib");
            }
            else if (target.MatchFilters(winMingwTarget))
            {
                libraries.Add("-lopengl32");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported OpenGL platform");
            }

            return libraries;
        }

#if OPUS_HOST_WINDOWS
        [Opus.Core.DependentModules(Platform=Opus.Core.EPlatform.Windows, Toolchains=new string[] { "visualc" })]
        Opus.Core.TypeArray winVCDependentModules = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
#endif
    }
}
