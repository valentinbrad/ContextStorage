using DataFactory.Learning.Context;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;




namespace ContextReader
{
    class Program
    {
        static void Main(string[] args)
        {

            // Task 1: explore ways of storing data to plain text files 
            // hint: maybe use json format (Newtonsoft.Json) and store each FieldsContext into a separate json serialized file?

            // Task 2: implement a plain text context storage - it should implement the IStoreContext interface 
            // and add specific implementations for Store and ReadAllFields method declarations

            // Task 3: to be discussed after Task 1 and Task 2 are done

            // General rule: focus on producing clear, easy to understand code and try to 
            // constantly apply what you have learned from Uncle Bob videos, internet, etc into achieving this

            // If you have questions, don't hesitate to ask :)


            string path = @"C:\Users\valentin.brad\Desktop\FieldsContext";
            CreateFolder(path);

            using (WebClient client = new WebClient())
            {
               
                string jsonData = client.DownloadString(@"http://latis-pc/DataFactoryContextHost/context");


                Console.Write("\n1 for TXT \t2 for Json \n\n>>> ");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        {
                            var stronglyTypedDataTxt = JArray.Parse(jsonData);
                            CreateTextFile(stronglyTypedDataTxt);

                            break;
                        }
                    case "2":
                        {

                            var stronglyTypedDataJson = JsonConvert.DeserializeObject<ImmutableArray<FieldContext>>(jsonData);
                            FieldContext fc = new FieldContext();
                            for (int i = 0; i < stronglyTypedDataJson.Length; i++)
                            {
                                fc = stronglyTypedDataJson[i];
                                CreateJsonFile(fc, i);
                            }

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }


        }


        static void CreateFolder(string path)
        {

           

            try
            {
                // Determine whether the directory exists. 
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");                 
                }
                else
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                }

            }
            catch (DirectoryNotFoundException) 
            {
                throw ;
            }

        }
        static void CreateTextFile(JArray stronglyTypedData)
        {
            
            string path ="";

            for (int i = 0; i < stronglyTypedData.Count; i++)
            {
                try
                {
                    path = @"C:\Users\valentin.brad\Desktop\FieldsContext\FieldsContext" + i + ".txt";
                    
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        File.Create(path).Dispose();
                                             
                    }
                    using (StreamWriter writer = new StreamWriter(path))

                    {
                        writer.Write(stronglyTypedData[i]);
                    }                 
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        static void CreateJsonFile(FieldContext fc,int i)
        {

            string path = "";

            
                

                try
                {
                    
                    path = @"C:\Users\valentin.brad\Desktop\FieldsContext\FieldsContext" + i + ".json";

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        File.Create(path).Dispose();

                    }
                    using (StreamWriter writer = new StreamWriter(path))
                    using (JsonWriter jw = new JsonTextWriter(writer))
                    {
                        jw.Formatting = Formatting.Indented;

                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(jw, fc);
                    }
                    
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine(ex.ToString());
                }




            
        }



    }
}
