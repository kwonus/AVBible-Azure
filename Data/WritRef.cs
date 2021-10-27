using AVSDK;
using AVText;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalAV.Data
{
    public class WritRefEx
    {
        public byte v { get; private set; }
        public UInt16 w { get; private set; }
        public bool paren { get; private set; }
        public bool parenOpen { get; private set; }
        public bool parenClose { get; private set; }
        public bool bov { get; private set; }

        public bool jesus { get; private set; }
        public bool italics { get; private set; }
        public string lex { get; private set; }
        public string punc { get; private set; }
        public UInt64 strongs { get; private set; }
        public byte trans { get; private set; }
        public UInt16 pnwc { get; private set; }
        public UInt32 pos { get; private set; }
        public UInt16 lemma { get; private set; }

        internal WritRefEx(WritRef wref)
        {
            this.w = wref.w;
            this.v = wref.v;
            this.paren = wref.paren;
            this.parenOpen = wref.parenOpen;
            this.parenClose = wref.parenClose;
            this.bov = wref.bov;

            this.jesus = wref.jesus;
            this.italics = wref.italics;
            this.lex = wref.lex;
            this.punc = wref.punc;
            this.strongs = wref.writ.strongs;
            this.trans = wref.writ.trans;
            this.pnwc = wref.writ.pnwc;
            this.pos = wref.writ.pos;
            this.lemma = wref.writ.lemma;
        }
    }
    public class WritRef
    {
        public static bool AVX { get; private set; } = false;
        public static void SetModern(bool modern)
        {
            WritRef.AVX = modern;
        }

        public byte v { get; private set; } = 0;
        public UInt16 w { get; private set; } = 0;
        public string vid
        {
            get
            {
                return "v" + v.ToString();
            }
        }
        public string vstr
        {
            get
            {
                return v.ToString();
            }
        }
        public string wid
        {
            get
            {
                return "w" + w.ToString();
            }
        }
        public bool paren { get; private set; } = false;
        public bool parenOpen { get; private set; } = false;
        public bool parenClose { get; private set; } = false;
        public bool bov { get; private set; } = true;

        public bool jesus { get; private set; } = false;
        public bool italics { get; private set; } = false;
        public string lex { get; private set; } = "";
        public string punc { get; private set; } = null;
        internal Writ176 writ = Writ176.InitializedWrit;

        public static bool Reset(ref WritRef existing, byte? verse = null)
        {
            if (existing != null)
            {
                existing.v = verse != null ? verse.Value : (byte) 0;
                existing.w = 0;
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
            this.w = 0;
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
                w = (UInt16) (writ.word & 0x3FFF);
                lex = WritRef.AVX ? AVLexicon.GetLexModern(writ.word) : AVLexicon.GetLex(writ.word);
                if ((writ.punc & 0x10) != 0)
                {
                    var s = (lex[lex.Length - 1] | 0x20) == 's';
                    lex += (s ? "'" : "'s");
                }
                bov = (writ.trans & (byte)AVSDK.Transitions.VerseTransition) == (byte)AVSDK.Transitions.BeginingOfVerse;
                if (bov)
                {
                    ++v;
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
        public WritRefEx asRecord()
        {
            return new WritRefEx(this);
        }
    }
}
