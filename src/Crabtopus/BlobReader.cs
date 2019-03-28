using System;
using System.Collections.Generic;
using System.Linq;

namespace Crabtopus
{
    public class BlobReader
    {
        private readonly string _content;

        public BlobReader(string content)
        {
            _content = content;
        }

        public List<Blob> GetBlobs()
        {
            ReadOnlySpan<char> content = _content.AsSpan();
            var blobs = new List<Blob>();
            int index = 0;
            while (index < content.Length)
            {
                int find = content.Slice(index).IndexOf("<== ");
                if (find == -1)
                {
                    break;
                }
                int startIndex = index + find + 4;
                index = startIndex;
                while (content[index] != '\n')
                {
                    index++;
                }

                index++;
                if (content[index] != '{' && content[index] != '[')
                {
                    continue;
                }

                string text = content.Slice(startIndex, index - startIndex - 1).ToString();

                int length = ReadBlobContent(content.Slice(index));

                blobs.Add(new Blob(text, content[index] == '[', content.Slice(index, length).ToString()));

                index += length;
            }

            return blobs.GroupBy(b => b.Name).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();
        }

        private static int ReadBlobContent(in ReadOnlySpan<char> content)
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
