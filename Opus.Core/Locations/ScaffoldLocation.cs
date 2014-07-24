// <copyright file="ScaffoldLocation.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// ScaffoldLocation is an abstract representation of many real Locations on disk.
    /// ScaffoldLocations are constructed from a base Location, with a search pattern added to it. This pattern may be wildcarded.
    /// Calling GetLocations() on a ScaffoldLocation will resolve the pattern to a list of real Locations, be they file or directory.
    /// </summary>
    public sealed class ScaffoldLocation :
        Location
    {
        public enum ETypeHint
        {
            Directory,
            File,
            Symlink
        }

        public
        ScaffoldLocation(
            ETypeHint typeHint)
        {
            this.TypeHint = typeHint;
            this.Exists = EExists.WillExist; // make no assumptions that a stub will exist
            this.Results = new LocationArray();
            this.Resolved = false;
        }

        public
        ScaffoldLocation(
            Location baseLocation,
            string pattern,
            ETypeHint typeHint) :
        this(typeHint)
        {
            this.Base = baseLocation;
            this.Pattern = pattern;
        }

        public
        ScaffoldLocation(
            Location baseLocation,
            string pattern,
            ETypeHint typeHint,
            EExists exists) :
        this(baseLocation, pattern, typeHint)
        {
            this.Exists = exists;
        }

        public
        ScaffoldLocation(
            Location baseLocation,
            ProxyModulePath proxyPath,
            ETypeHint typeHint) :
        this(typeHint)
        {
            this.Base = baseLocation;
            this.ProxyPath = proxyPath;
        }

        public
        ScaffoldLocation(
            Location baseLocation,
            ProxyModulePath proxyPath,
            ETypeHint typeHint,
            EExists exists) :
        this(baseLocation, proxyPath, typeHint)
        {
            this.Exists = exists;
        }

        private bool Resolved
        {
            get;
            set;
        }

        public ETypeHint TypeHint
        {
            get;
            private set;
        }

        public Location Base
        {
            get;
            private set;
        }

        public string Pattern
        {
            get;
            private set;
        }

        private ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        public void
        SetReference(
            Location reference)
        {
            if (this.IsValid)
            {
                throw new Exception("Cannot set a reference on a Location that is not a stub");
            }

            this.Base = reference;
            this.Exists = reference.Exists;
        }

        public void
        SpecifyStub(
            Location baseLocation,
            string pattern,
            Location.EExists exists)
        {
            if (!baseLocation.IsValid)
            {
                throw new Exception("Cannot specify a stub scaffold with an invalid base location");
            }

            this.Base    = baseLocation;
            this.Pattern = pattern;
            this.Exists  = exists;
        }

        private void
        ResolveDirectory(
            Location directory)
        {
            if (null == this.Pattern)
            {
                if ((null == this.ProxyPath) || this.ProxyPath.Empty)
                {
                    this.Results.AddUnique(directory);
                    return;
                }

                var proxiedBase = this.ProxyPath.Combine(directory);
                this.Results.AddUnique(proxiedBase);
                return;
            }
            else
            {
                var path = directory.AbsolutePath;
                if ((null != this.ProxyPath) && !this.ProxyPath.Empty)
                {
                    var newLocation = this.ProxyPath.Combine(directory);
                    path = newLocation.AbsolutePath;
                }

                var fullPath = System.IO.Path.Combine(path, this.Pattern);
                if (this.TypeHint == ETypeHint.File || this.TypeHint == ETypeHint.Symlink)
                {
                    if (this.Pattern.Contains("*"))
                    {
                        var pattern = this.Pattern;
                        var searchType = System.IO.SearchOption.TopDirectoryOnly;

                        // is it a recursive search?
                        if (this.Pattern.Equals("**"))
                        {
                            pattern = "*";
                            searchType = System.IO.SearchOption.AllDirectories;
                        }
                        var dirInfo = new System.IO.DirectoryInfo(path);
                        var files = dirInfo.GetFiles(pattern, searchType);
                        foreach (var file in files)
                        {
                            if (this.TypeHint == ETypeHint.File)
                            {
                                this.Results.AddUnique(FileLocation.Get(file.FullName, this.Exists));
                            }
                            else
                            {
                                this.Results.AddUnique(SymlinkLocation.Get(file.FullName, this.Exists));
                            }
                        }
                    }
                    else
                    {
                        if (this.TypeHint == ETypeHint.File)
                        {
                            this.Results.AddUnique(FileLocation.Get(fullPath, this.Exists));
                        }
                        else
                        {
                            this.Results.AddUnique(SymlinkLocation.Get(fullPath, this.Exists));
                        }
                    }
                }
                else
                {
                    this.Results.AddUnique(DirectoryLocation.Get(fullPath, this.Exists));
                }
            }
        }

        private void
        Resolve()
        {
            if (this.Resolved)
            {
                return;
            }

            if (this.Base is ScaffoldLocation)
            {
                var baseScaffold = this.Base as ScaffoldLocation;
                baseScaffold.Resolve();
                foreach (var result in baseScaffold.Results)
                {
                    if (result is DirectoryLocation)
                    {
                        this.ResolveDirectory(result);
                    }
                    else if (result is FileLocation || result is SymlinkLocation)
                    {
                        this.Results.Add(result);
                    }
                    else if (!result.IsValid)
                    {
                        throw new Exception("Resolved scaffold location is undefined");
                    }
                    else
                    {
                        throw new Exception("Resolved scaffold location is of an unknown type, {0}", result.GetType().ToString());
                    }
                }
            }
            else if (this.Base is DirectoryLocation)
            {
                this.ResolveDirectory(this.Base);
            }
            else if (this.Base is FileLocation | this.Base is SymlinkLocation)
            {
                this.Results.Add(this.Base);
            }
            else if (!this.IsValid) // same as asking if this.Base is null
            {
                throw new Exception("Location has not been set");
            }
            else
            {
                throw new Exception("Base location is of an unknown type, {0}", this.Base.GetType().ToString());
            }

            if (0 == this.Results.Count)
            {
                throw new Exception("Location has been resolved, but has no results. Base was '{0}'. Pattern was '{1}'", this.Base.GetSinglePath(), this.Pattern);
            }

            this.Resolved = true;
        }

        public override Location
        SubDirectory(
            string subDirName)
        {
            return new ScaffoldLocation(this, subDirName, ETypeHint.Directory, this.Exists);
        }

        public override Location
        SubDirectory(
            string subDirName,
            EExists exists)
        {
            return new ScaffoldLocation(this, subDirName, ETypeHint.Directory, exists);
        }

        private LocationArray Results
        {
            get;
            set;
        }

        public override string AbsolutePath
        {
            get
            {
                throw new Exception("Scaffold locations do not have an absolute path. Use GetLocations()");
            }
            protected set
            {
                throw new Exception("Scaffold locations cannot have their absolute path set. Use a FileLocation, SymlinkLocation or DirectoryLocation");
            }
        }

        public override LocationArray
        GetLocations()
        {
            this.Resolve();
            return this.Results;
        }

        public override bool IsValid
        {
            get
            {
                bool valid = (null != this.Base);
                return valid;
            }
        }

        public override string
        ToString()
        {
            if (this.Resolved)
            {
                return this.Results.ToString(" ");
            }
            else
            {
                return "Scaffold location is unresolved";
            }
        }
    }
}
