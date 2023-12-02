var lines = File.ReadAllText("input").Split(Environment.NewLine);

var constraints = new List<Pick>()
{
    new(12, Color.Red),
    new(13, Color.Green),
    new(14, Color.Blue)
};

static Game ProcessLine(string line)
{
    var head = line.Split(':')[0];
    var tail = line.Split(':')[1]; 

    var idParsed = int.TryParse(head.Replace("Game ", ""), out int id);

    var picks = ProcessPicks(tail);

    var topPicks = new List<Pick>();

    foreach (var pick in picks) 
    {
        var currentTopPickForColor = topPicks.FirstOrDefault(p => p.Color == pick.Color);

        if (currentTopPickForColor is not null) 
        {
            if (currentTopPickForColor.Count < pick.Count)
            {
                currentTopPickForColor.Count = pick.Count;
            }
        }
        else 
        {
            topPicks.Add(pick);
        }
    }

    return new Game(id, topPicks);
}

static List<Pick> ProcessPicks(string picksString)
{
    var pickSubstrings = picksString.Split(',').SelectMany(s => s.Split(';'));
    return pickSubstrings.Select(ProcessPick).ToList();
}

static Pick ProcessPick(string pickSubstring)
{
    var segments = pickSubstring.TrimStart().Split(' ');
    return new Pick(int.Parse(segments[0]), Enum.Parse<Color>(segments[1], ignoreCase: true));
}

static void GetGoodGameIdSum(string[] lines, List<Pick> constraints)
{
    var ggIdSum = 0;
    var powerSum = 0;

    var games = lines.Select(ProcessLine);

    var goodGames = games.Where(g => g.IsConstraintSatisfied(constraints)).ToList();
    ggIdSum = goodGames.Sum(g => g.Id);
    powerSum = games.Sum(g => g.Power);

    Console.WriteLine($"IdSum: {ggIdSum}, PowerSum: {powerSum}");
}

GetGoodGameIdSum(lines, constraints);

class Game
{
    private readonly List<Pick> topPicks;

    public int Power => topPicks.Aggregate(1, (curr, next) => curr * next.Count);
    public int Id {get; private set;}

    public Game(int id, List<Pick> picks)
    {
        Id = id;
        topPicks = picks;
    }

    public bool IsConstraintSatisfied(List<Pick> constraint) =>
        constraint.All(constraintPick => topPicks.All(topPick => !topPick.DoesViolateConstraint(constraintPick)));

    private string PicksDebugString() => string.Join(',', topPicks.Select(p => $"{p.Count} {p.Color}"));
    public string DebugString() => $"[Game {Id}: {PicksDebugString()}]";
}

class Pick
{
    private readonly Color color;

    public Pick(int count, Color color)
    {
        Count = count;
        this.color = color;
    }

    public int Count {get; set;}

    public Color Color => color;

    public bool DoesViolateConstraint(Pick constraintPick)
    {
        return color == constraintPick.Color && Count > constraintPick.Count;
    }
}

enum Color 
{
    Red,
    Green,
    Blue
}