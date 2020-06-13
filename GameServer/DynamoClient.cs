using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class DynamoClient
    {
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;
        private readonly DynamoDBContext _context;

        public DynamoClient()
        {
            //localstack ignores secrets
            _amazonDynamoDBClient = new AmazonDynamoDBClient(Constants.AWS_ACCESS_KEY_ID, Constants.AWS_SECRET_ACCESS_KEY,
                 new AmazonDynamoDBConfig
                 {
                     ServiceURL = Constants.AWS_DYNAMO_SERVICE_URL, //default localstack url                     
                     UseHttp = true,
                 });

            _context = new DynamoDBContext(_amazonDynamoDBClient, new DynamoDBContextConfig
            {
                TableNamePrefix = "Neoky_"
            });

            Console.WriteLine("Connexion to Database OK");
        }

        public async Task<CreateTableResponse> SetupAsync()
        {

            var createTableRequest = new CreateTableRequest
            {
                TableName = "clients",
                AttributeDefinitions = new List<AttributeDefinition>(),
                KeySchema = new List<KeySchemaElement>(),
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>(),
                LocalSecondaryIndexes = new List<LocalSecondaryIndex>(),
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                }
            };
            createTableRequest.KeySchema = new[]
            {
                new KeySchemaElement
                {
                    AttributeName = "client_id",
                    KeyType = KeyType.HASH,
                },

            }.ToList();

            createTableRequest.AttributeDefinitions = new[]
            {
                new AttributeDefinition
                {
                    AttributeName = "client_id",
                    AttributeType = ScalarAttributeType.N,
                }
            }.ToList();

            Console.WriteLine("Creating Table");
            return await _amazonDynamoDBClient.CreateTableAsync(createTableRequest);
        }

        public async Task SaveOrUpdatNeokyClients(Player nClients)
        {
            //Console.WriteLine("SaveOrUpdatNeokyClients : Lunched");
            await _context.SaveAsync(nClients);
            Console.WriteLine("SaveOrUpdatNeokyClients : Done");
        }

        public async Task<Player> GetNeokyClientsUsingHashKey(string id)
        {
            return await _context.LoadAsync<Player>(id);
        }

        public async Task<Player> ScanForNeokyClientsUsingUsername(string username)
        {
            var search = _context.ScanAsync<Player>
            (
                new[]
                {
                    new ScanCondition
                    (
                        nameof(Player.username),
                        ScanOperator.Equal,
                        username
                    )
                }
            );
            var result = await search.GetRemainingAsync();
            return result.FirstOrDefault();
        }

        public async Task<NeokyPlayerCollection> ScanForPlayerCollectionUsingSub(string _clientSub)
        {
            var search = _context.ScanAsync<NeokyPlayerCollection>
            (
                new[]
                {
                    new ScanCondition
                    (
                        nameof(NeokyPlayerCollection.client_sub),
                        ScanOperator.Equal,
                        _clientSub
                    )
                }
            );
            var result = await search.GetRemainingAsync();
            return result.FirstOrDefault();
        }

        public async Task<NeokyCollection> ScanForNeokyCollectionUsingCollectionID(string _collectionID)
        {
            var search = _context.ScanAsync<NeokyCollection>
            (
                new[]
                {
                    new ScanCondition
                    (
                        nameof(NeokyCollection.collection_id),
                        ScanOperator.Equal,
                        _collectionID
                    )
                }
            );
            var result = await search.GetRemainingAsync();
            return result.FirstOrDefault();
        }
    }
}