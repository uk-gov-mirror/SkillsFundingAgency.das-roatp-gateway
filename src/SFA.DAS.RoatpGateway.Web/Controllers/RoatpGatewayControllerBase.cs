﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Internal;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.RoatpGateway.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpGateway.Web.Models;
using SFA.DAS.RoatpGateway.Domain;
using SFA.DAS.RoatpGateway.Web.Attributes;
using SFA.DAS.RoatpGateway.Web.ViewModels;
using SFA.DAS.RoatpGateway.Web.Validators;
using SFA.DAS.RoatpGateway.Web.Domain;

namespace SFA.DAS.RoatpGateway.Web.Controllers
{
    [ExternalApiExceptionFilter]
    [Authorize(Roles = Roles.RoatpGatewayTeam)]
    public class RoatpGatewayControllerBase<T> : Controller
    {
        protected readonly IRoatpApplicationApiClient _applyApiClient;
        protected readonly ILogger<T> _logger;
        protected readonly IRoatpGatewayPageValidator GatewayValidator;
        protected const string GatewayViewsLocation = "~/Views/Gateway/pages";

        public RoatpGatewayControllerBase()
        {

        }

        public RoatpGatewayControllerBase(IRoatpApplicationApiClient applyApiClient,
                                          ILogger<T> logger, IRoatpGatewayPageValidator gatewayValidator)
        {
            _applyApiClient = applyApiClient;
            _logger = logger;
            GatewayValidator = gatewayValidator;
        }

        public string SetupGatewayPageOptionTexts(SubmitGatewayPageAnswerCommand command)
        {
            if (command?.Status == null) return string.Empty;
            command.OptionInProgressText = command.Status == SectionReviewStatus.InProgress && !string.IsNullOrEmpty(command.OptionInProgressText) ? command.OptionInProgressText : string.Empty;
            command.OptionPassText = command.Status == SectionReviewStatus.Pass && !string.IsNullOrEmpty(command.OptionPassText) ? command.OptionPassText : string.Empty;
            command.OptionFailText = command.Status == SectionReviewStatus.Fail && !string.IsNullOrEmpty(command.OptionFailText) ? command.OptionFailText : string.Empty;

            switch (command.Status)
            {
                case SectionReviewStatus.Pass:
                    return command.OptionPassText;
                case SectionReviewStatus.Fail:
                    return command.OptionFailText;
                case SectionReviewStatus.InProgress:
                    return command.OptionInProgressText;
                default:
                    return string.Empty;
            }
        }

        protected async Task<IActionResult> ValidateAndUpdatePageAnswer<VM>(SubmitGatewayPageAnswerCommand command,
                                                                  Func<Task<VM>> viewModelBuilder,
                                                                  string errorView) where VM : RoatpGatewayPageViewModel
        {
            var validationResponse = await GatewayValidator.Validate(command);
            if (validationResponse.Errors != null && validationResponse.Errors.Any())
            {
                var viewModel = await viewModelBuilder.Invoke();
                viewModel.Status = command.Status;
                viewModel.OptionFailText = command.OptionFailText;
                viewModel.OptionInProgressText = command.OptionInProgressText;
                viewModel.OptionPassText = command.OptionPassText;
                viewModel.ErrorMessages = validationResponse.Errors;
                return View(errorView, viewModel);
            }

            return await SubmitGatewayPageAnswer(command);
        }

        protected async Task<IActionResult> SubmitGatewayPageAnswer(SubmitGatewayPageAnswerCommand command)
        {
            var username = HttpContext.User.UserDisplayName();
            var userId = HttpContext.User.UserId();
            var comments = SetupGatewayPageOptionTexts(command);

            _logger.LogInformation($"{typeof(T).Name}-SubmitGatewayPageAnswer - ApplicationId '{command.ApplicationId}' - PageId '{command.PageId}' - Status '{command.Status}' - UserName '{username}' - Comments '{comments}'");
            try
            {
                await _applyApiClient.SubmitGatewayPageAnswer(command.ApplicationId, command.PageId, command.Status, userId, username, comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{typeof(T).Name}-SubmitGatewayPageAnswer - Error: '" + ex.Message + "'");
                throw;
            }

            return RedirectToAction("ViewApplication", "RoatpGateway", new { command.ApplicationId });
        }

    }
}
