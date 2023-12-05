using System.Collections.Immutable;

var lines = File.ReadAllText("input").Split(Environment.NewLine);

var cardTallies = lines.Select(l => new CardTally(new Card(l), 1)).ToList();

for (int i = 0; i < cardTallies.Count; i++)
{
    for (int copy = 0; copy < cardTallies[i].Copies; copy++)
    {
        for (int j = i+1; j<= i+cardTallies[i].Card.MatchCount; j++)
        {
            cardTallies[j].Copies++;
        }
    }
}

Console.WriteLine(cardTallies.Sum(c => c.Copies));

public class CardTally
{
    public Card Card {get;set;}
    public int Copies {get;set;}

    public CardTally(Card card, int copies)
    {
        Card = card;
        Copies = copies;
    }
}

public class Card
{
    public int Id {get;init;}
    public ImmutableHashSet<int> WinningNumbers {get;init;}
    public ImmutableHashSet<int> Numbers {get;init;}
    public int MatchCount => WinningNumbers.Intersect(Numbers).Count;
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