var lines = File.ReadAllText("input").Split(Environment.NewLine);

var constraints = new List<Pick>()
{
    new(12, Color.Red),
    new(13, Color.Green),
    new(14, Color.Blue)
};

var puzzle = new Puzzle(lines, constraints);

var answer = puzzle.GetAnswer();

Console.WriteLine($"The answer is: {answer}");

class Puzzle
{
    private readonly List<Game> games = new();
    private readonly List<Game> goodGames = new();
    public Puzzle(string[] inputLines, List<Pick> constraints)
    {        
        foreach (var line in inputLines)
        {
            var game = new Game(line); 
            games.Add(game);
            if (game.IsConstraintSatisfied(constraints))
            {
                goodGames.Add(game);
            }
        }
    }

    public PuzzleAnswer GetAnswer()
    {
        return new(goodGames.Sum(g => g.Id), games.Sum(g => g.Power));
    }

    public record PuzzleAnswer(int IdSum, int PowerSum);
}

class Game
{
    private readonly List<Pick> topPicks = new();

    public int Power => topPicks.Aggregate(1, (curr, next) => curr * next.Count);
    public int Id {get; private set;}

    public Game(string line)
    {
        var head = line.Split(':')[0];
        var tail = line.Split(':')[1].Replace(';', ','); 

        if (int.TryParse(head.Replace("Game ", ""), out int id))
        {
            Id = id;
        }
        else
        {
            throw new ArgumentException("Game id could not be parsed");
        }

        var picks = tail.Split(',').Select(pickSubstring => new Pick(pickSubstring));

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
    }

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

    public Pick(string pickSubstring)
    {
        var segments = pickSubstring.TrimStart().Split(' ');
        Count = int.Parse(segments[0]);
        color = ParseColor(segments[1]);
    }

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

    private static Color ParseColor(string colorString)
    {
        return colorString switch
        {
            "red" => Color.Red,
            "blue" => Color.Blue,
            "green" => Color.Green,
            _ => throw new ArgumentException($"unknown color: {colorString}"),
        };
    }
}

enum Color 
{
    Red,
    Green,
    Blue
}