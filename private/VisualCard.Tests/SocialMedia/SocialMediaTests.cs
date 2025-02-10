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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using VisualCard.Extras.Misc.SocialMedia;
using VisualCard.Parsers;
using VisualCard.Parts.Implementations;

namespace VisualCard.Tests.SocialMedia
{
    [TestClass]
    public class SocialMediaTests
    {
        private static readonly string targetContact =
            """
            BEGIN:VCARD
            VERSION:3.0
            TEL:495-522-3560
            EMAIL:john.s@acme.co
            NOTE:Note test for VisualCard
            FN:John Sanders
            N:Sanders;John;;;
            ADR:;;Los Angeles;;;;USA
            X-VISUALCARD-SOCIAL:FB;john.s
            X-VISUALCARD-SOCIAL:TW;john.s
            X-VISUALCARD-SOCIAL:MD;john.s@fedi.verse
            X-VISUALCARD-SOCIAL;TYPE=PREF:MD;fedi.verse;john.s@fedi.verse
            X-VISUALCARD-SOCIAL:BS;john.s@bsky.social
            X-VISUALCARD-SOCIAL:YT;john.s
            X-VISUALCARD-SOCIAL:OD;john.s
            X-VISUALCARD-SOCIAL:IG;john.s
            X-VISUALCARD-SOCIAL:SC;john.s
            X-VISUALCARD-SOCIAL:TK;john.s
            X-VISUALCARD-SOCIAL:WA;14955223560
            X-VISUALCARD-SOCIAL:TG;john.s
            END:VCARD
            """
        ;

        [TestMethod]
        [DataRow(SocialMediaApp.Facebook, 0, "https://facebook.com/john.s")]
        [DataRow(SocialMediaApp.Twitter, 1, "https://x.com/john.s")]
        [DataRow(SocialMediaApp.Mastodon, 2, "https://mastodon.social/john.s@fedi.verse")]
        [DataRow(SocialMediaApp.Mastodon, 3, "https://fedi.verse/john.s@fedi.verse")]
        [DataRow(SocialMediaApp.Bluesky, 4, "https://bsky.app/profile/john.s@bsky.social")]
        [DataRow(SocialMediaApp.YouTube, 5, "https://youtube.com/@john.s")]
        [DataRow(SocialMediaApp.Odysee, 6, "https://odysee.com/@john.s")]
        [DataRow(SocialMediaApp.Instagram, 7, "https://instagram.com/john.s")]
        [DataRow(SocialMediaApp.Snapchat, 8, "https://snapchat.com/add/john.s")]
        [DataRow(SocialMediaApp.TikTok, 9, "https://tiktok.com/@john.s")]
        [DataRow(SocialMediaApp.WhatsApp, 10, "https://wa.me/14955223560")]
        [DataRow(SocialMediaApp.Telegram, 11, "https://t.me/john.s")]
        public void TestGetSocialMediaUri(SocialMediaApp app, int idx, string expectedUri)
        {
            // Parse the contact
            var card = CardTools.GetCardsFromString(targetContact)[0];

            // Verify that we have the social media properties
            var xNames = card.GetPartsArray<XNameInfo>();
            xNames.ShouldNotBeEmpty();

            // Now, get the media from the index and construct a URI
            var media = xNames[idx];
            var mediaUri = SocialMediaTools.GetSocialMediaUri(app, media);

            // Verify the URI
            string mediaUriString = mediaUri.ToString();
            mediaUriString.ShouldBe(expectedUri);
        }
    }
}
