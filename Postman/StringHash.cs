using System;
using System.Collections.Generic;
using System.Text;

namespace Postman
{
    public static class StringHash
    {
        public static int SDBMLower(string s)
        {
            if (s == null)
            {
                return 0;
            }

            uint hash = 0;
            for (int index = 0; index < s.Length; index++)
            {
                hash = (uint)(char.ToLower(s[index]) + ((int)hash << 6) + ((int)hash << 16)) - hash;
            }

            return (int)hash;
        }
    }
}
