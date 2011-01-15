// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(GccCommon.ArgumentProcessor))]

namespace GccCommon
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string InstallPathSwitch = "-gcc.installpath";

        public bool Process(string argument)
        {
            string[] split = argument.Split(new char[] { '=' });
            if (split.Length == 2)
            {
                if (split[0] == InstallPathSwitch)
                {
                    Opus.Core.State.AddCategory("Gcc");
                    Opus.Core.State.Set("Gcc", "InstallPath", split[1]);

                    return true;
                }
            }

            return false;
        }
    }
}