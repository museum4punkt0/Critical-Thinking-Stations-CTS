using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gemelo.Components.Cts.Database.Databases
{
    public class CtsDatabaseContext : DbContext
    {
        #region Felder und Eigenschaften

        public DbSet<CtsUser> CtsUsers { get; set; }

        public DbSet<StationConfiguration> StationConfigurations { get; set; }

        public DbSet<SurveyQuestionVersion> SurveyQuestionVersions { get; set; }

        SqlServerOptions SqlServerOptions { get; set; }

        public TimeSpan UserValidityExpireInterval { get; set; } = TimeSpan.FromHours(8.0);

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public CtsDatabaseContext() : this(new SqlServerOptions()) { }

        public CtsDatabaseContext(DbContextOptions<CtsDatabaseContext> options) : base(options) { }

        public CtsDatabaseContext(SqlServerOptions sqlServerOptions) { SqlServerOptions = sqlServerOptions; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (SqlServerOptions != null)
            {
                optionsBuilder.UseSqlServer(SqlServerOptions.GetConnectionString());

            }
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public async Task<string> GetStationConfigurationDetailsAsJson(string stationID)
        {
            return await StationConfigurations
                .Where(configuration => configuration.StationID == stationID)
                .Select(configuration => configuration.DetailsAsJson)
                .FirstOrDefaultAsync();
        }

        public async Task<CtsUser> GetOrCreateUserForRfid(string rfid, Func<int, string> createInitialDetails)
        {
            try
            {
                using var transaction = await Database.BeginTransactionAsync();
                CtsUser user = await CtsUsers
                    .Where(user => user.IsActive && user.Rfid == rfid)
                    .OrderByDescending(user => user.LastUpdateTime)
                    .FirstOrDefaultAsync();
                if (user != null && DateTime.Now > user.LastUpdateTime + UserValidityExpireInterval)
                {
                    await CtsUsers
                        .Where(user => user.IsActive && user.Rfid == rfid)
                        .ForEachAsync(user => user.IsActive = false);
                    await SaveChangesAsync();
                    user = null;
                }
                if (user == null)
                {
                    user = new CtsUser
                    {
                        Rfid = rfid,
                        IsActive = true
                    };
                    CtsUsers.Add(user);
                    await SaveChangesAsync();
                    user.DetailsAsJson = createInitialDetails(user.CtsUserID);
                    await SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                return user;
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: "Handled exception while trying to get or create user for rfid " +
                        $"'{rfid}': {exception.Message}",
                    arguments: $"rfid: {rfid}",
                    category: nameof(CtsDatabaseContext),
                    exception: exception);
                return null;
            }
        }

        public async Task<bool> UpdateUserDetails(int ctsUserID, Func<string, string> updateDetailsFunc)
        {
            try
            {
                using var transaction = await Database.BeginTransactionAsync();
                CtsUser user = await CtsUsers.FindAsync(ctsUserID);
                if (user == null) return false;
                else
                {
                    user.DetailsAsJson = updateDetailsFunc(user.DetailsAsJson);
                    user.LastUpdateTime = DateTime.Now;
                    await SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: "Handled exception while trying to update user details for user " +
                        $"'{ctsUserID}': {exception.Message}",
                    arguments: $"ctsUserID: {ctsUserID}",
                    category: nameof(CtsDatabaseContext),
                    exception: exception);
                return false;
            }
        }

        public async Task<bool> UpdateSurveyQuestions(
            IEnumerable<(string questionID, string detailsAsJson)> newQuestions)
        {
            try
            {
                using var transaction = await Database.BeginTransactionAsync();
                var activeQuesions = SurveyQuestionVersions.Where(q => q.IsActive).ToList();
                await SurveyQuestionVersions.Where(q => q.IsActive).ForEachAsync(q => q.IsActive = false);
                await SaveChangesAsync();
                foreach ((string questionID, string detailsAsJson) in newQuestions)
                {
                    SurveyQuestionVersion surveyQuestionVersion = activeQuesions
                        .FirstOrDefault(q => q.QuestionID == questionID);
                    if (surveyQuestionVersion == null || surveyQuestionVersion.DetailsAsJson != detailsAsJson)
                    {
                        surveyQuestionVersion = new SurveyQuestionVersion
                        {
                            QuestionID = questionID,
                            CreateTime = DateTime.Now,
                            DetailsAsJson = detailsAsJson
                        };
                        SurveyQuestionVersions.Add(surveyQuestionVersion);
                    }
                    surveyQuestionVersion.IsActive = true;
                    await SaveChangesAsync();
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception while trying to update survey questions: {exception.Message}",
                    category: nameof(CtsDatabaseContext),
                    exception: exception);
                return false;
            }
        }

        #endregion Öffentliche Methoden
    }
}
