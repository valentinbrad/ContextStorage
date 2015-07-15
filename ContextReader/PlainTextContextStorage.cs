using DataFactory.Learning.Context;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace ContextReader
{
    class PlainTextContextStorage : IStoreContext
    {
        private readonly string storagePath = ConfigurationManager.AppSettings["DirectoryPath"];

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
            string filePath = GetFilePathUsingFullNameOfFile(id);
            return ReadJsonFile(filePath);
        }

        public IEnumerable<FieldContext> ReadSomeFields(string id)
        {
            List<FieldContext> listOfFieldContexts = new List<FieldContext>();
            List<string> listOfFilePaths = GetListOfFilePathsUsingIncompleteNameOfFile(id);
            AddEachFieldContextInList(listOfFieldContexts, listOfFilePaths);
            return listOfFieldContexts;
        }

        private void SaveContext(IEnumerable<FieldContext> extractionContext)
        {
            foreach (var item in extractionContext)
            {
                WriteJsonFile(item);
            }
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
            string filePath = storagePath + fc.FieldInfo.FieldName.ToString() + " " + fc.Document.DocumentId.ToString() + ".json";
            VerifyFileExistenceAndRemove(filePath);
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

        private static void VerifyFileExistenceAndRemove(string filePath)
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
    }
}
