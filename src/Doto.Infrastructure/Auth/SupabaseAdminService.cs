using Microsoft.Extensions.Configuration;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;

public class SupabaseAdminService : IAdminAuthService
{
    private readonly HttpClient _client;
    private readonly string _projectRef;
    private readonly string _serviceKey;

    public SupabaseAdminService(IConfiguration config)
    {
        _projectRef = config["SUPABASE_PROJECT_REF"]
            ?? throw new Exception("Missing SUPABASE_PROJECT_REF");

        _serviceKey = config["SUPABASE_SERVICE_ROLE_KEY"]
            ?? throw new Exception("Missing SUPABASE_SERVICE_ROLE_KEY");

        _client = new HttpClient();
    }

    public async Task<BaseResponse<bool>>DeleteUserAsync(string userId)
    {
        var url = $"https://{_projectRef}.supabase.co/auth/v1/admin/users/{userId}";

        var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Add("apikey", _serviceKey);
        req.Headers.Add("Authorization", $"Bearer {_serviceKey}");

        var response = await _client.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            return BaseResponse<bool>.Fail("Supabase user deleted successfully");
        }

        return BaseResponse<bool>.Ok("Supabase user deleted successfully");
    }
}
