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

        public RoatpGatewayPeopleInControlController(IRoatpApplicationApiClient roatpApiClient, ILogger<RoatpGatewayPeopleInControlController> logger, IRoatpGatewayPageValidator validator, IPeopleInControlOrchestrator orchestrator) : base(roatpApiClient, logger, validator)
        {
            {
                _orchestrator = orchestrator;
            }
        }

        [HttpGet("/Roatp/Gateway/{applicationId}/Page/PeopleInControl")]
        public async Task<IActionResult> GetGatewayPeopleInControlPage(Guid applicationId)
        {
            var userId = HttpContext.User.UserId();
            var username = HttpContext.User.UserDisplayName();
            var viewModel = await _orchestrator.GetPeopleInControlViewModel(new GetPeopleInControlRequest(applicationId, userId, username));
            return View(viewModel.GatewayReviewStatus == GatewayReviewStatus.ClarificationSent && !string.IsNullOrEmpty(viewModel.ClarificationBy)
                ? $"{GatewayViewsLocation}/Clarifications/PeopleInControl.cshtml"
                : $"{GatewayViewsLocation}/PeopleInControl.cshtml", viewModel);
        }

        [HttpPost("/Roatp/Gateway/{applicationId}/Page/PeopleInControl")]
        public async Task<IActionResult> EvaluatePeopleInControlPage(SubmitGatewayPageAnswerCommand command)
        {
            var userId = HttpContext.User.UserId();
            var username = HttpContext.User.UserDisplayName();
            Func<Task<PeopleInControlPageViewModel>> viewModelBuilder = () => _orchestrator.GetPeopleInControlViewModel(new GetPeopleInControlRequest(command.ApplicationId, userId, username));
            return await ValidateAndUpdatePageAnswer(command, viewModelBuilder, $"{GatewayViewsLocation}/PeopleInControl.cshtml");
        }


        [HttpPost("/Roatp/Gateway/{applicationId}/Page/PeopleInControl/Clarification")]
        public async Task<IActionResult> ClarifyPeopleInControlPage(SubmitGatewayPageAnswerCommand command)
        {
            var userId = HttpContext.User.UserId();
            var username = HttpContext.User.UserDisplayName();
            Func<Task<PeopleInControlPageViewModel>> viewModelBuilder = () => _orchestrator.GetPeopleInControlViewModel(new GetPeopleInControlRequest(command.ApplicationId, userId, username));
            return await ValidateAndUpdateClarificationPageAnswer(command, viewModelBuilder, $"{GatewayViewsLocation}/Clarifications/PeopleInControl.cshtml");
        }


        [HttpGet("/Roatp/Gateway/{applicationId}/Page/PeopleInControlRisk")]
        public async Task<IActionResult> GetGatewayPeopleInControlRiskPage(Guid applicationId)
        {
            var userId = HttpContext.User.UserId();
            var username = HttpContext.User.UserDisplayName();
            var viewModel = await _orchestrator.GetPeopleInControlHighRiskViewModel(new GetPeopleInControlHighRiskRequest(applicationId, userId, username));
            return View(viewModel.GatewayReviewStatus == GatewayReviewStatus.ClarificationSent && !string.IsNullOrEmpty(viewModel.ClarificationBy)
                ? $"{GatewayViewsLocation}/Clarifications/PeopleInControlHighRisk.cshtml"
                : $"{GatewayViewsLocation}/PeopleInControlHighRisk.cshtml", viewModel);
        }

        [HttpPost("/Roatp/Gateway/{applicationId}/Page/PeopleInControlRisk")]
        public async Task<IActionResult> EvaluatePeopleInControlHighRiskPage(SubmitGatewayPageAnswerCommand command)
        {
            var userId = HttpContext.User.UserId();
            var username = HttpContext.User.UserDisplayName();
            Func<Task<PeopleInControlHighRiskPageViewModel>> viewModelBuilder = () => _orchestrator.GetPeopleInControlHighRiskViewModel(new GetPeopleInControlHighRiskRequest(command.ApplicationId, userId, username));
            return await ValidateAndUpdatePageAnswer(command, viewModelBuilder, $"{GatewayViewsLocation}/PeopleInControlHighRisk.cshtml");
        }

        [HttpPost("/Roatp/Gateway/{applicationId}/Page/PeopleInControlRisk/Clarification")]
        public async Task<IActionResult> ClarifyPeopleInControlHighRiskPage(SubmitGatewayPageAnswerCommand command)
        {
            var userId = HttpContext.User.UserId();
            var username = HttpContext.User.UserDisplayName();
            Func<Task<PeopleInControlHighRiskPageViewModel>> viewModelBuilder = () => _orchestrator.GetPeopleInControlHighRiskViewModel(new GetPeopleInControlHighRiskRequest(command.ApplicationId, userId, username));
            return await ValidateAndUpdateClarificationPageAnswer(command, viewModelBuilder, $"{GatewayViewsLocation}/Clarifications/PeopleInControlHighRisk.cshtml");
        }
    
    }

}
