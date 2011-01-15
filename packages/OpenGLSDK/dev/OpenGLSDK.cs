// <copyright file="OpenGLSDK.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>OpenGLSDK package</summary>
// <author>Mark Final</author>
namespace OpenGLSDK
{
    class OpenGL : C.ThirdPartyModule
    {
        private const string winVCTarget = "win.*-.*-visualc";
        private const string winMingwTarget = "win.*-.*-mingw";

        public override Opus.Core.StringArray Libraries(Opus.Core.Target target)
        {
            Opus.Core.StringArray libraries = new Opus.Core.StringArray();

            if (target.MatchFilters(new string[] { winVCTarget }))
            {
                libraries.Add(@"OPENGL32.lib");
            }
            else if (target.MatchFilters(new string[] { winMingwTarget }))
            {
                libraries.Add("-lopengl32");
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported OpenGL platform");
            }

            return libraries;
        }

        [Opus.Core.DependentModules(winVCTarget)]
        Opus.Core.TypeArray winVCDependentModules = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );
    }
}
