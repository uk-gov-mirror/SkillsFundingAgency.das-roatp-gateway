﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpGateway.Domain;
using SFA.DAS.RoatpGateway.Web.Infrastructure.ApiClients.TokenService;
using SFA.DAS.RoatpGateway.Domain.Ukrlp;
using SFA.DAS.RoatpGateway.Domain.CompaniesHouse;
using SFA.DAS.RoatpGateway.Domain.CharityCommission;
using SFA.DAS.RoatpGateway.Domain.Roatp;
using System.Net.Http;
using SFA.DAS.RoatpGateway.Web.Infrastructure.ApiClients.Exceptions;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using SFA.DAS.AdminService.Common.Infrastructure;
using SFA.DAS.RoatpGateway.Domain.Apply;

namespace SFA.DAS.RoatpGateway.Web.Infrastructure.ApiClients
{
    public class RoatpApplicationApiClient : ApiClientBase<RoatpApplicationApiClient>, IRoatpApplicationApiClient
    {
        public RoatpApplicationApiClient(HttpClient client, ILogger<RoatpApplicationApiClient> logger, IRoatpApplicationTokenService tokenService)
            : base(client, logger)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(client.BaseAddress));
        }

        public async Task<Apply> GetApplication(Guid applicationId)
        {
            return await Get<Apply>($"/Application/{applicationId}");
        }

        public async Task<GetGatewayApplicationCountsResponse> GetApplicationCounts()
        {
            return await Get<GetGatewayApplicationCountsResponse>($"/GatewayReview/Counts");
        }

        public async Task<List<RoatpApplicationSummaryItem>> GetNewGatewayApplications()
        {
            return await Get<List<RoatpApplicationSummaryItem>>($"/GatewayReview/NewApplications");
        }

        public async Task<List<RoatpApplicationSummaryItem>> GetInProgressGatewayApplications()
        {
            return await Get<List<RoatpApplicationSummaryItem>>($"/GatewayReview/InProgressApplications");
        }

        public async Task<List<RoatpApplicationSummaryItem>> GetClosedGatewayApplications()
        {
            return await Get<List<RoatpApplicationSummaryItem>>($"/GatewayReview/ClosedApplications");
        }

        public async Task EvaluateGateway(Guid applicationId, bool isGatewayApproved, string userId, string userName)
        {
            await Post($"/GatewayReview/{applicationId}/Evaluate", new { isGatewayApproved, userId, userName });
        }

        public async Task WithdrawApplication(Guid applicationId, string comments, string userId, string userName)
        {
            await Post($"/GatewayReview/{applicationId}/Withdraw", new { comments, userId, userName });
        }

        public async Task RemoveApplication(Guid applicationId, string comments, string externalComments, string userId, string userName)
        {
            await Post($"/GatewayReview/{applicationId}/Remove", new { comments, externalComments, userId, userName });
        }

        public async Task<List<GatewayPageAnswerSummary>> GetGatewayPageAnswers(Guid applicationId)
        {
            return await Get<List<GatewayPageAnswerSummary>>($"/Gateway/{applicationId}/Pages");
        }

        public async Task<GatewayCommonDetails> GetPageCommonDetails(Guid applicationId, string pageId, string userId, string userName)
        {
            try
            {
               return await Post<GatewayCommonDetailsRequest, GatewayCommonDetails> ($"Gateway/{applicationId}/CommonDetails", new GatewayCommonDetailsRequest(pageId, userId, userName));
            }
            catch (RoatpApiClientException ex)
            {
                _logger.LogError("An error occurred when retrieving Gateway common details", ex);
                throw new ExternalApiException("An error occurred when retrieving Gateway common details", ex);
            }
        }

        public async Task<ContactAddress> GetOrganisationAddress(Guid applicationId)
        {
            return await Get<ContactAddress>($"/Gateway/{applicationId}/OrganisationAddress");
        }

        public async Task<IcoNumber> GetIcoNumber(Guid applicationId)
        {
            return await Get<IcoNumber>($"/Gateway/{applicationId}/IcoNumber");
        }

        public async Task SubmitGatewayPageAnswer(Guid applicationId, string pageId, string status, string userId, string username,
            string comments)
        {
                await SubmitGatewayPageAnswer(applicationId, pageId, status, userId, username, comments, null);
        }


        public async Task SubmitGatewayPageAnswer(Guid applicationId, string pageId, string status, string userId, string username,
            string comments, string clarificationAnswer)
        {
            _logger.LogInformation($"RoatpApplicationApiClient-SubmitGatewayPageAnswer - ApplicationId '{applicationId}' - PageId '{pageId}' - Status '{status}' - UserName '{username}' - Comments '{comments}' - ClarificationAnswer '{clarificationAnswer}'");

            try
            {
                await Post($"/Gateway/Page/Submit", new { applicationId, pageId, status, comments, userId, username, clarificationAnswer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoatpApplicationApiClient - SubmitGatewayPageAnswer - Error: '" + ex.Message + "'");
            }
        }

        public async Task SubmitGatewayPageAnswerClarification(Guid applicationId, string pageId, string status, string userId, string username,
            string comments, string clarificationAnswer)
        {
            _logger.LogInformation($"RoatpApplicationApiClient-SubmitGatewayPageAnswerClarification - ApplicationId '{applicationId}' - PageId '{pageId}' - Status '{status}' - UserName '{username}' - Comments '{comments}' - ClarificationAnswer '{clarificationAnswer}'");

            try
            {
                await Post($"/Gateway/Page/SubmitClarification", new { applicationId, pageId, status, comments, userId, username, clarificationAnswer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoatpApplicationApiClient - SubmitGatewayPageAnswerClarification - Error: '" + ex.Message + "'");
            }
        }

        public async Task SubmitGatewayPageAnswerPostClarification(Guid applicationId, string pageId, string status, string userId, string username,
            string comments, string clarificationAnswer)
        {
            _logger.LogInformation($"RoatpApplicationApiClient-SubmitGatewayPageAnswerPostClarification - ApplicationId '{applicationId}' - PageId '{pageId}' - Status '{status}' - UserName '{username}' - Comments '{comments}' - ClarificationAnswer '{clarificationAnswer}'");

            try
            {
                await Post($"/Gateway/Page/SubmitPostClarification", new { applicationId, pageId, status, comments, userId, username, clarificationAnswer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoatpApplicationApiClient - SubmitGatewayPageAnswerPostClarification - Error: '" + ex.Message + "'");
            }
        }

        public async Task UpdateGatewayReviewStatusAndComment(Guid applicationId, string gatewayReviewStatus, string gatewayReviewComment, string gatewayReviewExternalComment, string userId, string userName)
        {
            _logger.LogInformation($"RoatpApplicationApiClient-UpdateGatewayReviewStatusAndComment - ApplicationId '{applicationId}' - GatewayReviewStatus '{gatewayReviewStatus}' - GatewayReviewComment '{gatewayReviewComment}'- GatewayReviewExternalComment '{gatewayReviewExternalComment}' - UserName '{userName}'");

            try
            {
                var responseCode = await Post($"/Gateway/UpdateGatewayReviewStatusAndComment", new { applicationId, gatewayReviewStatus, gatewayReviewComment, gatewayReviewExternalComment, userId, userName });
                if (responseCode != System.Net.HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Unable to update RoATP gateway review status, response code {responseCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoatpApplicationApiClient-UpdateGatewayReviewStatusAndComment - Error: '" + ex.Message + "'");
                throw;
            }

        }

        public async Task UpdateGatewayReviewStatusAsClarification(Guid applicationId, string userId, string userName)
        {
            _logger.LogInformation($"RoatpApplicationApiClient-UpdateGatewayReviewStatusAsClarification - ApplicationId '{applicationId}' - UserName '{userName}'");

            try
            {
                var responseCode = await Post($"/Gateway/UpdateGatewayClarification", new { applicationId, userId, userName });
                if (responseCode != System.Net.HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Unable to update RoATP gateway review status as clarification, response code {responseCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoatpApplicationApiClient-UpdateGatewayReviewStatusAsClarification - Error: '" + ex.Message + "'");
                throw;
            }
        }


        public async Task<ProviderDetails> GetUkrlpDetails(Guid applicationId)
        {
            try
            {
                return await Get<ProviderDetails>($"Gateway/UkrlpData/{applicationId}");
            }
            catch (RoatpApiClientException ex)
            {
                _logger.LogError("An error occurred when retrieving UKRLP details", ex);
                throw new ExternalApiException("An error occurred when retrieving UKRLP details", ex);
            }
        }

        public async Task<CompaniesHouseSummary> GetCompaniesHouseDetails(Guid applicationId)
        {
            try
            {
                return await Get<CompaniesHouseSummary>($"Gateway/CompaniesHouseData/{applicationId}");
            }
            catch (RoatpApiClientException ex)
            {
                _logger.LogError("An error occurred when retrieving Companies House details", ex);
                throw new ExternalApiException("An error occurred when retrieving Companies House details", ex);
            }
        }

        public async Task<CharityCommissionSummary> GetCharityCommissionDetails(Guid applicationId)
        {
            try
            {
                return await Get<CharityCommissionSummary>($"Gateway/CharityCommissionData/{applicationId}");
            }
            catch (RoatpApiClientException ex)
            {
                _logger.LogError("An error occurred when retrieving Charity Commission details", ex);
                throw new ExternalApiException("An error occurred when retrieving Charity Commission details", ex);
            }
        }

        public async Task<OrganisationRegisterStatus> GetOrganisationRegisterStatus(Guid applicationId)
        {
            try
            {
                return await Get<OrganisationRegisterStatus>($"Gateway/RoatpRegisterData/{applicationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when retrieving RoATP details", ex);
                throw new ExternalApiException("An error occurred when retrieving RoATP details", ex);
            }
        }

        public async Task<DateTime?> GetSourcesCheckedOnDate(Guid applicationId)
        {
            return await Get<DateTime?>($"Gateway/SourcesCheckedOn/{applicationId}");
        }

        public async Task<string> GetTradingName(Guid applicationId)
        {
            return await Get<string>($"/Gateway/{applicationId}/TradingName");
        }

        public async Task<string> GetProviderRouteName(Guid applicationId)
        {
            return await Get<string>($"/Gateway/{applicationId}/ProviderRouteName");
        }

        public async Task<string> GetWebsiteAddressSourcedFromUkrlp(Guid applicationId)
        {
            return await Get<string>($"/Gateway/{applicationId}/WebsiteAddressFromUkrlp");
        }


        public async Task<string> GetWebsiteAddressManuallyEntered(Guid applicationId)
        {
            return await Get<string>($"/Gateway/{applicationId}/WebsiteAddressManuallyEntered");
        }

        public async Task<string> GetOrganisationWebsiteAddress(Guid applicationId)
        {
            return await Get<string>($"/Gateway/{applicationId}/OrganisationWebsiteAddress");
        }


        public async Task<ContactDetails> GetContactDetails(Guid applicationId)
        {
            return await Get<ContactDetails>($"/Application/{applicationId}/Contact");
        }

        public async Task<bool> UploadSubcontractorDeclarationClarificationFile(Guid applicationId, string userId, string userName,
            IFormFileCollection clarificationFiles)
        {
            var fileName = string.Empty;
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(userId), "userId");
            content.Add(new StringContent(userName), "userName");

            if (clarificationFiles != null && clarificationFiles.Any())
            {
                foreach (var file in clarificationFiles)
                {
                    fileName = file.FileName;
                    var fileContent = new StreamContent(file.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength = file.Length, ContentType = new MediaTypeHeaderValue(file.ContentType)
                        }
                    };
                    content.Add(fileContent, file.FileName, file.FileName);
                }

                try
                {
                    var response = await _httpClient.PostAsync($"/Gateway/SubcontractorDeclarationClarification/{applicationId}/Upload", content);

                    return response.StatusCode == HttpStatusCode.OK;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex,
                        $"Error when submitting Subcontractor Declaration Clarification File update for Application: {applicationId} | Filename: {fileName}");
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> RemoveSubcontractorDeclarationClarificationFile(Guid applicationId, string userId, string userName, string fileName)
        {
            try
            {
                var response = await Post($"/Gateway/SubcontractorDeclarationClarification/{applicationId}/Remove", new { userId, userName, fileName });

                return response == HttpStatusCode.OK;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    $"Error when removing Subcontractor Declaration Clarification File for Application: {applicationId} | Filename: {fileName}");
                return false;
            }
        }

        public async Task<ApplicationOversightDetails> GetOversightDetails(Guid applicationId)
        {
            return await Get<ApplicationOversightDetails>($"/Oversights/{applicationId}");
        }
    }
}
