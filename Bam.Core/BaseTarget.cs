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
