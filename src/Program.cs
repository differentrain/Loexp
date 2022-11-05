using Loexp.Internals;

Utils.RegisterOperators();

if (args.Length == 0)
    Console.WriteLine(Utils.GetUsages());
else
{
    Utils.RegisterOperators();
    using ExpReader ar = new(args);
    Console.WriteLine(ExpParseHelper.Parse(ar));
}
