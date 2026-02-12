using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicApp.Generators;
using MusicApp.Models;

namespace MusicApp.Pages;

public class IndexModel : PageModel
{
    private static readonly HashSet<string> SupportedLanguages =
    [
        "en-US",
        "de-DE",
        "ru-RU"
    ];

    [BindProperty(SupportsGet = true)]
    public string Lang { get; set; } = "en-US";

    [BindProperty(SupportsGet = true)]
    public ulong Seed { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public double Likes { get; set; } = 1.2;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string View { get; set; } = "table";

    public int PageSize { get; } = 10;

    public List<Song> Songs { get; set; } = new();

    public void OnGet()
    {
        Lang = (Lang ?? "en-US").Trim();
        if (!SupportedLanguages.Contains(Lang))
            Lang = "en-US";

        Likes = Math.Clamp(Likes, 0, 10);

        if (PageNumber < 1)
            PageNumber = 1;

        int effectivePage = PageNumber;

        if (View == "gallery" && !Request.Headers.ContainsKey("X-Requested-With"))
        {
            effectivePage = 1;
            PageNumber = 1;
        }

        ulong contentSeed = SeedHelper.Hash(Seed, Lang, effectivePage);
        ulong likesSeed = SeedHelper.Hash(Seed, "likes", effectivePage);
        ulong audioSeed = SeedHelper.Hash(Seed, "audio", effectivePage);
        ulong reviewSeed = SeedHelper.Hash(Seed, "review", effectivePage);

        var contentGenerator = new SongContentGenerator(Lang);

        for (int i = 0; i < PageSize; i++)
        {
            int index = (effectivePage - 1) * PageSize + i + 1;

            ulong recordContentSeed = SeedHelper.Hash(contentSeed, index);
            ulong recordLikesSeed = SeedHelper.Hash(likesSeed, index);
            ulong recordAudioSeed = SeedHelper.Hash(audioSeed, index);
            ulong recordReviewSeed = SeedHelper.Hash(reviewSeed, index);

            var song = contentGenerator.Generate(index, recordContentSeed);
            song.Likes = LikesGenerator.Generate(Likes, recordLikesSeed);

            song.Audio = AudioGenerator.Generate(recordAudioSeed);
            song.Review = ReviewGenerator.Generate(Lang.StartsWith("de") ? "de" : Lang.StartsWith("ru") ? "ru" : "en", recordReviewSeed);
            song.CoverUrl = CoverGenerator.BuildUrl(recordContentSeed, song.Title, song.Artist);

            Songs.Add(song);
        }
    }
}