
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
        IEnumerable<string> QueryOnMetadata(string searchString);
        //  MetadataContext ReadField(string id);
        // IEnumerable<MetadataContext> Query(string searchString);
    }
}
