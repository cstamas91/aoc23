using System.Globalization;

var input = File.ReadAllText("input").Split(Environment.NewLine);

var map = new Map(input);

var startPos = GetStartPosition(map);

var (path1, path2) = FindPathsFromStart(startPos, map);

path1.Build(map, startPos);

Console.WriteLine(path1.Length / 2);
Console.WriteLine($"{map.AntiFlood(path1)}");

(Path p1, Path p2) FindPathsFromStart(Position startPos, Map map)
{
    var neighboringPositions = map.GetAdjacentPositionsFor(startPos);

    var connectingPositions = neighboringPositions.Where(p => p.ConnectsTo(startPos, map)).ToList();

    if (connectingPositions.Count != 2)
    {
        throw new InvalidOperationException("could not find two connecting positions from start");
    }

    var path1 = new Path(connectingPositions[0]);
    var path2 = new Path(connectingPositions[1]);

    Console.WriteLine($"loop paths are: {connectingPositions[0]}, {connectingPositions[1]}");

    return (path1, path2);
}

static Position GetStartPosition(Map map)
{
    for (int rowIndex = 0; rowIndex <= map.Height; rowIndex++)
    {
        for (int colIndex = 0; colIndex <= map.Width; colIndex++)
        {
            if (map[rowIndex,colIndex] == 'S')
            {
                return new Position(rowIndex, colIndex, 'S');
            }
        }
    }

    throw new ArgumentException("No start position found in map");
}


class Map(string[] input)
{
    private string[] Content { get; init; } = input;
    public int Width { get; init; } = input[0].Length - 1;
    public int Height { get; init; } = input.Length - 1;

    public char this[int row, int col]
    {
        get => Content[row][col];
    }

    private static List<(int x, int y)> GetAdjacentIndices(Position p)
    {
        return [ (p.X -1, p.Y), (p.X + 1, p.Y), (p.X, p.Y-1), (p.X, p.Y+1) ];
    }

    private List<(int x, int y)> GetAdjacentIndices(int x, int y)
    {
        var res = new List<(int x, int y)>();

        if (x>0)
        {
            res.Add((x-1, y));
        }

        if (x < Height)
        {
            res.Add((x+1, y));
        }

        if (y > 0)
        {
            res.Add((x, y-1));
        }

        if (y < Width)
        {
            res.Add((x, y+1));
        }

        return res;
    }

    public List<Position> GetAdjacentPositionsFor(Position p, bool onlyFields = false)
    {
        var result = new List<Position>();

        foreach (var (x, y) in GetAdjacentIndices(p))
        {
            var adjacentPos = GetAdjacentPositionFor(x, y, onlyFields);
            if (adjacentPos is not null)
            {
                result.Add(adjacentPos.Value);
            }
        }

        return result;
    }

    public List<Position> GetAdjacentPositionsForFlooding(Position p, IReadOnlyList<Position> borders)
    {
        var result = new List<Position>();

        foreach (var (x, y) in GetAdjacentIndices(p))
        {
            var adjacentPos = GetAdjacentPositionFor(x, y, true);
            if (adjacentPos is not null && !borders.Any(p => adjacentPos.Value.X == p.X && adjacentPos.Value.Y == p.Y))
            {
                result.Add(adjacentPos.Value);
            }
        }

        return result;
    }

    private Position? GetAdjacentPositionFor(int x, int y, bool includeFields = false)
    {
        if (x >= 0 && x <= Height && y >= 0 && y <= Width)
        {
            if (includeFields)
            {
                return new Position(x, y, Content[x][y]);
            }

            if (!includeFields && Content[x][y] != '.')
            {
                return new Position(x, y, Content[x][y]);
            }
        }

        return null;
    }

    public Position? GetLeftFieldFor(Position p, Direction heading)
    {
        var adjacentIndices = GetAdjacentIndices(p);

        if (heading == Direction.HeadingDown)
        {
            var (x, y) = adjacentIndices.FirstOrDefault(i => p.X == i.x && p.Y < i.y);
            return GetAdjacentPositionFor(x, y, true);
        }

        if (heading == Direction.HeadingUp)
        {
            var (x, y) = adjacentIndices.FirstOrDefault(i => p.X == i.x && p.Y > i.y);
            return GetAdjacentPositionFor(x, y, true);
        }

        if (heading == Direction.HeadingLeft)
        {
            var (x ,y) = adjacentIndices.FirstOrDefault(i => p.Y == i.y && p.X < i.x);
            return GetAdjacentPositionFor(x, y, true);
        }

        if (heading == Direction.HeadingRight)
        {
            var (x, y) = adjacentIndices.FirstOrDefault(i => p.Y == i.y && p.X > i.x);
            return GetAdjacentPositionFor(x, y, true);
        }

        return null;
    }

