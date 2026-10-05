// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

using System.Net;

namespace LogBeam.Tests.Telemetry;

/// <summary>Servidor falso: guarda os pedidos e responde o que o teste mandar.</summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    public sealed record Request(HttpMethod Method, string Url, string? Body, bool HasApiKey);

    private readonly object _lock = new();
    private readonly List<Request> _requests = new();

    public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } =
        _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"status\":\"success\"}") };

    public IReadOnlyList<Request> Requests { get { lock (_lock) return _requests.ToList(); } }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
        lock (_lock)
            _requests.Add(new Request(request.Method, request.RequestUri!.ToString(), body, request.Headers.Contains("X-API-Key")));
        return Respond(request);
    }
}
