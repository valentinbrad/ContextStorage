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
    class PlainTextContextStorage : IStoreContext
    {
        private readonly string storagePath = ConfigurationManager.AppSettings["DirectoryPath"];
        private readonly string filePathForQuerry = ConfigurationManager.AppSettings["DirectoryPath"] + "A" + ".json";
        public void Store(IEnumerable<FieldContext> extractionContext)
        {
            TryCreateFolderIfNotExist();
            SaveContext(extractionContext);
        }

        public IEnumerable<FieldContext> ReadAllFields()
        {
            List<FieldContext> listOfContexts = new List<FieldContext>();
            foreach (var filePath in Directory.EnumerateFiles(storagePath, "*.json"))
            {
                listOfContexts.Add(ReadJsonFile(filePath));
            }
            return listOfContexts;
        }

        public FieldContext ReadField(string id)
        {
            string hashName = CalculateMD5Hash(id);
            string filePath = GetFilePathUsingFullNameOfFile(hashName);
            return ReadJsonFile(filePath);
        }

        public IEnumerable<FieldContext> Query(string searchString)
        {
            List<FieldContext> listOfFieldContexts = new List<FieldContext>();
            ReadJsonFileReturnListOfFieldContextsThatContainSearchString(searchString, listOfFieldContexts);
            return listOfFieldContexts;
        }

        private void SaveContext(IEnumerable<FieldContext> extractionContext)
        {
            MetadataContext metadataContext;
            using (TextWriter textWriter = File.CreateText(storagePath + "A" + ".json"))
            {
                textWriter.WriteLine("[");
                foreach (var item in extractionContext)
                {
                    WriteJsonFile(item);
                    metadataContext = InitializeMetadataContext(item);
                    var context = JsonConvert.SerializeObject(metadataContext, Formatting.Indented);
                    textWriter.WriteLine("{0}{1}", context, ",");
                }
                textWriter.Write("]");
            }
        }

        private MetadataContext InitializeMetadataContext(FieldContext item)
        {
            MetadataContext mc = new MetadataContext();
            string fieldId = item.FieldId.Replace(" ", "");
            string fieldName = item.FieldInfo.FieldName.Replace(" ", "");
            string text = item.Values[0].TextContext.Text.Replace(" ", "");
            string documentId = item.Document.DocumentId.Replace(" ", "");
            string documentIdentificationKey = fieldName + documentId;
            string hashName = CalculateMD5Hash(documentIdentificationKey);
            mc.FieldId = fieldId;
            mc.FieldName = fieldName;
            mc.Text = text;
            mc.DocumentId = documentId;
            mc.DocumentIdentificationKey = documentIdentificationKey;
            mc.HashName = hashName;
            return mc;
        }

        private void TryCreateFolderIfNotExist()
        {
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }
        }

        private void WriteJsonFile(FieldContext fc)
        {
            string hashName = CalculateMD5Hash(fc.FieldInfo.FieldName.Replace(" ", "") + fc.Document.DocumentId);
            string filePath = storagePath + hashName + ".json";
            VerifyFileExistenceAndRemoveIfExists(filePath);
            using (TextWriter textWriter = File.CreateText(filePath))
            {
                var context = JsonConvert.SerializeObject(fc, Formatting.Indented);
                textWriter.Write(context);
            }
        }

        private FieldContext ReadJsonFile(string filePath)
        {
            using (TextReader textReader = File.OpenText(filePath))
            {
                string context = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject<FieldContext>(context);
            }
        }

        private void ReadJsonFileReturnListOfFieldContextsThatContainSearchString(string searchString, List<FieldContext> listOfFieldContexts)
        {
            using (TextReader textReader = File.OpenText(filePathForQuerry))
            {
                string allText = textReader.ReadToEnd();
                var contexts = JsonConvert.DeserializeObject<ImmutableArray<MetadataContext>>(allText);
                foreach (var item in contexts)
                {
                    VerifyIfContextContainSearchString(searchString, listOfFieldContexts, item);
                }
            }
        }

        private void VerifyIfContextContainSearchString(string searchString, List<FieldContext> listOfFieldContexts, MetadataContext item)
        {
            if (item.FieldId.Contains(searchString))
            {
                AddEachFieldContextThatContainSearchStringInList(item, listOfFieldContexts);
            }
            else if (item.FieldName.Contains(searchString))
            {
                AddEachFieldContextThatContainSearchStringInList(item, listOfFieldContexts);
            }
            else if (item.DocumentId.Contains(searchString))
            {
                AddEachFieldContextThatContainSearchStringInList(item, listOfFieldContexts);
            }
            else if (item.Text.Contains(searchString))
            {
                AddEachFieldContextThatContainSearchStringInList(item, listOfFieldContexts);
            }
        }

        private void AddEachFieldContextThatContainSearchStringInList(MetadataContext item, List<FieldContext> listOfFieldContexts)
        {
            string fileName = GetFilePathUsingFullNameOfFile(item.HashName);
            listOfFieldContexts.Add(ReadJsonFile(fileName));
        }

        private static void VerifyFileExistenceAndRemoveIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetFilePathUsingFullNameOfFile(string id)
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return (di.EnumerateFiles().Select(f => f.FullName).Where(f => f.Contains(id + ".json"))).FirstOrDefault();
        }

        private List<string> GetListOfFilePathsUsingIncompleteNameOfFile(string id)
        {
            DirectoryInfo di = new DirectoryInfo(storagePath);
            return (di.EnumerateFiles().Select(f => f.FullName).Where(f => f.Contains(id))).ToList();
        }

        private void AddEachFieldContextInList(List<FieldContext> listOfFieldContexts, List<string> listOfFilePaths)
        {
            foreach (string filePath in listOfFilePaths)
            {
                listOfFieldContexts.Add(ReadJsonFile(filePath));
            }
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
    }
}
