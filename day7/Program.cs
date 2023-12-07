var lines = File.ReadAllText(args[0]).Split(Environment.NewLine);

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
    Console.WriteLine(hands[i-1].Type);
    points += hands[i-1].Bid * i;
}

Console.WriteLine(points);

class Hand : IComparable
{
    private List<CardType> originalInput;
    private readonly List<CardType> Cards;
    public HandType Type {get;init;}
    public long Bid {get;init;}
    public Hand(string input)
    {
        var parts = input.Split(' ');
        Bid = long.Parse(parts[1]);
        Cards = parts[0].Select(ConvertCard)
            .OrderByDescending(c => c)
            .ToList();
        Type = GetHandType();
        originalInput = parts[0].Select(ConvertCard).ToList();
    }

    private HandType GetHandType()
    {
        int i = 1;
        CardSet cardSet = new() { Type = Cards[0], Count = 1};
        var handTypes = new List<HandType>();
        while (i < Cards.Count)
        {
            if (Cards[i] == cardSet.Type)
            {
                cardSet.Count++;
            }
            else
            {
                var handType = cardSet.ToHandType();
                if ((handType.Strength == HandType.HandStrength.HighCard &&
                    !handTypes.Any(ht => ht.Strength == HandType.HandStrength.HighCard)) || 
                    handType.Strength != HandType.HandStrength.HighCard)
                {
                    handTypes.Add(handType);
                }

                cardSet = new() {Type=Cards[i], Count = 1};
            }

            i++;
        }

        handTypes.Add(cardSet.ToHandType());

        return ReduceHandTypes(handTypes);        
    }

    private static HandType ReduceHandTypes(List<HandType> handTypes)
    {
        var groupings = handTypes.GroupBy(ht => ht.Strength).ToDictionary(g => g.Key, g => g.ToList());

        if (groupings.ContainsKey(HandType.HandStrength.FiveOfAKind))
        {
            return groupings[HandType.HandStrength.FiveOfAKind].First();
        }
        else if (groupings.ContainsKey(HandType.HandStrength.FourOfAKind))
        {
            return groupings[HandType.HandStrength.FourOfAKind].First();
        }
        else if (groupings.ContainsKey(HandType.HandStrength.ThreeOfAKind))
        {
            if (groupings.ContainsKey(HandType.HandStrength.OnePair))
            {
                return new HandType(
                    HandType.HandStrength.FullHouse,
                    groupings[HandType.HandStrength.ThreeOfAKind].First().Of,
                    groupings[HandType.HandStrength.OnePair].First().Of);
            } 
            else 
            {
                return groupings[HandType.HandStrength.ThreeOfAKind].First();
            }
        }
        else if (groupings.ContainsKey(HandType.HandStrength.OnePair))
        {
            if (groupings[HandType.HandStrength.OnePair].Count == 2)
            {
                return new HandType(
                    HandType.HandStrength.TwoPair,
                    groupings[HandType.HandStrength.OnePair][0].Of,
                    groupings[HandType.HandStrength.OnePair][1].Of);
            }

            return groupings[HandType.HandStrength.OnePair].First();
        }
        else
        {
            return groupings[HandType.HandStrength.HighCard].MaxBy(t => t.Of)!;
        }
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

struct CardSet
{
    public CardType Type {get;init;}
    public int Count {get;set;}

    public readonly HandType ToHandType()
    {
        return Count switch 
        {
            1 => new HandType(HandType.HandStrength.HighCard, Type),
            2 => new HandType(HandType.HandStrength.OnePair, Type),
            3 => new HandType(HandType.HandStrength.ThreeOfAKind, Type),
            4 => new HandType(HandType.HandStrength.FourOfAKind, Type),
            5 => new HandType(HandType.HandStrength.FiveOfAKind, Type),
            _ => throw new InvalidOperationException("cant have more than five of a kind.")
        };
    }
}

enum CardType
{
    Two = 0,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Joker,
    Queen,
    King,
    Ace

}

class HandType : IComparable
{
    public HandStrength Strength {get;init;}
    public CardType Of {get;init;}
    public CardType? And {get;init;}
    public HandType(HandStrength strength, CardType of)
    {
        Strength = strength;
        Of = of;
    }
    public HandType(HandStrength strength, CardType of, CardType and) : this(strength, of)
    {
        And = and;
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
        if (And is null)
        {
            return $"[{Strength} of {Of}]";
        }

        return $"[{Strength} of {Of} and {And}]";
    }

    public int CompareTo(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return 1;
        }

        HandType other = (HandType)obj;

        return Strength.CompareTo(other.Strength);

        // if (Strength > other.Strength)
        // {
        //     return 1;
        // } 
        // else if (Strength < other.Strength)
        // {
        //     return -1;
        // }
        // else 
        // {
        //     var ofComparison = Of.CompareTo(other.Of);
        //     if (ofComparison == 0 && And is not null)
        //     {
        //         return And.Value.CompareTo(other.And!.Value);
        //     }
        //     return ofComparison;
        // }
    }
}
