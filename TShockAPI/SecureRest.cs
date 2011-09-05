﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HttpServer;

namespace TShockAPI
{
    public delegate bool VerifyD(string username, string password);
    public class SecureRest : Rest
    {
        public Dictionary<string, object> Tokens { get; protected set; }
        public event VerifyD Verify;
        public SecureRest(IPAddress ip, int port)
            : base(ip, port)
        {
            Tokens = new Dictionary<string, object>();
            Register(new RestCommand("/token/new/{username}/{password}", newtoken) { RequiesToken = false });
        }
        object newtoken(RestVerbs verbs, IParameterCollection parameters)
        {
            var user = verbs["username"];
            var pass = verbs["password"];

            var userAccount = TShock.Users.GetUserByName(user);
            if (userAccount == null)
            {
                return new Dictionary<string, string> { { "status", "401" }, { "error", "Invalid username/password combination provided. Please re-submit your query with a correct pair." } };
            }

            if (Tools.HashPassword(pass).ToUpper() != userAccount.Password.ToUpper())
            {
                return new Dictionary<string, string> { { "status", "401" }, { "error", "Invalid username/password combination provided. Please re-submit your query with a correct pair." } };
            }

            if (!Tools.GetGroup(userAccount.Group).HasPermission("api") && userAccount.Group != "superadmin")
            {
                return new Dictionary<string, string> { { "status", "403" }, { "error", "Although your account was successfully found and identified, your account lacks the permission required to use the API. (api)"} };
            }

            if (Verify != null && !Verify(user, pass))
                return new Dictionary<string, string> { { "status", "401" } , { "error", "Invalid username/password combination provided. Please re-submit your query with a correct pair." } };

            string hash = string.Empty;
            var rand = new Random();
            var randbytes = new byte[20];
            do
            {
                rand.NextBytes(randbytes);
                hash = Tools.HashPassword(randbytes);
            } while (Tokens.ContainsKey(hash));

            Tokens.Add(hash, new Object());

            return new Dictionary<string, string> { { "status", "200" } , { "token", hash } }; ;
        }
        protected override object ExecuteCommand(RestCommand cmd, RestVerbs verbs, IParameterCollection parms)
        {
            if (cmd.RequiesToken)
            {
                var strtoken = parms["token"];
                if (strtoken == null)
                    return new Dictionary<string, string> { { "status", "401" }, { "error", "Not authorized. The specified API endpoint requires a token." } };

                object token;
                if (!Tokens.TryGetValue(strtoken, out token))
                    return new Dictionary<string, string> { { "status", "403" }, { "error", "Not authorized. The specified API endpoint requires a token, but the provided token was not valid." } };
            }
            return base.ExecuteCommand(cmd, verbs, parms);
        }
    }
}
