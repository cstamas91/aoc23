var inputText = File.ReadAllText("input");

var parser = new PartNumberParser(inputText);
parser.Parse();

Console.WriteLine($"{parser.PartNumberSum} {parser.GearRatioSum}");


class PartNumberParser
{
    private readonly char[] numerics = new char[]
    {
        '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
    };
    private readonly string inputText;
    private readonly List<int> partNumbers = new();
    private readonly List<Gear> gears = new();

    public int PartNumberSum => partNumbers.Sum();
    public int GearRatioSum => gears.Sum(g => g.GearRatio);

    public PartNumberParser(string inputText)
    {
        this.inputText = inputText;
    }

    public void Parse()
    {
        var lines = inputText.Split(Environment.NewLine);

        var lineIndex = 0;

        while (lineIndex < lines.Length)
        {
            string currentLine = lines[lineIndex];
            string? previousLine = lineIndex > 0 ? lines[lineIndex-1] : null;
            string? nextLine = lineIndex < lines.Length-1 ? lines[lineIndex+1] : null;

            ParseLine(currentLine, previousLine, nextLine, lineIndex);

            lineIndex++;
        }
    }

    private void ParseLine(string currentLine, string? previousLine, string? nextLine, int lineIndex)
    {
        var charIndex = 0;

        while (charIndex < currentLine.Length)
        {
            var currentChar = currentLine[charIndex];

            if (numerics.Contains(currentChar))
            {
                charIndex = ParseNumber(charIndex, currentLine, previousLine, nextLine, lineIndex);
            }
            else 
            {
                charIndex++;
            }
        }
    }

    private int ParseNumber(int startCharIndex, string currentLine, string? previousLine, string? nextLine, int lineIndex)
    {
        int endCharIndex = startCharIndex;

        string currentNumber = string.Empty;

        while (endCharIndex < currentLine.Length && 
               numerics.Contains(currentLine[endCharIndex]))
        {
            currentNumber += currentLine[endCharIndex];
            endCharIndex++;
        }

        var number = int.Parse(currentNumber);

        if (IsEnginePartNumber(startCharIndex, endCharIndex, currentLine, previousLine, nextLine, lineIndex, number))
        {
            partNumbers.Add(number);
        }

        return endCharIndex;
    }

    private bool IsEnginePartNumber(
        int startCharIndex, 
        int endCharIndex, 
        string currentLine, 
        string? previousLine, 
        string? nextLine,
        int lineIndex,
        int number)
    {
        var checkRangeStart = Math.Max(0, startCharIndex - 1);
        var checkRangeEnd = Math.Min(currentLine.Length-1, endCharIndex);

        var isOk = CheckLineForSpecialSymbol(checkRangeStart, checkRangeEnd, currentLine, lineIndex, number);

        if (!isOk && previousLine is not null)
        {
            isOk = CheckLineForSpecialSymbol(checkRangeStart, checkRangeEnd, previousLine, lineIndex - 1, number);
        }

        if (!isOk && nextLine is not null)
        {
            isOk = CheckLineForSpecialSymbol(checkRangeStart, checkRangeEnd, nextLine, lineIndex + 1, number);
        }

        return isOk;
    }

    private bool CheckLineForSpecialSymbol(int checkRangeStart, int checkRangeEnd, string line, int lineIndex, int number)
    {
        for (int i = checkRangeStart; i <= checkRangeEnd; i++)
        {
            var currentChar = line[i];

            if (!numerics.Contains(currentChar) && currentChar != '.')
            {
                if (currentChar == '*')
                {
                    var foundGear = gears.FirstOrDefault(g => g.RowIndex == lineIndex && g.ColIndex == i);

                    if (foundGear is not null)
                    {
                        if (foundGear.SecondPartNumber is not null)
                        {
                            throw new InvalidOperationException($"Gear belongs to more than two parts? {lineIndex},{i}");
                        }

                        foundGear.SecondPartNumber = number;
                    }
                    else 
                    {
                        gears.Add(new Gear
                        {
                            ColIndex = i,
                            RowIndex = lineIndex,
                            FirstPartNumber = number
                        });
                    }
                }

                return true;
            }
        }

        return false;
    }

    private class Gear
    {
        public int ColIndex {get;set;}
        public int RowIndex {get;set;}

        public int? FirstPartNumber {get;set;}
        public int? SecondPartNumber {get;set;}

        public int GearRatio => (FirstPartNumber ?? 0) * (SecondPartNumber ?? 0);
    }
}