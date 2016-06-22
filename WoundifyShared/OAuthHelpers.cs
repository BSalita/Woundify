using System;

namespace WoundifyShared
{
    public class AccessTokenInfo
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
    }

    /*
     * This class demonstrates how to get a valid O-auth token.
     */
    public class Authentication : HttpMethods
    {
        private Uri accessUri;
        private System.Collections.Generic.List<Tuple<string, string>> header;
        private string grant;
        private AccessTokenInfo accessTokenInfo;
        private System.Threading.Timer accessTokenRenewer;

        //Access token expires every 10 minutes. Renew it every 9 minutes only.
        private const int RefreshTokenDuration = 9;

        public async System.Threading.Tasks.Task PerformAuthenticationAsync(Settings.BearerAuthentication bearer)
        {
            ServiceResponse sr = new ServiceResponse(this.ToString());
            string clientID = bearer.clientID;
            string clientSecret = bearer.clientSecret;
            string scope = bearer.scope;
            System.Collections.Generic.List<Tuple<string, string>> grantSubstitutes = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("{clientID}", System.Web.HttpUtility.UrlEncode(clientID)),
                new Tuple<string, string>("{clientSecret}", System.Web.HttpUtility.UrlEncode(clientSecret)),
                new Tuple<string, string>("{scope}", System.Web.HttpUtility.UrlEncode(scope)),
            };
            grant = bearer.grant;
            foreach (Tuple<string, string> r in grantSubstitutes)
            {
                grant = grant.Replace(r.Item1, r.Item2);
            }
            accessUri = new Uri(bearer.uri);
            header = new System.Collections.Generic.List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Content-Type", "application/x-www-form-urlencoded")
            };
            sr = await PostAsync(accessUri, grant, header);
            accessTokenInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenInfo>(sr.ResponseString);

            // renew the token every specfied minutes
            accessTokenRenewer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTokenExpiredCallbackAsync),
                                           this,
                                           TimeSpan.FromMinutes(RefreshTokenDuration),
                                           TimeSpan.FromMilliseconds(-1));
        }

        public AccessTokenInfo GetAccessToken()
        {
            return accessTokenInfo;
        }

        private async System.Threading.Tasks.Task RenewAccessTokenAsync()
        {
            ServiceResponse sr = new ServiceResponse(this.ToString());
            sr = await PostAsync(accessUri, grant, header);
            accessTokenInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenInfo>(sr.ResponseString); // warning: thread unsafe
            Log.WriteLine("Renewed token:" + accessTokenInfo.access_token);
        }

        private async void OnTokenExpiredCallbackAsync(object stateInfo)
        {
            try
            {
                await RenewAccessTokenAsync();
            }
            catch (Exception ex)
            {
                Log.WriteLine("Failed renewing access token. Details:" + ex.Message);
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Failed to reschedule the timer to renew access token. Details:" + ex.Message);
                }
            }
        }
    }
}
