[h1]GPS Helper[/h1]

Based on [i][url=https://steamcommunity.com/sharedfiles/filedetails/?id=2478246718]https://steamcommunity.com/sharedfiles/filedetails/?id=2478246718[/url][/i] by CrazyDave137, seemingly abandoned and non-functional. I picked it up, cleaned it a bit, and fixed issues with string filtering. Commands are the same, I just fixed some internals and minor performance issuese that nobody would have noticed anyway.

[h2]Usage[/h2]

[i][b][string][/b][/i] denotes an optional parameter. You may provide it, or leave it blank.

When adding a GPS, it will be used as the base name. Omitting will use the Player name instead.

When filtering, providing an argument will try to do a substring match ('contains') on the GPS name, description, and hexadecimal string representation of the colour.

[list]
[*][i][b]/gpsx [string][/b][/i] creates a GPS point at current location, noting the local planet in the description. An auto-incrementing hexadecimal auto-ID string is appended to uniquely identify each GPS.
[*][i][b]/gpsx_reset[/b][/i] resets the auto-incrementing auto-ID. Does not modify existing GPS.
[*][i][b]/gps_export [string][/b][/i] export all matching GPS to clipboard. No argument exports all GPS.
[*][i][b]/gps_toggle [string][/b][/i] toggle visibility of all matching GPS. No argument toggles all GPS.
[*][i][b]/gps_off [string][/b][/i] turn visibility of matching GPS off. No argument toggles all GPS.
[*][i][b]/gps_on [string][/b][/i] turn visibility of matching GPS on. No argument toggles all GPS.
[*][i][b]/gps_remove [string][/b][/i] remove all matching GPS. No argument removes all GPS. [b]WARNING: Can not be undone![/b]
[/list]

[h2]Props[/h2]

[list]
[*]CrazyDave137 for authoring the original mod
[*]Digi for putting up with my endless flood of questions and being an all round nice chap
[/list]

