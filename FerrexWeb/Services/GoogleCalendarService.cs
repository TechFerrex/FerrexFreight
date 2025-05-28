using AngleSharp.Dom.Events;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using GoogleEvent = Google.Apis.Calendar.v3.Data.Event;

namespace FerrexWeb.Services
{
    public class GoogleCalendarService
    {
        private readonly CalendarService _service;

        public GoogleCalendarService(IConfiguration config)
        {
            _service = new CalendarService(new BaseClientService.Initializer
            {
                ApiKey = config["Google:ApiKey"]
            });
        }

        public async Task<IList<GoogleEvent>> GetNextEventsAsync(int maxResults = 10)
        {
            // Preparo la petición
            var request = _service.Events.List("primary");
            request.TimeMin = DateTime.UtcNow;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.MaxResults = maxResults;

            // Ejecuto y devuelvo la lista de eventos
            Events events = await request.ExecuteAsync();
            return events.Items;
        }
    }
}
