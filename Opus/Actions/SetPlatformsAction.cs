// <copyright file="SetPlatformsAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetPlatformsAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetPlatformsAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-platforms";
            }
        }

        public string Description
        {
            get
            {
                return "Set platforms to build";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Platforms = new Opus.Core.StringArray(arguments.Split(';'));
        }

        private Core.StringArray Platforms
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.Array<Core.EPlatform> buildPlatforms = new Opus.Core.Array<Opus.Core.EPlatform>();

            foreach (string platform in this.Platforms)
            {
                Core.EPlatform p = Core.EPlatform.Invalid;
                foreach (Core.EPlatform plat in System.Enum.GetValues(typeof(Core.EPlatform)))
                {
                    if (plat.ToString().ToLower() == platform)
                    {
                        p = plat;
                        break;
                    }
                }

                if (Core.EPlatform.Invalid == p)
                {
                    throw new Core.Exception(System.String.Format("Platform '{0}' not recognized", platform));
                }

                if (buildPlatforms.Contains(p))
                {
                    throw new Core.Exception(System.String.Format("Platform '{0}' already specified", platform), false);
                }
                else
                {
                    Core.Log.DebugMessage("Adding configuration '{0}'", p.ToString());
                    buildPlatforms.Add(p);
                }
            }

            Core.State.BuildPlatforms = buildPlatforms;

            return true;
        }
    }
}