using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class CognitoManager
    {
        public AmazonCognitoIdentityProviderClient provider;
        public CognitoUserPool userPool;

        public CognitoManager()
        {
            provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.REGION);
            userPool = new CognitoUserPool(Constants.POOL_ID, Constants.CLIENTAPP_ID, provider, Constants.NeokySecret);            
        }
    }
}
