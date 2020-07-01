using Amazon.Extensions.CognitoAuthentication;
using Amazon.SecurityToken.Model;
using GameServer.Loots;
using GameServer.TurnBasedFights;
using NLog;
using NLog.Common;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;




namespace GameServer
{
    class ServerHandle
    {

        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            try
            {
                int _clientIdCheck = _packet.ReadInt();

                Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                if (_fromClient != _clientIdCheck)
                {
                    Console.WriteLine($"Player (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                }
            }catch(Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "WelcomeReceived",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public async static void CheckAuthenticationPlayerAsync(int _fromClient, Packet _packet)
        {
            // Getting the parameter from Client Packet
            int _clientIdCheck = _packet.ReadInt();
            UserSession _currentUserSession = _packet.ReadUserSession();

            if (_fromClient == _clientIdCheck)
            {
                // Update Session From Client
                //Server.clients[_fromClient].myUser.SessionTokens.IdToken = _currentUserSession.Id_Token;
                //Server.clients[_fromClient].myUser.SessionTokens.AccessToken = _currentUserSession.Access_Token;
                //Server.clients[_fromClient].myUser.SessionTokens.RefreshToken = _currentUserSession.Refresh_Token;

                /*
                Server.clients[_fromClient].myUser.SessionTokens.IdToken = "eyJraWQiOiJcL0hPRUhZN0UwZzJ0V1FNbmtkc1BHU05CdVwvNUFHeE1oS21hS0lBSHpadTg9IiwiYWxnIjoiUlMyNTYifQ.eyJzdWIiOiJmMDVlOGY1Ny1kMGU4LTRhNjQtOWVjOS1hZmMyNDc3MDgwMjEiLCJhdWQiOiIzYmhkbXVpM2poaHM4b29jdTV0czg4cGwyIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImV2ZW50X2lkIjoiNTc5M2Q5NzktOTY0NS00YjUwLWFiZWQtMzk1OTQ5NmVkZGJkIiwidG9rZW5fdXNlIjoiaWQiLCJhdXRoX3RpbWUiOjE1OTA2MDEyMDYsImlzcyI6Imh0dHBzOlwvXC9jb2duaXRvLWlkcC5ldS13ZXN0LTIuYW1hem9uYXdzLmNvbVwvZXUtd2VzdC0yXzk0WjBDOGtTZSIsImNvZ25pdG86dXNlcm5hbWUiOiJqZ3JlZWUiLCJleHAiOjE1OTA2MDQ4MDYsImlhdCI6MTU5MDYwMTIwNiwiZW1haWwiOiJKR1JFRUVAeW9wbWFpbC5jb20ifQ.gnnG8ze48N29KrAeG_btSSBGG0GXwUnetwO6VWYkwAAXFrBqQsKXQQOQjE8Cu-tUZ58oYxHgNWUPcMZDEy4urmJWG5sS88KbekcWzsGKOyeRDUgKVVGQZJ_Ix6p5B7iQN9ULiE0YyKIYTiHaZZhU17Yk_D71QqrrpERMeIAXaxhc0D7XW6sEthMVNqbFM-CwGFjuRV_rUZ0IcTPmkjZbK5nU1fYiQBwbJv5ZdY1nq_bY4-Zy7o-YRaTB-qYX6x7pBSQY6bf235LqjMiapD-u2NTPfPsnvO41Zc18St1PIW6m2dYCvQIESpcprln3rxnxegkJMIyKSW3Qr6QJyoRFlA";
                Server.clients[_fromClient].myUser.SessionTokens.AccessToken = "eyJraWQiOiJiWHZxWXBFcmF2U3hHY1wvZ2J6XC9SV09lUUpOVFdtMlh0bmczRm50aXdPTXc9IiwiYWxnIjoiUlMyNTYifQ.eyJzdWIiOiJmMDVlOGY1Ny1kMGU4LTRhNjQtOWVjOS1hZmMyNDc3MDgwMjEiLCJldmVudF9pZCI6IjU3OTNkOTc5LTk2NDUtNGI1MC1hYmVkLTM5NTk0OTZlZGRiZCIsInRva2VuX3VzZSI6ImFjY2VzcyIsInNjb3BlIjoiYXdzLmNvZ25pdG8uc2lnbmluLnVzZXIuYWRtaW4iLCJhdXRoX3RpbWUiOjE1OTA2MDEyMDYsImlzcyI6Imh0dHBzOlwvXC9jb2duaXRvLWlkcC5ldS13ZXN0LTIuYW1hem9uYXdzLmNvbVwvZXUtd2VzdC0yXzk0WjBDOGtTZSIsImV4cCI6MTU5MDYwNDgwNiwiaWF0IjoxNTkwNjAxMjA2LCJqdGkiOiJmMTk3MTY5ZS0wYzdjLTQxMDMtYmY3Ny0wOTIzNzIxNGU4N2YiLCJjbGllbnRfaWQiOiIzYmhkbXVpM2poaHM4b29jdTV0czg4cGwyIiwidXNlcm5hbWUiOiJqZ3JlZWUifQ.QgzHNz5i3JJ6OBHI7WclZghdPmCHYMc96ooHREF0VvdfgkCVXN7bXUvE4yB8T6bI4TDlY6iyBLY5kUscbNgGhaL86tjAcO-OG1PBnCmyYvNaKF1UvyqEKiiPASqdLWWQFXWGBWHBxk5UQZ6jII4CrZ9ShNrvsauoRfQVi0Sk092dNiTaMhx0yBd5jxdjIV3SQtrbH_4iOBiwiYprWzkqZJw-Qm-BeHoQYd2L0nrmy-OVoaCkv4l8_rIbCUbJdiv7PGYPuWS3ZqnwdPTA6Qwk5uzKnaGFcjtnNyddtxCTsY9ttShC_kWk_xkV9mDOanW8nyXItimLWRtI-Zsf7LgL9Q";
                Server.clients[_fromClient].myUser.SessionTokens.RefreshToken = "eyJjdHkiOiJKV1QiLCJlbmMiOiJBMjU2R0NNIiwiYWxnIjoiUlNBLU9BRVAifQ.ZUHrsIk7bonZlUM1r273jmB53R4MgI1kGfcTVc--EOCCwq2PrIxC-pr2_NY7mpWAmk8Q7BZ4ayD3ru1Sehj4DXBlfjhZg1-eIPEE7vlftSaZ-e-23Ds91CFA6_FYkEVylFnQTUHMIxYmjbru_u5u_7drtcjRMKUKQ59XNrE7cAdnWwZcBir1kDVXCLzBgwhZ2RljakqCpAag_df20NBI-fk4kr15xuhkdgkj2N_Sd0tCTgU0L1pvO9alZxUYMIliQGfyW6EY9KRO1PpMArXSE3_uugl6_Ly3z545D7PA_T_VUu0TfwbFbqTQQMpgPJHK4eBMFe7MUEjAgkvMdo4c2Q.6bXeDF-uOa6bSE_o.Y6QA32zq0sJuMUd9Lin592vizt_zLBjfWXpF1Wt4eGL609gEPobtq4B9Fe4CiI-WbO0-Fd6c9Hb-JFkLNXGNYnLhR6g0CmtRmhCtebRDmiU4UuGpanGlcpy8gINBr280kFqa2oko4vRO8XT-wc_hCbRklTuTnVCRO51oBW_eO0JTHwXcVfE1vVU5CXjvKYXP552pRZS5QcWQUeJ2UxohTlil7n8NUbfOLz4MBQKA8cqqwn2G5-EeKL1CVOuw4XlwMyKhxmpZUJttinixz6BJspk6x7Z4bu8r37oOLRHGj3nGtHFW6NXTl04pLRMU_6abaOOIF9341sbQKMoLzFt4VpoSsu70AUFmgJMEFQ8gXAYvWx-zHtBPPkCQHmPCaFWrIcjmEicKaTPCMOCuJ8yEqpFX7Q_6b8eAsH2rek0IVOhUj0xlqYgdbdWs3OyV2d82DyscxRNk9KGgVewvhvAG55m_kh8VX8jXS-mjjfhCeK5-DWBYIaez-LvAiloTCNKh5AwpOLncd05FeBUn9HqbZH2_hGUuy7gB1oA8BLeDmJuzAsYNUuIJpCOCn4aNwSc2jfN_SbCVV2zNgJ4WwDUv9YSgOaDKCSvdq0irLjo2c0_DtmfQhTWpjV0R3pjSxqUNK8NP29bGhWDgqUHOMQ2LOzk2g9Y5uJRCpmuKfQ2E1fE7A4jJwkTb_sa0xqD6BrJAkJEeEzC6CHbEcIKhEIP-zy42YHkQ7dXYcoyvZ9EwLjiwhSO4w_dIc5Otf7yTb9RvmaRVC_xA8wCheVbuH737gMCRU9NAnxuyQmlXMM6dxb8IRWG9ZBZW5dN8fJ5N63UTv3sg6vSPsKteuDpFK4E063lUiK7mT7wvExEosmFByea6RCJFQ3TcH9PuRE9KwAT1YQOetSinBYSsLaVwCizlefKifPfdmEgUhspVgu5_3y2KK_Ad2CfOJApc4ZhyKAodULD6LTHnzbcWINDCqJ0r1hBYCeEXRVWSm61JbEx2iXJfXEWu8cA0yPqd8vrIp1PoS4DDEAWYa27eqPtitS0ekw3HYIak4EWeBVU6850uxbkwvkmf3b9tC4ldvmSOraSOe9bMxhmN-8oLkUTPbmOWY-GiA-P4s1cATWGbB8XcnGceYzkEj7QoEuso5Mb2ck6H064_z8mq2S3dwN5wkN5tl-PcbULwAY11GPG0g7AWnxIQ1Hy9PWun5K-csnqjkBS8QqzZEt19LxjqIqsLL6zhn7Vrg4BLHD2rxIbvf310V-dYnZVmbaUH0Enc84ht43ZGF3_g.l2kSex0Pi__6JOBFq_4m2g";
                */

                //Check if Session is Claimed by the Righ UserId
                if (_currentUserSession.Refresh_Token == Server.clients[_clientIdCheck].myUser.SessionTokens.RefreshToken)
                {
                    //Check if Session is Still Valid
                    if (!Server.clients[_fromClient].myUser.SessionTokens.IsValid())
                    {
                        Console.WriteLine("IsValid check | Client UserSession Is Invalid !");
                        await Server.clients[_fromClient].GetNewValidTokensAsync(_currentUserSession.Refresh_Token);
                        //Console.WriteLine("UserSession Updated.");
                        
                    }
                    //Server.clients[_fromClient].myUser.StartWithRefreshTokenAuthAsync();
                    // For Valid Sessions, we can go to the desired Scene
                    if (Server.clients[_fromClient].myUser.SessionTokens.IsValid())
                    {

                        //target.InitializeTarget();

                        //FirstDungeon FirstDungeon = new FirstDungeon();
                        // Send client to FirstDungeon spawnScene
                        //if (FirstDungeon.playerCanAccessDungeon(Server.clients[_fromClient].player.level))
                        //{
                            //if (FirstDungeon.playerCanAccessDesiredScene(FirstDungeon.spawnScene, "playerCurrentScene"))
                            //{

                            //}
                        //}


                        //NlogClass.target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "NlogClass", "message1").WithContinuation(NlogClass.exceptions.Add));
                        //NlogClass.target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "NlogClass", "message2").WithContinuation(NlogClass.exceptions.Add));
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Info, "NlogClass", "message3"), NlogClass.exceptions.Add));
                        //NlogClass.target.GetType().
                        //logger.LogInformation("IsValid check | Client UserSession Is Valid !");
                        // NLog is configured in the NLog.config
                        //Logger logger = LogManager.GetCurrentClassLogger();
                        //logger.Info("IsValid check | Client UserSession Is Valid !");
                        //Console.WriteLine("IsValid check | Client UserSession Is Valid !");
                        //Server.clients[_fromClient].myUser.SessionTokens.RefreshToken.
                    }
                }
                else
                {
                    Console.WriteLine("This RefreshToken dont belong to this User !");
                }
            }
        }



