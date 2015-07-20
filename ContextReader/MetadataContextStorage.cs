using DataFactory.Learning.Context;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ContextReader
{
    class MetadataContextStorage : IStoreMetadata
    {
        private static readonly string folderPathMetadata = ConfigurationManager.AppSettings["DirectoryPathForMetadata"];
        private static readonly string metadataFilePath = folderPathMetadata + "MetadataFieldContext" + ".json";
        private static readonly string storagePath = ConfigurationManager.AppSettings["DirectoryPath"];
        private List<MetadataContext> listOfMedatada = new List<MetadataContext>();

        public IEnumerable<MetadataContext> ReadAllFields()
        {
            IEnumerable<MetadataContext> listOfMetadataContexts = ReadMetadataJsonFile(metadataFilePath);
            return listOfMetadataContexts;
        }

        public void StoreMetadata(IEnumerable<FieldContext> extractionContext)
        {
            TryCreateFolderIfNotExist(folderPathMetadata);
            VerifyFileExistenceAndRemoveIfExists(metadataFilePath);
            listOfMedatada = CreateListOfMetadata(extractionContext);
            SaveMetadataContext(listOfMedatada);
        }

        public void UpdateMetadata()
        {
            List<string> pathOfAllContexts = GetAllContextsFilePath();
            foreach (var filePath in pathOfAllContexts)
            {
                DateTime creationTime = File.GetCreationTime(filePath);
                DateTime writeTime = File.GetLastWriteTime(filePath);
                VerifyIfAnyFieldContextWasModified(filePath, creationTime, writeTime);
            }
            VerifyFileExistenceAndRemoveIfExists(metadataFilePath);
            WriteJsonFile(listOfMedatada);
        }

        public void DeleteMetadata()
        {
            IEnumerable<MetadataContext> allMetadata = ReadAllFields();
            if (DeleteMetadatThatAreNoLongerExist(allMetadata))
            {
                VerifyFileExistenceAndRemoveIfExists(metadataFilePath);
                WriteJsonFile(listOfMedatada);
            }
        }

        public IEnumerable<string> QueryOnMetadata(string searchString)
        {
            return ReturnListOfFieldContextsThatContainSearchString(searchString);
        }

        private ImmutableArray<MetadataContext> ReadMetadataJsonFile(string filePath)
        {
            using (TextReader textReader = File.OpenText(filePath))
            {
                string context = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject<ImmutableArray<MetadataContext>>(context);
            }
        }

        private MetadataContext ReadJsonFile(string filePath)
        {
            using (TextReader textReader = File.OpenText(filePath))
            {
                string context = textReader.ReadToEnd();
                var fieldContext = JsonConvert.DeserializeObject<FieldContext>(context);
                return new MetadataContext(fieldContext.FieldId, fieldContext.FieldInfo.FieldName, fieldContext.Values[0].TextContext.Text, fieldContext.Document.DocumentId);
            }
        }

        private void SaveMetadataContext(IEnumerable<MetadataContext> listOfMedatada)
        {
            WriteJsonFile(listOfMedatada);
        }

        private List<MetadataContext> CreateListOfMetadata(IEnumerable<FieldContext> extractionContext)
        {
            foreach (var item in extractionContext)
            {
                MetadataContext mc = new MetadataContext(item.FieldId, item.FieldInfo.FieldName, item.Values[0].TextContext.Text, item.Document.DocumentId);
                listOfMedatada.Add(mc);
            }
            return listOfMedatada;
        }

        private void WriteJsonFile(IEnumerable<MetadataContext> listOfMedatada)
        {
            using (TextWriter textWriter = File.AppendText(metadataFilePath))
            {
                var context = JsonConvert.SerializeObject(listOfMedatada, Formatting.Indented);
                textWriter.Write(context);
            }
        }

        private void TryCreateFolderIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void VerifyFileExistenceAndRemoveIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetFileIdentifier(string id)
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return (di.EnumerateFiles().Select(f => f.Name).Where(f => f.Contains(id + ".json"))).FirstOrDefault();
        }

        private List<string> GetAllContextsFilePath()
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return di.EnumerateFiles().Select(f => f.FullName).ToList();
        }

        private string GetFilePathUsingFullNameOfFile(string id)
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return (di.EnumerateFiles().Select(f => f.FullName).Where(f => f.Contains(id + ".json"))).FirstOrDefault();
        }

        private void VerifyIfAnyFieldContextWasModified(string filePath, DateTime creationTime, DateTime writeTime)
        {
            if (creationTime < writeTime)
            {
                MetadataContext metadataContext = ReadJsonFile(filePath);
                AddModifiedMetadata(metadataContext);
                AddUnmodifiedMetadata(metadataContext);
                File.SetCreationTime(filePath, writeTime);
            }
        }

        private void AddModifiedMetadata(MetadataContext metadataContext)
        {
            listOfMedatada.Add(metadataContext);
        }

        private void AddUnmodifiedMetadata(MetadataContext metadataContext)
        {
            List<MetadataContext> allMetadata = ReadAllFields().ToList();
            listOfMedatada = allMetadata.Where(x => x.Id != metadataContext.Id).ToList();
        }

        private bool DeleteMetadatThatAreNoLongerExist(IEnumerable<MetadataContext> allMetadata)
        {
            bool sem = false;
            listOfMedatada = allMetadata.ToList();
            for (int i = 0; i < listOfMedatada.Count; i++)
            {
                string hash = CalculateMD5Hash(listOfMedatada[i].Id);
                if (GetFileIdentifier(hash) == null)
                {
                    listOfMedatada.RemoveAt(i);
                    sem = true;
                    i--;
                }
            }
            return sem;
        }

        static string CalculateMD5Hash(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private List<string> ReturnListOfFieldContextsThatContainSearchString(string searchString)
        {
            List<string> ListOfContextsThatContainSearchString = new List<string>();
            using (TextReader textReader = File.OpenText(metadataFilePath))
            {
                string allText = textReader.ReadToEnd();
                var contexts = JsonConvert.DeserializeObject<ImmutableArray<MetadataContext>>(allText);
                foreach (var item in contexts)
                {
                    VerifyIfContextContainSearchString(searchString, ListOfContextsThatContainSearchString, item);
                }
            }
            return ListOfContextsThatContainSearchString;
        }

        private void VerifyIfContextContainSearchString(string searchString, List<string> ListOfContextsThatContainSearchString, MetadataContext item)
        {
            if ((item.Metadata).Contains(searchString))
            {
                string hash = CalculateMD5Hash(item.Id);
                ListOfContextsThatContainSearchString.Add(GetFilePathUsingFullNameOfFile(hash));
            }
        }
    }
}
