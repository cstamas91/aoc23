var input = File.ReadAllText("example");



class StarMap
{
    private char[,] nodes;
    private int height;
    private int width;
    public StarMap(string[] lines)
    {
        var (initialNodes, initialHeight, initialWidth) = CreateInitialNodes(lines);
        (nodes,height,width) = Expand(initialNodes,initialHeight, initialWidth);
    }

    private static (char[,], int, int) Expand(
        char[,] mapBeforeExpansion,
        int initialHeight,
        int initialWidth)
    {
        var rowsToExpand = new List<int>();
        var colsToExpand = new List<int>();
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
                rowsToExpand.Add(row);
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
                colsToExpand.Add(col);
            }
        }

        var newHeight = initialHeight+rowsToExpand.Count;
        var newWidth = initialWidth+colsToExpand.Count;
        var newMap = new char[newHeight,newWidth];

        for (int row=0; row < newHeight; row++)
        {
            for (int col=0; col < newWidth; col++)
            {
                
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