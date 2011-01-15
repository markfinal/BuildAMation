// <copyright file="VSSolutionBuilder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.DeclareBuilder("VSSolution", typeof(VSSolutionBuilder.VSSolutionBuilder))]

namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder : Opus.Core.IBuilder
    {
        private static string CapitalizeFirstLetter(string word)
        {
            if (System.String.IsNullOrEmpty(word))
            {
                return System.String.Empty;
            }
            char[] a = word.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        private static string GetConfigurationNameFromTarget(Opus.Core.Target target)
        {
            string platform = GetPlatformNameFromTarget(target);
            string configurationName = System.String.Format("{0}|{1}", CapitalizeFirstLetter(target.Configuration.ToString()), platform);
            return configurationName;
        }

        public static string GetPlatformNameFromTarget(Opus.Core.Target target)
        {
            string platform;
            if (target.Platform == Opus.Core.EPlatform.Win32)
            {
                platform = "Win32";
            }
            else if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                platform = "x64";
            }
            else
            {
                throw new Opus.Core.Exception("Only Win32 and Win64 are supported platforms for VisualStudio projects", false);
            }

            return platform;
        }

        internal static string RefactorPathForVCProj(string path, string outputDirectoryPath, string intermediateDirectoryPath, string projectName, System.Uri projectUri)
        {
            string refactoredPath = path;

            if (outputDirectoryPath != null)
            {
                refactoredPath = refactoredPath.Replace(outputDirectoryPath, "$(OutDir)");
            }

            if (intermediateDirectoryPath != null)
            {
                refactoredPath = refactoredPath.Replace(intermediateDirectoryPath, "$(IntDir)");
            }

            refactoredPath = refactoredPath.Replace(projectName, "$(ProjectName)");

            char splitter = refactoredPath[refactoredPath.Length - 1];
            if (!System.Char.IsLetterOrDigit(splitter))
            {
                string[] splitPath = refactoredPath.Split(new char[] { splitter });
                for (int i = 0; i < splitPath.Length; ++i)
                {
                    string split = splitPath[i];
                    if (System.String.IsNullOrEmpty(split))
                    {
                        continue;
                    }

                    split = split.Trim(new char[] { '\"' });

                    if (System.IO.Directory.Exists(split) || System.IO.File.Exists(split))
                    {
                        split = Opus.Core.RelativePathUtilities.GetPath(split, projectUri);
                        splitPath[i] = System.String.Format("\"{0}\"", split);
                    }
                }
                refactoredPath = System.String.Join(splitter.ToString(), splitPath);
            }

            return refactoredPath;
        }

        private SolutionFile solutionFile;
    }
}
