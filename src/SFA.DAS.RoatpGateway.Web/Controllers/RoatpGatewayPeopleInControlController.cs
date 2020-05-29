﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.RoatpGateway.Domain;
using SFA.DAS.RoatpGateway.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpGateway.Web.Models;
using SFA.DAS.RoatpGateway.Web.Services;
using SFA.DAS.RoatpGateway.Web.Validators;
using SFA.DAS.RoatpGateway.Web.ViewModels;

namespace SFA.DAS.RoatpGateway.Web.Controllers
{

    public class RoatpGatewayPeopleInControlController : RoatpGatewayControllerBase<RoatpGatewayPeopleInControlController>
    {

        private readonly IPeopleInControlOrchestrator _orchestrator;

        public RoatpGatewayPeopleInControlController(IHttpContextAccessor contextAccessor, IRoatpApplicationApiClient roatpApiClient, ILogger<RoatpGatewayPeopleInControlController> logger, IRoatpGatewayPageValidator validator, IPeopleInControlOrchestrator orchestrator) : base(contextAccessor, roatpApiClient, logger, validator)
        {
            {
                _orchestrator = orchestrator;
            }
        }

        [HttpGet("/Roatp/Gateway/{applicationId}/Page/PeopleInControl")]
        public async Task<IActionResult> GetGatewayPeopleInControlPage(Guid applicationId)
        {
            var username = _contextAccessor.HttpContext.User.UserDisplayName();
            var viewModel = await _orchestrator.GetPeopleInControlViewModel(new GetPeopleInControlRequest(applicationId, username));
            return View($"{GatewayViewsLocation}/PeopleInControl.cshtml", viewModel);
        }

        [HttpPost("/Roatp/Gateway/{applicationId}/Page/PeopleInControl")]
        public async Task<IActionResult> EvaluatePeopleInControlPage(SubmitGatewayPageAnswerCommand command)
        {
            Func<Task<PeopleInControlPageViewModel>> viewModelBuilder = () => _orchestrator.GetPeopleInControlViewModel(new GetPeopleInControlRequest(command.ApplicationId, _contextAccessor.HttpContext.User.UserDisplayName()));
            return await ValidateAndUpdatePageAnswer(command, viewModelBuilder, $"{GatewayViewsLocation}/PeopleInControl.cshtml");
        }


        [HttpGet("/Roatp/Gateway/{applicationId}/Page/PeopleInControlRisk")]
        public async Task<IActionResult> GetGatewayPeopleInControlRiskPage(Guid applicationId)
        {
            var username = _contextAccessor.HttpContext.User.UserDisplayName();
            var viewModel = await _orchestrator.GetPeopleInControlHighRiskViewModel(new GetPeopleInControlHighRiskRequest(applicationId, username));
            return View($"{GatewayViewsLocation}/PeopleInControlHighRisk.cshtml", viewModel);
        }

        [HttpPost("/Roatp/Gateway/{applicationId}/Page/PeopleInControlRisk")]
        public async Task<IActionResult> EvaluatePeopleInControlHighRiskPage(SubmitGatewayPageAnswerCommand command)
        {
            Func<Task<PeopleInControlHighRiskPageViewModel>> viewModelBuilder = () => _orchestrator.GetPeopleInControlHighRiskViewModel(new GetPeopleInControlHighRiskRequest(command.ApplicationId, _contextAccessor.HttpContext.User.UserDisplayName()));
            return await ValidateAndUpdatePageAnswer(command, viewModelBuilder, $"{GatewayViewsLocation}/PeopleInControlHighRisk.cshtml");
        }
    }

}
