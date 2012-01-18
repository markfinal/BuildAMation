// <copyright file="Target.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class Target : System.ICloneable, System.IComparable
    {
        private static System.Collections.Generic.Dictionary<EPlatform, System.Collections.Generic.Dictionary<EConfiguration, Target>> incompleteTargetMap = new System.Collections.Generic.Dictionary<EPlatform, System.Collections.Generic.Dictionary<EConfiguration, Target>>();

        private static System.Collections.Generic.Dictionary<EPlatform, System.Collections.Generic.Dictionary<EConfiguration, System.Collections.Generic.Dictionary<string, Target>>> completeTargetMap = new System.Collections.Generic.Dictionary<EPlatform, System.Collections.Generic.Dictionary<EConfiguration, System.Collections.Generic.Dictionary<string, Target>>>();

        private int HashCode
        {
            get;
            set;
        }

        public static Target CreateIncompleteTarget(EPlatform platform, EConfiguration configuration)
        {
            if (incompleteTargetMap.ContainsKey(platform))
            {
                if (incompleteTargetMap[platform].ContainsKey(configuration))
                {
                    return incompleteTargetMap[platform][configuration];
                }
                else
                {
                    incompleteTargetMap[platform].Add(configuration, null);
                }
            }
            else
            {
                incompleteTargetMap.Add(platform, new System.Collections.Generic.Dictionary<EConfiguration, Target>());
            }

            Target incompleteTarget = new Target(platform, configuration);
            incompleteTargetMap[platform][configuration] = incompleteTarget;
            return incompleteTarget;
        }

        public static Target CreateFullyFormedTarget(Target incompleteTarget, string toolchain)
        {
            EPlatform platform = incompleteTarget.Platform;
            EConfiguration configuration = incompleteTarget.Configuration;
            if (completeTargetMap.ContainsKey(platform))
            {
                if (completeTargetMap[platform].ContainsKey(configuration))
                {
                    if (completeTargetMap[platform][configuration].ContainsKey(toolchain))
                    {
                        return completeTargetMap[platform][configuration][toolchain];
                    }
                }
            }

            if (incompleteTargetMap.ContainsKey(platform))
            {
                if (incompleteTargetMap[platform].ContainsKey(configuration))
                {
                    Target target = incompleteTargetMap[platform][configuration];
                    Target completed = new Target(target, toolchain);

                    if (!completeTargetMap.ContainsKey(platform))
                    {
                        completeTargetMap.Add(platform, new System.Collections.Generic.Dictionary<EConfiguration, System.Collections.Generic.Dictionary<string, Target>>());
                    }
                    if (!completeTargetMap[platform].ContainsKey(configuration))
                    {
                        completeTargetMap[platform].Add(configuration, new System.Collections.Generic.Dictionary<string, Target>());
                    }
                    if (!completeTargetMap[platform][configuration].ContainsKey(toolchain))
                    {
                        completeTargetMap[platform][configuration].Add(toolchain, null);
                    }

                    completed.HashCode = completed.Key.GetHashCode();

                    completeTargetMap[platform][configuration][toolchain] = completed;
                    return completed;
                }
            }

            throw new Exception("Unable to locate incomplete target");
        }

        private Target(EPlatform platform, EConfiguration configuration)
        {
            this.IsFullyFormed = false;

            this.Platform = platform;
            this.Configuration = configuration;
            this.Toolchain = null;
        }

        private Target(Target incompleteTarget, string toolchainImplementation)
        {
            this.IsFullyFormed = true;
            this.Platform = incompleteTarget.Platform;
            this.Configuration = incompleteTarget.Configuration;
            this.Toolchain = toolchainImplementation;

            this.AddToGlobalCollection();
        }

        private Target(EPlatform platform, EConfiguration configuration, string toolchain)
        {
            this.IsFullyFormed = true;
            this.Platform = platform;
            this.Configuration = configuration;
            this.Toolchain = toolchain;
        }

        private void AddToGlobalCollection()
        {
            if (!this.IsFullyFormed)
            {
                return;
            }

            if (!State.Targets.Contains(this))
            {
                State.Targets.Add(this);
            }
        }

        public bool IsFullyFormed
        {
            get;
            private set;
        }
        
        public string Key
        {
            get
            {
                if (this.IsFullyFormed)
                {
                    string key = System.String.Format("{0}-{1}-{2}", this.Platform.ToString().ToLower(), this.Configuration.ToString().ToLower(), this.Toolchain);
                    return key;
                }
                else
                {
                    string key = System.String.Format("{0}-{1} (incomplete)", this.Platform.ToString().ToLower(), this.Configuration.ToString().ToLower());
                    return key;
                }
            }
        }

        private string directoryName = null;
        public string DirectoryName
        {
            get
            {
                if (null == this.directoryName)
                {
                    if (!State.Has(this.Toolchain, "Version"))
                    {
                        throw new Exception(System.String.Format("No 'Version' property registered for toolchain '{0}'. Is there a missing Opus.Core.RegisterTargetToolChain attribute?", this.Toolchain), false);
                    }

                    string versionString = State.Get(this.Toolchain, "Version") as string;
                    string directoryName = System.String.Format("{0}-{1}{2}-{3}", this.Platform.ToString().ToLower(), this.Toolchain, versionString, this.Configuration.ToString().ToLower());
                    this.directoryName = directoryName.ToLower();
                }
                return this.directoryName;
            }
        }

        private EPlatform platform;
        public EPlatform Platform
        {
            get
            {
                return this.platform;
            }

            private set
            {
                bool isValid = true;
                if (OSUtilities.IsWindows(value))
                {
                    if (!OSUtilities.IsWindowsHosting)
                    {
                        isValid = false;
                    }
                }
                else if (OSUtilities.IsUnix(value))
                {
                    if (!OSUtilities.IsUnixHosting)
                    {
                        isValid = false;
                    }
                }
                else if (OSUtilities.IsOSX(value))
                {
                    if (!OSUtilities.IsOSXHosting)
                    {
                        isValid = false;
                    }
                }
                else
                {
                    throw new Exception(System.String.Format("Platform '{0}' is not supported", value), false);
                }

                if (!isValid)
                {
                    throw new Exception(System.String.Format("Platform '{0}' is not supported on this OS '{1}'", value, System.Environment.OSVersion.Platform.ToString()), false);
                }

                this.platform = value;
            }
        }
        
        public EConfiguration Configuration
        {
            get;
            private set;
        }
        
        public string Toolchain
        {
            get;
            private set;
        }
        
        public override string ToString()
        {
            return this.Key;
        }

        public object Clone()
        {
            if (!this.IsFullyFormed)
            {
                throw new Exception("Cannot clone an incomplete Target", false);
            }

            Target clonedTarget = new Target(this.Platform, this.Configuration, this.Toolchain);
            return clonedTarget;
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

        public bool HasToolchain(string toolchain)
        {
            bool hasToolchain = System.Text.RegularExpressions.Regex.IsMatch(this.Toolchain.ToLower(), toolchain.ToLower());
            return hasToolchain;
        }

        public bool MatchFilters(ITargetFilters filterInterface)
        {
            if (!this.HasPlatform(filterInterface.Platform))
            {
                return false;
            }
            if (!this.HasConfiguration(filterInterface.Configuration))
            {
                return false;
            }
            foreach (string toolchain in filterInterface.Toolchains)
            {
                if (this.HasToolchain(toolchain))
                {
                    Log.DebugMessage("Target filter '{0}' matches target '{1}'", filterInterface.ToString(), this.ToString());
                    return true;
                }
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            Target objAs = obj as Target;
            int compared = this.Key.CompareTo(objAs.Key);
            return compared;
        }

        public static bool operator ==(Target lhs, Target rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (null == lhs || rhs == null)
            {
                return false;
            }

#if true
            bool same = (lhs.HashCode == rhs.HashCode);
            return same;
#else
            if (!lhs.IsFullyFormed || !rhs.IsFullyFormed)
            {
                bool platformMatch = lhs.Platform == rhs.Platform;
                bool configurationMatch = lhs.Configuration == rhs.Configuration;
                return platformMatch && configurationMatch;
            }
            else
            {
                bool keysMatch = lhs.Key == rhs.Key;
                return keysMatch;
            }
#endif
        }

        public static bool operator !=(Target lhs, Target rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}