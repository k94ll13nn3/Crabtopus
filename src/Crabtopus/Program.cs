using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crabtopus
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ReadOnlySpan<char> content = File
                .ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Wizards Of The Coast\MTGA\output_log.txt")
                .AsSpan();
            List<Blob> blobs = FindBlobs(content.ToString());

            // Sample
            Blob blob = blobs.Find(x => x.Name.StartsWith("PlayerInventory.GetPlayerInventory"));
            Console.WriteLine(JObject.Parse(ReadBlob(blob, content)).ToString(Formatting.Indented));
        }

        private static List<Blob> FindBlobs(string content)
        {
            var blobs = new List<Blob>();
            MatchCollection matches = Regex.Matches(content, @"(?:<== (\w*\.\w*\([\d]+\))[\r\n]*{)+", RegexOptions.Singleline);
            foreach (Match item in matches)
            {
                blobs.Add(new Blob(item.Groups[1].Value, false));
            }

            matches = Regex.Matches(content, @"(?:<== (\w*\.\w*\([\d]+\))[\r\n]*{)+", RegexOptions.Singleline);
            foreach (Match item in matches)
            {
                blobs.Add(new Blob(item.Groups[1].Value, true));
            }

            return blobs;
        }

        private static string ReadBlob(Blob blob, in ReadOnlySpan<char> content)
        {
            char start;
            char end;
            int countStart = 1;
            int countEnd = 0;

            if (blob.IsArray)
            {
                start = '[';
                end = ']';
            }
            else
            {
                start = '{';
                end = '}';
            }

            int indexOfBlob = content.IndexOf($"<== {blob.Name}".AsSpan());
            int indexOfStart = content.Slice(indexOfBlob).IndexOf(start);
            ReadOnlySpan<char> sliceToSearch = content.Slice(indexOfBlob + indexOfStart + 1);
            int index = 0;
            while (countStart != countEnd)
            {
                if (sliceToSearch[index] == start)
                {
                    countStart++;
                }

                if (sliceToSearch[index] == end)
                {
                    countEnd++;
                }

                index++;
            }

            return start + sliceToSearch.Slice(0, index).ToString();
        }
    }
}
