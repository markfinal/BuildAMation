#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
