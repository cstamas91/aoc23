var lines = File.ReadAllText("input").Split(Environment.NewLine);

var constraints = new Dictionary<string, int>
{
    {"red", 12},
    {"green", 13},
    {"blue", 14}
};

static Line ProcessLine(string line)
{
    var head = line.Split(':')[0];
    var tail = line.Split(':')[1]; 

    var idParsed = int.TryParse(head.Replace("Game ", ""), out int id);

    var picks = tail.Split(',').SelectMany(s => s.Split(';'));

    var pickStats = new Dictionary<string, (int min, int max)>();

    foreach (var pickSubstring in picks) 
    {
        var pick = ProcessPick(pickSubstring);

        if (pickStats.ContainsKey(pick.Color)){
            if (pickStats[pick.Color].max < pick.Count) {
                pickStats[pick.Color] = (pickStats[pick.Color].min, pick.Count);
            }
            if (pickStats[pick.Color].min > pick.Count) {
                pickStats[pick.Color] = (pick.Count, pickStats[pick.Color].max);
            }
        } else {
            pickStats.Add(pick.Color, (pick.Count, pick.Count));
        }
    }


    return new Line(id, pickStats);
}

static Pick ProcessPick(string pickSubstring)
{
    var segments = pickSubstring.TrimStart().Split(' ');

    return new Pick(int.Parse(segments[0]), segments[1]);
}

static void GetGoodGameIdSum(string[] lines, Dictionary<string, int> constraints)
{
    var ggIdSum = 0;
    var powerSum = 0;

    foreach (var line in lines)
    {
        var game = ProcessLine(line);
        if (game.IsConstraintSatisfied(constraints)) {
            ggIdSum += game.Id;
        }

        powerSum += game.Power;

        Console.WriteLine($"[{line}] -> {game.Power}");
    }

    Console.WriteLine($"IdSum: {ggIdSum}, PowerSum: {powerSum}");
}

GetGoodGameIdSum(lines, constraints);

class Line
{
    private readonly Dictionary<string, (int min, int max)> topPicks;

    public int Power => topPicks.Values.Aggregate(1, (curr, next) => curr * next.max);
    public int Id {get; private set;}

    public Line(int id, Dictionary<string, (int min, int max)> picks)
    {
        Id = id;
        this.topPicks = picks;
    }

    public bool IsConstraintSatisfied(Dictionary<string, int> constraint)
    {
        foreach (var color in constraint.Keys)
        {
            if (topPicks.ContainsKey(color) && constraint[color] < topPicks[color].max)
            {
                return false;
            }
        }

        return true;
    }
}

record Pick(int Count, string Color);