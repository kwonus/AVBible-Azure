using System;
using System.Linq;
using System.Threading.Tasks;
using AVSDK;
using QuelleHMI;

namespace DigitalAV.Data
{
    public class BibleSummaryData
    {
        public static (UInt32 c, UInt32 v, UInt32 w) Totals = (0, 0, 0);
        private static BookSummary[] SummaryData = new BookSummary[66];
        public static BookSummary[] GetSummaryData(IQuelleSearchResult results = null)
        {
            if (SummaryData[0] == null)
            {
                var api = Startup.api;
                int b = 1;
                foreach (var book in api.Books)
                {
                    var summary = new BookSummary();
                    summary.book = (byte)b;
                    summary.name = book.name;
                    summary.chapters = book.chapterCnt;

                    summary.verses = 0;
                    summary.words = 0;

                    for (byte c = 1; c <= book.chapterCnt; c++)
                    {
                        var chapter = api.Chapters[book.chapterIdx + c - 1];

                        summary.words += chapter.wordCnt;

                        if (b == 66 && c == 22)
                        {
                            summary.verses += 21;
                        }
                        else
                        {
                            var nextChapter = api.Chapters[book.chapterIdx + c];
                            var verses = nextChapter.verseIdx - chapter.verseIdx;
                            summary.verses += (UInt16)verses;
                        }
                    }
                    SummaryData[b - 1] = summary;
                    Totals.c += summary.chapters;
                    Totals.v += summary.verses;
                    Totals.w += summary.words;
                    b++;
                }
            }
            return SummaryData;
        }
        public static Book GetBook(byte num)
        {
            if (num >= 1 && num <= 66)
            {
                var book = Startup.api.Books[num - 1];
                return book;
            }
            return Startup.api.Books[0];
        }
    }
}
