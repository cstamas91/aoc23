using System.Collections.Concurrent;

var input = File.ReadAllText(args[0]).Split(Environment.NewLine);

var directions = input[0];

var nodes = new Dictionary<string, Node>();

for (var i = 1; i < input.Length; i++)
{
    if (string.IsNullOrEmpty(input[i]))
    {
        continue;
    }

    var node = new Node(input[i]);

    nodes.Add(node.Tag, node);
}

var directionIndex = 0;
var currentNodes = GetStartingNodes(nodes);

for (int i = 0; i < currentNodes.Count; i++)
{
    var stepsTaken = 0;
    var found = false;
    var currentNode = currentNodes[i];

    while (!found)
    {
        currentNode = Step(currentNode, directions[directionIndex], nodes);
        stepsTaken++;
        if (currentNode.Tag.EndsWith('Z'))
        {
            found = true;
            currentNodes[i].MinSteps = stepsTaken;
        }

        directionIndex++;
        if (directionIndex == directions.Length)
        {
            directionIndex = 0;
        }
    }
}

foreach (var node in currentNodes)
{
    Console.WriteLine(node.MinSteps);
}

static Node Step(Node currentNode, char v, Dictionary<string, Node> nodes)
{
    if (v == 'L')
    {
        return nodes[currentNode.Left];
    }

    return nodes[currentNode.Right];
}

static List<Node> GetStartingNodes(Dictionary<string, Node> nodes)
{
    return nodes.Where(n => n.Key.EndsWith('A')).Select(n => n.Value).ToList();
}

class Node 
{
    public string Tag {get;init;}
    public string Left {get;init;}
    public string Right {get;init;}
    public int MinSteps {get;set;} = int.MaxValue;
    public Node(string input)
    {
        var parts = input.Split(" = ");
        Tag = parts[0];
        var nextNodes = parts[1].Replace("(", string.Empty).Replace(")",string.Empty).Split(", ");
        Left = nextNodes[0];
        Right = nextNodes[1];
    }
}