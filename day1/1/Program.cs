var inputLines = File.ReadAllText("input.txt").Split(Environment.NewLine);

static int GetCalibrationValueFromLine(ReadOnlySpan<char> line) 
{
    var leftIndex = 0;
    var rightIndex = line.Length - 1;
    int? leftVal = null;
    int? rightVal = null;

    while (leftVal is null && leftIndex < line.Length)
    {
        if (int.TryParse(line[leftIndex].ToString(), out int number))
        {
            leftVal = number;
        }

        leftIndex++;
    }

    while (rightVal is null && rightIndex >= 0)
    {
        if (int.TryParse(line[rightIndex].ToString(), out int number))
        {
            rightVal = number;
        }

        rightIndex--;
    }    

    return ((leftVal ?? 0) * 10) + (rightVal ?? 0);
}

Console.WriteLine(inputLines.Select(l => GetCalibrationValueFromLine(l)).Sum());

