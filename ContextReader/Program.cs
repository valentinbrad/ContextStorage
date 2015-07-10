using DataFactory.Learning.Context;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Net;

namespace ContextReader
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebClient client = new WebClient())
            {
                //download the string content from url
                string jsonData = client.DownloadString(@"http://latis-pc/DataFactoryContextHost/context");

                //deserialize the string content into actual objects - not really necessary - for data navigation purposes at the moment
                var stronglyTypedData = JsonConvert.DeserializeObject<ImmutableArray<FieldContext>>(jsonData);
            }

            // Task 1: explore ways of storing data to plain text files 
            // hint: maybe use json format (Newtonsoft.Json) and store each FieldsContext into a separate json serialized file?

            // Task 2: implement a plain text context storage - it should implement the IStoreContext interface 
            // and add specific implementations for Store and ReadAllFields method declarations

            // Task 3: to be discussed after Task 1 and Task 2 are done

            // General rule: focus on producing clear, easy to understand code and try to 
            // constantly apply what you have learned from Uncle Bob videos, internet, etc into achieving this

            // If you have questions, don't hesitate to ask :)
        }
    }
}
