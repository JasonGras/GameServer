using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using System.ComponentModel;

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
            //CognitoUser user = Server.clients[_clientid].myUser;

            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = _password
            };

            AuthFlowResponse authFlowResponse = null;
            //Console.WriteLine("GetUserAttribute | Pré - isValid : " + user.SessionTokens.IsValid().ToString()); // = Null while SRPAUTH not run
            try
            {
                Console.WriteLine("Login Lunch");
                authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

                //authFlowResponse.AuthenticationResult.RefreshToken
                //Server.clients[_clientid].accessToken = authFlowResponse.AuthenticationResult.AccessToken; // Only Loged In Users have their Access Token Set.
                Console.WriteLine("GetUserAttribute | Post - isValid : " + user.SessionTokens.IsValid().ToString());
                Server.clients[_clientid].myUser = user;

                UserSession uSession = new UserSession(null, null, null);
                uSession.Access_Token = user.SessionTokens.AccessToken;
                uSession.Refresh_Token = user.SessionTokens.RefreshToken;
                uSession.Id_Token = user.SessionTokens.IdToken;
                ServerSend.SendTokens(_clientid, uSession);
                Server.clients[_clientid].SendIntoGame();
            }
            catch (Exception e)
            {
                //Win32Exception winEx = e as Win32Exception;
                
                //Console.WriteLine(e.GetType());
                //Console.WriteLine("Login Failed : " + e);
                switch (e.GetType().ToString())
                {
                    case "Amazon.CognitoIdentityProvider.Model.UserNotConfirmedException":
                        ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_USER_CONFIRMED_KO);                        
                        break;
                    default:
                        ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_KO);
                        break;
                }
                
                // Signaler au client que son authentification a Echoué pour X ou Y Raison
            }  
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
