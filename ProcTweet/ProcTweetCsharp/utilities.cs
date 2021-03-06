﻿using System;
using System.IO;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Dimebrain.TweetSharp;
using Dimebrain.TweetSharp.Extensions;
using Dimebrain.TweetSharp.Fluent;
using Dimebrain.TweetSharp.Model;
using EPB;
using System.Text.RegularExpressions;
using Dimebrain.TweetSharp.Core.Web;


namespace ProcTweetCsharp
{
    public class Utilities
    {
        private const string HtmlTagPattern = "<.*?>";
        public static string StripHtml(string inputString)
        {
            return Regex.Replace(inputString, HtmlTagPattern, string.Empty);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets an authorisation token and puts it in the configuration file. </summary>
        ///
        /// <remarks>   Olivier Gagnon, 2009-11-13. </remarks>
        ///
        /// <param name="logininfo">    The login informations. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void GetAuthToken(LoginInfo logininfo)
        {
            Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            IFluentTwitter tf = FluentTwitter.CreateRequest(logininfo.TcInfo);

            tf.Configuration.UseUrlShortening(ShortenUrlServiceProvider.TinyUrl);

            OAuthToken rtoken = tf.Authentication.GetRequestToken(logininfo.TcInfo.ConsumerKey, logininfo.TcInfo.ConsumerSecret).Request().AsToken();

            tf.Authentication.AuthorizeDesktop(rtoken.Token);

            var epb = new EnterPinBox();
            logininfo.Authtoken =
                tf.Authentication.GetAccessToken(logininfo.TcInfo.ConsumerKey, logininfo.TcInfo.ConsumerSecret, rtoken.Token,
                                                 (string)epb.PIN).Request().AsToken();
            oConfig.AppSettings.Settings.Remove("atoken");
            oConfig.AppSettings.Settings.Add("atoken", logininfo.Authtoken.Token);
            oConfig.AppSettings.Settings.Remove("atokens");
            oConfig.AppSettings.Settings.Add("atokens", logininfo.Authtoken.TokenSecret);
            oConfig.Save();
            logininfo.IsLogged = true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Verify twitter credentials. </summary>
        ///
        /// <remarks>   Olivier Gagnon, 2009-11-04. </remarks>
        ///
        /// <param name="username"> The username. </param>
        /// <param name="password"> The password. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool VerifyTwitterCredentials(string username, string password)
        {
            var twitter = FluentTwitter.CreateRequest()
                .AuthenticateAs(username, password)
                .Account().VerifyCredentials().AsJson().Request();
            
            var response = twitter.AsUser();
            if (response == null)
            {
                MessageBox.Show("Authentication failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Information about the account. </summary>
    ///
    /// <remarks>   Olivier Gagnon, 2009-11-11. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class AccountInfo
    {
        //Properties
        public string Username { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        /// <summary>
        /// Profile background image URI
        /// </summary>
        public string Background { get; set; }

        /// <summary>Constructor</summary>
        public AccountInfo(string username)
        {
            Username = username;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a twitter account information. </summary>
        ///
        /// <remarks>   Olivier Gagnon, 2009-11-04. </remarks>
        ///
        /// <param name="username"> The username. </param>
        ///
        /// <returns>   The twitter account information. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static AccountInfo GetTwitterAccountInfo(string username)
        {
            var ac = new AccountInfo(username);
            var request = FluentTwitter.CreateRequest()
                .Users().ShowProfileFor(username).AsJson().Request();
            var response = request.AsUser();

            ac.Name = response.Name;
            ac.Bio = response.Description;
            ac.Background = response.ProfileBackgroundImageUrl;

            return ac;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Information about the login. </summary>
    ///
    /// <remarks>   Olivier Gagnon, 2009-11-18. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class LoginInfo
    {
        public LoginInfo()
        {
            IsLogged = false;
            Username = "";
        }

        public bool IsLogged { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public TwitterClientInfo TcInfo { get; set; }
        public OAuthToken Authtoken { get; set; }

    }

}