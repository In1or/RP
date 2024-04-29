namespace Valuator.Model;

public class Text
{
    public string? Id { get; set; }

    public string Content { get; set; } = "";

    public double Rank { get; set; }

    public int Similarity { get; set; }
}