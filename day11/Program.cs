using System.Data;
using System.Collections.Concurrent;

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
    const long EXPANSION_RATE = 999_999;
    
    private readonly char[,] nodes;
    private readonly long height;
    private readonly long width;

    private List<int> expandableRows = new();
    private List<int> expandableColumns = new();

    private readonly Dictionary<long, (long Row,long Col)> Galaxies = new();
    public readonly ConcurrentDictionary<(long From, long To), long> Paths = new();
    public StarMap(string[] lines)
    {
        var (initialNodes, initialHeight, initialWidth) = CreateInitialNodes(lines);
        (nodes,height,width) = Expand(initialNodes,initialHeight, initialWidth);

        BuildPathLengthMap();
    }

    private void BuildPathLengthMap()
    {
        Parallel.ForEach(Galaxies.Keys, g => 
        {
            foreach (var gg in Galaxies.Keys.Where(k => k > g))
            {
                Paths.AddOrUpdate(
                    (g, gg), 
                    x => ShortestPathFast(x.From, x.To),
                    (x, _) => ShortestPathFast(x.From, x.To));
            }
        });
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

    private long ShortestPathFast(long from, long to)
    {
        var (fromR, fromC) = Galaxies[from];
        var (toR, toC) = Galaxies[to];

        fromR += expandableRows.Count(r => r < fromR) * EXPANSION_RATE;
        fromC += expandableColumns.Count(c => c < fromC) * EXPANSION_RATE;
        toR += expandableRows.Count(r => r < toR) * EXPANSION_RATE;
        toC += expandableColumns.Count(c => c < toC) * EXPANSION_RATE;

        return Math.Abs(fromR - toR) + Math.Abs(fromC - toC);
    }

    // private int FindShortestPathLengthTo(int from, int to)
    // {
    //     var (FromRow, FromCol) = Galaxies[from];

    //     var (ToRow, ToCol) = Galaxies[to];

    //     var nextSteps = GetAdjacentIndexPairs(FromRow, FromCol)
    //         .Where(p => CloserThan(p, (FromRow, FromCol), (ToRow, ToCol)))
    //         .ToList();

    //     ConcurrentBag<int> bag = new();

    //     Parallel.ForEach(nextSteps, (start) => 
    //     {
    //         var localShortestPath = FindAllPathsTo(start, (ToRow, ToCol)).Where(p => p.Length > 0).MinBy(p => p.Length)?.Length ?? int.MaxValue;
    //         bag.Add(localShortestPath);
    //     });

    //     return bag.Min() + 1;
    // }

    // private List<Path> FindAllPathsTo((int r, int c) from, (int r, int c) to)
    // {
    //     var nextSteps = GetAdjacentIndexPairs(from.r, from.c)
    //         .Where(p => CloserThan(p, from, to))
    //         .ToList();

    //     if (nextSteps.Contains(to))
    //     {
    //         return new List<Path> {new(from)};
    //     }

    //     if (nextSteps.Count == 0)
    //     {
    //         return new List<Path>();
    //     }

    //     var paths = nextSteps.Select(s => FindAllPathsTo(s, to)).SelectMany(p => p).Where(p => p.Length > 0).ToList();
    //     foreach (var path in paths)
    //     {
    //         path.Prepend(from);
    //     }

    //     return paths;
    // }

    // private List<(int row, int col)> GetAdjacentIndexPairs(int row, int col)
    // {
    //     var values = new List<(int row, int col)>
    //     {
    //         (row+1, col),
    //         (row, col+1),
    //         (row, col-1)
    //     };
    //     return values.Where(p => p.row >= 0 && p.row <= height && p.col >= 0 && p.col <= width).ToList();
    // }

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

    private (char[,], long, long) Expand(
        char[,] mapBeforeExpansion,
        long initialHeight,
        long initialWidth)
    {
        var rowsToExpand = new HashSet<long>();
        var colsToExpand = new HashSet<long>();
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
                expandableRows.Add(row);
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
                expandableColumns.Add(col);
            }
        }

        for (int i = 0; i < initialHeight; i++)
        {
            for (int j = 0; j < initialWidth; j++)
            {
                if (mapBeforeExpansion[i,j] == GALAXY_CHAR)
                {
                    Galaxies.Add(Galaxies.Count + 1, (i,j));
                }
            }
        }

        return (new char[0,0], 0,0);

        // var newHeight = initialHeight+(rowsToExpand.Count * EXPANSION_RATE);
        // var newWidth = initialWidth+(colsToExpand.Count * EXPANSION_RATE);
        // var newMap = new char[newHeight,newWidth];

        // for (long row=0,oldRow=0; row < newHeight; row++,oldRow++)
        // {
        //     for (long col=0,oldCol=0; col < newWidth; col++,oldCol++)
        //     {
        //         if (mapBeforeExpansion[oldRow,oldCol] == GALAXY_CHAR)
        //         {
        //             Galaxies.Add(Galaxies.Count + 1, (row,col));
        //         }

        //         newMap[row,col] = mapBeforeExpansion[oldRow,oldCol];

        //         if (colsToExpand.Contains(oldCol))
        //         {
        //             for (int i = 0; i < EXPANSION_RATE; i++)
        //             {
        //                 newMap[row, col + i]=EXPANSION_CHAR;
        //             }
        //             col+=EXPANSION_RATE;
        //         }
        //     }

        //     if (rowsToExpand.Contains(oldRow))
        //     {
        //         for (int i = 0; i < EXPANSION_RATE; i++)
        //         {
        //             for (int j = 0; j < newWidth; j++)
        //             {
        //                 newMap[row+i,j] = EXPANSION_CHAR;
        //             }
        //         }

        //         row+=EXPANSION_RATE;
        //     }
        // }

        // return (newMap, newHeight, newWidth);
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