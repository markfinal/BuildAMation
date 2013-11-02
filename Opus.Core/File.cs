// <copyright file="File.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class File
    {
        static private readonly string DirectorySeparatorString = new string(System.IO.Path.DirectorySeparatorChar, 1);
        static private readonly string AltDirectorySeparatorString = new string(System.IO.Path.AltDirectorySeparatorChar, 1);

        public Location AbsoluteLocation
        {
            get;
            set;
        }

        public void Include(Location baseLocation, string pattern, ScaffoldLocation.ETypeHint typeHint)
        {
            var location = new ScaffoldLocation(baseLocation, pattern, typeHint);
            this.AbsoluteLocation = location;
        }

        public void Include(Location baseLocation, string pattern)
        {
            this.Include(baseLocation, pattern, ScaffoldLocation.ETypeHint.File);
        }

        public string AbsolutePath
        {
            get
            {
                var locations = this.AbsoluteLocation.GetLocations();
                if (locations.Count > 1)
                {
                    throw new Exception("Expands to more than one location");
                }
                return locations[0].AbsolutePath;
            }
        }

        public override string ToString()
        {
            return this.AbsoluteLocation.ToString();
        }
    }
}