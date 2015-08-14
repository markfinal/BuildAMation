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
    public class BaseTarget
    {
        public static BaseTarget
        GetInstance(
            EPlatform platform,
            EConfiguration configuration)
        {
            int hashKey = GenerateHashKey(platform, configuration);

            BaseTarget target = null;
            if (factory.ContainsKey(hashKey))
            {
                target = factory[hashKey];
            }
            else
            {
                target = new BaseTarget(platform, configuration, hashKey);
            }

            return target;
        }

        public static BaseTarget
        GetInstance(
            BaseTarget baseTarget,
            EPlatform platformOverride = EPlatform.Invalid,
            EConfiguration configurationOverride = EConfiguration.Invalid)
        {
            var platform = (EPlatform.Invalid == platformOverride) ? baseTarget.Platform : platformOverride;
            var configuration = (EConfiguration.Invalid == configurationOverride) ? baseTarget.Configuration : configurationOverride;
            return GetInstance(platform, configuration);
        }

        public static BaseTarget
        GetInstance32bits(
            BaseTarget source)
        {
            if (!OSUtilities.Is64Bit(source))
            {
                return source;
            }

            if (OSUtilities.IsWindows(source))
            {
                return GetInstance(EPlatform.Win32, source.Configuration);
            }
            else if (OSUtilities.IsUnix(source))
            {
                return GetInstance(EPlatform.Unix32, source.Configuration);
            }
            else if (OSUtilities.IsOSX(source))
            {
                return GetInstance(EPlatform.OSX32, source.Configuration);
            }
            else
            {
                throw new Exception("Unrecognized platform '{0}'", source.PlatformName('='));
            }
        }

        public static BaseTarget
        GetInstance64bits(
            BaseTarget source)
        {
            if (OSUtilities.Is64Bit(source))
            {
                return source;
            }

            if (OSUtilities.IsWindows(source))
            {
                return GetInstance(EPlatform.Win64, source.Configuration);
            }
            else if (OSUtilities.IsUnix(source))
            {
                return GetInstance(EPlatform.Unix64, source.Configuration);
            }
            else if (OSUtilities.IsOSX(source))
            {
                return GetInstance(EPlatform.OSX64, source.Configuration);
            }
            else
            {
                throw new Exception("Unrecognized platform '{0}'", source.PlatformName('='));
            }
        }

        private static System.Collections.Generic.Dictionary<int, BaseTarget> factory = new System.Collections.Generic.Dictionary<int, BaseTarget>();

        private
        BaseTarget(
            EPlatform platform,
            EConfiguration configuration,
            int hashKey)
        {
            this.Platform = platform;
            this.Configuration = configuration;
            this.HashKey = hashKey;
        }

        private EPlatform Platform
        {
            get;
            set;
        }

        private EConfiguration Configuration
        {
            get;
            set;
        }

        private string Key
        {
            get;
            set;
        }

        public int HashKey
        {
            get;
            private set;
        }

        private static string
        GenerateKey(
            EPlatform platform,
            EConfiguration configuration)
        {
            var keyBuilder = new System.Text.StringBuilder();
            keyBuilder.AppendFormat("{0}-{1}", platform, configuration);
            return keyBuilder.ToString();
        }

        private static int
        GenerateHashKey(
            EPlatform platform,
            EConfiguration configuration)
        {
            return GenerateKey(platform, configuration).GetHashCode();
        }

        public bool
        HasPlatform(
            EPlatform platforms)
        {
            var hasPlatform = (0 != (this.Platform & platforms));
            return hasPlatform;
        }

        public bool
        HasConfiguration(
            EConfiguration configurations)
        {
            var hasConfiguration = (0 != (this.Configuration & configurations));
            return hasConfiguration;
        }

        public static bool
        operator==(
            BaseTarget lhs,
            BaseTarget rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            var same = (lhs.Platform == rhs.Platform) &&
                       (lhs.Configuration == rhs.Configuration);
            return same;
        }

        public static bool
        operator!=(
            BaseTarget lhs,
            BaseTarget rhs)
        {
            return !(lhs == rhs);
        }

        // overridden explicitly because operator== is defined
        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        // overridden explicitly because operator== is defined
        public override bool
        Equals(
            object obj)
        {
            return base.Equals(obj);
        }

        public static char ToStringSeparator
        {
            get
            {
                return '-';
            }
        }

        public override string
        ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", this.Platform, ToStringSeparator, this.Configuration);
            return builder.ToString();
        }

        public string
        PlatformName(
            char formatter)
        {
            var text = this.Platform.ToString();
            if (formatter == 'u')
            {
                return text.ToUpper();
            }
            else if (formatter == 'l')
            {
                return text.ToLower();
            }
            else if (formatter == 'p')
            {
                return StringUtilities.CapitalizeFirstLetter(text);
            }
            else if (formatter == '=')
            {
                return text;
            }
            else
            {
                throw new Exception("Unknown format specifier '%0'", formatter);
            }
        }

        public string
        ConfigurationName(
            char formatter)
        {
            var text = this.Configuration.ToString();
            if (formatter == 'u')
            {
                return text.ToUpper();
            }
            else if (formatter == 'l')
            {
                return text.ToLower();
            }
            else if (formatter == 'p')
            {
                return StringUtilities.CapitalizeFirstLetter(text);
            }
            else if (formatter == '=')
            {
                return text;
            }
            else
            {
                throw new Exception("Unknown format specifier '%0'", formatter);
            }
        }
    }
}
