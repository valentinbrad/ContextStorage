
using DataFactory.Learning.Context;
using System.Collections.Generic;
namespace Store
{
    public interface IStoreMetadata
    {
        void StoreMetadata(IEnumerable<FieldContext> extractionMetadataContext);
        IEnumerable<MetadataContext> FetchAll();
        void DeleteMetadata(string id);
        void Update();
        void AddMetadata(FieldContext fieldContext);
        IEnumerable<string> QueryOnMetadata(string searchString);
    }
}
