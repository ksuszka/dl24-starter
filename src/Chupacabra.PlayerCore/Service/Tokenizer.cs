using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Chupacabra.PlayerCore.Service
{
    public class Tokenizer
    {
        private readonly ILineReader _reader;
        private readonly Queue<string> _tokens = new Queue<string>();

        public Tokenizer(ILineReader reader)
        {
            _reader = reader;
        }

        private string ReadToken()
        {
            while (_tokens.Count == 0)
            {
                var line = _reader.ReadLine();
                if (line == null)
                {
                    throw new InvalidDataException("No more tokens!");
                }
                var tokens = line.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                tokens.ForEach(_tokens.Enqueue);
            }
            return _tokens.Dequeue();
        }

        public int ReadInt()
        {
            return int.Parse(ReadToken(), NumberFormatInfo.InvariantInfo);
        }

        public int? ReadNullableInt(string nullMarker = "NONE")
        {
            var token = ReadToken();
            if (token == nullMarker) return null;
            return int.Parse(token, NumberFormatInfo.InvariantInfo);
        }

        public double ReadDouble()
        {
            return double.Parse(ReadToken(), NumberFormatInfo.InvariantInfo);
        }

        public double? ReadNullableDouble(string nullMarker = "NONE")
        {
            var token = ReadToken();
            if (token == nullMarker) return null;
            return double.Parse(token, NumberFormatInfo.InvariantInfo);
        }

        public string ReadString()
        {
            return ReadToken();
        }
    }
}
