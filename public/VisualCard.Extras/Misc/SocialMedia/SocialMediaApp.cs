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

namespace VisualCard.Extras.Misc.SocialMedia
{
    /// <summary>
    /// Social media application enumeration
    /// </summary>
    public enum SocialMediaApp
    {
        /// <summary>
        /// Facebook account name (<c>X-VISUALCARD-SOCIAL:FB;name</c>, URL: <c>https://facebook.com/name</c>)
        /// </summary>
        Facebook,
        /// <summary>
        /// X/Twitter account handle (<c>X-VISUALCARD-SOCIAL:TW;name</c>, URL: <c>https://x.com/name</c>)
        /// </summary>
        Twitter,
        /// <summary>
        /// Mastodon account handle (<c>X-VISUALCARD-SOCIAL:MD;fediverse;name</c>, URL: <c>https://fediverse/name</c> with the <c>fediverse</c> field defaulting to <c>mastodon.social</c>)
        /// </summary>
        Mastodon,
        /// <summary>
        /// Bluesky account handle (<c>X-VISUALCARD-SOCIAL:BS;name</c>, URL: <c>https://bsky.app/profile/name</c>)
        /// </summary>
        Bluesky,
        /// <summary>
        /// YouTube channel name (<c>X-VISUALCARD-SOCIAL:YT;name</c>, URL: <c>https://youtube.com/@name</c>)
        /// </summary>
        YouTube,
        /// <summary>
        /// Odysee channel name (<c>X-VISUALCARD-SOCIAL:OD;name</c>, URL: <c>https://odysee.com/@name</c>)
        /// </summary>
        Odysee,
        /// <summary>
        /// Instagram account handle (<c>X-VISUALCARD-SOCIAL:IG;name</c>, URL: <c>https://instagram.com/name</c>)
        /// </summary>
        Instagram,
        /// <summary>
        /// Snapchat account handle (<c>X-VISUALCARD-SOCIAL:SC;name</c>, URL: <c>https://snapchat.com/add/name</c>)
        /// </summary>
        Snapchat,
        /// <summary>
        /// TikTok account handle (<c>X-VISUALCARD-SOCIAL:TK;name</c>, URL: <c>https://tiktok.com/@name</c>)
        /// </summary>
        TikTok,
        /// <summary>
        /// WhatsApp account phone number (<c>X-VISUALCARD-SOCIAL:WA;phonenum</c>, URL: <c>https://wa.me/phonenum</c>)
        /// </summary>
        WhatsApp,
        /// <summary>
        /// Telegram account name (<c>X-VISUALCARD-SOCIAL:TG;name</c>, URL: <c>https://t.me/name</c>)
        /// </summary>
        Telegram,
    }
}
