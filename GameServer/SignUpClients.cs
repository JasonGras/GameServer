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
using NLog.Common;
using NLog;
//using System.Linq;
//using Amazon.Runtime.Internal;

namespace GameServer
{
    public class SignUpClients
    {
        public async void SignUptoCognito(int _clientid,string _username, string _password, string _email)
        {
            // If the REgEx Formats are Respected, we proceed to Adhesion OR We Return an Error Format
            if (SecurityCheck.CheckUserPattern(_username))
            {
                if (SecurityCheck.CheckPasswordPattern(_password))
                {
                    if (SecurityCheck.CheckEmailPattern(_email))
                    {
                        SignUpRequest signUpRequest = new SignUpRequest()
                        {
                            ClientId = Constants.CLIENTAPP_ID,
                            Username = _username,
                            Password = _password,
                            SecretHash = CognitoHashCalculator.GetSecretHash(_username, Constants.CLIENTAPP_ID, Constants.NeokySecret)
                        };

                        List<AttributeType> attributes = new List<AttributeType>()
                        {
                            new AttributeType(){Name="email", Value = _email} 
                        };

                        // Send SignupRequest
                        signUpRequest.UserAttributes = attributes;

                        try
                        {
                            SignUpResponse result = await Server.cognitoManagerServer.provider.SignUpAsync(signUpRequest);

                            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                            {
                                ServerSend.SignUpStatusReturn(_clientid, Constants.ADHESION_OK);
                            }
                        }
                        catch (Exception e)
                        {                            
                            switch (e.GetType().ToString())
                            {
                                case "Amazon.CognitoIdentityProvider.Model.UsernameExistsException":
                                    ServerSend.SignUpStatusReturn(_clientid, Constants.ADHESION_ALREADY_EXIST);
                                    break;
                                default:
                                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SignUpClientToCognito", "Client ID : " + _clientid.ToString() + "  | New Exception | Code : " + e.GetType().ToString() + " | Exeption : " + e.Message), NlogClass.exceptions.Add));
                                    ServerSend.SignUpStatusReturn(_clientid, Constants.ADHESION_KO);
                                    break;
                            }

                        }
                    }
                    else
                    {
                        ServerSend.SignUpStatusReturn(_clientid, Constants.ADHESION_FORMAT_EMAIL_KO);
                    }
                }
                else
                {
                    ServerSend.SignUpStatusReturn(_clientid, Constants.ADHESION_FORMAT_PASSWORD_KO);
                }
            }
            else
            {
                ServerSend.SignUpStatusReturn(_clientid, Constants.ADHESION_FORMAT_USERNAME_KO);
            }
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