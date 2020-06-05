using System;
using System.Collections.Generic;
using System.Text;

using Amazon.DynamoDBv2.DataModel;

namespace GameServer
{
    [DynamoDBTable("Player_Collection")]
    public class NeokyPlayerCollection
    {
        [DynamoDBHashKey]
        public string client_sub { get; set; }

        [DynamoDBProperty(AttributeName = "PlayerCollection")]
        public Dictionary<string,Dictionary<string,int>> PlayerCollection { get; set; }
    }
}
