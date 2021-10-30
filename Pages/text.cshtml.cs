using DigitalAV.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;

namespace DigitalAV.Html
{
    public class HtmlModel : PageModel
    {
        public WritRef wref = null;
        public AVSDK.Book book { get; private set; }
        public UInt16 chapterIdx { get; private set; } = 0;
        public AVSDK.Chapter chapter { get; private set; }
        public byte ch { get; private set; } = 1;
        public UInt32  first { get; private set; } = Startup.api.Chapters[0].writIdx;
        public UInt32 last { get; private set; } = (UInt32)(Startup.api.Chapters[0].writIdx + Startup.api.Chapters[0].wordCnt - 1);

        private string GetBookAndChapter()
        {
            var parts = new string[] { "foo", "1&3" }; // UriHelper.Uri.Split('#')[0].Split('?');
            if (parts.Length >= 2)
            {
                try
                {
                    parts = parts[parts.Length - 1].Split('&');
                    if (parts.Length >= 2)
                    {
                        var b = byte.Parse(parts[0]);
                        book = BibleSummaryData.GetBook(b);
                        ch = byte.Parse(parts[parts.Length - 1]);
                    }
                }
                catch
                {
                    ch = 1;
                    book = BibleSummaryData.GetBook(1);
                }
            }
            chapter = Startup.api.Chapters[book.chapterIdx + ch - 1];
            first = chapter.writIdx;
            last = (UInt32)(first + chapter.wordCnt - 1);
            return book.name + " " + ch.ToString();

        }
        public string Message { get; private set; } = "PageModel in C#";

        public void OnGet()
        {
            GetBookAndChapter();
        }
    }
}