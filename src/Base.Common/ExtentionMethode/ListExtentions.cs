using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Common.ExtentionMethode;
public static class ListExtentions
{
    public static IEnumerable<List<T>> SplitList<T>(List<T> list, int nSize = 30)
    {
        if (list != null)
        {
           for (int i = 0; i < list.Count; i += nSize)
                {
                    yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
                }
        }
    }
}
