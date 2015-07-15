using DataFactory.Learning.Context;
using System.Collections.Generic;

namespace ContextReader
{
    public interface IStoreContext
    {
        void Store(IEnumerable<FieldContext> extractionContext);
        IEnumerable<FieldContext> ReadAllFields();
        FieldContext ReadField(string id);
        IEnumerable<FieldContext> ReadSomeFields(string id);
    }
}
