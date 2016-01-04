#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// <summary>
    /// Utility class for configuration based operations.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Convert a string into an EConfiguration, if applicable.
        /// </summary>
        /// <returns>The EConfiguration if the parameter matches, or throws a Bam.Core.Exception.</returns>
        /// <param name="configurationName">Name of the configuration to match. Case is not important.</param>
        public static EConfiguration
        FromString(
            string configurationName)
        {
            var configuration = EConfiguration.Invalid;
            if (0 == System.String.Compare(configurationName, "Debug", true))
            {
                configuration = EConfiguration.Debug;
            }
            else if (0 == System.String.Compare(configurationName, "Optimized", true))
            {
                configuration = EConfiguration.Optimized;
            }
            else if (0 == System.String.Compare(configurationName, "Profile", true))
            {
                configuration = EConfiguration.Profile;
            }
            else
            {
                throw new Exception("Configuration name '{0}' not recognized", configurationName);
            }
            return configuration;
        }
    }
}
