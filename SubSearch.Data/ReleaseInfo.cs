using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubSearch.Data
{
    public class ReleaseInfo
    {
        /// <summary>
        ///     The zero space char.
        /// </summary>
        public static readonly char Separator = '\u200B';

        private static readonly string TempPadding = "¶";

        private static readonly string[] ReleaseTypes =
        {
            "BD25", "BD5", "BD50", "BD9", "BDMV", "BDR[13]", "BDRip", "BDSCR", "BluRay", "Blu-Ray", "BRip", "BRRip",
            "CAM", "CAM-Rip", "DDC", "DSR", "DSRip", "DTHRip", "DTVRip", "DVBRip", "DVD-5", "DVD-9", "DVD-Full",
            "DVDMux", "DVDR[11]", "DVDRip", "DVDSCR", "DVDSCREENER", "Full-Rip", "HC", "HDRip", "HD-Rip", "HDTC",
            "HDTS", "HDTV", "HDTVRip", "ISO rip", "lossless rip", "PDTV", "PDVD", "PPV", "PPVRip", "PreDVDRip", "R5",
            "R5.AC3.5.1.HQ", "R5.LINE", "SATRip", "SCR", "SCREENER", "TC", "TELECINE", "TELESYNC", "TS", "TVRip",
            "untouched rip", "VODR", "VODRip", "WEB (Scene)", "WEB Cap", "WEB DL", "WEB Rip", "WEBCAP", "WEB-Cap",
            "WEBDL", "WEB-DL", "WEB-DLRip", "WEBRip", "WEB-Rip", "WORKPRINT", "WP[7]"
        };

        private static readonly string[] Sizes = {"480p", "720p", "1080p", "2160p", "4K"};

        private static readonly string[] Encoders =
        {
            "x264", "OpenH264", "x265", "Xvid", "libvpx", "FFmpeg codecs", "Lagarith", "libtheora", "dirac-research",
            "Huffyuv", "Daala", "Thor", "Turing", "AV1"
        };

        private static readonly string[] AudioChannels = {"2.0", "2.1", "5.1", "6.1", "7.1"};

        private static readonly string[] Formats =
            ReleaseTypes.Union(Sizes).Union(Encoders).Union(AudioChannels).ToArray();

        private static readonly string NormalizedFormatsRegex =
            GetOrRegexStr(Formats.Select(i => Normalize(i, TempPadding)).Distinct().ToArray());

        public ReleaseInfo(string fullName)
        {
            FullName = fullName;
            Parse();
        }

        public string FullName { get; }
        public string NormalizedFullName { get; private set; }
        public string Title { get; private set; }
        public string Episode { get; private set; }
        public string Format { get; private set; }
        public string Extra { get; private set; }
        public string Year { get; private set; }

        public bool IsValid { get; private set; }

        private void Parse()
        {
            Normalize();
            var title = "Title";
            var episode = "Episode";
            var format = "Format";
            var year = "Year";
            var other = "Other";
            var regexp =
                $"^(?<{title}>.+?)({Separator}(?<{episode}>S?\\d+[Ex]\\d+(.+?)?))?((?<{format}>({Separator}({NormalizedFormatsRegex}))+)|(?<{year}>{Separator}\\d{{4}}))+(?<{other}>.+)?$";
            var match = Regex.Match(NormalizedFullName, regexp, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Title = Val(match, title);
                Episode = Val(match, episode);
                Format = Val(match, format);
                Year = Val(match, year);
                Extra = Val(match, other);
                IsValid = true;
            }
        }

        private static string Val(Match match, string groupName)
        {
            var group = match.Success ? match.Groups[groupName] : null;
            return group != null && group.Success ? group.Value.Trim(Separator) : null;
        }

        private void Normalize()
        {
            NormalizedFullName = FullName;
            foreach (var format in Formats)
                NormalizedFullName = NormalizedFullName.Replace(format, Normalize(format, TempPadding));

            NormalizedFullName =
                string.Join(Separator.ToString(),
                    Normalize(NormalizedFullName, Separator.ToString())
                        .Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries));
        }

        public static implicit operator ReleaseInfo(string releaseName)
        {
            return new ReleaseInfo(releaseName);
        }

        private static string Normalize(string input, string replace)
        {
            var regexStr = GetOrRegexStr(new[] {".", " ", "-", "_", "(", ")", "[", "]"});
            return Regex.Replace(input, regexStr, replace);
        }

        private static string GetOrRegexStr(string[] conditions)
        {
            return string.Join("|", conditions.Select(Regex.Escape).ToArray());
        }
    }
}