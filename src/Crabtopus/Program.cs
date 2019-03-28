using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crabtopus
{
    public static class Program
    {
        public static void Main()
        {
            string content = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Wizards Of The Coast\MTGA\output_log.txt");
            List<Blob> blobs = new BlobReader(content).GetBlobs();
            foreach (IGrouping<string, Blob> item in blobs.GroupBy(x => x.Type))
            {
                Console.WriteLine(item.Key);
                foreach (Blob item2 in item)
                {
                    Console.WriteLine($"\t{item2.Method}({item2.Id})");
                }
            }

            // Sample
            Blob blob = blobs.First(x => x.Method == "GetPlayerQuests");
            Console.WriteLine(blob.FullName);
            if (blob.IsArray)
            {
                Console.WriteLine(JArray.Parse(blob.Content).ToString(Formatting.Indented));
            }
            else
            {
                Console.WriteLine(JObject.Parse(blob.Content).ToString(Formatting.Indented));
            }
        }
    }
}
