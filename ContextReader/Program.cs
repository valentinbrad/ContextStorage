using DataFactory.Learning.Context;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;


// Task 1: explore ways of storing data to plain text files 
// hint: maybe use json format (Newtonsoft.Json) and store each FieldsContext into a separate json serialized file?

// Task 2: implement a plain text context storage - it should implement the IStoreContext interface 
// and add specific implementations for Store and ReadAllFields method declarations

// Task 3: to be discussed after Task 1 and Task 2 are done

// General rule: focus on producing clear, easy to understand code and try to 
// constantly apply what you have learned from Uncle Bob videos, internet, etc into achieving this

// If you have questions, don't hesitate to ask :)


namespace ContextReader
{
    class Program
    {
        static readonly string storagePath = ConfigurationManager.AppSettings["DirectoryPath"];
        static void Main(string[] args)
        {
            TimeSpan begin = Process.GetCurrentProcess().TotalProcessorTime;



            try
            {
                using (WebClient client = new WebClient())
                {
                    string jsonData = client.DownloadString(@"http://latis-pc/DataFactoryContextHost/context");
                    var contexts = JsonConvert.DeserializeObject<ImmutableArray<FieldContext>>(jsonData);
                    PlainTextContextStorage ContextStorage = new PlainTextContextStorage();
                    Console.Write("\r\n1 for Store \t2 for ReadAllFields \t3 for ReadField \t4 for Query \t5 for SaveInDifferentFormat \t0 for Exit\r\n\r\n>>> ");
                    string option = Console.ReadLine();
                    while (option != "0")
                    {
                        switch (option)
                        {
                            case "1":
                                {
                                    ContextStorage.Store(contexts);
                                    break;
                                }
                            case "2":
                                {
                                    IEnumerable<FieldContext> listOfContexts = new List<FieldContext>();
                                    listOfContexts = ContextStorage.ReadAllFields();
                                    break;
                                }
                            case "3":
                                {
                                    string fileName = contexts[21].FieldInfo.FieldName + contexts[21].Document.DocumentId;
                                    FieldContext fc = ContextStorage.ReadField(fileName);
                                    break;
                                }
                            case "4":
                                {
                                    IEnumerable<FieldContext> listOfContexts = new List<FieldContext>();
                                    Console.Write("Give field context name: ");
                                    string fieldContextIdentifier = Console.ReadLine();
                                    listOfContexts = ContextStorage.Query(fieldContextIdentifier);
                                    break;
                                }
                            case "5":
                                {
                                    TryCreateFolderIfNotExist();
                                    SaveInDifferentFormat(contexts);
                                    break;
                                }
                            case "0":
                                {
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        TimeSpan end = Process.GetCurrentProcess().TotalProcessorTime;
                        Console.WriteLine("Measured time: " + (end - begin).TotalMilliseconds + " ms.");
                        Console.Write("\r\n1 for Store \t2 for ReadAllFields \t3 for ReadField \t4 for Query \t5 for SaveInDifferentFormat \t5 for Exit\r\n\r\n>>> ");
                        option = Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        static void SaveInDifferentFormat(IEnumerable<FieldContext> contexts)
        {
            string option = ChooseFormatForFile();
            SaveInChosenFormat(contexts, option);
        }

        private static string ChooseFormatForFile()
        {
            Console.Write("\r\n1 for TXT \t2 for Json \r\n\r\n>>> ");
            string option = Console.ReadLine();
            return option;
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

        private static void SaveInChosenFormat(IEnumerable<FieldContext> contexts, string option)
        {
            switch (option)
            {
                case "1":
                    {
                        SaveInTxtFormat(contexts);
                        break;
                    }
                case "2":
                    {
                        SaveInJsonFormat(contexts);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static void SaveInJsonFormat(IEnumerable<FieldContext> contexts)
        {
            foreach (var item in contexts)
            {
                WriteJsonFile(item);
            }
        }

        private static void SaveInTxtFormat(IEnumerable<FieldContext> contexts)
        {
            foreach (var item in contexts)
            {
                string FieldContext = PrintFieldId(item) + PrintFieldInfo(item) + PrintValues(item) + PrintDocument(item);
                WriteTextFile(FieldContext, item.FieldInfo.FieldType, item.Document.DocumentId);
            }
        }

        static string PrintFieldInfo(FieldContext fc)
        {
            return "\r\nFieldInfo" + "\r\n\tFieldType\r\n\t\t" + fc.FieldInfo.FieldType + "\r\n\tFieldName\r\n\t\t" + fc.FieldInfo.FieldName + "\r\n\tValueFieldType\r\n\t\t" + fc.FieldInfo.ValueFieldType + "\r\n";
        }

        static string PrintFieldId(FieldContext fc)
        {
            return "FieldId\r\n\t" + fc.FieldId + "\r\n";
        }

        static string PrintDocument(FieldContext fc)
        {
            string Document = "\r\nDocument\r\n" + "\tDocumentId\r\n\t\t" + fc.Document.DocumentId + "\r\n\tDocumentTypeId\r\n\t\t" + fc.Document.DocumentTypeId + "\r\n\tLanguage\r\n\t\t" + fc.Document.Language + "\r\n\tTextLength\r\n\t\t" + fc.Document.TextLength + "\r\n\tPageCount\r\n\t\t" + fc.Document.PageCount + "\r\n\tTimestamp\r\n\t\t" + fc.Document.Timestamp + "\r\n\tExtractedValues\r\n";

            for (int i = 0; i < fc.Document.ExtractedValues.Length; i++)
            {
                Document = Document + "\t\t" + fc.Document.ExtractedValues[i] + "\r\n";
            }
            return Document;
        }

        static string PrintValues(FieldContext fc)
        {
            string a = "";
            for (int i = 0; i < fc.Values.Length; i++)
            {
                a = "\r\nValues\r\n" + "\tPage\r\n\t\t" + fc.Values[i].Page + "\r\n\tIndex\r\n" + "\t\tIndexInDocument\r\n\t\t\t" + fc.Values[i].Index.IndexInDocument + "\r\n\t\tLength\r\n\t\t\t" + fc.Values[i].Index.Length + "\r\n\tValue\r\n\t\t" + fc.Values[i].Value + "\r\n\tSectionContext\r\n";
                for (int j = 0; j < fc.Values[i].SectionContext.Length; j++)
                {
                    a = a + "\t\tIndex\r\n\t\t\t" + "IndexInDocument\r\n\t\t\t\t" + fc.Values[i].SectionContext[j].Index.IndexInDocument + "\r\n\t\t\t" + "Length\r\n\t\t\t\t" + fc.Values[i].SectionContext[j].Index.Length + "\r\n\t\tWordGroups\r\n";

                    for (int k = 0; k < fc.Values[i].SectionContext[j].WordGroups.Length; k++)
                    {
                        a = a + "\t\t\t" + "IndexInDocument\r\n\t\t\t\t" + fc.Values[i].SectionContext[j].WordGroups[k].IndexInDocument + "\r\n\t\t\t" + "Length\r\n\t\t\t\t" + fc.Values[i].SectionContext[j].WordGroups[k].Length + "\r\n";
                    }
                }
                a = a + "\t\tTextContext\r\n" + "\t\t\tText\r\n\t\t\t\t" + fc.Values[i].TextContext.Text + "\r\n\t\t\tLanguage\r\n\t\t\t\t" + fc.Values[i].TextContext.Language + "\r\n\t\t\tIndex\r\n" + "\t\t\t\tIndexInDocument\r\n\t\t\t\t\t" + fc.Values[i].TextContext.Index.IndexInDocument + "\r\n\t\t\t\tLength\r\n\t\t\t\t\t" + fc.Values[i].TextContext.Index.Length + "\r\n\t\tPrefixWordGroups\r\n";
                for (int j = 0; j < fc.Values[i].PrefixWordGroups.Length; j++)
                {
                    a = a + "\t\t\tIndexInDocument\r\n\t\t\t\t" + fc.Values[i].PrefixWordGroups[j].IndexInDocument + "\r\n\t\t\tLength\r\n\t\t\t\t" + fc.Values[i].PrefixWordGroups[j].Length + "\r\n";
                }
                a = a + "\t\tSuffixWordGroups\r\n";
                for (int j = 0; j < fc.Values[i].SuffixWordGroups.Length; j++)
                {
                    a = a + "\t\t\tIndexInDocument\r\n\t\t\t\t" + fc.Values[i].SuffixWordGroups[j].IndexInDocument + "\r\n\t\t\tLength\r\n\t\t\t\t" + fc.Values[i].SuffixWordGroups[j].Length + "\r\n";
                }
                a = a + "\t\tOverlappingValues\r\n";
                for (int j = 0; j < fc.Values[i].OverlappingValues.Length; j++)
                {
                    a = a + "\t\t\tReference\r\n" + "\t\t\t\tIndexInDocument\r\n\t\t\t\t\t" + fc.Values[i].OverlappingValues[j].Reference.IndexInDocument + "\r\n\t\t\t\tLength\r\n\t\t\t\t\t" + +fc.Values[i].OverlappingValues[j].Reference.Length + "\r\n\t\t\tFieldId\r\n\t\t\t\t" + fc.Values[i].OverlappingValues[j].FieldId + "\r\n";
                }
            }
            return a;
        }

        static void TryCreateFolderIfNotExist()
        {
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }
        }

        static void WriteTextFile(string FieldContext, string FieldName, string DocumentId)
        {
            string filePath = storagePath + FieldName + " " + DocumentId + ".json";
            VerifyFileExistenceAndRemoveIfExists(filePath);
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(FieldContext);
            }
        }

        static void WriteJsonFile(FieldContext fc)
        {
            string hashName = CalculateMD5Hash(fc.FieldInfo.FieldName.Replace(" ", "") + "-" + fc.Document.DocumentId);
            string filePath = storagePath + hashName + ".json";
            VerifyFileExistenceAndRemoveIfExists(filePath);
            using (TextWriter textWriter = File.CreateText(filePath))
            {
                var context = JsonConvert.SerializeObject(fc, Formatting.Indented);
                textWriter.Write(context);
            }
        }

        private static void VerifyFileExistenceAndRemoveIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}
