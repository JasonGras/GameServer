using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public class UserSession
    {
        public UserSession(string accessToken, string idToken, string refreshToken)
        {
            Id_Token = idToken;
            Access_Token = accessToken;
            Refresh_Token = refreshToken;
        }

        public string Id_Token { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }

    }
}
