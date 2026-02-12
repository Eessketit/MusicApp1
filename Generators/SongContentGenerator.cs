using Bogus;
using System.Text;
using MusicApp.Models;

namespace MusicApp.Generators;

public class SongContentGenerator
{
    private readonly string _lang;
    private readonly Faker _faker;

    private static readonly Dictionary<string, string> FakerLocales = new()
    {
        ["en-US"] = "en",
        ["de-DE"] = "de",
        ["ru-RU"] = "ru"
    };
    private static readonly Dictionary<string, string[]> TitleWords = new()
    {
        ["en-US"] = new[] { "Midnight", "Fire", "Shadow", "Light", "Dream", "Storm", "Echo", "Waves", "Silence", "Sky", "Gravity", "Flame" },
        ["de-DE"] = new[] { "Nacht", "Feuer", "Schatten", "Licht", "Traum", "Sturm", "Echo", "Wellen", "Stille", "Himmel", "Kraft", "Glanz" },
        ["ru-RU"] = new[] { "Ночь", "Огонь", "Тень", "Свет", "Мечта", "Буря", "Эхо", "Волны", "Тишина", "Небо", "Сила", "Пламя" }
    };

    private static readonly Dictionary<string, string[]> TitleTemplates = new()
    {
        ["en-US"] = new[] { "{0}", "{0} of {1}", "The {0}", "{0} and {1}" },
        ["de-DE"] = new[] { "{0}", "{0} und {1}", "Das {0}", "{0} aus {1}" },
        ["ru-RU"] = new[] { "{0}", "{0} и {1}", "Только {0}", "{0} внутри" }
    };

    public SongContentGenerator(string lang)
    {
        _lang = NormalizeLang(lang);
        _faker = new Faker(FakerLocales[_lang]);
    }

    public Song Generate(int index, ulong seed)
    {
        Randomizer.Seed = new Random((int)(seed % int.MaxValue));

        return new Song
        {
            Index = index,
            Title = GenerateTitle(),
            Artist = GenerateArtist(),
            Album = GenerateAlbum(),
            Genre = _faker.Music.Genre(),
            Lyrics = GenerateLyrics()
        };
    }

    private static string NormalizeLang(string? lang)
    {
        var value = (lang ?? "en-US").Trim();
        return FakerLocales.ContainsKey(value) ? value : "en-US";
    }

    private string GenerateTitle()
    {
        var template = _faker.PickRandom(TitleTemplates[_lang]);
        var words = TitleWords[_lang];
        return string.Format(template, _faker.PickRandom(words), _faker.PickRandom(words));
    }

    private string GenerateArtist()
    {
        if (_faker.Random.Bool())
        {
            return _faker.Name.FullName();
        }

        var left = _faker.PickRandom(TitleWords[_lang]);
        var right = _faker.PickRandom(TitleWords[_lang]);

        return _lang switch
        {
            "de-DE" => $"Die {left} {right}",
            "ru-RU" => $"Группа «{left} {right}»",
            _ => $"The {left} {right}"
        };
    }

    private string GenerateAlbum()
    {
        if (_faker.Random.Double() < 0.35)
        {
            return "Single";
        }
        var word = _faker.PickRandom(TitleWords[_lang]);

        return _lang switch
        {
            "de-DE" => $"Album \"{word}\"",
            "ru-RU" => $"Альбом «{word}»",
            _ => $"Album \"{word}\""
        };
    }

    private string GenerateLyrics()
    {
        var sb = new StringBuilder();
        AppendSection(sb, 1);
        AppendSection(sb, 2);
        return sb.ToString().Trim();
    }

    private void AppendSection(StringBuilder sb, int verse)
    {
        var verseLabel = _lang switch
        {
            "de-DE" => $"Strophe {verse}",
            "ru-RU" => $"Куплет {verse}",
            _ => $"Verse {verse}"
        };

        var chorusLabel = _lang switch
        {
            "de-DE" => "Refrain",
            "ru-RU" => "Припев",
            _ => "Chorus"
        };

        sb.AppendLine(verseLabel);
        sb.AppendLine(GenerateLines(4));
        sb.AppendLine();
        sb.AppendLine(chorusLabel);
        sb.AppendLine(GenerateLines(2, repeatLast: true));
        sb.AppendLine();
    }

    private string GenerateLines(int count, bool repeatLast = false)
    {
        var sb = new StringBuilder();
        string? lastLine = null;

        for (int i = 0; i < count; i++)
        {
            var line = GenerateLyricLine();
            sb.AppendLine(line);
            lastLine = line;
        }

        if (repeatLast && lastLine is not null)
        {
            sb.AppendLine(lastLine);
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateLyricLine()
    {
        var templates = _lang switch
        {
            "de-DE" => new[]
            {
                "Ich werde {emotion}, wenn das {noun} beginnt zu {verb}",
                "Wir {verb} durch das {noun}",
                "Dein {noun} macht mich {emotion}",
                "Ich höre das {noun} in der Nacht",
                "Heute werden wir {verb}"
            },
            "ru-RU" => new[]
            {
                "Я становлюсь {emotion}, когда {noun} начинает {verb}",
                "Мы {verb} сквозь {noun}",
                "Твой {noun} делает меня {emotion}",
                "Я слышу {noun} в тишине",
                "Сегодня мы будем {verb}"
                 },
            _ => new[]
            {
                "I feel {emotion} when the {noun} starts to {verb}",
                "We {verb} through the {noun}",
                "Your {noun} makes me feel {emotion}",
                "I hear the {noun} in the night",
                "Tonight we will {verb}"
                }
        };
        return ApplyTemplate(_faker.PickRandom(templates));
    }

    private string ApplyTemplate(string template)
    {
        return template
            .Replace("{emotion}", _faker.Hacker.Adjective())
            .Replace("{noun}", _faker.Hacker.Noun())
            .Replace("{verb}", _faker.Hacker.Verb());
    }
}