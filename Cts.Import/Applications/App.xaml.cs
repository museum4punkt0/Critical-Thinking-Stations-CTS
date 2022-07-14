using Gemelo.Components.Cms.Data.Base;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Cts.Applications;
using Gemelo.Components.Cts.Database.Databases;
using System.Threading.Tasks;

namespace Gemelo.Applications.Cts.Import
{
    /// <summary>
    /// Applikation CTS Import
    /// </summary>
    public partial class App : CtsApplication
    {
        #region Felder und Eigenschaften

        public static new App Current => CtsApplication.Current as App;

        public override bool UseRfid => false;

        public override string StationID { get; protected set; } = "Import";

        public override string CmsStationName => "Cts.Import";

        protected override CmsStationDefinition CmsStationDefinition => null;

        public SqlServerOptions SqlServerOptions = new SqlServerOptions();

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public App()
        {
            Settings.AdditionalStartArgumentHandlers.Add(new StartArgumentHandler
            {
                MatchingKey = "database",
                NeedsValue = true,
                Description = "Sets the database server",
                ValuePlaceholder = "database-server",
                ValueAction = value => SqlServerOptions = new SqlServerOptions { Server = value }
            });
            Flags.UseBorderedWindows = true;
        }

        protected override Task UpdateStationDefinitionInternally() => Task.CompletedTask;

        #endregion Konstruktor und Initialisierung
    }
}
