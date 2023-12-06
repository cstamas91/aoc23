var input = File.ReadAllText(args[0])
    .Split(Environment.NewLine);

var times = input[0].Replace("Time:", "").Trim().Split(" ").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
var records = input[1].Replace("Distance:", "").Trim().Split(" ").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
var races = new List<Race>();
for (int i = 0; i < times.Count; i++)
{
    races.Add(new Race(times[i], records[i]));
}

Console.WriteLine(races.Aggregate(1, (curr, nxt) => curr * nxt.GetRecordBeatingInputCount()));

class Race
{
    private int Length {get;init;}
    private int RecordToBeat {get;init;}

    public Race(int length, int recordToBeat)
    {
        Length = length;
        RecordToBeat = recordToBeat;
    }

    public int GetRecordBeatingInputCount()
    {
        return Enumerable.Range(0, Length).Where(i => i * (Length - i) > RecordToBeat).Count();
    }
}