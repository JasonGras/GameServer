using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ExceptionServices;

namespace GameServer
{
    static class SignInClients
    {
        public static async Task SignInClientToCognito(string _username, string _password, int _clientid)
        {
            //var credentials = new BasicAWSCredentials("abc", "xyz");

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
                Console.WriteLine("SignInClients.cs | Authentication Requested");
                authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
                Console.WriteLine("SignInClients.cs | Authentication Success");
                //authFlowResponse.AuthenticationResult.RefreshToken
                //Server.clients[_clientid].accessToken = authFlowResponse.AuthenticationResult.AccessToken; // Only Loged In Users have their Access Token Set.



                if (authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    Console.WriteLine("SignInClients.cs | Challenge New Password Required");

                    // New Password Asked 
                    ServerSend.ClientNewPasswordRequired(_clientid, Constants.SCENE_REDEFINE_PWD);
                }
                else
                {
                    Console.WriteLine("SignInClients.cs | Authentication Normal User");                    
                    UpdateUserAndSendIntoGame(_clientid, user, Constants.SCENE_AUTHENTICATION); // Set the Scene to Unload to Authentication Scene
                }                
            }
            catch (Exception e)
            {
                HandleExceptions(e, _clientid);
            }

        }

        public static async Task PwdRedefinedAuthentication(string _username, string _password, int _clientid, string _newPassword)
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
                //Console.WriteLine("SignInClients.cs | Authentication Requested");
                authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
                //Console.WriteLine("SignInClients.cs | Authentication Success");
                //authFlowResponse.AuthenticationResult.RefreshToken
                //Server.clients[_clientid].accessToken = authFlowResponse.AuthenticationResult.AccessToken; // Only Loged In Users have their Access Token Set.

                Console.WriteLine("SignInClients.cs | Challenge New Password still Required");

                    // Updating New Password
                   Console.WriteLine("SignInClients.cs | Updating user with New Password");
                   authFlowResponse = await user.RespondToNewPasswordRequiredAsync(new RespondToNewPasswordRequiredRequest()
                   {
                            SessionID = authFlowResponse.SessionID,
                            NewPassword = _newPassword
                   });

                    Console.WriteLine("SignInClients.cs | Authentication Redefined PWD User");                    
                    UpdateUserAndSendIntoGame(_clientid, user,Constants.SCENE_REDEFINE_PWD); // Set the Scene to Unload to Redefine PWD Scene

            }
            catch (Exception e)
            {
                HandleExceptions(e, _clientid);
            }
        }

        public static void UpdateUserAndSendIntoGame(int _clientid,CognitoUser user, string sceneToUnload)
        {
            // Update the Client CognitoUser
            Console.WriteLine("SignInClients.cs | Update the Client CognitoUser");
            //Console.WriteLine("GetUserAttribute | Post - isValid : " + user.SessionTokens.IsValid().ToString());
            Server.clients[_clientid].myUser = user;

            // Initialise Session
            Console.WriteLine("SignInClients.cs | Initialize UserSession with Tokens from authentication");
            UserSession uSession = new UserSession(null, null, null);
            uSession.Access_Token = user.SessionTokens.AccessToken;
            uSession.Refresh_Token = user.SessionTokens.RefreshToken;
            uSession.Id_Token = user.SessionTokens.IdToken;

            //user.SessionTokens = new CognitoUserSession(user.SessionTokens.IdToken, user.SessionTokens.AccessToken, user.SessionTokens.RefreshToken, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

            // Send Session
            Console.WriteLine("SignInClients.cs | Sending UserSession to Client");
            ServerSend.SendTokens(_clientid, uSession);

            // Send User Into Game
            Console.WriteLine("SignInClients.cs | Send User Into Game");
            Server.clients[_clientid].SendIntoGame(user.Username, sceneToUnload);
        }

        public static void HandleExceptions(Exception e,int _clientid)
        {
            Console.WriteLine("SignInClients.cs | My User Authentication Failed");
            switch (e.GetType().ToString())
            {
                case "Amazon.CognitoIdentityProvider.Model.NotAuthorizedException":
                    Console.WriteLine("SignInClients.cs | Exception | Unknown Username or password.");
                    ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_KO);
                    break;
                case "Amazon.CognitoIdentityProvider.Model.UserNotConfirmedException":
                    Console.WriteLine("SignInClients.cs | Exception | Need User Email Confirmation.");
                    ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_USER_CONFIRMED_KO);
                    break;
                default:
                    Console.WriteLine("SignInClients.cs | Unknown Exception | " + e.GetType().ToString() + " | " + e);
                    ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_KO);
                    break;
            }
        }
        /*public async Task SendUserToChangePasswordSceneAsync(AmazonCognitoIdentityProviderClient provider,string _username, AuthFlowResponse authFlowResponse)
         {
             var hash = Util.GetUserPoolSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret);

             var challengeResponses = new Dictionary<string, string>
                     {
                     { "USERNAME", _username },
                     { "NEW_PASSWORD", "Pas0304121988&" },
                     { "SECRET_HASH", hash }
                     };

             var respondToAuthChallengeRequest = new AdminRespondToAuthChallengeRequest
             {
                 ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                 ChallengeResponses = challengeResponses,
                 ClientId = Constants.CLIENTAPP_ID,//"app-client-id-here",
                 Session = authFlowResponse.SessionID,
                 UserPoolId = Constants.POOL_ID//"user-pool-id"
             };

             var challengeResponse = await provider.AdminRespondToAuthChallengeAsync(respondToAuthChallengeRequest).ConfigureAwait(false);

             //challengeResponse will have a populated AuthenticationResult with IdToken, etc.
         }*/
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
