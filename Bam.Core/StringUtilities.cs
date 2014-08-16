// <copyright file="StringUtilties.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public static class StringUtilities
    {
        // Pascal case
        public static string
        CapitalizeFirstLetter(
            string word)
        {
            if (System.String.IsNullOrEmpty(word))
            {
                return System.String.Empty;
            }
            var a = word.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
