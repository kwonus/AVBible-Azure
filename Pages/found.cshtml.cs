using DigitalAV.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;

namespace DigitalAV.Found
{
    public class FoundModel : PageModel
    {
        public SearchModel find = new();
        public (bool success, QuelleHMI.HMICommand hmi, QuelleHMI.IQuelleSearchResult result) found = (false, null, null);
        public UInt32 cursor;
        public UInt32 last;
        public byte verse;

        public WritRef wref = null;

        public string ExecuteSearch()
        {
            if (this.Request.QueryString.HasValue && (this.Request.QueryString.Value.Length > 1) && (this.Request.QueryString.Value[0] == '?'))
            {
                var spec = this.Request.QueryString.Value.Substring(1);
                found = QuelleCommand(spec);
            }
            else
            {
                found = (false, null, null);
            }
            return "";
        }
        public string GetBookChapterVerse(UInt16 vidx)
        {
            byte bk;
            byte ch;
            byte vs;
            byte ignore;

            if (Startup.api.XVerse.GetEntry(vidx, out bk, out ch, out vs, out ignore))
            {
                return Startup.api.XBook.GetBookByNum(bk).Value.name + " " + ch.ToString() + ":" + vs.ToString();
            }
            return "";
        }
        public string GetHyperlink(UInt16 vidx)
        {
            byte bk;
            byte ch;
            byte vs;
            byte ignore;

            if (Startup.api.XVerse.GetEntry(vidx, out bk, out ch, out vs, out ignore))
            {
                return "/chapter?" + bk.ToString() + "&" + ch.ToString() + "#v" + vs.ToString();
            }
            return "";
        }
        public (bool success, QuelleHMI.HMICommand hmi, QuelleHMI.IQuelleSearchResult result) QuelleCommand(string text)
        {
            (bool success, QuelleHMI.HMICommand hmi, QuelleHMI.IQuelleSearchResult result) command = (false, null, null);

            command.hmi = new QuelleHMI.HMICommand(text.Replace('+', ';')); // allow plus to be used to delimit search segments

            if (command.hmi.statement != null && command.hmi.statement.segmentation != null && command.hmi.statement.segmentation.Count >= 1 && command.hmi.errors.Count == 0)
            {
                command.result = command.hmi.statement.ExecuteEx(Startup.api);
            }
            else
            {
                command.result = null;
            }
            command.success = (command.result != null);

            return command;
        }
    }
}