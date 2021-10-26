using AVSDK;
using AVText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalAV.Data
{
    public class WritRef
    {
        public static bool avx { get; private set; }

        public byte v { get; private set; } = 0;
        public string vstr { get; private set; } = "1";
        public string vid { get; private set; } = "v1";
        public string wid { get; private set; } = "w1";
        public bool paren { get; private set; } = false;
        public bool parenOpen { get; private set; } = false;
        public bool parenClose { get; private set; } = false;
        public bool bov { get; private set; } = true;

        public bool jesus { get; private set; } = false;
        public bool italics { get; private set; } = false;
        public string lex { get; private set; } = "";
        public string punc { get; private set; } = null;
        private Writ176 writ = Writ176.InitializedWrit;

        public static bool Reset(ref WritRef existing, byte? verse = null)
        {
            if (existing != null)
            {
                existing.v = verse != null ? verse.Value : (byte) 0;
                existing.vstr = verse != null ? verse.Value.ToString() : "1";
                existing.vid = "v" + verse != null ? existing.vstr : "1";
                existing.wid = "w1";
                existing.paren = false;
                existing.parenOpen = false;
                existing.parenClose = false;
                existing.bov = true;
                existing.jesus = false;
                existing.italics = false;
                existing.lex = "";
                existing.punc = null;
                existing.writ = Writ176.InitializedWrit;
            }
            else
            {
                existing = new WritRef(verse);
            }
            return true;
        }

        private WritRef(byte? verse = null)
        {
            this.v = verse != null ? verse.Value : (byte)0;
            this.vstr = verse != null ? verse.Value.ToString() : "1";
            this.vid = "v" + verse != null ? this.vstr : "1";
            this.wid = "w1";
            this.paren = false;
            this.parenOpen = false;
            this.parenClose = false;
            this.bov = true;

            this.jesus = false;
            this.italics = false;
            this.lex = "";
            this.punc = null;
            this.writ = Writ176.InitializedWrit;
        }

        public bool GetWrit(UInt32 cursor)
        {
            var result = Startup.api.XWrit.GetRecord(cursor, ref writ);
            if (result)
            {
                parenClose = false;
                wid = "w" + writ.word;
                lex = WritRef.avx ? AVLexicon.GetLexModern(writ.word) : AVLexicon.GetLex(writ.word);
                if ((writ.punc & 0x10) != 0)
                {
                    var s = (lex[lex.Length - 1] | 0x20) == 's';
                    lex += (s ? "'" : "'s");
                }
                bov = (writ.trans & (byte)AVSDK.Transitions.VerseTransition) == (byte)AVSDK.Transitions.BeginingOfVerse;
                if (bov)
                {
                    vstr = (++v).ToString();
                    vid = "v" + vstr;
                }
                jesus = (writ.punc & 0x01) != 0;
                italics = (writ.punc & 0x02) != 0;

                if (((writ.punc & 0x04) != 0) && !paren)
                {
                    paren = true;
                    parenOpen = true;
                }
                else
                {
                    parenOpen = false;
                }
                punc = WritRef.PostPunc(writ.punc);

                Startup.api.XWrit.Next();

                if (paren)
                {
                    var next = Startup.api.XWrit.GetRecord(cursor, ref writ);

                    if (((writ.punc & 0x04) == 0) && !paren)
                    {
                        paren = false;
                        parenClose = true;
                    }
                }
            }
            return result;
        }
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
        public static UInt32 GetCursorForVerse(UInt16 vidx, out UInt32 last, out byte vnum)
        {
            UInt32 cursor = 0;

            byte b;
            byte c;
            byte v;
            byte w;
            byte vlast = 0;
            byte wordCnt = 0;
            last = 0;
            vnum = 0;

            if (Startup.api.XVerse.GetEntry(vidx, out b, out c, out v, out w))
            {
                vlast = v;
                wordCnt = w;
                var chapterIdx = Startup.api.XBook.books[b - 1].chapterIdx + c - 1;
                UInt16 vbase = (UInt16)(Startup.api.XChapter.chapters[chapterIdx].verseIdx - 1);

                cursor = Startup.api.XChapter.chapters[chapterIdx].writIdx;

                for (byte verse = 1; verse < vlast; verse++)
                {
                    if (!Startup.api.XVerse.GetEntry((UInt16)(vbase + verse), out b, out c, out v, out w))
                        return 0;
                    cursor += w;
                }
                last = cursor + wordCnt - 1;
            }
            vnum = vlast;
            return cursor;
        }
    }
}
