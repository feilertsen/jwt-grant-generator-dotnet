using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace maskinporten;

class Program
{
    // this should be stored in a super secret place, this is just an example
    // generated with https://mkjwk.org/ for demo purposes, 
    // public key must be uploaded manually or programatically to maskinporten
    private static string rsaKey = "{'p':'7OZvK7jDWOgj3R_Zsxx1qKQQYMqJ6vkvr7r7kfKEm4yuYSE9OVWytKJJag5oOZgl1HijjJT-pgUYMDjIO101xIXW9MY8AJA5-q8025QrcQYNqgqgA20UJYfzpQSvEupycW_qrHhveBFGrVZSUEUOGdX5uOV_6vc1kj13YPZUbDM','kty':'RSA','q':'3SwN-Sj-fWIWsTrT7OyklfYRDdjNgoyoWQlpoQ8JDLZCcsbbQJ1H7yR8ncb_Afozo4xy9lmDUJIJEjLp0yqbsxHpzpCH7Hi0rDs5SQkauF6y-BDXwI19mMnC-B0vSOxgW6U_GPWdigJHOBrMnZ1oV1ipxjj7eHNzJQ-nZcXmD8E','d':'IrVzXlHXRahMZvObdoN5YD9MdTXueW4xd3zYiZZHCGa_sLC9MqgK6yF8YgLH6b_62AkMSM98zXhheiZhpHMCCbjXIxLwrT0h3EPhghPOB37z8x93KJxMW3DmmwIL2750hhWBdE__WnA_4TVaqRT4KnFHkVtxR4oalfIpvFoHfBjMk8vOgIuxzGgqgr38ZtulgyZI9FBq0gimwjq_JbjmXfK8qb0fECeLo2KyqaSONQUG-0uSgnsEXj5qzCVobV3Pi4Gl6zx-7utMYZDhqBwyW21C4V__OaU49KA7TsHfAAzjJFlKyX4ZQhbQ3H2nBuhEZjSr_sPk_fTEvLyNSZWTAQ','e':'AQAB','use':'sig','kid':'KrxwhpaqcEUgXkwsmMhXNaqcAqbRzvJsN_ARv1EmfC0','qi':'B0IvrqZy7j4-JfixVq_1vOBGvMutkv6qnGMbk5v5k-v_jWQx1hbY7EpUO822cqh0iMyeKzof4MWagr4K3pB7Cm20Tr9iX94QfHy1G-apzSMTDfIiwrMJfkzjBO-uc-7iwcVwImzdrvbTd1FpFcWDbTeKgwDe7QuVjtym18K8IQg','dp':'J2s8imDnGG8gMJYxKk2NAm-yTfjFtDSci7goTiO1jxB8n6rhPh4Va1sprh0RmKvJd65PQIA2Uze7y6JXJxGedcHzf61QpKNbEmx-9h6Uj-z67xVvhMJsvMX_c7Dw1MvwGRIPUX2ExszRHtyjO7oR25iwQeTmQRm7b0cNCzRR3eM','alg':'RS256','dq':'YS_AnCH1Yic-DXbqNQvvbq4H5GvGMn2YZDeMoOc4dMuJ-2GCaqwyNdV0pOgSmk3VbyKCSdofp8HkFokk0lPAwzk14j0EXVbVHXGQxJUplqaWQgc0pzoFXKQb9mbspkCoPt0oEtGq_j_uB6tPCltmxCsDv0S-y7j_eeOdDU2ewIE','n':'zKuygh-7QUS8H-EZEFRk4e7hAgk000iIH6frJCLXecx_gNy1lhJ2pvW3M3BP5OjXaheYHQSBeSPc3ME0WjdWw011y0D_CWS64lOEH0OtJ1GS3dmhvx-ryBo2ugml7qO7Xv4xVayEXQxagtySF3x1XiZ3j6nSRu44ewTUbnc4Y-OVkYHZkZp081f39omsH_QQC68AaTw6TjpE_5TRuKD2OmXDMeRA8kGIniY4xASBZqjMSSSmW_RS2PqtrhWTr1megSk7aZ3grNj74pTqFbFH1fqYCHZCVctXoUzXEAV3BHlpGPNkGzHhO_UoFhosUdbBHwANRaMjs3yw4urEHM6Pcw'}";

    public static async Task Main()
    {
        var jwk = new JsonWebKey(rsaKey);
        var signingCredentials = new SigningCredentials(jwk, SecurityAlgorithms.RsaSha256);

        //var cert = new X509Certificate2("file...");
        //var signingCredentials2 = new SigningCredentials(new X509SecurityKey(cert), SecurityAlgorithms.RsaSha256Signature);

        // Configuration
        var tokenUrl = "https://maskinporten.no/token";
        var clientId = "a2f9b9e6-c34c-4805-a163-ae29f1559253";
        var scope = "fdir:echoapi";

        var clientAssertionToken = CreateClientToken(signingCredentials, tokenUrl, clientId, scope);

        Console.WriteLine("Client assertion token:");
        Console.WriteLine(clientAssertionToken);
        Console.WriteLine();

        var response = await RequestTokenAsync(clientAssertionToken, tokenUrl);
        var responseJson = await response.Content.ReadAsStringAsync();

        Console.WriteLine("\nToken response:");
        Console.WriteLine(JsonPrettify(responseJson));
        Console.ReadLine();
    }

    static async Task<HttpResponseMessage> RequestTokenAsync(string clientToken, string tokenUrl)
    {
        var httpClient = new HttpClient();
        var param = new Dictionary<string, string>()
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
            { "assertion", clientToken }
        };

        var request = new FormUrlEncodedContent(param);
        var response = await httpClient.PostAsync(tokenUrl, request);

        return response;
    }

    static string CreateClientToken(SigningCredentials signingCredentials, string audience, string issuer, string scope)
    {
        var now = DateTime.UtcNow;
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new List<Claim>()
            {
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("iat", now.ToEpochTime().ToString(), ClaimValueTypes.Integer64),
                new Claim("scope", scope)
            },
            expires: now.AddMinutes(1),
            signingCredentials: signingCredentials);

        return tokenHandler.WriteToken(token);
    }

    public static string JsonPrettify(string json)
    {
        using var j = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(j, new JsonSerializerOptions { WriteIndented = true });
    }
}
