using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Text;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Dah.Code.IO;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class CtsCommunicationClient
    {
        #region Konstanten

        public static readonly string DefaultServerAddressFormat = "http://{0}:8536";

        private const string ConstUrlPing = "cts/ping";
        private const string ConstUrlFormatGetTouchBeamerStationDefinition =
            "cts/GetTouchBeamerStationDefinition?stationID={0}";
        private const string ConstUrlGetDeepDiveStationDefinition = "cts/GetDeepDiveStationDefinition";
        private const string ConstUrlGetWallStationDefinition = "cts/GetWallStationDefinition";
        private const string ConstUrlFormatGetUserForRfid =
            "cts/GetUserForRfid?rfid={0}&language={1}";
        private const string ConstUrlSaveAnswers = "cts/SaveAnswers";
        private const string ConstUrlSaveDeepDiveVisit = "cts/SaveDeepDiveVisit";
        private const string ConstUrlChangeUserState = "cts/ChangeUserState";
        private const string ConstUrlGetMediaFilesInformation = "media/GetMediaFilesInformation";
        private const string ConstUrlDownloadMediaFile = "media/DownloadMediaFile";
        private const string ConstUrlGetSurveyResultSet = "cts/GetSurveyResultSet";

        #endregion Konstanten

        public Uri ServerBaseAddress { get; set; } = new("http://localhost:8536");

        private readonly HttpClient m_HttpClient = new();

        private bool? m_LastCheckConnectionResult = null;

        public async Task<bool> CheckConnection()
        {
            Uri uri = new(ServerBaseAddress, ConstUrlPing);
            try
            {
                var result = await m_HttpClient.GetAsync(uri);
                if (result.IsSuccessStatusCode)
                {
                    if (m_LastCheckConnectionResult != true)
                    {
                        TraceX.WriteInformational(
                            message: $"Checked connection successful!",
                            category: nameof(CtsCommunicationClient));
                    }
                    m_LastCheckConnectionResult = true;
                    return true;
                }
                else
                {
                    if (m_LastCheckConnectionResult != false)
                    {
                        TraceX.WriteWarning(
                            message: $"Check connection failed with HTTP status: " +
                                $"{result.StatusCode} {result.ReasonPhrase}",
                            category: nameof(CtsCommunicationClient));
                    }
                    m_LastCheckConnectionResult = false;
                    return false;
                }
            }
            catch (Exception exception)
            {
                if (m_LastCheckConnectionResult != false)
                {
                    TraceX.WriteWarning(
                        message: $"Check connection failed with exception: {exception.Message}",
                        exception: exception,
                        category: nameof(CtsCommunicationClient));
                }
                m_LastCheckConnectionResult = false;
                return false;
            }
        }

        public async Task<TouchBeamerStationDefinition> GetTouchBeamerStationDefinition(string stationID)
        {
            Uri uri = new Uri(ServerBaseAddress,
                string.Format(ConstUrlFormatGetTouchBeamerStationDefinition, HttpUtility.UrlEncode(stationID)));
            string json = await m_HttpClient.GetStringAsync(uri);
            return TouchBeamerStationDefinition.FromJson(json);
        }

        public async Task<DeepDiveStationDefinition> GetDeepDiveStationDefinition()
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlGetDeepDiveStationDefinition);
            string json = await m_HttpClient.GetStringAsync(uri);
            return DeepDiveStationDefinition.FromJson(json);
        }

        public async Task<WallStationDefinition> GetWallStationDefinition()
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlGetWallStationDefinition);
            string json = await m_HttpClient.GetStringAsync(uri);
            return WallStationDefinition.FromJson(json);
        }

        public async Task<CtsUserInformation> GetUserForRfid(RfidUserInformation rfid)
        {
            string relativeUri = string.Format(ConstUrlFormatGetUserForRfid,
                HttpUtility.UrlEncode(rfid.ID), HttpUtility.UrlEncode(rfid.Language ?? Languages.Default));
            Uri uri = new Uri(ServerBaseAddress, relativeUri);
            string json = await m_HttpClient.GetStringAsync(uri);
            if (json.IsNotNullOrEmpty() && GetUserForRfidResult.TryFromJson(json, out GetUserForRfidResult result))
            {
                if (result.IsSuccessful) return result.CtsUserInformation;
                else
                {
                    TraceX.WriteWarning(
                        message: $"{nameof(GetUserForRfid)}() was not successful: message='{result.Message}'",
                        arguments: $"rfid: {rfid.ID}, language: {rfid.Language}",
                        category: nameof(CtsCommunicationClient));
                    return null;
                }
            }
            else
            {
                TraceX.WriteWarning(
                    message: $"{nameof(GetUserForRfid)}() was not successful: no json result!",
                    arguments: $"rfid: {rfid.ID}, language: {rfid.Language}",
                    category: nameof(CtsCommunicationClient));
                return null;
            }
        }

        public async Task<SaveAnswersResult> SaveAnswers(SaveAnswersRequest request)
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlSaveAnswers);
            string requestJson = JsonConvert.SerializeObject(request);
            HttpContent httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var httpResponse = await m_HttpClient.PostAsync(uri, httpContent);
            if (httpResponse.IsSuccessStatusCode && httpResponse.Content != null)
            {
                string responseJson = await httpResponse.Content.ReadAsStringAsync();
                if (responseJson.IsNotNullOrEmpty() &&
                    SaveAnswersResult.TryFromJson(responseJson, out SaveAnswersResult result))
                {
                    return result;
                }
                else
                {
                    TraceX.WriteWarning(
                        message: $"{nameof(SaveAnswers)}() was not successful: no json result!",
                        arguments: $"ctsUserID: {request?.CtsUserID}, questionID: {request?.QuestionID}",
                        category: nameof(CtsCommunicationClient));
                    return null;
                }
            }
            else
            {
                TraceX.WriteWarning(
                    message: $"{nameof(SaveAnswers)}() was not successful: no successful http result or " +
                        $"empty content: {httpResponse.StatusCode}!",
                    arguments: $"ctsUserID: {request?.CtsUserID}, questionID: {request?.QuestionID}",
                    category: nameof(CtsCommunicationClient));
                return null;
            }
        }

        public async Task<SaveDeepDiveVisitResult> SaveDeepDiveVisit(SaveDeepDiveVisitRequest request)
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlSaveDeepDiveVisit);
            string requestJson = JsonConvert.SerializeObject(request);
            HttpContent httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var httpResponse = await m_HttpClient.PostAsync(uri, httpContent);
            if (httpResponse.IsSuccessStatusCode && httpResponse.Content != null)
            {
                string responseJson = await httpResponse.Content.ReadAsStringAsync();
                if (responseJson.IsNotNullOrEmpty() &&
                    SaveDeepDiveVisitResult.TryFromJson(responseJson, out SaveDeepDiveVisitResult result))
                {
                    return result;
                }
                else
                {
                    TraceX.WriteWarning(
                        message: $"{nameof(SaveDeepDiveVisit)}() was not successful: no json result!",
                        arguments: $"ctsUserID: {request?.CtsUserID}, questionID: {request?.QuestionID}",
                        category: nameof(CtsCommunicationClient));
                    return null;
                }
            }
            else
            {
                TraceX.WriteWarning(
                    message: $"{nameof(SaveDeepDiveVisit)}() was not successful: no successful http result or " +
                        $"empty content: {httpResponse.StatusCode}!",
                    arguments: $"ctsUserID: {request?.CtsUserID}, questionID: {request?.QuestionID}",
                    category: nameof(CtsCommunicationClient));
                return null;
            }
        }

        public async Task<ChangeUserStateResult> ChangeUserState(ChangeUserStateRequest request)
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlChangeUserState);
            string requestJson = JsonConvert.SerializeObject(request);
            HttpContent httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var httpResponse = await m_HttpClient.PostAsync(uri, httpContent);
            if (httpResponse.IsSuccessStatusCode && httpResponse.Content != null)
            {
                string responseJson = await httpResponse.Content.ReadAsStringAsync();
                if (responseJson.IsNotNullOrEmpty() &&
                    ChangeUserStateResult.TryFromJson(responseJson, out ChangeUserStateResult result))
                {
                    return result;
                }
                else
                {
                    TraceX.WriteWarning(
                        message: $"{nameof(ChangeUserState)}() was not successful: no json result!",
                        arguments: $"CtsUserID: {request?.CtsUserID}",
                        category: nameof(CtsCommunicationClient));
                    return null;
                }
            }
            else
            {
                TraceX.WriteWarning(
                    message: $"{nameof(ChangeUserState)}() was not successful: no successful http result or " +
                        $"empty content: {httpResponse.StatusCode}!",
                    arguments: $"CtsUserID: {request?.CtsUserID}",
                    category: nameof(CtsCommunicationClient));
                return null;
            }
        }

        public async Task<GetMediaFilesInformationResult> GetMediaFilesInformation(
            GetMediaFilesInformationRequest request)
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlGetMediaFilesInformation);
            string requestJson = JsonConvert.SerializeObject(request);
            HttpContent httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var httpResponse = await m_HttpClient.PostAsync(uri, httpContent);
            if (httpResponse.IsSuccessStatusCode && httpResponse.Content != null)
            {
                string responseJson = await httpResponse.Content.ReadAsStringAsync();
                if (responseJson.IsNotNullOrEmpty() && GetMediaFilesInformationResult.TryFromJson(
                    responseJson, out GetMediaFilesInformationResult result))
                {
                    return result;
                }
                else
                {
                    string message = $"{nameof(GetMediaFilesInformation)}() was not successful: no json result!";
                    TraceX.WriteWarning(
                        message: message,
                        arguments: $"filename: {request?.Filename}, questionID: {request?.QuestionID}",
                        category: nameof(CtsCommunicationClient));
                    return new GetMediaFilesInformationResult
                    {
                        IsSuccessful = false,
                        Message = message
                    };
                }
            }
            else
            {
                string message = $"{nameof(GetMediaFilesInformation)}() was not successful: no successful http result or" +
                        $" empty content: {httpResponse.StatusCode}!";
                TraceX.WriteWarning(
                    message: message,
                    arguments: $"filename: {request?.Filename}, questionID: {request?.QuestionID}",
                    category: nameof(CtsCommunicationClient));
                return new GetMediaFilesInformationResult
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        public async Task DownloadMediaFile(DownloadMediaFileRequest request, string destinationFilePath)
        {
            Uri uri = new Uri(ServerBaseAddress, ConstUrlDownloadMediaFile);
            string requestJson = JsonConvert.SerializeObject(request);
            HttpContent httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var httpResponse = await m_HttpClient.PostAsync(uri, httpContent);
            if (httpResponse.IsSuccessStatusCode && httpResponse.Content != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
                Stream stream = await httpResponse.Content.ReadAsStreamAsync();
                using FileStream file = File.Create(destinationFilePath);
                await stream.CopyToAsync(file);
            }
            else
            {
                TraceX.WriteWarning(
                    message: $"{nameof(DownloadMediaFile)}() was not successful: no successful http result or" +
                        $" empty content: {httpResponse.StatusCode}!",
                    arguments: $"filename: {request?.Filename}, questionID: {request?.QuestionID}",
                    category: nameof(CtsCommunicationClient));
            }
        }

        public async Task<GetSurveyResultSetResult> GetSurveyResultSet(GetSurveyResultSetRequest request)
        {
            try
            {
                Uri uri = new Uri(ServerBaseAddress, ConstUrlGetSurveyResultSet);
                string requestJson = JsonConvert.SerializeObject(request);
                HttpContent httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var httpResponse = await m_HttpClient.PostAsync(uri, httpContent);
                if (httpResponse.IsSuccessStatusCode && httpResponse.Content != null)
                {
                    string responseJson = await httpResponse.Content.ReadAsStringAsync();
                    if (responseJson.IsNotNullOrEmpty() && GetSurveyResultSetResult.TryFromJson(
                        responseJson, out GetSurveyResultSetResult result))
                    {
                        return result;
                    }
                    else
                    {
                        string message = $"{nameof(GetSurveyResultSet)}() was not successful: no json result!";
                        TraceX.WriteWarning(
                            message: message,
                            category: nameof(CtsCommunicationClient));
                        return new GetSurveyResultSetResult
                        {
                            IsSuccessful = false,
                            Message = message
                        };
                    }
                }
                else
                {
                    string message = $"{nameof(GetSurveyResultSet)}() was not successful: no successful http result or" +
                            $" empty content: {httpResponse.StatusCode}!";
                    TraceX.WriteWarning(
                        message: message,
                        category: nameof(CtsCommunicationClient));
                    return new GetSurveyResultSetResult
                    {
                        IsSuccessful = false,
                        Message = message
                    };
                }
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception in {nameof(GetSurveyResultSet)}(): {exception.Message}",
                    category: nameof(CtsCommunicationClient),
                    exception: exception);
                return new GetSurveyResultSetResult
                {
                    IsSuccessful = false,
                    Message = exception.Message
                };
            }
        }
    }
}