    public Position? GetRightFieldFor(Position p, Direction heading)
    {
        var adjacentIndices = GetAdjacentIndices(p);

        if (heading == Direction.HeadingDown)
        {
            var (x, y) = adjacentIndices.FirstOrDefault(i => p.X == i.x && p.Y > i.y);
            return GetAdjacentPositionFor(x, y, true);
        }

        if (heading == Direction.HeadingUp)
        {
            var (x, y) = adjacentIndices.FirstOrDefault(i => p.X == i.x && p.Y < i.y);
            return GetAdjacentPositionFor(x, y, true);
        }

        if (heading == Direction.HeadingLeft)
        {
            var (x ,y) = adjacentIndices.FirstOrDefault(i => p.Y == i.y && p.X > i.x);
            return GetAdjacentPositionFor(x, y, true);
        }

        if (heading == Direction.HeadingRight)
        {
            var (x, y) = adjacentIndices.FirstOrDefault(i => p.Y == i.y && p.X < i.x);
            return GetAdjacentPositionFor(x, y, true);
        }

        return null;
    }

    private class FloodWorkItem
    {
        public Position Position {get;set;}
        public bool Seen {get;set;}
    }

    public int FloodCount(List<Position> starters, Path loop)
    {
        var distinctAreas = new List<List<Position>>();
        var workSet = starters.Select(p => new FloodWorkItem{Position = p, Seen = false}).ToList();

        while (!workSet.All(fwi => fwi.Seen))
        {
            var currentAreaStart = workSet.First(fwi => !fwi.Seen);
            currentAreaStart.Seen = true;
            var currentArea = FloodFrom(currentAreaStart.Position, loop.PathElements);

            foreach (var fwi in workSet)
            {
                if (currentArea.Contains(fwi.Position))
                {
                    fwi.Seen = true;
                }
            }

            distinctAreas.Add(currentArea);
        }

        return distinctAreas.Sum(a => a.Count);
    }

    private List<Position> FloodFrom(Position start, IReadOnlyList<Position> borders)
    {
        var result = new List<Position>();
        var stack = new Stack<Position>();
        stack.Push(start);
        result.Add(start);

        while (stack.Count > 0)
        {
            var currentPos = stack.Pop();
            var adjacentFields = GetAdjacentPositionsForFlooding(currentPos, borders);

            foreach (var field in adjacentFields)
            {
                if (!result.Contains(field))
                {
                    result.Add(field);
                    stack.Push(field);
                }
            }
        }

        return result;
    }

    private void PrintHelper(char[,] map)
    {
        for (int i = 0; i <= Height; i++)
        {
            for (int j = 0; j <= Width; j++)
            {
                Console.Write(map[i,j]);
            }
            Console.WriteLine();
        }
    }

    public int AntiFlood(Path path)
    {
        var map = new char[Height+1, Width+1];
        for (int i = 0; i <= Height; i++)
        {
            for (int j = 0; j <= Width; j++)
            {
                if (!path.PathElements.Any(e => e.X == i && e.Y == j))
                {
                    map[i,j] = Content[i][j];
                }
                else
                {
                    map[i,j] = ' ';
                }
            }
        }

        Stack<(int x, int y)> stack = new();
        stack.Push((0,0));

        while (stack.Count != 0)
        {
            var (x, y) = stack.Pop();
            map[x,y] = 'X';
            foreach (var ind in GetAdjacentIndices(x,y))
            {
                if (map[ind.x,ind.y] != 'X' && map[ind.x,ind.y] != ' ')
                {
                    stack.Push(ind);
                }
            }
        }

        int sum = 0;
        for (int i = 0; i <= Height; i++)
        {
            for (int j = 0; j <= Width; j++)
            {
                Console.Write(map[i,j]);
                if (map[i,j] != 'X' && map[i,j] != ' ')
                {
                    sum++;
                }
            }
            Console.WriteLine();
        }

        return sum;
    }

    public int Print(Path path)
    {
        var map = new char[Height+1,Width+1];
        for (int i = 0; i <= Height; i++)
        {
            for (int j = 0; j <= Width; j++)
            {
                if (!path.PathElements.Any(e => e.X == i && e.Y == j))
                {
                    map[i,j] = Content[i][j];
                }
                else
                {
                    map[i,j] = ' ';
                }
            }
        }

        PrintHelper(map);

        for (int i = 0; i <= Height; i++)
        {
            var j = 0;
            while (map[i,j] != ' ' && j < Width)
            {
                map[i,j] = ' ';
                j++;
            }

            j = Width;
            while (map[i,j] != ' ' && j > 0)
            {
                map[i,j] = ' ';
                j--;
            }
        }

        Console.WriteLine();

        PrintHelper(map);

        return 0;
    }
}

