﻿using UNIFIEDDASHBOARD.WebApi.Application.Identity.Roles;
using UNIFIEDDASHBOARD.WebApi.Application.Identity.Users;

namespace UNIFIEDDASHBOARD.WebApi.Application.Dashboard;

public class GetStatsRequest : IRequest<StatsDto>
{
}

public class GetStatsRequestHandler : IRequestHandler<GetStatsRequest, StatsDto>
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IStringLocalizer _t;

    public GetStatsRequestHandler(IUserService userService, IRoleService roleService, IStringLocalizer<GetStatsRequestHandler> localizer)
    {
        _userService = userService;
        _roleService = roleService;
        _t = localizer;
    }

    public async Task<StatsDto> Handle(GetStatsRequest request, CancellationToken cancellationToken)
    {
        var stats = new StatsDto
        {
            UserCount = await _userService.GetCountAsync(cancellationToken),
            RoleCount = await _roleService.GetCountAsync(cancellationToken)
        };

        int selectedYear = DateTime.UtcNow.Year;
        double[] productsFigure = new double[13];
        double[] brandsFigure = new double[13];
        for (int i = 1; i <= 12; i++)
        {
            int month = i;
            var filterStartDate = new DateTime(selectedYear, month, 01).ToUniversalTime();
            var filterEndDate = new DateTime(selectedYear, month, DateTime.DaysInMonth(selectedYear, month), 23, 59, 59).ToUniversalTime(); // Monthly Based
         
        }

        stats.DataEnterBarChart.Add(new ChartSeries { Name = _t["Entries"], Data = productsFigure });
        stats.DataEnterBarChart.Add(new ChartSeries { Name = _t["Promotions"], Data = brandsFigure });

        return stats;
    }
}
