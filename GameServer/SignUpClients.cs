using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;


using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
//using Amazon;
//using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;
//using System.Linq;
//using Amazon.Runtime.Internal;

namespace GameServer
{
    static class SignUpClients
    {      

        public static async Task SignUpClientToCognito(int _clientID, string _username, string _password, string _email, string _clientAppId)
        {
            AmazonCognitoIdentityProviderClient provider =
        new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.REGION);

            //string SECRET_HASH = CognitoHashCalculator.GetSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret);

            SignUpRequest signUpRequest = new SignUpRequest()
            {
                ClientId = _clientAppId,
                Username = _username,
                Password = _password,
                SecretHash = CognitoHashCalculator.GetSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret)
            };

            List<AttributeType> attributes = new List<AttributeType>()
        {
            new AttributeType(){Name="email", Value = _email} // , si >> 1 sauf a la fin
            //new AttributeType(){Name="custom:Money", Value = "1000"}
        };

            // Send SignupRequest
            signUpRequest.UserAttributes = attributes;

            Console.WriteLine("SignUpClientToCognito : Init.");
            try
            {
                SignUpResponse result = await provider.SignUpAsync(signUpRequest);

                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("SignUpClientToCognito : Creation de Compte Finalisée.");
                    //Server.clients[_clientID].myUser = user;
                    ServerSend.SignUpStatusReturn(_clientID,Constants.ADHESION_OK);
                    // Retourner le Statut Adhesion_OK
                }
            }
            catch (Exception e)
            {
                // Retourner le Statut Adhesion_KO
                Console.WriteLine("SignUpClientToCognito | New Exception | Code : " + e.GetType().ToString() + " | Exeption : " + e.Message);
                ServerSend.SignUpStatusReturn(_clientID, Constants.ADHESION_KO);
            }
            Console.WriteLine("SignUpClientToCognito : Over.");
        }
    }

    public static class CognitoHashCalculator
    {
        public static string GetSecretHash(string username, string appClientId, string appSecretKey)
        {
            var dataString = username + appClientId;

            var data = Encoding.UTF8.GetBytes(dataString);
            var key = Encoding.UTF8.GetBytes(appSecretKey);

            return Convert.ToBase64String(HmacSHA256(data, key));
        }

        public static byte[] HmacSHA256(byte[] data, byte[] key)
        {
            using (var shaAlgorithm = new System.Security.Cryptography.HMACSHA256(key))
            {
                var result = shaAlgorithm.ComputeHash(data);
                return result;
            }
        }
    }
}