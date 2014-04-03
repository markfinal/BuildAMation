// <copyright file="IActionCommandComparison.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IActionCommandComparison
    {
        bool Compare(string command1, string command2);
    }
}