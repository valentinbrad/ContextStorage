using DataFactory.Learning.Context;
using Newtonsoft.Json;
using System;
using Store;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Diagnostics;
using System.Net;


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
        static readonly bool useElasticSearch = bool.Parse(ConfigurationManager.AppSettings["UseElastic"]);

        static void Main(string[] args)
        {
            TimeSpan begin = Process.GetCurrentProcess().TotalProcessorTime;
            try
            {
                using (WebClient client = new WebClient())
                {
                    string jsonData = client.DownloadString(@"http://latis-pc/DataFactoryContextHost/context");
                    var contexts = JsonConvert.DeserializeObject<ImmutableArray<FieldContext>>(jsonData);
                    //if (useElasticSearch)
                    //{
                    Store.PlainTextContextStorage ContextStorage = new Store.PlainTextContextStorage(new Store.MetadataContextStorage());
                    //}
                    //else
                    //{
                    //    PlainTextContextStorage ContextStorage = new PlainTextContextStorage(new ElasticMetadataStore());
                    //}
                    Console.Write("\r\n1 for Add contexts \r\n2 for Get all contexts \t\r\n3 for Get one context \t\r\n4 for Query \r\n5 for Delete context \r\n6 for Add context \r\n0 for Exit\r\n\r\n>>> ");
                    string option = Console.ReadLine();
                    while (option != "0")
                    {
                        switch (option)
                        {
                            case "1":
                                {
                                    ContextStorage.AddContexts(contexts);
                                    break;
                                }
                            case "2":
                                {
                                    IEnumerable<FieldContext> listOfContexts = new List<FieldContext>();
                                    listOfContexts = ContextStorage.FetchAll();
                                    break;
                                }
                            case "3":
                                {
                                    string fileName = contexts[21].FieldInfo.FieldName + contexts[21].Document.DocumentId;
                                    FieldContext fc = ContextStorage.Get(fileName);
                                    break;
                                }
                            case "4":
                                {
                                    IEnumerable<FieldContext> listOfContexts = new List<FieldContext>();
                                    Console.Write("Give search string: ");
                                    string searchString = Console.ReadLine();
                                    listOfContexts = ContextStorage.Query(searchString);
                                    break;
                                }
                            case "5":
                                {
                                    ContextStorage.Delete(contexts[1].FieldInfo.FieldName + contexts[1].Document.DocumentId);
                                    break;
                                }
                            case "6":
                                {
                                    ContextStorage.AddContext(contexts[1]);
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
                        Console.Write("\r\n1 for Add contexts \r\n2 for Get all contexts \t\r\n3 for Get one context \t\r\n4 for Query \r\n5 for Delete context \r\n6 for Add context \r\n0 for Exit\r\n\r\n>>> ");
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
    }
}
