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
namespace Bam.Core
{
namespace V2
{
    // only here because not all modules require a path
    public interface IInputPath
    {
        TokenizedString InputPath
        {
            get;
            set;
        }
    }

    public interface IChildModule
    {
        Module Parent
        {
            get;
            set;
        }
    }
}
    public delegate void
    UpdateOptionCollectionDelegate(
        IModule module,
        Target target);

    public interface IModule
    {
        event UpdateOptionCollectionDelegate UpdateOptions;

        BaseOptionCollection Options
        {
            get;
            set;
        }
    }
}
