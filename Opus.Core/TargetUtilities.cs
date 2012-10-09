// <copyright file="TargetUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class TargetUtilities
    {
        /// <summary>
        /// For a given Target, identify whether the provided filters for platform, configuration and toolchain are a match or not.
        /// </summary>
        /// <param name="target">The Target to evaluate.</param>
        /// <param name="filterInterface">The filters to look for.</param>
        /// <returns>True if the Target matches the filters, false otherwise.</returns>
        public static bool MatchFilters(Target target, ITargetFilters filterInterface)
        {
            BaseTarget baseTarget = (BaseTarget)target;
            if (!baseTarget.HasPlatform(filterInterface.Platform))
            {
                return false;
            }
            if (!baseTarget.HasConfiguration(filterInterface.Configuration))
            {
                return false;
            }
            foreach (string toolchain in filterInterface.Toolchains)
            {
                if (target.HasToolchain(toolchain))
                {
                    Log.DebugMessage("Target filter '{0}' matches target '{1}'", filterInterface.ToString(), target.ToString());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine a standard name for a directory for this Target.
        /// </summary>
        /// <param name="target">Target to get the directory name for.</param>
        /// <returns>The Target's directory name.</returns>
        public static string DirectoryName(Target target)
        {
            // NEW STYLE
#if true
            if (!State.Has("ToolsetInfo", target.Toolchain))
            {
                throw new Exception(System.String.Format("No tool set information registered for toolchain '{0}'.", target.Toolchain), false);
            }

            string versionString = (State.Get("ToolsetInfo", target.Toolchain) as IToolsetInfo).Version(target);
#else
            // TODO: this needs changing. See comment at the top of Target
            if (!State.Has(target.Toolchain, "Version"))
            {
                throw new Exception(System.String.Format("No 'Version' property registered for toolchain '{0}'. Is there a missing Opus.Core.RegisterTargetToolChain attribute?", target.Toolchain), false);
            }

            string versionString = State.Get(target.Toolchain, "Version") as string;
#endif

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}", target.ToString(), versionString);
            return builder.ToString().ToLower();
        }
    }
}