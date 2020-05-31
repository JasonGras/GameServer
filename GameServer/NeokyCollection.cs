using System;
using System.Collections.Generic;
using System.Text;

using Amazon.DynamoDBv2.DataModel;

namespace GameServer
{
    [DynamoDBTable("Collection")]
    public class NeokyCollection
    {
        [DynamoDBHashKey]
        public string collection_id { get; set; }

        public string collection_prefab { get; set; }

        public float attackDamages { get; set; }

        public string collection_name { get; set; }

        public float attackSpeed { get; set; }

        public float lifePoints { get; set; }

        [DynamoDBIgnore]
        public bool isSpawned { get; set; } = false;
    }
}
