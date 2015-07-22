
using DataFactory.Learning.Context;
using System.Collections.Generic;
namespace ContextReader
{
    public interface IStoreMetadata
    {
        void StoreMetadata(IEnumerable<FieldContext> extractionMetadataContext);
        IEnumerable<MetadataContext> ReadAllFields();
        void DeleteMetadata();
        void UpdateMetadata();
        void AddMetadata();
        void AddOneMetadataFromInside(FieldContext fc);
        void DeleteOneMetadataFromInside(FieldContext fc);
        IEnumerable<string> QueryOnMetadata(string searchString);
    }
}
