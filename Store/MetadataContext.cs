
namespace Store
{
    public class MetadataContext
    {
        public string Id;
        public string Metadata;

        public MetadataContext(string fieldId, string fieldName, string text, string documentId)
        {
            this.Id = fieldName + documentId;
            this.Metadata = fieldId + "," + fieldName + "," + text + "," + documentId;
        }

        public MetadataContext()
        {
            this.Id = "";
            this.Metadata = "";
        }
    }
}
