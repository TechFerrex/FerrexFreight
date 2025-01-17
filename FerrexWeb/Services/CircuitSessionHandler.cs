using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using FerrexWeb.Services;
using FerrexWeb.Models;
using Microsoft.EntityFrameworkCore;

public class CircuitSessionHandler : CircuitHandler
{
    // Tu diccionario concurrente (como lo tenías)
    public static ConcurrentDictionary<string, string> CircuitSessions = new ConcurrentDictionary<string, string>();

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CircuitAccessor _circuitAccessor;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    // Inyectamos el dbContextFactory para crear el contexto cuando sea necesario
    public CircuitSessionHandler(
        IHttpContextAccessor httpContextAccessor,
        CircuitAccessor circuitAccessor,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _circuitAccessor = circuitAccessor;
        _dbContextFactory = dbContextFactory;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            Console.WriteLine("HttpContext is null. Skipping session initialization.");
            await base.OnCircuitOpenedAsync(circuit, cancellationToken);
            return;
        }

        var sessionId = _httpContextAccessor.HttpContext.Session.Id;
        var circuitId = circuit.Id;

        // Guardamos la relación Circuit -> SessionId en tu diccionario
        CircuitSessions.TryAdd(circuitId, sessionId);

        // === LÓGICA PARA GUARDAR EN BASE DE DATOS ===
        try
        {
            // Crear el contexto
            using var dbContext = _dbContextFactory.CreateDbContext();

            // Obtener IP y user-agent
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

            // Crear un nuevo registro de Visitor
            var newVisitor = new Visitor
            {
                SessionId = sessionId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedTime = DateTime.Now,
                PolicyAccepted = false // Por defecto, asume que no aceptó
            };

            dbContext.Visitor.Add(newVisitor);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error guardando el registro de visitante: {ex.Message}");
            // Manejo de error o logging
        }

        // Llamar al método base para continuar
        await base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        // Quitamos el Circuit del diccionario
        CircuitSessions.TryRemove(circuit.Id, out _);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
