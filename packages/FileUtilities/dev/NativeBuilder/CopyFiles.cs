// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.CopyFile copyFile, out bool success)
        {
            string sourceFilePath = copyFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

            success = false;
            return null;
        }
    }
}
