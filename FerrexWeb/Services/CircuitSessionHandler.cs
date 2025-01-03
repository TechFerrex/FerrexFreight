using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using FerrexWeb.Services;
public class CircuitSessionHandler : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CircuitAccessor _circuitAccessor;

    public static ConcurrentDictionary<string, string> CircuitSessions = new ConcurrentDictionary<string, string>();

    public CircuitSessionHandler(IHttpContextAccessor httpContextAccessor, CircuitAccessor circuitAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _circuitAccessor = circuitAccessor;

    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            Console.WriteLine("HttpContext is null. Skipping session initialization.");
            return Task.CompletedTask;
        }

        var sessionId = _httpContextAccessor.HttpContext.Session.Id;
        var circuitId = circuit.Id;

        CircuitSessions.TryAdd(circuitId, sessionId);

        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }



    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitSessions.TryRemove(circuit.Id, out _);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
