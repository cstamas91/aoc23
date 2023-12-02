using System.Text;

var examples = File.ReadAllText("exampleInput.txt").Split(Environment.NewLine);
var inputLines = File.ReadAllText("input.txt").Split(Environment.NewLine);

var wordToInt = new Dictionary<string, int>()
{
    {"one", 1},
    {"two", 2},
    {"three", 3},
    {"four", 4},
    {"five", 5},
    {"six", 6},
    {"seven", 7},
    {"eight", 8},
    {"nine", 9},
    {"1", 1},
    {"2", 2},
    {"3", 3},
    {"4", 4},
    {"5", 5},
    {"6", 6},
    {"7", 7},
    {"8", 8},
    {"9", 9},
    {"0", 0},
};

static string DebugDict(Dictionary<int, int> dict) 
{
    var sb = new StringBuilder();
    foreach (var k in dict.Keys)
    {
        sb.Append($"{k}: {dict[k]},");
    }

    return sb.ToString();
}

static int GetCalibrationValueFromLine(Dictionary<string, int> words, ReadOnlySpan<char> line) 
{
    var matches = new Dictionary<int, int>();
    foreach (var key in words.Keys)
    {
        var index = line.IndexOf(key);
        if (index > -1)
        {
            matches.Add(index, words[key]);
        }

        var lastIndex = line.LastIndexOf(key);
        if (lastIndex > -1 && lastIndex != index)
        {
            matches.Add(lastIndex, words[key]);
        }
    }

    var minKey = matches.Keys.Min();
    var maxKey = matches.Keys.Max();

    var res = (matches[minKey] * 10) + matches[maxKey];

    return res;
}

// Console.WriteLine("example");
// Console.WriteLine(examples.Select(l => GetCalibrationValueFromLine(wordToInt, l)).Sum());
Console.WriteLine("inputs");
Console.WriteLine(inputLines.Select(l => GetCalibrationValueFromLine(wordToInt, l)).Sum());