using DataFactory.Learning.Context;
using System.Collections.Generic;
using System.Threading;

namespace ContextReader
{
    public interface IStoreContext
    {
        void Store(IEnumerable<FieldContext> extractionContext);
        IEnumerable<FieldContext> ReadAllFields(CancellationToken token);
    }
}
