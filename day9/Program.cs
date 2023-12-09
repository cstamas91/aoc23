var input = File.ReadAllText("input").Split(Environment.NewLine);

var histories = input.Select(l => new History(l)).ToList();

long nextSum = 0;
long prevSum = 0;
foreach (var history in histories)
{
    var nextValue = history.PredictNextSequenceItem();
    var prevValue = history.PredictPreviousSequenceItem();
    nextSum += nextValue;
    prevSum += prevValue;

    Console.WriteLine($"{prevValue} {nextValue}");
}
Console.WriteLine();
Console.WriteLine(nextSum);
Console.WriteLine(prevSum);

class History
{
    private readonly List<long> inputSequence = new();
    private List<List<long>> helperSequences = new();

    public History(string input)
    {
        inputSequence.AddRange(input.Split(' ').Select(long.Parse));
        helperSequences.AddRange(GenerateHelperSequences(inputSequence));
    }

    public long PredictNextSequenceItem()
    {
        for (int i = helperSequences.Count - 1; i >= 1; i--)
        {
            helperSequences[i-1].Add(helperSequences[i][^1] + helperSequences[i-1][^1]);
        }

        return inputSequence[^1] + helperSequences[0][^1];
    }

    public long PredictPreviousSequenceItem()
    {
        for (int i = helperSequences.Count - 1; i >= 1; i--)
        {
            helperSequences[i-1].Insert(0, helperSequences[i-1][0] - helperSequences[i][0]);
        }

        return inputSequence[0] - helperSequences[0][0];
    }

    private static List<List<long>> GenerateHelperSequences(List<long> seq)
    {
        var res = new List<List<long>>();
        var helperSeq = GenerateHelperSequence(seq);

        while (!helperSeq.All(n => n == 0))
        {
            res.Add(helperSeq);
            helperSeq = GenerateHelperSequence(helperSeq);
        }

        res.Add(helperSeq);

        return res;
    }

    private static List<long> GenerateHelperSequence(List<long> seq)
    {
        var res = new List<long>();
        var rangeEnd = seq.Count - 1;
        for (int i = 0; i <  rangeEnd; i++)
        {
            res.Add(seq[i+1] - seq[i]);
        }

        return res;
    }
}