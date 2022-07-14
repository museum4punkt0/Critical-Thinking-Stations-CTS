using Gemelo.Applications.Cts.Import.Code;
using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Wpf.Controls.Logos;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Database.Databases;
using Gemelo.Components.Cts.Database.Models;
using Gemelo.Components.Cts.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Gemelo.Applications.Cts.Import
{
    /// <summary>
    /// Hauptfenster vom Import für CTS
    /// </summary>
    public partial class MainWindow : CtsMainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            m_LogoGemelo.Logo = AnimatedLogoDefinition.GemeloLogo;
            m_LogoGemelo.ShowUp();
        }

        public override void TryRestartRestartTimer() { }

        private static void AskForImport()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select Excel file ...",
                Filter = "Excel file (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    DoImport(dlg.FileName);
                    MessageBox.Show("Completed.");
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Error occured: {exception.Message}");
                }
            }
        }

        private static void DoImport(string filePath)
        {
            CtsImportExcelReader reader = new CtsImportExcelReader();
            reader.Open(new FileInfo(filePath));
            CtsDatabaseContext context = new CtsDatabaseContext(App.Current.SqlServerOptions);
            SaveSurveyQuestions(reader, context);
            SaveTouchBeamerStationDefinitionsInDatabbase(reader, context);
            SaveDeepDiveStationDefinitionInDatabase(reader, context);
            SaveWallStationDefinitionInDatabase(reader, context);
            context.SaveChanges();
        }

        private static void AskForClearAllDemoUserData()
        {
            if (MessageBox.Show(
                messageBoxText: "Should all demo users' data really be deleted?",
                caption: "Clear demo data",
                button: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DoClearDemoUserData();
                MessageBox.Show("Completed.");
            }
        }

        private static void DoClearDemoUserData()
        {
            CtsDatabaseContext context = new CtsDatabaseContext(App.Current.SqlServerOptions);
            var demoUsers = context.CtsUsers.Where(user => user.Rfid.StartsWith("demo"));
            context.CtsUsers.RemoveRange(demoUsers);
            context.SaveChanges();
        }

        private void AskForClearAllUserData()
        {
            if (MessageBox.Show(
                messageBoxText: "Should ALL (!!!!) users' data really be deleted?",
                caption: "Clear all user data",
                button: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (MessageBox.Show(
                    messageBoxText: "Are you really sure? Should REALLY ALL (!!!!) users' data be deleted?",
                    caption: "Clear all user data",
                    button: MessageBoxButton.YesNoCancel,
                    icon: MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DoClearAllUserData();
                    MessageBox.Show("Completed.");
                }
            }
        }

        private static void DoClearAllUserData()
        {
            CtsDatabaseContext context = new CtsDatabaseContext(App.Current.SqlServerOptions);
            var allUsers = context.CtsUsers.ToArray();
            context.CtsUsers.RemoveRange(allUsers);
            context.SaveChanges();
        }

        private static async void SaveSurveyQuestions(CtsImportExcelReader reader, CtsDatabaseContext context)
        {
            List<(string questionID, string detailsAsJson)> questions = new();
            reader.Questions.Keys.ForEach(stationID =>
            {
                questions.AddRange(reader.Questions[stationID].Select(q => (q.QuestionID, q.ToJson())));
            });
            if (!await context.UpdateSurveyQuestions(questions))
            {
                throw new InvalidOperationException("Questions cannot be save to database!");
            }
        }

        private static void SaveTouchBeamerStationDefinitionsInDatabbase(CtsImportExcelReader reader,
            CtsDatabaseContext context)
        {
            foreach (string stationID in reader.TouchBeamerStationDefinitions.Keys)
            {
                var stationConfiguration = context.StationConfigurations
                    .Where(config => config.StationID == stationID)
                    .FirstOrDefault();
                if (stationConfiguration == null)
                {
                    stationConfiguration = new StationConfiguration
                    {
                        StationID = stationID
                    };
                    context.StationConfigurations.Add(stationConfiguration);
                }
                stationConfiguration.DetailsAsJson = reader.TouchBeamerStationDefinitions[stationID].ToJson();
            }
        }

        private static void SaveDeepDiveStationDefinitionInDatabase(CtsImportExcelReader reader,
            CtsDatabaseContext context)
        {
            var stationConfiguration = context.StationConfigurations
                .Where(config => config.StationID == DeepDiveStationDefinition.StationID)
                .FirstOrDefault();
            if (stationConfiguration == null)
            {
                stationConfiguration = new StationConfiguration
                {
                    StationID = DeepDiveStationDefinition.StationID
                };
                context.StationConfigurations.Add(stationConfiguration);
            }
            stationConfiguration.DetailsAsJson = reader.DeepDiveStationDefinition.ToJson();
        }

        private static void SaveWallStationDefinitionInDatabase(CtsImportExcelReader reader,
            CtsDatabaseContext context)
        {
            var stationConfiguration = context.StationConfigurations
                .Where(config => config.StationID == WallStationDefinition.StationID)
                .FirstOrDefault();
            if (stationConfiguration == null)
            {
                stationConfiguration = new StationConfiguration
                {
                    StationID = WallStationDefinition.StationID
                };
                context.StationConfigurations.Add(stationConfiguration);
            }
            stationConfiguration.DetailsAsJson = reader.WallStationDefinition.ToJson();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            AskForImport();
        }

        private void BtnClearDemoUserData_Click(object sender, RoutedEventArgs e)
        {
            AskForClearAllDemoUserData();
            //string deepDiveFilePath = Directories.GetPathInApplicationDirectory(@"Data\DeepDive.json");
            //string json = File.ReadAllText(deepDiveFilePath);
            //ExportStationConfigurationAsCsv(DeepDiveStationDefinition.FromJson(json));
        }

        private void BtnClearAllUserData_Click(object sender, RoutedEventArgs e)
        {
            AskForClearAllUserData();
        }

        //private void ExportStationConfigurationAsCsv(object stationConfiguration)
        //{
        //    StringBuilder export = new StringBuilder();
        //    Type configType = stationConfiguration.GetType();
        //    foreach (var propertyInfo in configType.GetProperties())
        //    {
        //        if (propertyInfo.PropertyType == typeof(LocalizationString))
        //        {
        //            LocalizationString value = propertyInfo.GetValue(stationConfiguration) as LocalizationString;
        //            export.AppendLine($"\"{propertyInfo.Name}\";\"{value?.GetFor(Languages.German)}\";\"{value?.GetFor(Languages.English)}\"");
        //        }
        //        else export.AppendLine($"\"{propertyInfo.Name}\";\"{propertyInfo.GetValue(stationConfiguration)}\";\"\"");
        //    }
        //    File.WriteAllText(path: @"c:\temp\export.csv", contents: export.ToString(), encoding: Encoding.UTF8);
        //}
    }
}
