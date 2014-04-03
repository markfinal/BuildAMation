// <copyright file="ICommandLineDelegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public interface ICommandLineDelegate
    {
        Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }
}