﻿using NUnit.Framework;
using SFA.DAS.RoatpGateway.Domain;
using SFA.DAS.RoatpGateway.Web.Validators;
using SFA.DAS.RoatpGateway.Web.ViewModels;
using System.Linq;

namespace SFA.DAS.RoatpGateway.Web.UnitTests.Validators
{
    [TestFixture]
    public class RoatpGatewayApplicationViewModelValidatorTests
    {
        private RoatpGatewayApplicationViewModel _viewModel;

        private IRoatpGatewayApplicationViewModelValidator _validator;
        [SetUp]
        public void Setup()
        {
            _validator = new RoatpGatewayApplicationViewModelValidator();
        }

        [TestCase(GatewayReviewStatus.ClarificationSent, "Clarification Message", "Declined Message", "Approved Message", false)]
        [TestCase(GatewayReviewStatus.ClarificationSent, null, "Declined Message", "Approved Message", true)]
        [TestCase(GatewayReviewStatus.Fail, "Clarification Message", "Declined Message", "Approved Message", false)]
        [TestCase(GatewayReviewStatus.Fail, "Clarification Message", null, "Approved Message", true)]
        [TestCase(GatewayReviewStatus.Pass, "Clarification Message", "Declined Message", "Approved Message", false)]
        [TestCase(GatewayReviewStatus.Pass, null, "Declined Message", "Approved Message", false)]
        [TestCase(null, null, null, null, true)]
        public void Test_cases_for_no_status_and_no_fail_text_to_check_messages_as_expected(string gatewayReviewStatus, string clarificationMessage, string declinedMessage, string approvedMessage, bool hasErrorMessage)
        {
            _viewModel = new RoatpGatewayApplicationViewModel
            {
                GatewayReviewStatus = gatewayReviewStatus,
                OptionAskClarificationText = clarificationMessage,
                OptionFailedText = declinedMessage,
                OptionApprovedText = approvedMessage
            };

            var result = _validator.Validate(_viewModel).Result;

            Assert.AreEqual(hasErrorMessage, result.Errors.Any());
        }

        [TestCase(GatewayReviewStatus.ClarificationSent, 150, false)]
        [TestCase(GatewayReviewStatus.ClarificationSent, 151, true)]
        [TestCase(GatewayReviewStatus.Fail, 150, false)]
        [TestCase(GatewayReviewStatus.Fail, 151, true)]
        [TestCase(GatewayReviewStatus.Pass, 150, false)]
        [TestCase(GatewayReviewStatus.Pass, 151, true)]
        public void Test_cases_where_input_is_too_long(string gatewayReviewStatus, int wordCount, bool hasErrorMessage)
        {
            var words = string.Empty;
            for (var i = 0; i < wordCount; i++)
            {
                words = $"{words}{i} ";
            }

            _viewModel = new RoatpGatewayApplicationViewModel();
            _viewModel.GatewayReviewStatus = gatewayReviewStatus;
            _viewModel.OptionAskClarificationText = words;
            _viewModel.OptionFailedText = words;
            _viewModel.OptionApprovedText = words;

            var result = _validator.Validate(_viewModel).Result;

            Assert.AreEqual(hasErrorMessage, result.Errors.Any());
        }
    }
}
