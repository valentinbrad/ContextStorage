using DataFactory.Learning.Context;
using System.Collections.Generic;

namespace Store
{
    public interface IStoreContext
    {
        void AddContexts(IEnumerable<FieldContext> extractionContext);
        void AddContext(FieldContext extractionContext);
        IEnumerable<FieldContext> FetchAll();
        FieldContext Get(string id);
        IEnumerable<FieldContext> Query(string searchString);
        void Delete(string id);
    }
}
