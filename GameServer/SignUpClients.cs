using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

//using Amazon;
//using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
//using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
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

            SignUpRequest signUpRequest = new SignUpRequest()
            {
                ClientId = _clientAppId,
                Username = _username,
                Password = _password
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
}