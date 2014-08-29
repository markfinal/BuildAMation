#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
{
    public static class TargetUtilities
    {
        /// <summary>
        /// For a given Target, identify whether the provided filters for platform, configuration and toolchain are a match or not.
        /// </summary>
        /// <param name="target">The Target to evaluate.</param>
        /// <param name="filterInterface">The filters to look for.</param>
        /// <returns>True if the Target matches the filters, false otherwise.</returns>
        public static bool
        MatchFilters(
            Target target,
            ITargetFilters filterInterface)
        {
            var baseTarget = (BaseTarget)target;
            if (!baseTarget.HasPlatform(filterInterface.Platform))
            {
                return false;
            }
            if (!baseTarget.HasConfiguration(filterInterface.Configuration))
            {
                return false;
            }
            if (null == filterInterface.ToolsetTypes)
            {
                return true;
            }

            foreach (var toolsetType in filterInterface.ToolsetTypes)
            {
                if (target.HasToolsetType(toolsetType))
                {
                    //Log.DebugMessage("Target filter '{0}' matches target '{1}'", filterInterface.ToString(), target.ToString());
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
        public static string
        DirectoryName(
            Target target)
        {
            if (null == target.Toolset)
            {
                throw new Exception("Getting the directory name for a null Toolset is not supported");
            }
            var versionString = target.Toolset.Version((BaseTarget)target);
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}", target.ToString(), versionString);
            return builder.ToString().ToLower();
        }
    }
}
