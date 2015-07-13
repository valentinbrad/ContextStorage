using DataFactory.Learning.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextReader
{
    class PlainTextContextStorage:IStoreContext
    {

        public void Store(IEnumerable<int> extractionContext)
        {

            var enumerator = extractionContext.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Console.WriteLine(enumerator.Current);
            }
        }

        public IEnumerable<int> ReadAllFields()
        {
            IEnumerable<int> a = new int[] { 1, 2, 3, 4, 5, 6, 7, 87 };
        return a;
        
        }

    }
}
