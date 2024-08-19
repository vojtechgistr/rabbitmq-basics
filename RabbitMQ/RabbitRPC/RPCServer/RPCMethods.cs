namespace RPCServer;
internal static class RPCMethods
{
    public static int Fib(int n)
    {
        if (n is 0 or 1)
        {
            return n;
        }

        return Fib(n - 1) + Fib(n - 2);
    }
}