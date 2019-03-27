using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Crabtopus
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string content = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Wizards Of The Coast\MTGA\output_log.txt");
            List<Blob> blobs = FindBlobs(content);
            foreach (Blob blob in blobs)
            {
                Console.WriteLine(blob.Name);
            }
        }

        private static List<Blob> FindBlobs(string content)
        {
            var blobs = new List<Blob>();
            MatchCollection matches = Regex.Matches(content, @"(?:(\w*\.\w*\([\d]+\))[\r\n]*{.*?})+", RegexOptions.Singleline);
            foreach (Match item in matches)
            {
                blobs.Add(new Blob(item.Groups[1].Value, false));
            }

            matches = Regex.Matches(content, @"(?:(\w*\.\w*\([\d]+\))[\r\n]*\[.*?}[\r\n]*])+", RegexOptions.Singleline);
            foreach (Match item in matches)
            {
                blobs.Add(new Blob(item.Groups[1].Value, true));
            }

            return blobs;
        }

        private static string ReadBlob(Blob blob, string content)
        {
            Match match;
            string suffix;
            string prefix;
            if (blob.IsArray)
            {
                match = Regex.Match(content, Regex.Escape(blob.Name) + @"[\r\n]*\[(.*?)}[\r\n]*]", RegexOptions.Singleline);
                suffix = "}]";
                prefix = "[";
            }
            else
            {
                match = Regex.Match(content, Regex.Escape(blob.Name) + @"[\r\n]*{(.*?)}", RegexOptions.Singleline);
                suffix = "}";
                prefix = "{";
            }

            return match.Success ? Regex.Replace(prefix + match.Groups[1].Value + suffix, @"\t|\n|\r", "") : null;
        }
    }
}
