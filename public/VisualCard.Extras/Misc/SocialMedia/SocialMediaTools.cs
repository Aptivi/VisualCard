//
// VisualCard  Copyright (C) 2021-2025  Aptivi
//
// This file is part of VisualCard
//
// VisualCard is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// VisualCard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using Textify.General;
using VisualCard.Common.Diagnostics;
using VisualCard.Common.Parts.Implementations;

namespace VisualCard.Extras.Misc.SocialMedia
{
    /// <summary>
    /// Social media tools for vCards and vCalendars
    /// </summary>
    public static class SocialMediaTools
    {
        private static readonly Dictionary<SocialMediaApp, (string, string)> mediaInfo = new()
        {
            { SocialMediaApp.Facebook,  ("FB", "facebook.com/") },
            { SocialMediaApp.Twitter,   ("TW", "x.com/") },
            { SocialMediaApp.Mastodon,  ("MD", "mastodon.social/") },
            { SocialMediaApp.Bluesky,   ("BS", "bsky.app/profile/") },
            { SocialMediaApp.YouTube,   ("YT", "youtube.com/@") },
            { SocialMediaApp.Odysee,    ("OD", "odysee.com/@") },
            { SocialMediaApp.Instagram, ("IG", "instagram.com/") },
            { SocialMediaApp.Snapchat,  ("SC", "snapchat.com/add/") },
            { SocialMediaApp.TikTok,    ("TK", "tiktok.com/@") },
            { SocialMediaApp.WhatsApp,  ("WA", "wa.me/") },
            { SocialMediaApp.Telegram,  ("TG", "t.me/") },
        };

        /// <summary>
        /// Gets a URI instance of the social media account link
        /// </summary>
        /// <param name="app">Social media app to use</param>
        /// <param name="socialMediaValue">Value info from the X-VISUALCARD-SOCIAL key</param>
        /// <returns>A social media URI</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Uri GetSocialMediaUri(SocialMediaApp app, XNameInfo socialMediaValue)
        {
            // Check the app
            if (!mediaInfo.TryGetValue(app, out (string appAbbreviation, string appHostPart) appInfo))
            {
                LoggingTools.Error("Invalid social media app {0}", app);
                throw new ArgumentException("There is no such social media app.");
            }

            // Check the social media value info using the X- nonstandard names
            LoggingTools.Info("Parsing {0} fields with key name {1}", socialMediaValue.XValues?.Length ?? 0, socialMediaValue.XKeyName);
            var socialMediaFields = socialMediaValue.XValues ??
                throw new ArgumentException("There are no social media fields.");
            if (socialMediaValue.XKeyName != "VISUALCARD-SOCIAL")
                throw new ArgumentException("The value info is not a social media value for {0}. Expected VISUALCARD-SOCIAL, got {1} with {2} values".FormatString(appInfo.appAbbreviation, socialMediaValue.XKeyName, socialMediaFields.Length));

            // Check the value. It should be two or more values with the first one holding the social media abbreviation
            // name in two letters.
            if (socialMediaFields.Length < 2)
                throw new ArgumentException("For {0}, expected at least an abbreviation and a name. Got {1} values. Hint:".FormatString(appInfo.appAbbreviation, socialMediaFields.Length) + $" X-VISUALCARD-SOCIAL:{appInfo.appAbbreviation};NAME.");
            string valueAbbreviation = socialMediaFields[0];
            string valueName = socialMediaFields[1];
            LoggingTools.Info("Checking abbreviation {0} by comparing it with {1}", valueAbbreviation, appInfo.appAbbreviation);
            if (appInfo.appAbbreviation != valueAbbreviation)
                throw new ArgumentException("For {0}, expected a matching abbreviation for the app. Got {1}. Hint:".FormatString(appInfo.appAbbreviation, valueAbbreviation) + $" X-VISUALCARD-SOCIAL:{appInfo.appAbbreviation};NAME.");

            // Now, initialize the string builder for the URL
            var uriBuilder = new UriBuilder();
            string hostPart = appInfo.appHostPart;
            LoggingTools.Info("Builder initialized with {0}", hostPart);

            // Check for Mastodon, since the account could be in a site other than mastodon.social.
            if (app == SocialMediaApp.Mastodon)
            {
                // Check to see if we need to change the host name
                LoggingTools.Info("Checking for fediverse host...");
                if (socialMediaFields.Length > 2)
                {
                    hostPart = socialMediaFields[1] + "/";
                    valueName = socialMediaFields[2];
                    LoggingTools.Debug("Host part is {0}, value is {1}", hostPart, valueName);
                }
            }

            // Now, build the URI with all the available information
            string hostName = hostPart.Substring(0, hostPart.IndexOf('/'));
            string hostPath = hostPart.Substring(hostPart.IndexOf('/')) + valueName;
            LoggingTools.Debug("Host name is {0}, path is {1}", hostName, hostPath);
            uriBuilder.Scheme = "https";
            uriBuilder.Host = hostName;
            uriBuilder.Path = hostPath;

            // Return the string
            LoggingTools.Info("Returning {0}...", uriBuilder.Uri.ToString());
            return uriBuilder.Uri;
        }
    }
}
