﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor.Docs.Models
{
    public class TeamMember
    {
        public string Name { get; set; }
        public string From { get; set; }
        public string GitHub { get; set; }
        public bool GitHubSponsor { get; set; }
        public string Avatar => $"https://github.com/{GitHub}.png?size=56";
        public string LinkedIn { get; set; }
        public string Bio { get; set; }
    }
}
