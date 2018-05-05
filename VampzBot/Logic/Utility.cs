using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VampzBot.Logic
{
    public static class Utility
    {

        public static string FormatGoogleSheetDate(string date)
        {
            char[] chars = date.ToCharArray();

            char[] charDate = new char[10];

            for(int i = 0; i < 10; i++)
            {
                charDate[i] = chars[i];
            }

            char[] time = new char[5];

            for(int j = 11; j < chars.Length; j++)
            {
                time[j - 11] = chars[j];
            }

            string timeZone = " PM CEST";

            return new string(charDate) + " " + new string(time) + timeZone;
        }

    }
}
