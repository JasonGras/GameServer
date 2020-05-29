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
            //CognitoUser user = new CognitoUser(_username, Constants.CLIENTAPP_ID, Server.cognitoManagerServer.userPool, Server.cognitoManagerServer.provider, Constants.NeokySecret);
            //CognitoUser user = Server.clients[_clientid].myUser;

            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = _password
            };

            //AuthFlowResponse authFlowResponse = null;
            //Console.WriteLine("GetUserAttribute | Pré - isValid : " + user.SessionTokens.IsValid().ToString()); // = Null while SRPAUTH not run
            try
            {
                Console.WriteLine("SignInClients.cs | Authentication Requested");
                AuthFlowResponse authFlowResponse = await Server.clients[_clientid].myUser.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
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
                    UpdateUserAndSendIntoGame(_clientid, Constants.SCENE_AUTHENTICATION); // Set the Scene to Unload to Authentication Scene
                }                
            }
            catch (Exception e)
            {
                HandleExceptions(e, _clientid,Constants.SCENE_AUTHENTICATION);
            }

        }
        public static async Task ClientForgotPwdRequest(string _username, string email, int _clientid)
        {
            using (var cognito = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.REGION))
            {
                ForgotPasswordRequest ForgotPasswordRequest = new ForgotPasswordRequest()
                {
                    Username = _username,
                    ClientId = Constants.CLIENTAPP_ID,
                    SecretHash = CognitoHashCalculator.GetSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret),
                };

                ForgotPasswordResponse ForgotPasswordResponse = new ForgotPasswordResponse();

                try
                {
                    Console.WriteLine("SignInClients.cs | ForgotPasswordAsync Sending");
                    ForgotPasswordResponse = await cognito.ForgotPasswordAsync(ForgotPasswordRequest).ConfigureAwait(false);
                    Console.WriteLine("SignInClients.cs | ForgotPasswordAsync OK");
                    //ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_CONFIRMED);
                }
                catch (ExpiredCodeException ex)
                {
                    // Username Unknown or Code Expired
                    //ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_CODE_EXPIRED_KO);
                }
                catch (Exception e)
                {
                    Console.WriteLine("SignInClients.cs | My User ClientForgotPwdRequest Failed");
                    switch (e.GetType().ToString())
                    {
                        default:
                            Console.WriteLine("SignInClients.cs | Unknown Exception | " + e.GetType().ToString() + " | " + e);
                            //ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_KO);
                            break;
                    }
                }
            }
        }

        public static async Task ResetPwdForgot(string _username, string _code, int _clientid, string _newPassword)
        {
            using (var cognito = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.REGION))
            {
                ConfirmForgotPasswordRequest confirmForgotPasswordRequest = new ConfirmForgotPasswordRequest()
                {
                    Username = _username,
                    ClientId = Constants.CLIENTAPP_ID,
                    Password = _newPassword,
                    SecretHash = CognitoHashCalculator.GetSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret),
                    ConfirmationCode = _code


                };

                ConfirmForgotPasswordResponse confirmForgotPasswordResponse = new ConfirmForgotPasswordResponse();

                try
                {
                    confirmForgotPasswordResponse = await cognito.ConfirmForgotPasswordAsync(confirmForgotPasswordRequest).ConfigureAwait(false);
                    ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_CONFIRMED);
                }
                catch (CodeMismatchException ex)
                {
                    // Username Unknown or Code Expired
                    ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_CODE_MISMATCH_KO);
                }
                catch (ExpiredCodeException ex)
                {
                    // Username Unknown or Code Expired
                    ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_CODE_EXPIRED_KO);
                }
                catch (Exception e)
                {
                    HandleForgotPwdExceptions(e, _clientid);
                }
            }

            

            // Initiate Forgot Password Modification By Code
            /*try
            {
                // Lunch myUser Change Forgotten Pwd
                //await Server.clients[_clientid].myUser.ConfirmForgotPasswordAsync(_code, _newPassword);
                await Server.cognitoManagerServer.provider.
                ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_CONFIRMED);
                
                // Authenticate & Check Challenge. 
                
            }
            catch (Exception e)
            {
                HandleExceptions(e, _clientid, Constants.SCENE_FORGOT_PASSWORD);
            }*/
        }
        public static async Task PwdRedefinedAuthentication(string _username, string _password, int _clientid, string _newPassword)
        {
            //CognitoUser user = new CognitoUser(_username, Constants.CLIENTAPP_ID, Server.cognitoManagerServer.userPool, Server.cognitoManagerServer.provider, Constants.NeokySecret);


            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = _password
            };

            //AuthFlowResponse authFlowResponse = null;
            //Console.WriteLine("GetUserAttribute | Pré - isValid : " + user.SessionTokens.IsValid().ToString()); // = Null while SRPAUTH not run
            try
            {
                //Console.WriteLine("SignInClients.cs | Authentication Requested");
                AuthFlowResponse authFlowResponse = await Server.clients[_clientid].myUser.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
                //Console.WriteLine("SignInClients.cs | Authentication Success");
                //authFlowResponse.AuthenticationResult.RefreshToken
                //Server.clients[_clientid].accessToken = authFlowResponse.AuthenticationResult.AccessToken; // Only Loged In Users have their Access Token Set.

                Console.WriteLine("SignInClients.cs | Authentication Redefine PWD OK");
                if (authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    // Updating New Password
                    Console.WriteLine("SignInClients.cs | Updating user with New Password");
                    authFlowResponse = await Server.clients[_clientid].myUser.RespondToNewPasswordRequiredAsync(new RespondToNewPasswordRequiredRequest()
                    {
                        SessionID = authFlowResponse.SessionID,
                        NewPassword = _newPassword
                    });
                    Console.WriteLine("SignInClients.cs | New Password Updated");

                    Console.WriteLine("SignInClients.cs | Authentication With Redefined PWD User");
                    UpdateUserAndSendIntoGame(_clientid, Constants.SCENE_REDEFINE_PWD); // Set the Scene to Unload to Redefine PWD Scene

                }
                else
                {
                    Console.WriteLine("SignInClients.cs | Authentication Without Redefined PWD User");
                    UpdateUserAndSendIntoGame(_clientid, Constants.SCENE_REDEFINE_PWD); // Set the Scene to Unload to Redefine PWD Scene
                }               
            }
            catch (Exception e)
            {
                HandleExceptions(e, _clientid,Constants.SCENE_REDEFINE_PWD);
            }
        }

        public static void UpdateUserAndSendIntoGame(int _clientid, string sceneToUnload)
        {
            // Update the Client CognitoUser
            //Console.WriteLine("SignInClients.cs | Update the Client CognitoUser");
            //Console.WriteLine("GetUserAttribute | Post - isValid : " + user.SessionTokens.IsValid().ToString());
            //Server.clients[_clientid].myUser = user;

            // Initialise Session
            Console.WriteLine("SignInClients.cs | Initialize UserSession with Tokens from authentication");
            UserSession uSession = new UserSession(null, null, null);
            uSession.Access_Token = Server.clients[_clientid].myUser.SessionTokens.AccessToken;
            uSession.Refresh_Token = Server.clients[_clientid].myUser.SessionTokens.RefreshToken;
            uSession.Id_Token = Server.clients[_clientid].myUser.SessionTokens.IdToken;

            //user.SessionTokens = new CognitoUserSession(user.SessionTokens.IdToken, user.SessionTokens.AccessToken, user.SessionTokens.RefreshToken, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

            // Send Session
            Console.WriteLine("SignInClients.cs | Sending UserSession to Client");
            ServerSend.SendTokens(_clientid, uSession);

            // Send User Into Game
            Console.WriteLine("SignInClients.cs | Send User Into Game");
            Server.clients[_clientid].SendIntoGame(Server.clients[_clientid].myUser.Username, sceneToUnload);
        }
        public static void HandleForgotPwdExceptions(Exception e, int _clientid)
        {
            Console.WriteLine("SignInClients.cs | My User ForgotPwdExceptions Failed");
            switch (e.GetType().ToString())
            {                
                default:
                    Console.WriteLine("SignInClients.cs | Unknown Exception | " + e.GetType().ToString() + " | " + e);
                    ServerSend.ClientForgotPasswordStatus(_clientid, Constants.FORGOT_PASSWORD_KO);
                    break;
            }
        }
        public static void HandleExceptions(Exception e,int _clientid,string authenticationScene)
        {
            Console.WriteLine("SignInClients.cs | My User Authentication Failed");
            switch (e.GetType().ToString())
            {
                case "Amazon.CognitoIdentityProvider.Model.PasswordResetRequiredException":
                    Console.WriteLine("SignInClients.cs | Exception | Handle Reset Password");// TO DO
                    ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_KO_RESET_PWD_REQUIRED);
                    break;
                case "Amazon.CognitoIdentityProvider.Model.NotAuthorizedException":
                    if(authenticationScene == Constants.SCENE_AUTHENTICATION)
                    {
                        Console.WriteLine("SignInClients.cs | Exception | Unknown Username or password.");
                        ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_KO);
                    }else if (authenticationScene == Constants.SCENE_REDEFINE_PWD)
                    {
                        Console.WriteLine("SignInClients.cs | Exception | Unknown Username or password.");
                        ServerSend.AuthenticationStatus(_clientid, Constants.AUTHENTICATION_REDEFINE_PWD_KO);
                    }                    
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
    }
}
