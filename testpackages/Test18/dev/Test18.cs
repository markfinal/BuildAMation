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
namespace Test18
{
    public sealed class ControlV2 :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.LinkAgainst<XV2>();
            this.LinkAgainst<YV2>();
            this.LinkAgainst<ZV2>();
        }
    }

    public sealed class XV2 :
        C.V2.StaticLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            this.CompileAgainst<YV2>();
        }
    }

    public sealed class YV2 :
        C.V2.DynamicLibrary
    {
    }

    public sealed class ZV2 :
        C.V2.StaticLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            this.CompileAgainst<XV2>();
        }
    }

    class Control : C.Application
    {
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray deps = new Bam.Core.TypeArray(
            typeof(X),
            typeof(Y),
            typeof(Z)
            );
    }

    class X : C.StaticLibrary
    {
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray deps = new Bam.Core.TypeArray(
            typeof(Y)
            );
    }

    class Y : C.DynamicLibrary
    {
    }

    class Z : C.StaticLibrary
    {
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray deps = new Bam.Core.TypeArray(
            typeof(X)
            );
    }
}
