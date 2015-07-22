using DataFactory.Learning.Context;
using System.Collections.Generic;

namespace ContextReader
{
    public interface IStoreContext
    {
        void Store(IEnumerable<FieldContext> extractionContext);
        IEnumerable<FieldContext> ReadAllFields();
        FieldContext ReadField(string id);
        IEnumerable<FieldContext> Query(string searchString);
        void AddContextFromOutside();
        void UpdateContextFromOutside();
        void DeleteContextFromOutside();
        void AddOneContextFromInside(FieldContext fc);
        void DeleteOneContextFromInside(FieldContext fc);
        void DeleteContextsFromInside(IEnumerable<FieldContext> extractionContext);
        void DeleteContextsWithProperty(string searchString);
    }
}
