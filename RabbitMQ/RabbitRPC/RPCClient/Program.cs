namespace RPCClient;

public class Rpc
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("RPC Client");
        Console.WriteLine("----------------");

        string n = args.Length > 0 ? args[0] : "30";
        await InvokeAsync(n);
        
        Console.WriteLine(" Press any key to exit.");
        Console.ReadKey();
    }

    private static async Task InvokeAsync(string n)
    {
        RpcClient client = new();
        Console.WriteLine($" [x] Requesting fib({n})");
        var response = await client.CallAsync(n);
        Console.WriteLine($" [.] Got '{response}'");
    }
}