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

namespace Store
{
    public class MetadataContextStorage : IStoreMetadata
    {
        private static readonly string folderPathMetadata = new AppSettingsReader().GetValue("DirectoryPathForMetadata", typeof(System.String)).ToString();
        private static readonly string metadataFilePath = folderPathMetadata + "MetadataFieldContext" + ".json";
        private static readonly string storagePath = new AppSettingsReader().GetValue("DirectoryPath", typeof(System.String)).ToString();
        private List<MetadataContext> listOfMetadata = new List<MetadataContext>();

        public IEnumerable<string> QueryOnMetadata(string searchString)
        {
            return ReturnListOfFieldContextsThatContainSearchString(searchString);
        }

        public IEnumerable<MetadataContext> FetchAll()
        {
            IEnumerable<MetadataContext> listOfMetadataContexts = ReadMetadataJsonFile(metadataFilePath);
            return listOfMetadataContexts;
        }

        public void StoreMetadata(IEnumerable<FieldContext> extractionContext)
        {
            List<MetadataContext> listOfNewMetadata = CreateListOfMetadata(extractionContext);
            List<string> listOfMetadataHashId = GetListOfMetadataHashId();
            VerifyIfTheNewMetadataExistAndAddItIfNot(listOfNewMetadata, listOfMetadataHashId);
            SaveMetadataContext(listOfMetadata);
        }

        public void AddMetadata(FieldContext fc)
        {
            MetadataContext mc = new MetadataContext(fc.FieldId, fc.FieldInfo.FieldName, fc.Values[0].TextContext.Text, fc.Document.DocumentId);
            List<string> listOfMetadataHashId = GetListOfMetadataHashId();
            string hash = CalculateMD5Hash(mc.Id);
            if (!listOfMetadataHashId.Contains(hash + ".json"))
            {
                listOfMetadata.Add(mc);
                WriteJsonFile(listOfMetadata);
            }
        }

        public void DeleteMetadata(string id)
        {
            listOfMetadata = FetchAll().ToList();
            var metadataContext = listOfMetadata.First(l => l.Id.Equals(id));
            listOfMetadata.Remove(metadataContext);
            WriteJsonFile(listOfMetadata);
        }
        public void Update()
        {
            listOfMetadata = FetchAll().ToList();
            if (AddMetadata(listOfMetadata) || DeleteMetadata(listOfMetadata))
            {
                WriteJsonFile(listOfMetadata);
            }
        }

        private bool DeleteMetadata(List<MetadataContext> listOfMetadata)
        {
            bool sem = false;
            foreach (var item in listOfMetadata)
            {
                string hash = CalculateMD5Hash(item.Id);
                if (GetFileIdentifier(hash) == null)
                {
                    var metadataContext = listOfMetadata.First(l => l.Id.Contains(item.Id));
                    listOfMetadata.Remove(metadataContext);
                    sem = true;
                }
            }
            return sem;
        }

        private bool AddMetadata(List<MetadataContext> listOfMetadata)
        {
            List<string> pathOfAllContexts = GetAllContextsFileName();
            List<string> listOfMetadataHashId = GetListOfMetadataHashId();
            return VerifyAndAddIfAnyFieldContextWasAdded(pathOfAllContexts, listOfMetadataHashId);
        }

        private void VerifyIfTheNewMetadataExistAndAddItIfNot(List<MetadataContext> listOfNewMetadata, List<string> listOfMetadataHashId)
        {
            foreach (MetadataContext newMetadata in listOfNewMetadata)
            {
                string hash = CalculateMD5Hash(newMetadata.Id);
                if (!listOfMetadataHashId.Contains(hash + ".json"))
                {
                    listOfMetadata.Add(newMetadata);
                }
            }
        }

        private bool VerifyAndAddIfAnyFieldContextWasAdded(List<string> pathOfAllContexts, List<string> listOfMetadataHashId)
        {
            bool sem = false;
            foreach (var filePath in pathOfAllContexts)
            {
                if (!listOfMetadataHashId.Contains(filePath))
                {
                    MetadataContext mc = ReadJsonFile(storagePath + filePath);
                    listOfMetadata.Add(mc);
                    sem = true;
                }
            }
            return sem;
        }

        private List<string> GetListOfMetadataHashId()
        {
            listOfMetadata = FetchAll().ToList();
            List<string> listOfMetadataHashId = new List<string>();
            foreach (MetadataContext mc in listOfMetadata)
            {
                string hash = CalculateMD5Hash(mc.Id);
                listOfMetadataHashId.Add(hash + ".json");
            }
            return listOfMetadataHashId;
        }

        private ImmutableArray<MetadataContext> ReadMetadataJsonFile(string filePath)
        {
            TryCreateFolderIfNotExist(folderPathMetadata);
            CreateFileIfNotExist();
            using (TextReader textReader = File.OpenText(filePath))
            {
                string context = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject<ImmutableArray<MetadataContext>>(context);
            }
        }

        private static void CreateFileIfNotExist()
        {
            if (!File.Exists(metadataFilePath))
            {
                using (TextWriter textWriter = File.CreateText(metadataFilePath))
                {
                    textWriter.Write("[]");
                }
            }
        }

        private MetadataContext ReadJsonFile(string filePath)
        {
            TryCreateFolderIfNotExist(folderPathMetadata);
            CreateFileIfNotExist();
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
                listOfMetadata.Add(mc);
            }
            return listOfMetadata;
        }

        private void WriteJsonFile(IEnumerable<MetadataContext> listOfMedatada)
        {
            TryCreateFolderIfNotExist(folderPathMetadata);
            VerifyFileExistenceAndRemoveIfExists(metadataFilePath);
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

        private List<string> GetAllContextsFileName()
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return di.EnumerateFiles().Select(f => f.Name).ToList();
        }

        private string GetFilePathUsingFullNameOfFile(string id)
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return (di.EnumerateFiles().Select(f => f.FullName).Where(f => f.Contains(id + ".json"))).FirstOrDefault();
        }

        private void VerifyIfAnyFieldContextWasModified(string filePath, DateTime creationTime, DateTime writeTime, MetadataContext metadataContext)
        {
            if (creationTime < writeTime)
            {
                AddModifiedMetadata(metadataContext);
                File.SetCreationTime(filePath, writeTime);
            }
            else
            {
                AddUnmodifiedMetadata(metadataContext);
            }
        }

        private void AddModifiedMetadata(MetadataContext metadataContext)
        {
            listOfMetadata.Add(metadataContext);
        }

        private void AddUnmodifiedMetadata(MetadataContext metadataContext)
        {
            listOfMetadata.Add(metadataContext);
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
