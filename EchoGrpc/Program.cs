using DiscountManager.Shared;
using Google.Protobuf;
using Grpc.Net.Client;
using System.Text;
using static DiscountManager.Shared.Discount;

namespace EchoGrpc
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var bytes = Encoding.UTF8.GetBytes("7");

            var request = new GenerateRequest { Count = 10, Length = ByteString.CopyFrom(bytes) };

            using var channel = GrpcChannel.ForAddress("https://localhost:5012");
            var client = new DiscountClient(channel);
            var exitKey = string.Empty;
            while (exitKey != "q" || exitKey != "Q")
            {
                try
                {
                    var reply = await client.GenerateCodesAsync(request);
                    Console.WriteLine("Greeting: " + reply.Result);
                    exitKey = Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                    await Task.Delay(10_000);
                }
            }
        }
    }
}
