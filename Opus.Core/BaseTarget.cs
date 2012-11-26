// <copyright file="BaseTarget.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class BaseTarget
    {
        public static BaseTarget GetInstance(EPlatform platform, EConfiguration configuration)
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

        private static System.Collections.Generic.Dictionary<int, BaseTarget> factory = new System.Collections.Generic.Dictionary<int, BaseTarget>();

        private BaseTarget(EPlatform platform, EConfiguration configuration, int hashKey)
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

        private static string GenerateKey(EPlatform platform, EConfiguration configuration)
        {
            System.Text.StringBuilder keyBuilder = new System.Text.StringBuilder();
            keyBuilder.AppendFormat("{0}-{1}", platform, configuration);
            return keyBuilder.ToString();
        }

        private static int GenerateHashKey(EPlatform platform, EConfiguration configuration)
        {
            return GenerateKey(platform, configuration).GetHashCode();
        }

        public bool HasPlatform(EPlatform platforms)
        {
            bool hasPlatform = (0 != (this.Platform & platforms));
            return hasPlatform;
        }

        public bool HasConfiguration(EConfiguration configurations)
        {
            bool hasConfiguration = (0 != (this.Configuration & configurations));
            return hasConfiguration;
        }

        public static bool operator ==(BaseTarget lhs, BaseTarget rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            bool same = (lhs.Platform == rhs.Platform) &&
                        (lhs.Configuration == rhs.Configuration);
            return same;
        }

        public static bool operator !=(BaseTarget lhs, BaseTarget rhs)
        {
            return !(lhs == rhs);
        }

        // overridden explicitly because operator== is defined
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // overridden explicitly because operator== is defined
        public override bool Equals(object obj)
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

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}{1}{2}", this.Platform, ToStringSeparator, this.Configuration);
            return builder.ToString();
        }

        public string PlatformName(char formatter)
        {
            string text = this.Platform.ToString();
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
                throw new Exception(System.String.Format("Unknown format specifier '%0'", formatter), false);
            }
        }

        public string ConfigurationName(char formatter)
        {
            string text = this.Configuration.ToString();
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
                throw new Exception(System.String.Format("Unknown format specifier '%0'", formatter), false);
            }
        }
    }
}