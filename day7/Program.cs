var lines = File.ReadAllText(args.Length > 0 ? args[0] : "example").Split(Environment.NewLine);

var hands = new List<Hand>();
foreach (var line in lines)
{
    var play = new Hand(line);
    hands.Add(play);
}

hands = hands.OrderBy(h => h).ToList();
long points = 0;
for (int i = 1; i <= hands.Count; i++)
{
    Console.WriteLine($"{i,4}. {hands[i-1]}");
    points += hands[i-1].Bid * i;
}

Console.WriteLine(points);

class Hand : IComparable
{

    public override string ToString()
    {
        return $"{Type,12} ({string.Join(string.Empty, originalInputString)})";
    }
    private string originalInputString;
    private List<CardType> originalInput;
    private readonly List<CardType> Cards;
    private CardSet currentCardSet;
    private CardSet? jokerCardSet;
    public HandType Type {get;init;}
    public long Bid {get;init;}
    public Hand(string input)
    {
        var parts = input.Split(' ');
        Bid = long.Parse(parts[1]);
        originalInputString = parts[0];
        Cards = parts[0].Select(ConvertCard)
            .OrderByDescending(c => c)
            .ToList();
        originalInput = parts[0].Select(ConvertCard).ToList();
        currentCardSet = new(){Type=Cards[0], Count = 1};
        if (currentCardSet.Type == CardType.Joker)
        {
            jokerCardSet = currentCardSet;
        }
        Type = GetHandType();
    }

    private CardSet? firstCardSet;
    private CardSet? secondCardSet;
    private HandType GetHandType()
    {
        int i = 1;
        while (i < Cards.Count)
        {
            if (Cards[i] == currentCardSet.Type)
            {
                currentCardSet.Count++;
            }
            else
            {
                CloseCardSet(i);
            }

            i++;
        }

        CloseCardSet(i-1);

        if (jokerCardSet is not null && firstCardSet!.Type != CardType.Joker)
        {
            firstCardSet!.Count += jokerCardSet.Count;
            if (firstCardSet.Count + secondCardSet?.Count > 5)
            {
                secondCardSet = null;
            }
        }

        if (secondCardSet is not null)
        {
            return new HandType(firstCardSet!, secondCardSet);
        }

        return new HandType(firstCardSet!);
    }

    private void CloseCardSet(int i)
    {
        if (currentCardSet.Type == CardType.Joker)
        {
            jokerCardSet = currentCardSet;
        }

        if (firstCardSet is null)
        {
            firstCardSet = currentCardSet;
        }
        else if (currentCardSet.Type != CardType.Joker && (currentCardSet.Count > 1 || firstCardSet.Type == CardType.Joker))
        {
            if (firstCardSet.Count == 1 || firstCardSet.Type == CardType.Joker)
            {
                firstCardSet = currentCardSet;
            }
            else
            {
                secondCardSet = currentCardSet;
            }

        }

        currentCardSet = new() {Type=Cards[i], Count = 1};
    }

    private static CardType ConvertCard(char character)
    {
        return character switch
        {
            '2' => CardType.Two,
            '3' => CardType.Three,
            '4' => CardType.Four,
            '5' => CardType.Five,
            '6' => CardType.Six,
            '7' => CardType.Seven,
            '8' => CardType.Eight,
            '9' => CardType.Nine,
            'T' => CardType.Ten,
            'J' => CardType.Joker,
            'Q' => CardType.Queen,
            'K' => CardType.King,
            'A' => CardType.Ace,
            _ => throw new ArgumentException("unknown card character", nameof(character)),
        };
    }

    public int CompareTo(object? obj)
    {
        if (obj is null && GetType() != obj!.GetType())
        {
            return 1;
        }

        Hand other = (Hand)obj;

        var typeComparison = Type.CompareTo(other.Type);

        if (typeComparison == 0)
        {
            for (int i = 0; i < originalInput.Count; i++)
            {
                var charComparison = originalInput[i].CompareTo(other.originalInput[i]);
                if (charComparison != 0)
                {
                    return charComparison;
                }
            }
        }

        return typeComparison;
    }
}

class CardSet
{
    public CardType Type {get;init;}
    public int Count {get;set;}
}

enum CardType
{
    Joker = 0,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Queen,
    King,
    Ace

}

class HandType : IComparable
{
    public HandStrength Strength {get;init;}
    public CardType Of {get;init;}
    public CardType? And {get;init;}


    public HandType(CardSet set)
    {
        (Strength, Of) = set.Count switch 
        {
            1 => (HandStrength.HighCard, set.Type),
            2 => (HandStrength.OnePair, set.Type),
            3 => (HandStrength.ThreeOfAKind, set.Type),
            4 => (HandStrength.FourOfAKind, set.Type),
            5 => (HandStrength.FiveOfAKind, set.Type),
            _ => throw new ArgumentException("invalid number of cards")
        };
    }

    public HandType(CardSet firstSet, CardSet secondSet)
    {
        (Strength, Of, And) = (firstSet.Count, secondSet.Count) switch {
            (2,2) => (HandStrength.TwoPair, firstSet.Type, secondSet.Type),
            (3,2) or (2,3) => (HandStrength.FullHouse, firstSet.Type, secondSet.Type),
            (3, _) => (HandStrength.ThreeOfAKind, firstSet.Type, secondSet.Type),
            (4, _) => (HandStrength.FourOfAKind, firstSet.Type, secondSet.Type),
            (5, _) => (HandStrength.FiveOfAKind, firstSet.Type, secondSet.Type),
            _ => throw new ArgumentException("cant have more than 5 cards or less than 1")
        };    
    }

    public enum HandStrength 
    {
        HighCard = 0,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind,
        FiveOfAKind,
    }

    public override string ToString()
    {
        return Strength.ToString();
    }

    public int CompareTo(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return 1;
        }

        HandType other = (HandType)obj;

        return Strength.CompareTo(other.Strength);
    }
}
