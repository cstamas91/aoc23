using System.Collections.Concurrent;

var input = File.ReadAllText(args[0]);

Console.WriteLine("Processing almanac");

var almanac = new Almanac(input);

Console.WriteLine("Almanac processed");

Console.WriteLine(almanac.GetLowestLocation(long.Parse(args[1])));

class Almanac
{
    private List<(long begin, long length)> seedsRanges = new ();
    private List<MapDefinition> mapDefinitions = new();
    public Almanac(string input)
    {
        var segments = input.Split($"{Environment.NewLine}{Environment.NewLine}");
        var seedRangeInput = segments[0]
            .Replace("seeds: ", "")
            .Split(' ');

        for (int i = 0; i < seedRangeInput.Length; i+=2)
        {
            seedsRanges.Add((long.Parse(seedRangeInput[i]), long.Parse(seedRangeInput[i+1])));
        }

        for(int i = 1; i < segments.Length; i++)
        {
            var mapDefinition = new MapDefinition(segments[i]);
            mapDefinitions.Add(mapDefinition);
            var previousMap = mapDefinitions.FirstOrDefault(md => md.To == mapDefinition.From);

            if (previousMap is not null)
            {
                previousMap.Next = mapDefinition;
            }
        }
    }

    private long MinSearch(long begin, long end)
    {
        var min = long.MaxValue;

        for (long i = begin; i < end; i++)
        {
            var val = mapDefinitions[0].Map(i);
            if (val < min)
            {
                min = val;
            }

            if (min == 0)
            {
                break;
            }
        }

        return min;
    }

    private static IEnumerable<(long begin, long end)> GetSlices(long begin, long end, long windowSize)
    {
        var currentBegin = begin;
        var currentEnd = Math.Min(begin + windowSize, end);

        yield return (currentBegin, currentEnd);

        while (currentEnd < end)
        {
            currentBegin += windowSize;
            currentEnd = Math.Min(currentEnd + windowSize, end);
            yield return (currentBegin, currentEnd);
        }
    }

    public long GetLowestLocation(long windowSize)
    {
        var subResults = new ConcurrentBag<long>();

        Parallel.ForEach(seedsRanges, sr => 
        {
            var slices = GetSlices(sr.begin, sr.begin + sr.length, windowSize).ToList();
            Console.WriteLine($"starting range ({sr.begin},{sr.begin+sr.length}) - {slices.Count} slices...");
            
            var bag = new ConcurrentBag<long>();

            Parallel.ForEach(slices, val => 
            {
                bag.Add(MinSearch(val.begin, val.end));
            });

            subResults.Add(bag.Min());
            Console.WriteLine($"range ({sr.begin},{sr.begin+sr.length}) done");
        });

        return subResults.Min();
    }

    class MapDefinition
    {
        public string Name {get;init;}
        public string From {get;init;}
        public string To {get;init;}

        public MapDefinition? Next {get; set;}

        public List<Conversion> Conversions {get;init;}

        public MapDefinition(string input)
        {
            var segmentLines = input.Split($":{Environment.NewLine}");

            Name = segmentLines[0].Replace(" map", "");
            From = Name.Split("-to-")[0];
            To = Name.Split("-to-")[1];
            Conversions = segmentLines[1].Split(Environment.NewLine).Select(l => new Conversion(l)).ToList();
        }

        public long Map(long source)
        {
            var conversionResult = source;

            var conversions = Conversions
                .Where(c => c.DoesConvert(source))
                .ToList();

            if (conversions.Count > 1)
            {
                throw new InvalidOperationException("Overlapping conversion ranges");
            }

            if (conversions.Count == 1)
            {
                conversionResult = conversions[0].Map(source);
            }

            if (Next is not null)
            {
                return Next.Map(conversionResult);
            }

            return conversionResult;
        }
    }

    class Conversion
    {
        private long RangeLength {get;init;}
        private long SourceRangeStart {get;init;}
        private long SourceRangeEnd {get;init;} 
        private long DestinationRangeStart {get;init;}
        private long MapOperation {get;init;}
        public Conversion(string input)
        {
            var segments = input.Split(' ')
                .Select(long.Parse)
                .ToList();
            RangeLength = segments[2];
            DestinationRangeStart = segments[0];
            SourceRangeStart = segments[1];
            SourceRangeEnd = SourceRangeStart + RangeLength;
            MapOperation = DestinationRangeStart - SourceRangeStart;
        }

        public long Map(long source)
        {
            if (DoesConvert(source))
            {
                return source + MapOperation;
            }

            return source;
        }

        public bool DoesConvert(long source)
        {
            return SourceRangeStart <= source && source < SourceRangeEnd;
        }
    }
}