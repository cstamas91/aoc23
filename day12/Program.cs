using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

var input = File.ReadAllText("example").Split(Environment.NewLine);

var records = input.Select(l => new Record(l)).ToList();

foreach (var record in records)
{
    Console.WriteLine($"{record.conditionPart} -> {record.GetPossiblePermutationCount()}");
}

Console.WriteLine(records.Sum(r => r.GetPossiblePermutationCount()));

class Record
{
    private const char WORKING_SPRING = '.';
    private const char BROKEN_SPRING = '#';
    private const char UNKNOWN_SPRING = '?';
    public readonly string conditionPart;
    private readonly List<int> revisedRecord;
    private readonly Regex revisionRegex;

    public Record(string input)
    {
        var parts = input.Split(' ');
        conditionPart = string.Join('?', Enumerable.Range(0, 5).Select(_ => parts[0]));
        revisedRecord = string.Join(',', Enumerable.Range(0, 5).Select(_ => parts[1])).Split(',').Select(int.Parse).ToList();
        revisionRegex = BuildRegex();
    }

    private bool IsRevisedRecordSatisfied(string conditionRecord)
    {
        return revisionRegex.IsMatch(conditionRecord);
    }

    private Regex BuildRegex()
    {
        var sb = new StringBuilder();
        sb.Append(@"^(\.*)");
        sb.Append(string.Join(@"[\.]+", revisedRecord.Select(i => $@"[#]{{{i}}}")));
        sb.Append(@"($|(\.+$))");

        return new Regex(sb.ToString());
    }

    private List<string> GetPermutations()
    {
        var result = new List<string>(){string.Empty};

        for (int i = 0; i < conditionPart.Length; i++)
        {
            result = AddChar(result, conditionPart[i]);
        }

        return result;
    }

    private static List<string> AddChar(List<string> currentPermutations, char c)
    {
        var result = new List<string>();
        if (c == WORKING_SPRING || c == BROKEN_SPRING)
        {
            foreach (var p in currentPermutations)
            {
                result.Add(p + c);
            }

            return result;
        }

        foreach (var perm in currentPermutations)
        {
            result.Add(perm + WORKING_SPRING);
            result.Add(perm + BROKEN_SPRING);
        }

        return result;
    }

    public int GetPossiblePermutationCount()
    {
        var permutations = GetPermutations();
        // foreach (var p in permutations)
        // {
        //     Console.WriteLine($"{p} -> {IsRevisedRecordSatisfied(p)}");
        // }
        return permutations.Count(IsRevisedRecordSatisfied);
    }
}