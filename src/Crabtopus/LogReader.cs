using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Crabtopus.Model;

namespace Crabtopus
{
    public class LogReader
    {
        private const string Delimiter = "<== ";
        private const string EndpointDelimiter = "EndpointHashPath = ";

        public Uri AssetsUri { get; private set; }

        public List<Blob> Blobs { get; private set; }

        public string Version { get; private set; }

        public string Endpoint { get; private set; }

        public void ReadLog()
        {
            string logFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\Wizards Of The Coast\MTGA\output_log.txt";
            if (!File.Exists(logFilePath))
            {
                throw new InvalidOperationException($"Log file not found ({logFilePath}).");
            }

            ReadOnlySpan<char> content = File.ReadAllText(logFilePath).AsSpan();

            var endpointUri = new Uri(GetEndpoint(content));

            AssetsUri = new Uri(endpointUri.Scheme + Uri.SchemeDelimiter + endpointUri.Host);
            Version = Regex.Match(endpointUri.PathAndQuery, @"\d+_\d+").Value;
            Endpoint = endpointUri.PathAndQuery;
            Blobs = GetBlobs(content);
        }

        private string GetEndpoint(in ReadOnlySpan<char> content)
        {
            int startIndex = content.IndexOf(EndpointDelimiter) + EndpointDelimiter.Length;
            int length = 0;
            while (content[startIndex + length] != '\n' && content[startIndex + length] != '\r')
            {
                length++;
            }

            return content.Slice(startIndex, length).ToString();
        }

        private List<Blob> GetBlobs(in ReadOnlySpan<char> content)
        {
            var blobs = new List<Blob>();
            int currentIndex = 0;
            while (currentIndex < content.Length)
            {
                int indexOfDelimiter = content.Slice(currentIndex).IndexOf(Delimiter);
                if (indexOfDelimiter == -1)
                {
                    break;
                }

                int startIndex = currentIndex + indexOfDelimiter + Delimiter.Length;
                int lengthOfText = 0;
                while (content[startIndex + lengthOfText] != '\n' && content[startIndex + lengthOfText] != '\r')
                {
                    lengthOfText++;
                }

                currentIndex = startIndex + lengthOfText;
                while (content[currentIndex] == '\n' || content[currentIndex] == '\r')
                {
                    currentIndex++;
                }

                if (content[currentIndex] != '{' && content[currentIndex] != '[')
                {
                    continue;
                }

                string text = content.Slice(startIndex, lengthOfText).ToString();
                int length = ReadBlobContent(content.Slice(currentIndex));
                blobs.Add(new Blob(text, content[currentIndex] == '[', content.Slice(currentIndex, length).ToString()));
                currentIndex += length;
            }

            // Take the last id for each blob.
            return blobs.GroupBy(b => b.Name).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();
        }

        private int ReadBlobContent(in ReadOnlySpan<char> content)
        {
            char start = content[0];
            char end = content[0] == '[' ? ']' : '}';
            int countStart = 1;
            int countEnd = 0;
            int index = 1;

            while (countStart != countEnd)
            {
                if (content[index] == start)
                {
                    countStart++;
                }

                if (content[index] == end)
                {
                    countEnd++;
                }

                index++;
            }

            return index;
        }
    }
}