readonly struct Position(int x, int y)
{

    public Position(int x, int y, char val) : this(x, y)
    {
        Value = val;
    }
    public int X {get;init;} = x;
    public int Y {get;init;} = y;
    public char? Value {get;init;}
    public readonly (Position p1, Position p2) ConnectingPositions(Map map)
    {
        return Value switch 
        {
            '|' => (new Position(X-1, Y, map[X-1, Y]), new Position(X+1, Y, map[X+1, Y])),
            '-' => (new Position(X, Y-1, map[X, Y-1]), new Position(X, Y+1, map[X, Y+1])),
            'L' => (new Position(X-1, Y, map[X-1, Y]), new Position(X, Y+1, map[X, Y+1])),
            'J' => (new Position(X-1, Y, map[X-1, Y]), new Position(X, Y-1, map[X, Y-1])),
            '7' => (new Position(X+1, Y, map[X+1, Y]), new Position(X, Y-1, map[X, Y-1])),
            'F' => (new Position(X+1, Y, map[X+1, Y]), new Position(X, Y+1, map[X, Y+1])),
            _ => throw new InvalidOperationException("no known connecting positions")
        };
    }

    public bool ConnectsTo(Position p, Map map)
    {
        var (p1, p2) = ConnectingPositions(map);
        return (p1.X == p.X && p1.Y == p.Y) ||
            (p2.X == p.X && p2.Y == p.Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})[{Value}]";
    }

    public TurnType GetStepType(Position next)
    {
        if (Value == 'L')
        {
            if (next.X < X && next.Y == Y) return TurnType.Right;
            if (next.Y > Y && next.X == X) return TurnType.Left;
        }

        if (Value == 'J')
        {
            if (next.Y < Y && next.X == X) return TurnType.Right;
            if (next.X < X && next.Y == Y) return TurnType.Left;
        }

        if (Value == 'F')
        {
            if (next.X > X && next.Y == Y) return TurnType.Left;
            if (next.Y > Y && next.X == X) return TurnType.Right;
        }

        if (Value == '7')
        {
            if (next.Y < Y && next.X == X) return TurnType.Left;
            if (next.X > X && next.Y == Y) return TurnType.Right;
        }

        return TurnType.Straight;
    }

    public Direction GetDirection(Position next)
    {
        if (next.X == X)
        {
            if (next.Y < Y)
                return Direction.HeadingLeft;

            if (next.Y > Y)
                return Direction.HeadingRight;
        }

        if (next.Y == Y)
        {
            if (next.X < X)
            {
                return Direction.HeadingUp;
            }

            if (next.X > X)
            {
                return Direction.HeadingDown;
            }
        }

        throw new InvalidOperationException();
    }
}

enum TurnType
{
    Straight,
    Left,
    Right
}

enum Direction
{
    HeadingDown,
    HeadingUp,
    HeadingRight,
    HeadingLeft,
}

class Path(Position s)
{
    public int Length {get; private set;} = 1;
    private Position CurrentPosition = s;

    public int LeftTurns {get;private set;} = 0;
    public int RightTurns {get;private set;} = 0;

    private List<Position> LeftFields = new List<Position>();
    private List<Position> RightFields = new List<Position>();

    private List<Position> pathElements = new();
    public IReadOnlyList<Position> PathElements => pathElements;

    public int EnclosedFields {get;private set;}

    public void Build(Map map, Position target)
    {
        var previousPosition = target;
        pathElements.Add(target);
        while (CurrentPosition.X != target.X || CurrentPosition.Y != target.Y)
        {
            var (p1, p2) = CurrentPosition.ConnectingPositions(map);

            var nextPos = p1.X == previousPosition.X && p1.Y == previousPosition.Y ?
                p2 : p1;


            pathElements.Add(CurrentPosition);

            previousPosition = CurrentPosition;
            CurrentPosition = nextPos;
            Length++;
        }

        Walk(map);
    }

    private void Walk(Map map)
    {
        var previousPosition = pathElements[0];
        CurrentPosition = pathElements[1];
        while (CurrentPosition.X != pathElements[0].X || CurrentPosition.Y != pathElements[0].Y)
        {
            var (p1, p2) = CurrentPosition.ConnectingPositions(map);

            var nextPos = p1.X == previousPosition.X && p1.Y == previousPosition.Y ?
                p2 : p1;

            ProcessTurn(nextPos, map);

            previousPosition = CurrentPosition;
            CurrentPosition = nextPos;
        }

        EnclosedFields = map.FloodCount(LeftTurns > RightTurns ? LeftFields : RightFields, this);
    }

    private void ProcessTurn(Position nextPos, Map map)
    {
        var stepType = CurrentPosition.GetStepType(nextPos);

        if (stepType == TurnType.Left)
        {
            LeftTurns++;
        }
        else if (stepType == TurnType.Right)
        {
            RightTurns++;
        }

        var direction = CurrentPosition.GetDirection(nextPos);

        var leftField = map.GetLeftFieldFor(CurrentPosition, direction);
        if (leftField is not null && 
            !LeftFields.Any(p => p.X == leftField.Value.X && p.Y == leftField.Value.Y) &&
            !pathElements.Any(p => p.X == leftField.Value.X && p.Y == leftField.Value.Y))

            LeftFields.Add(leftField.Value);

        var rightField = map.GetRightFieldFor(CurrentPosition, direction);
        if (rightField is not null && 
            !RightFields.Any(p => p.X == rightField.Value.X && p.Y == rightField.Value.Y) &&
            !pathElements.Any(p => p.X == rightField.Value.X && p.Y == rightField.Value.Y))
            RightFields.Add(rightField.Value);
    }
}