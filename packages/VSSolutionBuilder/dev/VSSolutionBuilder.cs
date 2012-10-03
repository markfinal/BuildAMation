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
        private static System.Type GetProjectClassType()
        {
            Opus.Core.PackageInformation toolchainPackage = Opus.Core.State.PackageInfo["VisualC"];
            if (null != toolchainPackage)
            {
                string projectClassTypeName = null;
                switch (toolchainPackage.Version)
                {
                    case "8.0":
                    case "9.0":
                        projectClassTypeName = "VSSolutionBuilder.VCProject";
                        break;

                    case "10.0":
                        projectClassTypeName = "VSSolutionBuilder.VCXBuildProject";
                        break;
                }

                System.Type projectClassType = System.Type.GetType(projectClassTypeName);
                return projectClassType;
            }
            else
            {
                toolchainPackage = Opus.Core.State.PackageInfo["DotNetFramework"];
                if (null != toolchainPackage)
                {
                    string projectClassTypeName = "VSSolutionBuilder.CSBuildProject";
                    System.Type projectClassType = System.Type.GetType(projectClassTypeName);
                    return projectClassType;
                }
                else
                {
                    throw new Opus.Core.Exception("Unable to locate a suitable toolchain package");
                }
            }
        }

        private static string GetConfigurationNameFromTarget(Opus.Core.Target target)
        {
            string platform = GetPlatformNameFromTarget(target);
            string configurationName = System.String.Format("{0}|{1}", ((Opus.Core.BaseTarget)target).ConfigurationName('p'), platform);
            return configurationName;
        }

        private static string GetConfigurationNameFromTarget(Opus.Core.Target target, string platformName)
        {
            string configurationName = System.String.Format("{0}|{1}", ((Opus.Core.BaseTarget)target).ConfigurationName('p'), platformName);
            return configurationName;
        }

        public static string GetPlatformNameFromTarget(Opus.Core.Target target)
        {
            string platform;
            if (target.HasPlatform(Opus.Core.EPlatform.Win32))
            {
                platform = "Win32";
            }
            else if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                platform = "x64";
            }
            else
            {
                throw new Opus.Core.Exception("Only Win32 and Win64 are supported platforms for VisualStudio projects", false);
            }

            return platform;
        }

        private static string UseOutDirMacro(string path, string outputDirectory)
        {
            string updatedPath = path.Replace(outputDirectory, "$(OutDir)");
            return updatedPath;
        }

        private static string UseIntDirMacro(string path, string intermediateDirectory)
        {
            string updatedPath = path.Replace(intermediateDirectory, "$(IntDir)");
            return updatedPath;
        }

        private static string UseProjectMacro(string path, string projectName)
        {
            string updatedPath = path.Replace(projectName, "$(ProjectName)");
            return updatedPath; 
        }

        // semi-colons are the splitter in arguments
        private static readonly char PathSplitter = ';';

        private static string QuotePathsWithSpaces(string path, System.Uri projectUri)
        {
            if (path.Contains(new string(new char[] { PathSplitter })))
            {
                throw new Opus.Core.Exception("Path should not contain splitter");
            }

            string quote = new string(new char[] { '\"' });
            if (path.StartsWith(quote) && path.EndsWith(quote))
            {
                return path;
            }

            // remove any stray quotes
            string quotedPath = path.Trim(new char[] { '\"' });

            // only interested in paths
            if (quotedPath.Length < 2)
            {
                return quotedPath;
            }
            // need to test local drives as well as network paths
            bool isLocalDrive = (quotedPath[1] == System.IO.Path.VolumeSeparatorChar);
            bool isNetworkDrive = (quotedPath[0] == System.IO.Path.AltDirectorySeparatorChar &&
                                   quotedPath[1] == System.IO.Path.AltDirectorySeparatorChar);
            if (!isLocalDrive && !isNetworkDrive)
            {
                return quotedPath;
            }

            quotedPath = Opus.Core.RelativePathUtilities.GetPath(quotedPath, projectUri);
            if (quotedPath.Contains(" "))
            {
                quotedPath = System.String.Format("\"{0}\"", quotedPath);
            }

            return quotedPath;
        }

        internal static string RefactorPathForVCProj(string path, string outputDirectoryPath, string intermediateDirectoryPath, string projectName, System.Uri projectUri)
        {
            if (System.String.IsNullOrEmpty(path))
            {
                Opus.Core.Log.DebugMessage("Cannot refactor an empty path for VisualStudio projects");
                return path;
            }

            string[] splitPath = path.Split(PathSplitter);

            System.Text.StringBuilder joinedPath = new System.Text.StringBuilder();
            foreach (string split in splitPath)
            {
                if (0 == split.Length)
                {
                    continue;
                }

                string refactoredPath = split;
                if (null != outputDirectoryPath)
                {
                    refactoredPath = UseOutDirMacro(refactoredPath, outputDirectoryPath);
                }
                refactoredPath = UseIntDirMacro(refactoredPath, intermediateDirectoryPath);
                refactoredPath = UseProjectMacro(refactoredPath, projectName);
                refactoredPath = QuotePathsWithSpaces(refactoredPath, projectUri);

                joinedPath.AppendFormat("{0};", refactoredPath);
            }

            return joinedPath.ToString().TrimEnd(PathSplitter);
        }

        // no intermediate directory
        internal static string RefactorPathForVCProj(string path, string outputDirectoryPath, string projectName, System.Uri projectUri)
        {
            if (System.String.IsNullOrEmpty(path))
            {
                Opus.Core.Log.DebugMessage("Cannot refactor an empty path for VisualStudio projects");
                return path;
            }

            string[] splitPath = path.Split(PathSplitter);

            System.Text.StringBuilder joinedPath = new System.Text.StringBuilder();
            foreach (string split in splitPath)
            {
                string refactoredPath = split;
                refactoredPath = UseOutDirMacro(refactoredPath, outputDirectoryPath);
                refactoredPath = UseProjectMacro(refactoredPath, projectName);
                refactoredPath = QuotePathsWithSpaces(refactoredPath, projectUri);

                joinedPath.AppendFormat("{0};", refactoredPath);
            }

            return joinedPath.ToString().TrimEnd(PathSplitter);
        }

        private SolutionFile solutionFile;
    }
}
