using System;

namespace DigitalAV.Data
{
    public class BookSummary
    {
        public string name { get; set; }

        public byte book { get; set; }

        public byte chapters { get; set; }

        public UInt16 verses { get; set; }

        public UInt32 words { get; set; }
    }
}
