﻿using DataFactory.Learning.Context;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ContextReader
{
    public class PlainTextContextStorage : IStoreContext
    {
        private static readonly string storagePath = ConfigurationManager.AppSettings["DirectoryPath"];
        readonly IStoreMetadata MetadataStorage;
        public PlainTextContextStorage(IStoreMetadata storeMetadata)
        {
            this.MetadataStorage = storeMetadata;
        }

        public void DeleteOneContextFromInside(FieldContext fc)
        {
            string hashName = CalculateMD5Hash(fc.FieldInfo.FieldName + fc.Document.DocumentId);
            string hashPath = GetFilePathUsingFullNameOfFile(hashName);
            if (hashPath != null)
            {
                File.Delete(hashPath);
                MetadataStorage.DeleteOneMetadataFromInside(fc);
            }
        }

        public void AddOneContextFromInside(FieldContext fc)
        {
            TryCreateFolderIfNotExist(storagePath);
            WriteJsonFile(fc);
            MetadataStorage.AddOneMetadataFromInside(fc);
        }

        public void DeleteContextFromOutside()
        {
            MetadataStorage.DeleteMetadata();
        }

        public void AddContextFromOutside()
        {
            MetadataStorage.AddMetadata();
        }

        public void UpdateContextFromOutside()
        {
            MetadataStorage.UpdateMetadata();
        }

        public void DeleteContextsFromInside(IEnumerable<FieldContext> extractionContext)
        {
            foreach (FieldContext fc in extractionContext)
            {
                DeleteOneContextFromInside(fc);
            }
        }

        public void DeleteContextsWithProperty(string searchString)
        {
            IEnumerable<FieldContext> extractionContext = Query(searchString);
            foreach (FieldContext fc in extractionContext)
            {
                DeleteOneContextFromInside(fc);
            }
        }

        public void Store(IEnumerable<FieldContext> extractionContext)
        {
            TryCreateFolderIfNotExist(storagePath);
            SaveContext(extractionContext);
            MetadataStorage.StoreMetadata(extractionContext);

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
            IEnumerable<string> listOfFieldContextsThatContainSearchString = this.MetadataStorage.QueryOnMetadata(searchString);
            foreach (var filePath in listOfFieldContextsThatContainSearchString)
            {
                listOfFieldContexts.Add(ReadJsonFile(filePath));
            }
            return listOfFieldContexts;
        }

        private void SaveContext(IEnumerable<FieldContext> extractionContext)
        {
            foreach (var item in extractionContext)
            {
                WriteJsonFile(item);
            }
        }

        private void TryCreateFolderIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void WriteJsonFile(FieldContext fc)
        {
            string hashName = CalculateMD5Hash(fc.FieldInfo.FieldName + fc.Document.DocumentId);
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

        private static bool VerifyFileExistenceAndRemoveIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
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
