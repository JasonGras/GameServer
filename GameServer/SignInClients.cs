using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;


namespace GameServer
{
    static class SignInClients
    {
        public static async Task SignInClientToCognito(string _username, string _password, int _clientid)
        {
            AmazonCognitoIdentityProviderClient provider =
                new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.REGION);

            //string SECRET_HASH = CognitoHashCalculator.GetSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret);

            CognitoUserPool userPool = new CognitoUserPool(Constants.POOL_ID, Constants.CLIENTAPP_ID, provider, Constants.NeokySecret);

            CognitoUser user = new CognitoUser(_username, Constants.CLIENTAPP_ID, userPool, provider, Constants.NeokySecret);            

            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = _password
            };

            AuthFlowResponse authFlowResponse = null;
            try
            {
                Console.WriteLine("Login Lunch");
                authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);                
            }
            catch (Exception e)
            {
                Console.WriteLine("Login Failed : " + e);
                ServerSend.AuthenticationStatus(_clientid, null, Constants.AUTHENTICATION_KO);
                // Signaler au client que son authentification a Echoué pour X ou Y Raison
            }           
           
            //authFlowResponse.AuthenticationResult.RefreshToken
            //Server.clients[_clientid].accessToken = authFlowResponse.AuthenticationResult.AccessToken; // Only Loged In Users have their Access Token Set.
            //Console.WriteLine("GetUserAttribute Seems Ok | Token Set : "+ user.SessionTokens.RefreshToken);
            ServerSend.AuthenticationStatus(_clientid, user.SessionTokens.RefreshToken, Constants.AUTHENTICATION_OK);
            
            //REnvoyer au client son Token.



        }
    }
}
/*
// Get users Attribute
GetUserRequest getUserRequest = new GetUserRequest();
getUserRequest.AccessToken = authFlowResponse.AuthenticationResult.AccessToken;
//Get User Values
/*GetUserResponse getUser = await provider.GetUserAsync(getUserRequest);
string _curMoney = getUser.UserAttributes.Where(a => a.Name == "custom:Money").First().Value;
int _userMoney = Convert.ToInt32(_curMoney);

Console.WriteLine("Total sur le compte de" + _username + ":" + _userMoney);*/

// Attribute type definition
/*AttributeType attributeType = new AttributeType()
{
    Name = "custom:Money",
    Value = Convert.ToString(_userMoney + 10),// Valeur mise a jour
};


// Update Attribute Request
UpdateUserAttributesRequest updateUserAttributesRequest = new UpdateUserAttributesRequest()
{
    AccessToken = authFlowResponse.AuthenticationResult.AccessToken
};

updateUserAttributesRequest.UserAttributes.Add(attributeType);
provider.UpdateUserAttributes(updateUserAttributesRequest);

Debug.Log("+10 on the Money of account " + _username);*/
