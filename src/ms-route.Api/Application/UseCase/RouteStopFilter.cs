using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class RouteStopFilter : IRouteStopFilter
{
    public IEnumerable<RouteStop> FilterStops(
        IEnumerable<RouteStop> routeStops,
        Dictionary<Guid, Stop> stopById,
        RouteStudentAssignment? assignment,
        bool isParent,
        Guid? studentProfileId)
    {
        var filteredStops = routeStops
            .Where(rs => rs.Status == Status.Active && stopById.ContainsKey(rs.StopId));

        if (isParent && studentProfileId.HasValue)
        {
            // Padre: solo parada casa (asignada) + parada colegio (OrderSequence max)
            var assignedStop = filteredStops.FirstOrDefault(rs => rs.StopId == assignment?.RouteStopId);
            var schoolStop = filteredStops.OrderByDescending(rs => rs.OrderSequence).FirstOrDefault();

            var stopsToInclude = new List<RouteStop>();
            if (assignedStop != null) stopsToInclude.Add(assignedStop);
            if (schoolStop != null && schoolStop.StopId != assignedStop?.StopId) stopsToInclude.Add(schoolStop);

            filteredStops = stopsToInclude;
        }
        else if (studentProfileId.HasValue && !isParent)
        {
            // Estudiante: solo su parada asignada
            var assignedStop = filteredStops.FirstOrDefault(rs => rs.StopId == assignment?.RouteStopId);
            if (assignedStop != null) filteredStops = new[] { assignedStop };
            else filteredStops = Enumerable.Empty<RouteStop>();
        }

        return filteredStops;
    }
}