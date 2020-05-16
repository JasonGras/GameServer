using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;

namespace GameServer
{
    public static class SecurityCheck
    {
        public static bool CheckEmailPattern(string _text)
        {
            string pattern;
            pattern = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$"; // Email pattern

            Regex rgx = new Regex(pattern);

            if (rgx.IsMatch(_text))
            {
                return true;
            }
            else
            {                
                return false;
            }
        }
        public static bool CheckUserPattern(string _text)
        {
            string pattern;
            pattern = @"^(?=.{4,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$"; // Username pattern

            Regex rgx = new Regex(pattern);

            if (rgx.IsMatch(_text))
            {
                return true;
            }
            else
            {                
                return false;
            }
        }

        public static bool CheckPasswordPattern(string _text)
        {
            string pattern;
            pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})"; // Password pattern (1 Min 1 Maj 1 Numeric 1 Symbol)

            Regex rgx = new Regex(pattern);

            if (rgx.IsMatch(_text))
            {
                return true;
            }
            else
            {                
                return false;
            }
        }
    }
}
