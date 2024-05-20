using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Repository;
using Valuator.Model;
using Microsoft.AspNetCore.Connections;
using System.Text;
using NATS.Client;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    readonly ITextRepository _textRepository;

    public IndexModel(ILogger<IndexModel> logger, ITextRepository textRepository)
    {
        _logger = logger;
        _textRepository = textRepository;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);
        if (text == "" || text == null || string.IsNullOrEmpty(text))
        {
            return Redirect("/");
        }

        Text textModel = new()
        {
            Rank = CalculateRank(text),
            Similarity = DuplicateSearch(text),
            Content = text
        };

        //TODO: сохранить в БД text по ключу textKey    
        //TODO: посчитать rank и сохранить в БД по ключу rankKey
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        textModel = _textRepository.Store(textModel);

        Options options = ConnectionFactory.GetDefaultOptions();
        options.Url = "127.0.0.1:4222";
        IConnection _natsConnection = new ConnectionFactory().CreateConnection(options);

        _natsConnection.Publish("RankCalculator", Encoding.UTF8.GetBytes(textModel.Id));

        System.Threading.Thread.Sleep(1000);

        return Redirect($"summary?id={textModel.Id}");
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int totalCharacters = text.Length;
        int nonAlphabetCharacters = text.Count(c => !char.IsLetter(c));

        return (double)nonAlphabetCharacters / totalCharacters;
    }

    private int DuplicateSearch(string text)
    {
        return _textRepository.GetAll().Find(textModel => textModel.Content == text) != null ? 1 : 0;
    }
}
