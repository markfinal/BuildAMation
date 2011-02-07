// <copyright file="DestinationDirectoryAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class DestinationDirectoryAttribute : Opus.Core.DependentModulesAttribute
    {
        public DestinationDirectoryAttribute(EDirectoryChoice directoryChoice)
            : base()
        {
            this.DirectoryChoice = directoryChoice;
        }

        public EDirectoryChoice DirectoryChoice
        {
            get;
            private set;
        }
    }
}