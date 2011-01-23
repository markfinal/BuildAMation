// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(Qt.ArgumentProcessor))]

namespace Qt
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string InstallPathSwitch = "-Qt.installpath";

        public bool Process(string argument)
        {
            string[] split = argument.Split(new char[] { '=' });
            if (split.Length == 2)
            {
                if (split[0] == InstallPathSwitch)
                {
                    Opus.Core.State.AddCategory("Qt");
                    Opus.Core.State.Set("Qt", "InstallPath", split[1]);

                    return true;
                }
            }

            return false;
        }
    }
}