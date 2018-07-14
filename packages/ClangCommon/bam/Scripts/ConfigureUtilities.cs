#region License
// Copyright (c) 2010-2018, Mark Final
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
using System.Linq;
namespace ClangCommon
{
    public static class ConfigureUtilities
    {
        static ConfigureUtilities()
        {
            try
            {
                xcrunPath = Bam.Core.OSUtilities.GetInstallLocation("xcrun").First();
            }
            catch (Bam.Core.Exception)
            {
                if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    throw;
                }

                // this needs to be an executable that exists, and will return immediately
                // as it's checked for in the PrebuiltTool code
                xcrunPath = Bam.Core.OSUtilities.GetInstallLocation("dir").First();
            }
        }

        public static string
        xcrunPath
        {
            get;
            private set;
        }

        public static string
        SetSDK(
            Bam.Core.StringArray expectedSDKs,
            string definedSDK)
        {
            var allSDKs = GetValidSDKs(expectedSDKs);

            // default SDK
            if (null != definedSDK)
            {
                if (!IsSDKValid(allSDKs, definedSDK))
                {
                    throw new Bam.Core.Exception("The SDK requested, {0}, is not in the available list of SDKs:\n{1}",
                        definedSDK,
                        allSDKs.ToString('\n'));
                }
                return definedSDK;
            }
            else
            {
                return GetDefaultMacOSXSDK();
            }
        }

        public static string
        GetSDKPath(
            string sdkVersion)
        {
            return Bam.Core.OSUtilities.RunExecutable(xcrunPath, System.String.Format("--sdk {0} -show-sdk-path", sdkVersion));
        }

        private static bool
        IsSDKValid(
            Bam.Core.StringArray availableSDKs,
            string sdk)
        {
            return availableSDKs.Contains(sdk);
        }

        private static Bam.Core.StringArray
        GetValidSDKs(
            Bam.Core.StringArray expectedSDKs)
        {
            var installedSDKOutput = Bam.Core.OSUtilities.RunExecutable("xcodebuild", "-showsdks");
            if (null == installedSDKOutput)
            {
                throw new Bam.Core.Exception("Unable to locate developer SDKs. Is Xcode installed?");
            }

            var availableSDKs = new Bam.Core.StringArray();
            foreach (var line in installedSDKOutput.Split('\n'))
            {
                var index = line.IndexOf("-sdk ");
                if (-1 == index)
                {
                    continue;
                }
                availableSDKs.Add(line.Substring(index + "-sdk ".Length));
            }

            foreach (var sdk in expectedSDKs)
            {
                if (!IsSDKValid(availableSDKs, sdk))
                {
                    throw new Bam.Core.Exception("SDK {0} was not available in the Xcode installation on this machine.", sdk);
                }
            }

            return availableSDKs;
        }

        private static string
        GetDefaultSDK(
            string sdkType)
        {
            var defaultSDK = Bam.Core.OSUtilities.RunExecutable(xcrunPath, System.String.Format("--sdk {0} --show-sdk-version", sdkType));
            return System.String.Format("{0}{1}", sdkType, defaultSDK);
        }

        private static string
        GetDefaultMacOSXSDK()
        {
            return GetDefaultSDK("macosx");
        }

        private static string
        GetDefaultiPhoneSDK(
            bool useSimulator)
        {
            return useSimulator ? GetDefaultSDK("iphonesimulator") : GetDefaultSDK("iphoneos");
        }

        public static string
        GetClangVersion(
            string sdkType)
        {
            try
            {
                var versionOutput = Bam.Core.OSUtilities.RunExecutable(
                    xcrunPath,
                    System.String.Format("--sdk {0} clang --version", sdkType)
                );
                var split = versionOutput.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
                return split[0];
            }
            catch (System.NullReferenceException)
            {
                return "Unknown Clang Version";
            }
        }
    }
}
