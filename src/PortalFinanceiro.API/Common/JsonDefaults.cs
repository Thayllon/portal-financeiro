using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortalFinanceiro.API.Common;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Api = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static JsonDefaults()
    {
        Api.Converters.Add(new JsonStringEnumConverter());
    }
}