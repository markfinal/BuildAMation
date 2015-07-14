#region License
// Copyright 2010-2015 Mark Final
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
namespace ProxyTest
{
    public sealed class ProxiedObjectFileV2 :
        C.V2.ObjectFile
    {
        public ProxiedObjectFileV2()
        {
            this.Macros["proxypkgroot"] = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/../../FakePackage", this);
        }

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(proxypkgroot)/main.c", this);
        }
    }

    public sealed class ProxiedObjectFileCollectionV2 :
        C.V2.CObjectFileCollection
    {
        public ProxiedObjectFileCollectionV2()
        {
            this.Macros["proxypkgroot"] = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/../../FakePackage", this);
        }

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.AddFile("$(proxypkgroot)/main.c");
        }
    }

    class ProxiedObjectFile :
        C.ObjectFile
    {
        public
        ProxiedObjectFile()
        {
            this.Include(this.PackageLocation, "main.c");

            // note that the proxy is set AFTER the Include call, as the filename expansion is deferred
            this.ProxyPath.Assign("..", "..", "FakePackage");
        }
    }

    class ProxiedObjectFileCollection :
        C.ObjectFileCollection
    {
        public
        ProxiedObjectFileCollection()
        {
            this.Include(this.PackageLocation, "main.c");

            // note that the proxy is set AFTER the Include call, as the filename expansion is deferred
            this.ProxyPath.Assign("..", "..", "FakePackage");
        }
    }

    class ProxiedWildcardObjectFileCollection :
        C.ObjectFileCollection
    {
        public
        ProxiedWildcardObjectFileCollection()
        {
            this.Include(this.PackageLocation, "*.c");

            // note that the proxy is set AFTER the Include call, as the filename expansion is deferred
            this.ProxyPath.Assign("..", "..", "FakePackage");
        }
    }
}
