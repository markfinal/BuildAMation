// <copyright file="UniqueList.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class UniqueList<T> :
        System.Collections.Generic.List<T>
    {
        public new void
        Add(
            T obj)
        {
            if (0 == this.Count)
            {
                base.Add(obj);
                return;
            }

            var comparable = obj as System.IComparable;
            if (null != comparable)
            {
                foreach (var item in this)
                {
                    if (0 == comparable.CompareTo(item))
                    {
                        return;
                    }
                }

                base.Add(obj);
                return;
            }
            else
            {
                if (!this.Contains(obj))
                {
                    base.Add(obj);
                }
            }
        }

        public override string
        ToString()
        {
            var text = new System.Text.StringBuilder();
            foreach (var item in this)
            {
                text.AppendFormat("{0} ", item.ToString());
            }
            return text.ToString();
        }
    }
}
