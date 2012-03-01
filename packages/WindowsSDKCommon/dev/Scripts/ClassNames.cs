// <copyright file="ClassNames.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>WindowsSDKCommon package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.Win32ResourceCompilerTool, typeof(C.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

namespace C
{
    public static partial class ClassNames
    {
        public const string Win32ResourceCompilerTool = "ClassWin32ResourceCompiler";
        public const string Win32ResourceCompilerToolOptions = "ClassWin32ResourceCompilerOptions";
    }
}
