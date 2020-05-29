using System;
using System.Collections.Generic;
using System.Text;

using NLog;
using NLog.Targets;
using NLog.Config;

using NLog.AWS.Logger;
using Amazon.Runtime;

namespace GameServer
{
    public static class NlogClass
    {
        //My static Variables to access it from Other Classes  
        public static Target target;
        public static List<Exception> exceptions;

        public static void ConfigureNLog()
        {           
            // Just a Config to access to AWS CloudWatch
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var awsTarget = new AWSTarget()
            {
                LogGroup = "NeokyGroup",
                Region = "eu-west-2",
                Credentials = new BasicAWSCredentials(Constants.AWS_ACCESS_KEY_ID, Constants.AWS_SECRET_ACCESS_KEY)
            };
            config.AddTarget("aws", awsTarget);

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, awsTarget));

            LogManager.Configuration = config;
            target = LogManager.Configuration.FindTargetByName("aws");
            exceptions = new List<Exception>();
        }
    }
}
