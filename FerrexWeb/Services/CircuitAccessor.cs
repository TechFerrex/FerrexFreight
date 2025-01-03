using Microsoft.AspNetCore.Components;

namespace FerrexWeb.Services
{
    public class CircuitAccessor
    {
        public string CircuitId { get; private set; }

        public void SetCircuitId(string circuitId)
        {
            CircuitId = circuitId;
        }
    }
}
