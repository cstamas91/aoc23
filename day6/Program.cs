var input = File.ReadAllText(args[0])
    .Split(Environment.NewLine);

var time = long.Parse(input[0].Replace("Time:", "").Replace(" ", ""));
var record = long.Parse(input[1].Replace("Distance:", "").Replace(" ", ""));
var race = new Race(time, record);

Console.WriteLine(race.GetRecordBeatingInputCount());

class Race
{
    private long Length {get;init;}
    private long RecordToBeat {get;init;}

    public Race(long length, long recordToBeat)
    {
        Length = length;
        RecordToBeat = recordToBeat;
    }

    public int GetRecordBeatingInputCount()
    {
        var res = 0;
        for (long i = 0; i < Length; i++)
        {
            if (i*(Length-i) > RecordToBeat)
            {
                res++;
            }
        }

        return res;
    }
}