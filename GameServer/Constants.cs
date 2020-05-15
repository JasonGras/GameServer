using System;
using System.Collections.Generic;
using System.Text;

using Amazon;

namespace GameServer
{
    class Constants
    {
        public const int TICKS_PER_SEC = 30; // How many ticks per second
        public const float MS_PER_TICK = 1000f / TICKS_PER_SEC; // How many milliseconds per tick
        public const string NeokySecret = "48h4sa5vvsficajh9ggc5urh5l0e5httn59fijl261t2kj0len0";

        public const string POOL_ID = "eu-west-2_htPUpskO5";
        public const string CLIENTAPP_ID = "13d9pfoekgd1om3o12gh33nl4k";
        public const string FED_POOL_ID = "eu-west-2:1685b7b4-a171-492b-ad7c-62527ccd80d1";
        public static RegionEndpoint REGION = RegionEndpoint.EUWest2;

        public const string SCENE_NOSCENE = "NOSCENE"; 
        public const string SCENE_HOMEPAGE = "HomePage"; 
        public const string SCENE_AUTHENTICATION = "Authentication";


        /*Constants Statut*/

        // Adhesion
        public const string ADHESION_OK = "ADHESION_OK";
        public const string ADHESION_KO = "ADHESION_KO";
        public const string AUTHENTICATION_OK = "AUTHENTICATION_OK";
        public const string AUTHENTICATION_KO = "AUTHENTICATION_KO";

    }
}
