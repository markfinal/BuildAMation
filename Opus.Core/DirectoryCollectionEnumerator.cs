// <copyright file="DirectoryCollectionEnumerator.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class DirectoryCollectionEnumerator : System.Collections.IEnumerator
    {
        private DirectoryCollection collection;
        private int enumeratorIndex;

        public DirectoryCollectionEnumerator(DirectoryCollection collection)
        {
            this.collection = collection;
            this.Reset();
        }

        public object Current
        {
            get
            {
                return this.collection[this.enumeratorIndex];
            }
        }

        public bool MoveNext()
        {
            ++this.enumeratorIndex;
            if (this.enumeratorIndex >= this.collection.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Reset()
        {
            this.enumeratorIndex = -1;
        }
    }
}