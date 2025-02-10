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
using System.Text;
using VisualCard.Parts;
using VisualCard.Parts.Implementations;

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
                throw new ArgumentException("There is no such social media app.");

            // Check the social media value info using the X- nonstandard names
            var socialMediaFields = socialMediaValue.XValues ??
                throw new ArgumentException("There are no social media fields.");
            if (socialMediaValue.XKeyName != "VISUALCARD-SOCIAL")
                throw new ArgumentException($"The value info is not a social media value for {appInfo.appAbbreviation}. Expected VISUALCARD-SOCIAL, got {socialMediaValue.XKeyName} with {socialMediaFields.Length} values");

            // Check the value. It should be two or more values with the first one holding the social media abbreviation
            // name in two letters.
            if (socialMediaFields.Length < 2)
                throw new ArgumentException($"For {appInfo.appAbbreviation}, expected at least an abbreviation and a name. Got {socialMediaFields.Length} values. Hint: X-VISUALCARD-SOCIAL:{appInfo.appAbbreviation};NAME.");
            string valueAbbreviation = socialMediaFields[0];
            string valueName = socialMediaFields[1];
            if (appInfo.appAbbreviation != valueAbbreviation)
                throw new ArgumentException($"For {appInfo.appAbbreviation}, expected a matching abbreviation for the app. Got {valueAbbreviation}. Hint: X-VISUALCARD-SOCIAL:{appInfo.appAbbreviation};NAME.");

            // Now, initialize the string builder for the URL
            var uriBuilder = new UriBuilder();
            string hostPart = appInfo.appHostPart;

            // Check for Mastodon, since the account could be in a site other than mastodon.social.
            if (app == SocialMediaApp.Mastodon)
            {
                // Check to see if we need to change the host name
                if (socialMediaFields.Length > 2)
                {
                    hostPart = socialMediaFields[1] + "/";
                    valueName = socialMediaFields[2];
                }
            }

            // Now, build the URI with all the available information
            string hostName = hostPart.Substring(0, hostPart.IndexOf('/'));
            string hostPath = hostPart.Substring(hostPart.IndexOf('/')) + valueName;
            uriBuilder.Scheme = "https";
            uriBuilder.Host = hostName;
            uriBuilder.Path = hostPath;

            // Return the string
            return uriBuilder.Uri;
        }
    }
}
