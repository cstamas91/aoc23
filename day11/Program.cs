using System.Data;

var input = File.ReadAllText("input").Split(Environment.NewLine);

var starMap = new StarMap(input);

Console.WriteLine(starMap.Paths.Values.Sum());
class StarMap
{
    #if DEBUG
    const char EXPANSION_CHAR='*';
    #else
    const char EXPANSION_CHAR='.';
    #endif
    const char GALAXY_CHAR='#';
    
    private readonly char[,] nodes;
    private readonly int height;
    private readonly int width;

    private readonly Dictionary<int, (int Row,int Col)> Galaxies = new();
    public readonly Dictionary<(int From, int To), int> Paths = new();
    public StarMap(string[] lines)
    {
        var (initialNodes, initialHeight, initialWidth) = CreateInitialNodes(lines);
        (nodes,height,width) = Expand(initialNodes,initialHeight, initialWidth);

        BuildPathLengthMap();
    }

    private void BuildPathLengthMap()
    {
        foreach (var g in Galaxies.Keys)
        {
            foreach (var gg in Galaxies.Keys)
            {
                if (g == gg)
                {
                    continue;
                }

                if (!Paths.Any(kvp => (kvp.Key.From == g && kvp.Key.To == gg) ||
                                      (kvp.Key.From == gg && kvp.Key.To == g) ))
                {
                    Paths.Add((g, gg), FindShortestPathLengthTo(g, gg));
                }
            }
        }
    }

    private static bool CloserThan((int r, int c) p1, (int r, int c) p2, (int r, int c) to)
    {
        if ((Math.Abs(to.r - p1.r) < Math.Abs(to.r - p2.r) && (Math.Abs(to.c - p1.c) <= Math.Abs(to.c - p2.c))) || 
            (Math.Abs(to.c - p1.c) < Math.Abs(to.c - p2.c) && (Math.Abs(to.r - p1.r) <= Math.Abs(to.r - p2.r))))
        {
            return true;
        }

        return false;
    }

    private int FindShortestPathLengthTo(int from, int to)
    {
        var (FromRow, FromCol) = Galaxies[from];

        var (ToRow, ToCol) = Galaxies[to];

        var nextSteps = GetAdjacentIndexPairs(FromRow, FromCol)
            .Where(p => CloserThan(p, (FromRow, FromCol), (ToRow, ToCol)))
            .ToList();

        int shortestPathLength = int.MaxValue;

        foreach (var start in nextSteps)
        {
            var allPaths = FindAllPathsTo(start, (ToRow, ToCol));
            var localShortestPath = allPaths.Where(p => p.Length > 0).MinBy(p => p.Length)?.Length ?? int.MaxValue;
            if (localShortestPath < shortestPathLength)
            {
                shortestPathLength = localShortestPath;
            }
        }

        return shortestPathLength + 1;
    }

    private List<Path> FindAllPathsTo((int r, int c) from, (int r, int c) to)
    {
        var nextSteps = GetAdjacentIndexPairs(from.r, from.c)
            .Where(p => CloserThan(p, from, to))
            .ToList();

        if (nextSteps.Contains(to))
        {
            return new List<Path> {new(from)};
        }

        if (nextSteps.Count == 0)
        {
            return new List<Path>();
        }

        var paths = nextSteps.Select(s => FindAllPathsTo(s, to)).SelectMany(p => p).Where(p => p.Length > 0).ToList();
        foreach (var path in paths)
        {
            path.Prepend(from);
        }

        return paths;
    }

    private List<(int row, int col)> GetAdjacentIndexPairs(int row, int col)
    {
        var values = new List<(int row, int col)>
        {
            (row+1, col),
            (row, col+1),
            (row, col-1)
        };
        return values.Where(p => p.row >= 0 && p.row <= height && p.col >= 0 && p.col <= width).ToList();
    }

    public void Print()
    {
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                Console.Write(nodes[r,c]);
            }
            Console.WriteLine();
        }

        foreach (var g in Galaxies.Keys)
        {
            Console.WriteLine($"{g}: ({Galaxies[g].Row},{Galaxies[g].Col})");
        }
    }

    private (char[,], int, int) Expand(
        char[,] mapBeforeExpansion,
        int initialHeight,
        int initialWidth)
    {
        var rowsToExpand = new HashSet<int>();
        var colsToExpand = new HashSet<int>();
        for (int row = 0; row < initialHeight; row++)
        {
            var isEmptyRow = true;
            for (int col = 0; col < initialWidth; col++)
            {
                if (mapBeforeExpansion[row,col] != '.')
                {
                    isEmptyRow = false;
                }
            }

            if (isEmptyRow)
            {
                rowsToExpand.Add(row + rowsToExpand.Count);
            }
        }

        for (int col = 0; col < initialWidth; col++)
        {
            var isEmptyCol = true;
            for (int row = 0; row < initialHeight; row++)
            {
                if (mapBeforeExpansion[row,col] != '.')
                {
                    isEmptyCol = false;
                }
            }

            if (isEmptyCol)
            {
                colsToExpand.Add(col + colsToExpand.Count);
            }
        }

        var newHeight = initialHeight+rowsToExpand.Count;
        var newWidth = initialWidth+colsToExpand.Count;
        var newMap = new char[newHeight,newWidth];

        for (int row=0,oldRow=0; row < newHeight; row++,oldRow++)
        {
            for (int col=0,oldCol=0; col < newWidth; col++,oldCol++)
            {
                if (mapBeforeExpansion[oldRow,oldCol] == GALAXY_CHAR)
                {
                    Galaxies.Add(Galaxies.Count + 1, (row,col));
                }

                newMap[row,col] = mapBeforeExpansion[oldRow,oldCol];

                if (colsToExpand.Contains(col))
                {
                    newMap[row,col+1]=EXPANSION_CHAR;
                    col++;
                }
            }

            if (rowsToExpand.Contains(row))
            {
                for (int j = 0; j < newWidth; j++)
                {
                    newMap[row+1,j] = EXPANSION_CHAR;
                }

                row++;
            }
        }

        return (newMap, newHeight, newWidth);
    }

    private static (char[,], int, int) CreateInitialNodes(string[] lines)
    {
        var height = lines.Length;
        var width = lines[0].Length;

        var res = new char[height+1,width+1];

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                res[row,col]=lines[row][col];
            }
        }

        return (res, height, width);
    }
}

class Path
{
    public int Length => points.Count;
    private readonly List<(int r, int c)> points = new();
    public Path((int r, int c) start)
    {
        points.Add(start);
    }

    public void Prepend((int r, int c) point)
    {
        points.Insert(0, point);
    }
}