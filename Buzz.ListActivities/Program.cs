﻿/*
Copyright 2011 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using DotNetOpenAuth.OAuth2;
using Google.Apis;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Util;

namespace Buzz.ListActivities
{
    /// <summary>
    /// Buzz API sample
    /// Uses the Buzz API to list all recent activities.
    /// http://code.google.com/apis/buzz/v1/using_rest.html
    /// </summary>
    internal class Program
    {
        private static readonly string Scope = BuzzService.Scopes.BuzzReadonly.GetStringValue();

        [STAThread]
        static void Main(string[] args)
        {
            // Display the header and initialize the sample.
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Buzz: List activities");

            // Register the authenticator.
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
            provider.ClientIdentifier = ClientCredentials.ClientID;
            provider.ClientSecret = ClientCredentials.ClientSecret;
            AuthenticatorFactory.GetInstance().RegisterAuthenticator(
                () => new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthentication));

            // Create the service.
            var service = new BuzzService();
            ListBuzzActivities(service);
            CommandLine.PressAnyKeyToExit();
        }

        private static IAuthorizationState GetAuthentication(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "google.samples.dotnet.buzz";
            const string KEY = "b9=Branuhe7ufrab?exA#a?e";

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY, Scope);
            if (state != null)
            {
                client.RefreshToken(state);
                return state; // Yes - we are done.
            }

            // Retrieve the authorization url:
            state = new AuthorizationState(new[] { Scope })
                        { Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl) };
            Uri authUri = client.RequestUserAuthorization(state);

            // Do a new authorization request.
            string authCode = AuthorizationMgr.RequestAuthorization(authUri);
            state = client.ProcessUserAuthorization(authCode, state);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state, Scope);
            return state;
        }

        private static void ListBuzzActivities(BuzzService service)
        {
            // Execute the request.
            CommandLine.WriteAction("Fetching activities ...");
            var response = service.Activities.List(ActivitiesResource.ScopeEnum.Consumption, "@me").Fetch();
            CommandLine.WriteLine();

            if (response.Items == null)
            {
                CommandLine.WriteError("There are no activities to fetch!");
                return;
            }

            // List all activities.
            foreach (Activity item in response.Items)
            {
                CommandLine.WriteResult(item.Actor.Name, item.Title);
            }
        }
    }
}