        /*private static bool isRefreshTokenOfClaimedID(string _refreshToken,int _clientIdCheck)
        {
            if(_refreshToken == Server.clients[_clientIdCheck].myUser.SessionTokens.RefreshToken)
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/

        /// <summary>Function handling "switchScene" Packets from Client</summary>
        /// <param name="_desiredScene">Desired Scene Name recieved from the Client.</param>
        /// <param name="_currentUserSession">The Client SessionTokens, permit to check if he is still Authenticated</param>
        /// UPDATED 13/06/2020
        public static void DesiredPlayerScene(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                UserSession _currentUserSession = _packet.ReadUserSession();
                string _desiredscene = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    if (Server.clients[_fromClient].CheckSessionClaimedByUser(_currentUserSession)) // The Session Claimed Should be the Same of the Session Claimed
                    {
                        Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} want to switch to {_desiredscene}.");
                        Server.clients[_fromClient].SwitchScene(_desiredscene, _currentUserSession);
                    }
                    else
                    {
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "DesiredPlayerScene", "Client Username : " + Server.clients[_fromClient].myUser.Username + " | RefreshToken claimed is not in Client SessionTokens"), NlogClass.exceptions.Add));
                    }
                }
            }catch(Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "DesiredPlayerScene",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        /// <summary>Function Handling "updateCollection" Packets from Client</summary>
        /// <param name="_currentUserSession">The Client SessionTokens, permit to check if he is still Authenticated</param>
        /// UPDATED 13/06/2020
        public static void PlayerAskCollection(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                UserSession _currentUserSession = _packet.ReadUserSession();

                if (_fromClient == _clientIdCheck) // The client ID claimed shoud be equal to the Client ID Observed
                {
                    if (Server.clients[_fromClient].CheckSessionClaimedByUser(_currentUserSession)) // The Session Claimed Should be the Same of the Session Claimed
                    {
                        Server.clients[_fromClient].UpdatePlayerCollection(_currentUserSession);
                    }
                    else
                    {
                        NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "PlayerAskCollection", "Client Username : " + Server.clients[_fromClient].myUser.Username + " | RefreshToken claimed is not in Client SessionTokens"), NlogClass.exceptions.Add));
                    }
                }
                else
                {
                    NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "PlayerAskCollection", "Claimed client : " + Server.clients[_fromClient].myUser.Username + " | _fromClient : ["+ _fromClient + "] is not equal to _clientIdCheck: ["+ _clientIdCheck + "]"), NlogClass.exceptions.Add));
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "PlayerAskCollection", "Claimed client : " + Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public static void PlayerAskEnterDungeon(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                UserSession _currentUserSession = _packet.ReadUserSession();
                string _desiredDungeon = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    Server.clients[_fromClient].EnterDungeon(_currentUserSession,_desiredDungeon);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "PlayerAskEnterDungeon",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public static void AttackPacketReceieved(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                UserSession _currentUserSession = _packet.ReadUserSession();
                int _UserUnitNumber = _packet.ReadInt();
                int _EnemyUnitNumber = _packet.ReadInt();


                if (_fromClient == _clientIdCheck)
                {
                    Server.clients[_fromClient].UnitAttack(_currentUserSession,_UserUnitNumber, _EnemyUnitNumber);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "AttackPacketReceieved", "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ) , NlogClass.exceptions.Add));
            }
        }

        public static void FightPacketReceieved(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                UserSession _currentUserSession = _packet.ReadUserSession();
                string _fightRequest = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} request with FIGHT Packets : {_fightRequest}.");


                    //Check if Session is Still Valid
                    if (!Server.clients[_fromClient].myUser.SessionTokens.IsValid())
                    {
                        //_ = Server.clients[_fromClient].GetNewValidTokensAsync();
                        Console.WriteLine("UserSession Updated.");
                    }
                    //Server.clients[_fromClient].myUser.StartWithRefreshTokenAuthAsync();
                    // For Valid Sessions, we can go to the desired Scene
                    if (Server.clients[_fromClient].myUser.SessionTokens.IsValid())
                    {
                        switch (_fightRequest)
                        {
                            case "INIT_FIGHT":
                                // Check if player is in right to Init a fight
                                Server.clients[_fromClient].setPlayerFight(_currentUserSession);
                                break;
                            case "FIGHT_READY":
                                // Check if player is in right to Init a fight
                                Server.clients[_fromClient].player.FightManager.isClientInitOver = true;                                
                                break;

                            default:
                                Console.WriteLine("Unkown Fight Packet Recieved.");
                                break;
                        }




                    }
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "FightPacketReceieved",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public async static void GetRedefinedPwd(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();
                string _currentPwd = _packet.ReadString();
                string _newPwd = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    await SignInClients.PwdRedefinedAuthentication(_username, _currentPwd, _fromClient, _newPwd);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetRedefinedPwd",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }
        public async static void GetForgotPwd(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet ForgotPassword Request Pwd Change
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();
                string _code = _packet.ReadString();
                string _newPwd = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    await SignInClients.ResetPwdForgot(_username, _code, _fromClient, _newPwd);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetForgotPwd",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public async static void ForgotPwdClientRequest(int _fromClient, Packet _packet)
        {
            try
            {
                // Getting the parameter from Client Packet ForgotPassword Request Pwd Change
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    // Check in DB if Email is Right to improve Random Forget Password Requests
                    await SignInClients.ClientForgotPwdRequest(_username, _fromClient);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "ForgotPwdClientRequest",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        /// <summary>Check if the UserSession is Valid, if Not Renew Tokens </summary>
        /// <param name="_clientIdCheck">The Client Claimed Client ID</param>
        /// <param name="_username">The Client Claimed User Name</param>
        /// <param name="_password">The Client Claimed User Password</param>
        /// <param name="_email">The Client Claimed User Email</param>
        /// UPDATED 13/06/2020
        public static void SignUpClientRequest(int _fromClient, Packet _packet)
        {
            try
            {

                // Getting the parameter from Client Packet
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();
                string _password = _packet.ReadString();
                string _email = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    //Server.clients[_fromClient].SignUptoCognito(_username, _password, _email);
                    SignUpClients NewSignUp = new SignUpClients();
                    NewSignUp.SignUptoCognito(_clientIdCheck, _username, _password, _email);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SignUpClientRequest",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public static void SignIpClientRequest(int _fromClient, Packet _packet)
        {
            try
            {

                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();
                string _password = _packet.ReadString();

                if (_fromClient == _clientIdCheck)
                {
                    Server.clients[_fromClient].SignInToCognito(_username, _password);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "SignIpClientRequest",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }

        public static void GetRandomLoot(int _fromClient, Packet _packet)
        {
            try
            {
                int _clientIdCheck = _packet.ReadInt();
                UserSession _currentUserSession = _packet.ReadUserSession();
                int _coinType = _packet.ReadInt();
                int _coinQuality = _packet.ReadInt();
                

                if (_fromClient == _clientIdCheck)
                {

                    Server.clients[_fromClient].GetRandomLoot(_coinType,_coinQuality, _currentUserSession);
                    //CoinLoots coin = new CoinLoots();
                    //coin.GetRandomLoot((CoinLoots.CoinAverageQuality)_coinQuality);
                }
            }
            catch (Exception e)
            {
                NlogClass.target.WriteAsyncLogEvent(new AsyncLogEventInfo(new LogEventInfo(LogLevel.Error, "GetRandomLoot",  "Client Username : "+ Server.clients[_fromClient].myUser.Username + " | Read the Packet failed | Exception : " + e ), NlogClass.exceptions.Add));
            }
        }
    }
}
