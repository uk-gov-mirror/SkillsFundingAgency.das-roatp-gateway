﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.AdminService.Common.Validation;
using SFA.DAS.RoatpGateway.Domain;
using SFA.DAS.RoatpGateway.Web.Controllers;
using SFA.DAS.RoatpGateway.Web.Models;
using SFA.DAS.RoatpGateway.Web.Services;
using SFA.DAS.RoatpGateway.Web.ViewModels;

namespace SFA.DAS.RoatpGateway.Web.UnitTests.Controllers.OrganisationChecks
{
    [TestFixture]
    public class TradingNameTests : RoatpGatewayControllerTestBase<RoatpGatewayOrganisationChecksController>
    {
        private RoatpGatewayOrganisationChecksController _controller;
        private Mock<IGatewayOrganisationChecksOrchestrator> _orchestrator;
        private static string ClarificationAnswer => "Clarification answer";

        [SetUp]
        public void Setup()
        {
            CoreSetup();

            _orchestrator = new Mock<IGatewayOrganisationChecksOrchestrator>();
            _controller = new RoatpGatewayOrganisationChecksController(ApplyApiClient.Object, GatewayValidator.Object, _orchestrator.Object, Logger.Object);

            _controller.ControllerContext = MockedControllerContext.Setup();
            UserId = _controller.ControllerContext.HttpContext.User.UserId();
            Username = _controller.ControllerContext.HttpContext.User.UserDisplayName();
        }

        [Test]
        public void check_trading_name_request_is_called()
        {
            var applicationId = Guid.NewGuid();

            _orchestrator.Setup(x => x.GetTradingNameViewModel(It.IsAny<GetTradingNameRequest>()))
                .ReturnsAsync(new TradingNamePageViewModel())
                .Verifiable("view model not returned");

            var _result = _controller.GetGatewayTradingNamePage(applicationId).Result;
            _orchestrator.Verify(x => x.GetTradingNameViewModel(It.IsAny<GetTradingNameRequest>()), Times.Once());
        }

        [Test]
        public void post_trading_name_happy_path()
        {
            var applicationId = Guid.NewGuid();
            var pageId = "1-20";

            var vm = new TradingNamePageViewModel
            {
                Status = SectionReviewStatus.Pass,
                SourcesCheckedOn = DateTime.Now,
                ErrorMessages = new List<ValidationErrorDetail>()
            };

            vm.SourcesCheckedOn = DateTime.Now;
            var command = new SubmitGatewayPageAnswerCommand(vm);

            ApplyApiClient.Setup(x =>
                x.SubmitGatewayPageAnswer(applicationId, pageId, vm.Status, UserId, Username, It.IsAny<string>()));

            var result = _controller.EvaluateTradingNamePage(command).Result;

            ApplyApiClient.Verify(x => x.SubmitGatewayPageAnswer(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),null), Times.Once);
            _orchestrator.Verify(x => x.GetTradingNameViewModel(It.IsAny<GetTradingNameRequest>()), Times.Never());
        }

        [Test]
        public void post_trading_name_clarification_happy_path()
        {
            var applicationId = Guid.NewGuid();
            var pageId = "1-20";

            var vm = new TradingNamePageViewModel
            {
                Status = SectionReviewStatus.Pass,
                SourcesCheckedOn = DateTime.Now,
                ErrorMessages = new List<ValidationErrorDetail>(),
                ClarificationAnswer = ClarificationAnswer
            };

            vm.SourcesCheckedOn = DateTime.Now;
            var command = new SubmitGatewayPageAnswerCommand(vm);

            GatewayValidator.Setup(v => v.ValidateClarification(command)).ReturnsAsync(new ValidationResponse { Errors = new List<ValidationErrorDetail>() });

            ApplyApiClient.Setup(x =>
                x.SubmitGatewayPageAnswer(applicationId, pageId, vm.Status, UserId, Username, It.IsAny<string>()));

            var result = _controller.ClarifyTradingNamePage(command).Result;

            ApplyApiClient.Verify(x => x.SubmitGatewayPageAnswerPostClarification(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), ClarificationAnswer), Times.Once);
            _orchestrator.Verify(x => x.GetTradingNameViewModel(It.IsAny<GetTradingNameRequest>()), Times.Never());
        }
        
        [Test]
        public void post_trading_name_path_with_errors()
        {
            var applicationId = Guid.NewGuid();
            var pageId = "1-20";

            var vm = new TradingNamePageViewModel
            {
                Status = SectionReviewStatus.Fail,
                SourcesCheckedOn = DateTime.Now,
                ErrorMessages = new List<ValidationErrorDetail>()

            };

            vm.ApplicationId = applicationId;
            vm.PageId = vm.PageId;
            vm.SourcesCheckedOn = DateTime.Now;
            var command = new SubmitGatewayPageAnswerCommand(vm);

            GatewayValidator.Setup(v => v.Validate(command))
                .ReturnsAsync(new ValidationResponse
                {
                    Errors = new List<ValidationErrorDetail>
                        {
                            new ValidationErrorDetail {Field = "OptionFail", ErrorMessage = "needs text"}
                        }
                }
                );

            _orchestrator.Setup(x => x.GetTradingNameViewModel(It.Is<GetTradingNameRequest>(y => y.ApplicationId == vm.ApplicationId
                                                                                && y.UserName == Username))).ReturnsAsync(vm);

            ApplyApiClient.Setup(x =>
                x.SubmitGatewayPageAnswer(applicationId, pageId, vm.Status, UserId, Username, It.IsAny<string>()));

            var result = _controller.EvaluateTradingNamePage(command).Result;

            ApplyApiClient.Verify(x => x.SubmitGatewayPageAnswer(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Test]
        public void post_trading_name_clarification_path_with_errors()
        {
            var applicationId = Guid.NewGuid();
            var pageId = "1-20";

            var vm = new TradingNamePageViewModel
            {
                Status = SectionReviewStatus.Fail,
                SourcesCheckedOn = DateTime.Now,
                ErrorMessages = new List<ValidationErrorDetail>(),
                ClarificationAnswer = ClarificationAnswer

            };

            vm.ApplicationId = applicationId;
            vm.PageId = vm.PageId;
            vm.SourcesCheckedOn = DateTime.Now;
            var command = new SubmitGatewayPageAnswerCommand(vm);

            GatewayValidator.Setup(v => v.ValidateClarification(command))
                .ReturnsAsync(new ValidationResponse
                    {
                        Errors = new List<ValidationErrorDetail>
                        {
                            new ValidationErrorDetail {Field = "OptionFail", ErrorMessage = "needs text"}
                        }
                    }
                );

            _orchestrator.Setup(x => x.GetTradingNameViewModel(It.Is<GetTradingNameRequest>(y => y.ApplicationId == vm.ApplicationId
                && y.UserName == Username))).ReturnsAsync(vm);

            ApplyApiClient.Setup(x =>
                x.SubmitGatewayPageAnswer(applicationId, pageId, vm.Status, UserId, Username, It.IsAny<string>()));

            var result = _controller.ClarifyTradingNamePage(command).Result;

            ApplyApiClient.Verify(x => x.SubmitGatewayPageAnswer(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }
    }
}