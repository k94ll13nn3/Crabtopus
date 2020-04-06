﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crabtopus.Model;

namespace Crabtopus
{
    internal class LogReader
    {
        private const string Delimiter = "<== ";
        private const string EndpointDelimiter = "EndpointHashPath = ";

        public LogReader()
        {
            string logFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\Wizards Of The Coast\MTGA\output_log.txt";
            if (!File.Exists(logFilePath))
            {
                throw new InvalidOperationException($"Log file not found ({logFilePath}).");
            }

            var builder = new StringBuilder();
            using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    builder.AppendLine(line);
                }
            }

            ReadOnlySpan<char> content = builder.ToString().AsSpan();

            var endpointUri = new Uri(GetEndpoint(content));

            AssetsUri = new Uri(endpointUri.Scheme + Uri.SchemeDelimiter + endpointUri.Host);
            Version = Regex.Match(endpointUri.PathAndQuery, @"\d+_\d+").Value;
            Endpoint = endpointUri.PathAndQuery;
            Blobs = GetBlobs(content);
        }

        public Uri AssetsUri { get; }

        public List<Blob> Blobs { get; }

        public string Version { get; }

        public string Endpoint { get; }

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
                blobs.Add(new Blob(text));
            }

            // Take the last id for each blob.
            return blobs.GroupBy(b => b.Name).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();
        }
    }
}