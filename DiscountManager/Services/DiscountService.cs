using DiscountManager.DTO;
using DiscountManager.Models;
using DiscountManager.Shared;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DiscountManager.Services;

public class DiscountService : Discount.DiscountBase
{
    private const uint MAX_GENERATE_COUNT = 2_000;
    private const uint MAX_LENGTH = 8;
    private const uint MIN_LENGTH = 7;

    private readonly byte[] secretKey;

    private readonly ILogger<DiscountService> logger;

    private readonly DiscountDbContext dbContext;

    public DiscountService(ILogger<DiscountService> logger,
        IOptions<GeneratorOptions> options,
        DiscountDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        secretKey = Encoding.ASCII.GetBytes(options.Value.SecretKey);
    }

    public override async Task<GetCodesReply> GetCodes(GetCodesRequest request, ServerCallContext context)
    {
        var codes = await dbContext.DiscountCodes.Select(dc => dc.Code).ToListAsync();

        await using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, codes, typeof(List<string>));
        return new GetCodesReply { Result = ByteString.CopyFrom(ms.ToArray()) };
    }

    public override async Task<GenerateReply> GenerateCodes(GenerateRequest request, ServerCallContext context)
    {
        try
        {
            if (request.Count <= 0)
            {
                logger.LogInformation($"None code was generated.");

                return new GenerateReply { Result = true };
            }

            if (request.Count > MAX_GENERATE_COUNT)
            {
                logger.LogInformation($"Can not generate discount codes, max limit was reached {MAX_GENERATE_COUNT}");

                return new GenerateReply { Result = false };
            }

            var codeLength = BitConverter.ToInt16(request.Length.ToByteArray()); ;
            
            if (codeLength < MIN_LENGTH || codeLength > MAX_LENGTH)
            {
                logger.LogInformation($"Can not generate discount codes, length is not supported {codeLength}");

                return new GenerateReply { Result = false };
            }

            logger.LogInformation($"Generating {request.Count} codes with length {codeLength}");

            var generatedCodes = Enumerable
                .Range(1, (int)request.Count)
                .AsParallel()
                .Select(_ => new DiscountCode { Code = GenerateUniqueCode(secretKey, (int)codeLength) })
                .ToList();

            await dbContext.DiscountCodes.AddRangeAsync(generatedCodes, context.CancellationToken);

            await dbContext.SaveChangesAsync(context.CancellationToken);

            return new GenerateReply { Result = true };
        }
        catch (Exception ex)
        {
            logger.LogError("Error to generating codes {0}", ex);

            return new GenerateReply { Result = false };
        }
    }

    public override async Task<CodeReply> UseCode(CodeRequest request, ServerCallContext context)
    {
        var discountCode = await dbContext.DiscountCodes
            .SingleOrDefaultAsync(dc => dc.Code == request.Code, context.CancellationToken);

        var discountCodeDTO = (discountCode == null) switch
        {
            true => new DiscountCodeDTO { Code = string.Empty, Status = "CODE_NOT_FOUND" },
            _ => new DiscountCodeDTO { Code = discountCode.Code, Status = "CODE_USED" }
        };

        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, discountCodeDTO, typeof(DiscountCodeDTO));
        return new CodeReply { Result = ByteString.CopyFrom(ms.ToArray()) };
    }

    private static string GenerateUniqueCode(byte[] secret, int size)
    {
        var randomBytes = Guid
            .NewGuid()
            .ToByteArray();

        var byteArray = secret
            .Concat(randomBytes)
            .ToArray();

        var hashBytes = SHA1.HashData(byteArray);

        var hash = hashBytes
            .Take(10)
            .ToArray();

        var code = BitConverter.ToString(hash)
            .Replace("-", "")
            .ToUpper()
            .Take(size);

        return string.Join(string.Empty, code);
    }
}
