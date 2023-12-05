using System.Collections.Immutable;
using System.Text.RegularExpressions;

var lines = File.ReadAllText("input").Split(Environment.NewLine);

var pointSum = 0;

foreach (var line in lines)
{
    var card = new Card(line);
    pointSum += card.Points;
}

Console.WriteLine(pointSum);

public class Card
{
    public int Id {get;init;}
    public ImmutableHashSet<int> WinningNumbers {get;init;}
    public ImmutableHashSet<int> Numbers {get;init;}
    private int MatchCount => WinningNumbers.Intersect(Numbers).Count;
    public int Points => (int)Math.Pow(2, MatchCount - 1);
    public Card(string line)
    {
        var parts = line.Split(':');
        Id = int.Parse(parts[0].Replace("Card ", ""));
        var tailParts = parts[1].Split('|');
        WinningNumbers = tailParts[0]
            .Trim()
            .Split(' ')
            .Where(s => !string.IsNullOrEmpty(s.Trim()))
            .Select(int.Parse)
            .ToImmutableHashSet<int>();
        Numbers = tailParts[1]
            .Trim()
            .Split(' ')
            .Where(s => !string.IsNullOrEmpty(s.Trim()))
            .Select(int.Parse)
            .ToImmutableHashSet<int>();
    }
}