namespace Valuator.Repository;

using System.Globalization;
using System.Text.Json;
using StackExchange.Redis;
using Valuator.Model;
using Valuator.Repository;

public class TextRepository : ITextRepository
{
    private const string IDS_KEY = "IDS";
    private const string TEXT_KEY = "TEXT";
    private const string RANK_KEY = "RANK";
    private const string SIMILARITY_KEY = "SIMILARITY";

    private readonly IDatabase _database;

    public TextRepository(IDatabase database)
    {
        _database = database;
    }

    public Text Store(Text text)
    {
        text.Id ??= Guid.NewGuid().ToString();
        StoreText(text);
        StoreRank(text);
        StoreSimilarity(text);
        UpdateIdsList(text);

        return text;
    }

    public Text? Get(string id)
    {
        var content = GetContent(id);
        if (content == null)
        {
            return null;
        }

        return new Text
        {
            Id = id,
            Rank = GetRank(id),
            Similarity = GetSimilarity(id),
            Content = content,
        };
    }

    public List<Text> GetAll()
    {
        return GetIds().Select(Get).ToList();
    }

    private void StoreText(Text text)
    {
        _database.StringSet($"{TEXT_KEY}-{text.Id}", text.Content);
    }

    private void StoreRank(Text text)
    {
        _database.StringSet($"{RANK_KEY}-{text.Id}", text.Rank);
    }

    private void StoreSimilarity(Text text)
    {
        _database.StringSet($"{SIMILARITY_KEY}-{text.Id}", text.Similarity);
    }

    private void UpdateIdsList(Text text)
    {
        var idsList = GetIds();
        idsList.Add(text.Id);
        _database.StringSet(IDS_KEY, JsonSerializer.Serialize(idsList));
    }

    private string? GetContent(string id)
    {
        var content = _database.StringGet($"{TEXT_KEY}-{id}");
        return content.IsNull ? null : content.ToString();
    }

    private double GetRank(string id)
    {
        string value = _database.StringGet($"{RANK_KEY}-{id}");
        double rank = double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        return rank;
    }

    private int GetSimilarity(string id)
    {
        return (int)_database.StringGet($"{SIMILARITY_KEY}-{id}");
    }

    private List<string> GetIds()
    {
        var ids = _database.StringGet(IDS_KEY);
        return ids.IsNullOrEmpty ? new List<string>() : JsonSerializer.Deserialize<List<string>>(ids);
    }
}