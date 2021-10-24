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

        public static string PostPunc(byte punc)
        {
            switch (punc & 0xE0)
            {
                case 0x80: return "!";
                case 0xC0: return "?";
                case 0xE0: return ".";
                case 0xA0: return "-";
                case 0x20: return ";";
                case 0x40: return ",";
                case 0x60: return ":";
                default: return null;
            }
        }
        public static UInt32 GetCursorForVerse(UInt16 vidx, out UInt32 last)
        {
            UInt32 cursor = 0;

            byte b;
            byte c;
            byte v;
            byte w;
            byte vlast = 0;
            byte wordCnt = 0;
            last = 0;

            if (Startup.api.XVerse.GetEntry(vidx, out b, out c, out v, out w))
            {
                vlast = v;
                wordCnt = w;
                var chapterIdx = Startup.api.XBook.books[b-1].chapterIdx + c - 1;
                UInt16 vbase = (UInt16) (Startup.api.XChapter.chapters[chapterIdx].verseIdx - 1);

                cursor = Startup.api.XChapter.chapters[chapterIdx].writIdx;

                for (byte verse = 1;  verse < vlast; verse++)
                {
                    if (!Startup.api.XVerse.GetEntry((UInt16)(vbase+verse), out b, out c, out v, out w))
                        return 0;
                    cursor += w;
                }
                last = cursor + wordCnt - 1;
            }
            return cursor;
        }
    }
}
