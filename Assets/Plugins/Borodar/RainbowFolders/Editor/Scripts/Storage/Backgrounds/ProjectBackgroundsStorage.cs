using System;
using System.Collections.Generic;
using UnityEngine;
using StorageHelper = Borodar.RainbowFolders.TexturesStorageHelper<Borodar.RainbowFolders.ProjectBackground>;

namespace Borodar.RainbowFolders
{
    public static class ProjectBackgroundsStorage
    {
        private static readonly Dictionary<ProjectBackground, Texture2D> BACKGROUND_TEXTURES = new Dictionary<ProjectBackground, Texture2D>();
        private static readonly Dictionary<ProjectBackground, Lazy<string>> BACKGROUND_STRINGS = new Dictionary<ProjectBackground, Lazy<string>>()
        {
            {ProjectBackground.ClrRed,         new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAqElEQVQ4EZWRgQ3DIAwEoZNklOy/QydpqjRPZevyhIhEAj7YfzZQ38uyllK+GNuhNT6DEXF5ttcx6av/Jec91b2oAVCaQ66snrMTQIMnMnbSBLBtahq43zQBTHzcgWgzpi6HHbA9dnOn8xU68sDlRU6vMAsJdsuPI3TkyLKVRZonAAo4xLzXvwFQlHRqOr1IXqICHqRxpPMSVZFVZ2AtP47gBsJG1bVff5VFJEHEsuQUAAAAAElFTkSuQmCC")},
            {ProjectBackground.ClrVermilion,   new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAApUlEQVQ4EZ1QiRGAMAhr3cJtHMD9F3AM34YzXqCnVr1DUkhCaZ7GfkgpbSWWEuuZZ8nADHDYM25XCvj2EtlQ+w+aRINWmYmUTIM4PZ5V4zANUFR3xU4gBwzJaiC9ZrjTIE78tUKzSO7nVtBbKBa+g+BcK6Dz9QbG5xtEcTxzdFWnAQlvWVczfGegxCdT94hPRPbiCu4RdWok0qDKdytUxLOgQ1DKB60fIWpsPsbCAAAAAElFTkSuQmCC")},
            {ProjectBackground.ClrOrange,      new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAsUlEQVQ4EZWQAQ7DIAwDYb/ob/axvXbP2FqGozpyQ0ArEiK49iW0vl/bs5Ry9P3tez/Pj5yodcPn3ke/ZKtmYteiXglok8BKRqYRALLSZ8BBJ8Bo0k5hIl+amE5AnEBDWscJ/B+oaVUPk3EChCI9A0WP/0SYlR6NGcyezQkQ0JDCsrBrBLhwo7CmBNg4N8Kw2pQEQNAn4J4tfZr5FaCBf2DmJyAGtJOCh5qA4cNEiI3KD5IJJZo84XQDAAAAAElFTkSuQmCC")},
            {ProjectBackground.ClrAmber,       new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAqklEQVQ4EZWRAQ6DMAwD2/1iv+HDe9WesQGrOzkyTisVpFCL2pdE1PfruZVS9lZHq6/Up+lRqed4NBOe+j/ifYa6iuQjAIFZSBHJQwBMSletANXwVAJWAhqmPgnghzsn1okJEEz7DWg+6WUCvxzkcxOugO4rEyi0NyTAu6/AuocAJd/SCvApRiD3TP+CGwnz1eIvpAsm7FRw17MV1KgMbQQdKyDglxqkTuAf96cnzA70DsIAAAAASUVORK5CYII=")},
            {ProjectBackground.ClrYellow,      new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAsElEQVQ4EY1TCRKDMAiE/sLf9P/f8BmtTYFIu4BkdAa5ls1KRt737UlEH7G32HF6jV9ELDbENDY764Y1/EMa+ozp8rspTxircwJL8vhFnhnZCRSLJBnoXIjR2kACB608ELPFSADNoKYhnHAkaIDLcruD5RQ0ww7wEwATwrzEoCAgm6Qc4jvIjZw3fP9rVGlZ3tVQwaCCO6cWjBNk5px3an5LVOY7Q4WoU1Cklsn5B48vAIEn/yp7a7UAAAAASUVORK5CYII=")},
            {ProjectBackground.ClrLime,        new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAqUlEQVQ4Ea2Qiw3CMAxEE7Zgmy7cqRgDSsmzuOgS1MpIteT4dz47ruvjvpRS3k1fTbevxX8eKDVht1sLkL1pDe/PRwTZ5hm3i4ANXOZYNc+HLwIAXpwnicAtmCqCCKzqZJYe7jRsQOBNmQ0g7jcgyDaBlQxfUPLM+pbg+gZz4YxEtdjYj6gCNkMYGBF4M37mHmD6DWjKTAXn0m9A0qdmyK79AmyZqT9bfgCtoyjL+TMqmQAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrChartreuse,  new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAArElEQVQ4EZWRgQ3DMAgE7W7RbbpYpu0YSeqCk4+ep64cS8jAw4GTuryfr1LKx2w328hW89Vc9xzq9ocFOA3OnzvVABCEOiaw1H0AQovRAtDFs5Pz7lcAmEz17h6HO5GzuwFAuXvuBECXiwMmAGF5pV3fwLEqxlGDiDcIowb1Kc2AJP5IpCEjQCo8YfrM6zd6g4qyQJcTGBtos8YGS729BgCZNhX2rRmQRkxg2hejsySZiTECtgAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrHarlequin,   new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAr0lEQVQ4EY1RWxLEIAjDvUSv0+89/F6oT0HTRqRdO0MFAiFo+v6mWUS2bHu2tdpCp/psqNGe7ZN/+A44L2dy2HETjLSLdFU3ged2o2roqxIIFPBgRMEKzAeBBgxGzWEOBAqyAvbDxpqkS3wrizEbwgpKWVlkeJ2eoIgfWUGHNK8QC/2ThYJhyY7vukSVzCTsu54mvFbwDU93wHkbihWUlsFmDAU8yHwQGBsVPrndkBNLLyVSzgXMlAAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrGreen,       new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAmElEQVQ4EZWRCxKAIAhEtTN4oO7c8fqYkDQELpPNmAjLAzGXrawppauto62z72TvYFHs1S7tQF99tqk/5wjgb2Y2wjwLcJ0KgMia7oS9stawSwCmM3i04IoArhJCIgDSazDbGmDbG0G0huzpVwg7GFWMfNQBHGKUKLHPDPg+Egl2PQOWoSE6IYJqANIgPxcRAN3HTRhlav8N3vMf/ZzaXywAAAAASUVORK5CYII=")},
            {ProjectBackground.ClrEmerald,     new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAnUlEQVQ4EZ1RQQ6AIAwD3+FjPPltH6aClFgcyBaFZLCwtuvAz9u6OOfOFCHFcccuTuQM1FlDHqa0cUUmxtliohTwBlEtSQEVZBQ8BYa6Q5gCmK2dz2j8lCgAB39dZDwFIDnioPzCCBlNq0f8MsILo43wAqJdWtIp8moEjZSZ2kYHQ2SIUkBrYN3nplKgna9Hlk6BL78AsCz2yL27eAFBSiFE/wbSXQAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrSpringGreen, new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAuElEQVQ4EY1QiRHDMAizu0W2yVqdt2PkcQFHOSHXbXNHwFgPuC6v51pKOSxOi/2KjbLXHMB4Ph7286/2dP/1jAvuRw0BBzSgpKZ2woQrBJjsBHZiAe43J0FASSoIkaEPAVZWMZA1D2+ggE9nNUorDON1hcRhjNcVK6gbAalUlJ1DwDxCje6TLfW1bCFwebAV10rCOUxmK/w7QXoDJs0mUExfATN9z8EdhLECK090gsu4qCEwIf1s1zfz0iebR6oUSwAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrAquamarine,  new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAm0lEQVQ4Ea2QgQ2AIAwEwS3cxnWdyzFUkEdKPgVJjZoUHmmvT/28rYtzLqQ4Upxl32mHlkCO3EGHKS34Ygqf1ctFANZiNOIvCoB/QluBXgAgM501wxmctQBwaC65smgNfnyCTuywbsfiAAWWIg2qM2D7Oml4ZgfDxM4lmtYZfHagG5iB/AQusg60DhEOrEXs9r8ZoDs/gbuwbnIuS1cjyK4dccAAAAAASUVORK5CYII=")},
            {ProjectBackground.ClrCyan,        new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAtUlEQVQ4EY2RixHEIAhE9bpIN+m/jZRx+RyQwDwxOpcZZHWXFWJdtm0tpZwSh8T+xBdZsYfyzik+P7Lod90p1hqoBVlX3GBU0Ja3O6253KCl+o6c50XWzauBqCj0Ys15hEqDIAUEZrXgzpgGHZmK89b0NBjdykJqDNOAwlE3+Txegc5qlPduzvNuBBfNcu4gXkEJksQ0ZAd6HiPYhso/sJnxJ45upVfWxAgUzfB0hFmhc+zA8A9ikycD6t24QAAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrSkyBlue,     new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAsElEQVQ4EY2Qiw3DMAhE7W7Rbbpwp+oYbZIa1EPPH5pEQjnguAPX+/P1KKXsLbYWH8S74VWI4/xbI+k7BNqfGOVSkThHApawSYyZGUpg7qwr3MxNJHDVkbzuhNGPTuyxbmJVG5BkmE5jr8slQGUjjHk39EuMc0hgRTirdSdYQtfsBNanDdg8c49+dgK3CfKwpdczAQ5l2DfOBC6fIwF/EFjFCX+UnCMBmyU3cChBnfALCzcmyTdoAdUAAAAASUVORK5CYII=")},
            {ProjectBackground.ClrAzure,       new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAs0lEQVQ4EaWRgRGDMAhFk27RbVys03aMVi2QEl+ISb2rdyjC5/E1+f54LimlVWKTeCNekp8FNdtNRHplid2y+U11flnuAC2y6aLZUxfuDriyPcI6BxRcAZrGHegwh0af02kIoIMjH6HKwkzAuZQ7D2zNHBCHf4zV+b9OwSh0wK3RUV0ZEwdonUOExZnmnYCm0b0QX5paaU5hvrV0idFK/Ynqn83OwLfAJa0DdtTaiIC6OfgAbicnnDm2y8cAAAAASUVORK5CYII=")},
            {ProjectBackground.ClrCerulean,    new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAr0lEQVQ4EY2SgRHCMAwDE7ZgGwZg/wUYA0qJfHXu42Jw79w6liLZTfr1/ri11rYR7xEvxHPk34Kc7TJIevqI3bLfL/H8Ud5dQEWCTvr33V2g4h7FbI8LVEeIIs0FBFRGYKfGpwDBk9NRoMkyghYEM4FoMk+hslmi5FnOETJX1tnBaQQSs5wdiDPvQQQyAXZgHB8hAnGdCS73ICUBYKfKl1Mou0Jw/gPVqA7OkkaT/gE58CRsKSA1HwAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrBlue,        new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAArElEQVQ4EZWQAQ6AIAhFtdt0i+5/ku5QaUDSPojL2lj4+TzQvK77llK6KArFCXFQHgV6ykKm6KuRGGkKyFEx0LyvKoAmmqHeGLAeSQHUkJGAOTajLkMUwKbpqUDMCEA6eEzqh7xvwC4sYo6EbsjfDRAmuQKALCmcux4jKIDFtrb8RlfwunlEQ5448JbvIzJ5eu0Gl230Ctzs12u+4U96FDB0fRTMFT68XVk2vgEQ1iRfQ76zdwAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrIndigo,      new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAq0lEQVQ4EY2S0Q2EMAxDWyZhlNvh9v9mCEAcDkr1GgoXpEKIHcdG1O+8fEopO8521jrr4HjfOft0knTV6/H3fkSGC9yASBy8a2l1AeEZF5FzUGCw5LUl152DTIzIaQ4sz+u+C2QEm2EEqrOmLvuqmwMCGuAmCrDfOSDAgVhzkdWMkBEhR3WLEDc9vdOBcdzBDXhSQF8z7T8wOwAzZRfB1BJT/AadgzibiWRiP5amKH4XBiZzAAAAAElFTkSuQmCC")},
            {ProjectBackground.ClrViolet,      new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAArklEQVQ4EZWSAQ7DIAhFdXfZPXq5nXWH2FonzJ88xSWsCQXh+0Db+rg/j1LK2e3q9h72grdYZnXVbM956y976tf99W6mFsAXie1BJ0Bir0s4qccCsGDKda0GnGA6ggTyFCq3800T7Iq7XJiMgEzXoCGAHUOnUWTe4koAi6HTADDvMQGZCVbNdImkU/grNv10BArTMB3BaSDwPpAOP9h0BAozE0xfYe24rgUn2OL2AUS9JrOiCXynAAAAAElFTkSuQmCC")},
            {ProjectBackground.ClrPurple,      new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAArElEQVQ4EY2Qiw3DMAhE7Y6UCbp0huoM+Tg+5ItOJqRYIoA5Hjh1XX7fUsrZbe92DL+JR6wGHe389ASndasW5T+mJwAJIDwRTO9NTwAatUjQX0+ATkfTnBPk7gmYn8CGN289BECYeYLTEIDVdD0nHKuoxmICoobRF7pKABQK0UlhNwoKeBVGRQKyE3VLMBsBKGQgTkMACkrXGJOeDjT3T8xu4EDcwBWCi3mzdgHb3ST1vqEGMwAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrMagenta,     new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAuElEQVQ4EZWSgRHDIAhFsSt1ge4/RBZpGwso6ZNorvUO/crnBXOW7b49ROStsWu8ejyxmo6wfORM7zedbBSN6qpNtp8NeixfA5ATs2I7y+ASgFzwK1AIIJ2acILNc1yBpitNsMOiA6ddVa5yAWBrK6+dn3wBYGtTY6dmH39iZZK61/rCDtwTHWh2qKGxAYb0FwbA+X6tss8NSUzuYLCvNuzMNTtgEb/Ec2rzHE+ZZJqyzuC/X2IGygdrCCcSvgMbtAAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrFuchsia,     new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAArklEQVQ4EZ2QARLDIAgEtS/Li/OrvqJJU4/MMSeUpFNnkBNhEftzWZfW2j7sPWwTew39zZiDmu0xNqx+Ot8PV7OIeZ2AOS0D4z3PRwVgwq2vANUIGjddAarO+gfQ0x8oXRMrGOL+B1p8VZDuOII9J93mgDaCLkfIpWckjcYXKBmp8UxgjOMPPKZ01SyG17jpATCPzUlBo5Ar5XAEJCidBbeeACX/CuMI1uSf7tb0A2xIJvWEOFUAAAAAAElFTkSuQmCC")},
            {ProjectBackground.ClrRose,        new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAuElEQVQ4EZWRAQ7CMAwDW37DL/gW7+UZMEYd5s5yGgkqZXNS55Jp/XG931pr24j3iJfEc2gP3KNG33YZCU4fsYf6ZqeexSTQ0wiAjgLEQJ06CvWDAJ/o+YoATyfAJ3q+AsCzE+CGaoNUrwDVBqleAdKkY0WthyZAL/xzNNcNQhOA5BeIeqDnX8AUpSNfneThBm5OxsOgG8TWBOgFvJ77gJkT4E3VBrORggA0aFO1gXqCQQCB/7xjyAdN/ienW2LnbwAAAABJRU5ErkJggg==")},
            {ProjectBackground.ClrCrimson,     new Lazy<string>(()=> "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAs0lEQVQ4EYWS0Q3DIAxEIat0kf53/w26RZNQ2+iiM+6lkQzHYT8wSn8/Xs/W2mFxWuwUH9O/Ajles282+NfndI3jUlmseR2AnFaB6z7WQwGQ8HdWANUC+6EVQJ3Mb+A6vQHTOVHB3Lc3mKlcfFdQ9rY2S+M6ZbcafJBr2UItnU5pDY/IZE9d1wCufvoPmM4axT6zHxo38AXTWTOA/dAAOJvpXHSrARjp/HxVBSgtqETlRwtfnlAndOonlR0AAAAASUVORK5CYII=")},
        };

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static Texture2D GetBackground(int type)
        {
            return GetBackground((ProjectBackground) type);
        }

        public static Texture2D GetBackground(ProjectBackground type)
        {
            return StorageHelper.GetTexture(type, FilterMode.Bilinear, BACKGROUND_STRINGS, BACKGROUND_TEXTURES);
        }
    }
}