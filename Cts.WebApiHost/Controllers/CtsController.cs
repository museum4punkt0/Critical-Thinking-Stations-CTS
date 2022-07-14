using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Database.Databases;
using Gemelo.Components.Cts.Database.Models;
using Gemelo.Components.Cts.WebApiHost.Code;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Gemelo.Components.Cts.WebApiHost.Controllers
{
    [ApiController]
    public class CtsController : ControllerBase
    {
        private const string ConstMimeJson = "application/json";

        private CtsDatabaseContext m_Context;

        public CtsController(CtsDatabaseContext context)
        {
            m_Context = context;
        }

        [HttpGet]
        [Route("cts/hello")]
        public string Hello()
        {
            TraceX.WriteInformational("Hello World!");
            return "Hello World!";
        }

        [HttpGet]
        [Route("cts/ping")]
        public bool Ping()
        {
            return true;
        }

        [HttpGet]
        [Route("cts/GetTouchBeamerStationDefinition")]
        public async Task<string> GetTouchBeamerStationDefinition(string stationID)
        {
            return await m_Context.GetStationConfigurationDetailsAsJson(stationID);
        }

        [HttpGet]
        [Route("cts/GetDeepDiveStationDefinition")]
        public async Task<string> GetDeepDiveStationDefinition()
        {
            return await m_Context.GetStationConfigurationDetailsAsJson(DeepDiveStationDefinition.StationID);
        }

        [HttpGet]
        [Route("cts/GetWallStationDefinition")]
        public async Task<string> GetWallStationDefinition()
        {
            return await m_Context.GetStationConfigurationDetailsAsJson(WallStationDefinition.StationID);
        }

        [HttpGet]
        [Route("cts/GetUserForRfid")]
        public async Task<IActionResult> GetUserForRfid(string rfid, string language)
        {
            try
            {
                CtsUser databaseUser = await m_Context.GetOrCreateUserForRfid(rfid, id =>
                {
                    CtsUserInformation userInformation = new CtsUserInformation
                    {
                        CtsUserID = id,
                        Rfid = rfid,
                        Language = language
                    };
                    return userInformation.ToJson();
                });
                if (databaseUser?.DetailsAsJson == null)
                {
                    string message = $"Database returns no user or no user details for {nameof(GetUserForRfid)}()!";
                    TraceX.WriteWarning(
                        message: message,
                        arguments: $"rfid: {rfid}, language: {language}, databaseUser.ID: {databaseUser?.CtsUserID}",
                        category: nameof(CtsController));
                    return Content(new GetUserForRfidResult
                    {
                        IsSuccessful = false,
                        Message = message
                    }.ToJson(), ConstMimeJson);
                }
                if (CtsUserInformation.TryFromJson(databaseUser.DetailsAsJson, out CtsUserInformation result))
                {
                    return Content(new GetUserForRfidResult
                    {
                        IsSuccessful = true,
                        CtsUserInformation = result
                    }.ToJson(), ConstMimeJson);
                }
                else
                {
                    string message = $"Cannot parse user details at {nameof(GetUserForRfid)}()!";
                    TraceX.WriteWarning(
                        message: message,
                        arguments: $"rfid: {rfid}, language: {language}, databaseUser.ID: {databaseUser?.CtsUserID}",
                        category: nameof(CtsController));
                    return Content(new GetUserForRfidResult
                    {
                        IsSuccessful = false,
                        Message = message
                    }.ToJson(), ConstMimeJson);
                }
            }
            catch (Exception exception)
            {
                string message = $"Exception while trying to get CTS user for rfid: {rfid}: {exception.Message}!";
                TraceX.WriteWarning(
                    message: message,
                    arguments: $"rfid: {rfid}, language: {language}",
                    category: nameof(CtsController));
                return Content(new GetUserForRfidResult
                {
                    IsSuccessful = false,
                    Message = message
                }.ToJson(), ConstMimeJson);
            }
        }

        [HttpPost]
        [Route("cts/SaveAnswers")]
        public async Task<IActionResult> SaveAnswers(SaveAnswersRequest request)
        {
            try
            {
                bool result = await m_Context.UpdateUserDetails(request.CtsUserID, json =>
                {
                    return AddAnswersToUserDetails(
                        json: json,
                        request: request);
                });
                if (result)
                {
                    return Content(new SaveAnswersResult
                    {
                        IsSuccessful = true
                    }.ToJson(), ConstMimeJson);
                }
                else
                {
                    return Content(new SaveAnswersResult
                    {
                        IsSuccessful = false,
                        Message = "Cannot update user details"
                    }.ToJson(), ConstMimeJson);

                }
            }
            catch (Exception exception)
            {
                string message = $"Exception while trying to save answers for user: " +
                    $"{request?.CtsUserID}: {exception.Message}!";
                TraceX.WriteWarning(
                    message: message,
                    arguments: $"ctsUserID: {request?.CtsUserID}, questionID: {request?.QuestionID}",
                    category: nameof(CtsController));
                return Content(new SaveAnswersResult
                {
                    IsSuccessful = false,
                    Message = message
                }.ToJson(), ConstMimeJson);
            }
        }

        [HttpPost]
        [Route("cts/SaveDeepDiveVisit")]
        public async Task<IActionResult> SaveDeepDiveVisit(SaveDeepDiveVisitRequest request)
        {
            try
            {
                bool result = await m_Context.UpdateUserDetails(request.CtsUserID, json =>
                {
                    return AddDeepDiveVisitToUserDetails(
                        json: json,
                        request: request);
                });
                if (result)
                {
                    return Content(new SaveDeepDiveVisitResult
                    {
                        IsSuccessful = true
                    }.ToJson(), ConstMimeJson);
                }
                else
                {
                    return Content(new SaveDeepDiveVisitResult
                    {
                        IsSuccessful = false,
                        Message = "Cannot update user details"
                    }.ToJson(), ConstMimeJson);

                }
            }
            catch (Exception exception)
            {
                string message = $"Exception while saving deep dive visit for user: " +
                    $"{request?.CtsUserID}: {exception.Message}!";
                TraceX.WriteWarning(
                    message: message,
                    arguments: $"ctsUserID: {request?.CtsUserID}, questionID: {request?.QuestionID}",
                    category: nameof(CtsController));
                return Content(new SaveDeepDiveVisitResult
                {
                    IsSuccessful = false,
                    Message = message
                }.ToJson(), ConstMimeJson);
            }
        }

        [HttpPost]
        [Route("cts/ChangeUserState")]
        public async Task<IActionResult> ChangeUserState(ChangeUserStateRequest request)
        {
            try
            {
                bool result = await m_Context.UpdateUserDetails(request.CtsUserID, json =>
                {
                    return ChangeUserState(
                        json: json,
                        request: request);
                });
                if (result)
                {
                    return Content(new ChangeUserStateResult
                    {
                        IsSuccessful = true
                    }.ToJson(), ConstMimeJson);
                }
                else
                {
                    return Content(new ChangeUserStateResult
                    {
                        IsSuccessful = false,
                        Message = "Cannot update user state"
                    }.ToJson(), ConstMimeJson);

                }
            }
            catch (Exception exception)
            {
                string message = $"Exception while trying to update state for user: " +
                    $"{request?.CtsUserID}: {exception.Message}!";
                TraceX.WriteWarning(
                    message: message,
                    arguments: $"CtsUserID: {request?.CtsUserID}",
                    category: nameof(CtsController));
                return Content(new ChangeUserStateResult
                {
                    IsSuccessful = false,
                    Message = message
                }.ToJson(), ConstMimeJson);
            }
        }

        [HttpPost]
        [Route("cts/GetSurveyResultSet")]
        public async Task<IActionResult> GetSurveyResultSet(GetSurveyResultSetRequest request)
        {
            try
            {
                SurveyResultSet resultSet = await m_Context.GetSurveyResultSet(request.QuestionIDs);
                if (resultSet != null)
                {
                    return Content(new GetSurveyResultSetResult
                    {
                        IsSuccessful = true,
                        ResultSet = resultSet
                    }.ToJson(), ConstMimeJson);
                }
                else
                {
                    return Content(new GetSurveyResultSetResult
                    {
                        IsSuccessful = false,
                        Message = "Cannot fetch survey result set"
                    }.ToJson(), ConstMimeJson);

                }
            }
            catch (Exception exception)
            {
                string message = $"Exception while trying to fetch survey result set: {exception.Message}!";
                TraceX.WriteWarning(
                    message: message,
                    category: nameof(CtsController));
                return Content(new GetSurveyResultSetResult
                {
                    IsSuccessful = false,
                    Message = message
                }.ToJson(), ConstMimeJson);
            }
        }

        private string AddAnswersToUserDetails(string json, SaveAnswersRequest request)
        {
            CtsUserInformation userInformation = CtsUserInformation.FromJson(json);
            userInformation.ProcessSaveAnswersRequest(request);
            return userInformation.ToJson();
        }

        private string AddDeepDiveVisitToUserDetails(string json, SaveDeepDiveVisitRequest request)
        {
            CtsUserInformation userInformation = CtsUserInformation.FromJson(json);
            userInformation.ProcessSaveDeepDiveVisitRequest(request);
            return userInformation.ToJson();
        }

        private string ChangeUserState(string json, ChangeUserStateRequest request)
        {
            CtsUserInformation userInformation = CtsUserInformation.FromJson(json);
            userInformation.ProcessChangeUserState(request);
            return userInformation.ToJson();
        }
    }
}
