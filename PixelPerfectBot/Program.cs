namespace PixelPerfectBot
{
    class Program
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();
    }
}