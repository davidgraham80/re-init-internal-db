using System;
using System.IO;
using System.Linq;

namespace NavBox.Files
{
    public class Locator
    {
        private const string _settingsFile = "C:\\NavBoxCore\\NavBox.ini";
        private static string _root;

        public string NavboxDrive { get { return _root.Substring(0, 1); } }
        #region Folders
        public string Root { get { return _root; } }
        public string Core { get { return "C:\\NavBoxCore\\"; } }
        public string DataLayersSubPath { get { return "DataLayers\\"; } }
        public string Log { get { return Path.Combine(_root, "Log\\"); } }
        public string Weather { get { return Path.Combine(_root, "Weather\\"); } }
        public string TMSDataSubpath { get { return "TMS\\"; } }
        public string OSRSubPath { get { return "OSR\\"; } }
        public string OSRPath { get { return Path.Combine(_root, OSRSubPath); } }
        public string OSRParametersPath { get { return Path.Combine(OSRPath, "Parameters\\"); } }
        public string OSRClimatology { get { return Path.Combine(OSRPath, "Climatology\\"); } }
        public string OSRClimatologyPath { get { return Path.Combine(OSRClimatology, "Climatology.bin"); } }
        public string OSRModelPath { get { return Path.Combine(OSRPath, "Model\\"); } }
        public string OSRVoyagePath { get { return Path.Combine(OSRPath, "Voyage\\"); } }
        public string OSRNVIPath { get { return Path.Combine(OSRPath, "Nvi\\"); } }
        public string TMSData { get { return Path.Combine(_root, TMSDataSubpath); } }
        public string WeatherSubscriptionSubpath { get { return "WeatherSubscription\\"; } }
        public string WeatherSubscription { get { return Path.Combine(_root, WeatherSubscriptionSubpath); } }
        public string NavStationSubpath { get { return "NavStation\\"; } }
        public string NavStation { get { return Path.Combine(_root, NavStationSubpath); } }
        public string NavBoxSubpath { get { return "NavBox\\"; } }
        public string NavBox { get { return Path.Combine(_root, NavBoxSubpath); } }
        public string ConfigSubpath { get { return Path.Combine(NavBoxSubpath, "Config\\"); } }
        public string Config { get { return Path.Combine(_root, ConfigSubpath); } }
        public string Executables { get { return Path.Combine(NavBox, "Executables\\"); } }
        public string Messages { get { return Path.Combine(NavBox, "Messages\\"); } }
        public string LargeMessages { get { return Path.Combine(NavBox, "LargeMessages\\"); } }
        public string PassagePlansToSend { get { return Path.Combine(NavStation, "PassagePlansToSend\\"); } }
        public string ReportPath { get { return Path.Combine(NavStation, "Report\\"); } }
        public string Commands { get { return Path.Combine(NavBox, "Commands\\"); } }
        public string SoftwareDatabases { get { return Path.Combine(NavBox, "SoftwareDatabases\\"); } }
        public string Routes { get { return Path.Combine(NavStation, "Routes\\"); } }
        public string RoutesBin { get { return Path.Combine(NavStation, "RoutesBin\\"); } }
        public string RoutesDb { get { return Path.Combine(NavStation, "RoutesDb\\"); } }
        public string PassagePlans { get { return Path.Combine(NavStation, "PassagePlans\\"); } }
        public string VesselEventsPath { get { return Path.Combine(NavStation, "VesselEvents\\"); } }
        public string RoutesExchange { get { return Path.Combine(NavStation, "RoutesExchange\\"); } }
        public string ENCSubpath { get { return "ENC\\"; } }
        public string ENC { get { return Path.Combine(_root, ENCSubpath); } }
        public string SharedRoutesPath { get { return Path.Combine(ENC, "Routes\\"); } }
        public string DataExchangeRoot { get { return Path.Combine(_root, "DataExchange\\"); } }
        public string DataExchangeIn { get { return Path.Combine(DataExchangeRoot, "FromECDIS\\"); } }
        public string DataExchangeOut { get { return Path.Combine(DataExchangeRoot, "ToECDIS\\"); } }
        public string MailStorage { get { return Path.Combine(NavBox, "Mail\\"); } }
        public string Pcap { get { return Path.Combine(_root, "Pcap"); } }
        public string NavBoxUI { get { return Path.Combine(Executables, "NavBoxUI"); } }
        public string DataLayersPath { get { return Path.Combine(_root, DataLayersSubPath ); } }
        public string AdvisoryService { get { return Path.Combine( _root, "AdvisoryService" ); }}
        public string ComponentService { get { return Path.Combine(_root, "ComponentService"); } }
        public string MessageStoragePath { get { return Path.Combine(NavBox, "MessageStorage"); } }
        public string ComponentServiceBlobsPath { get { return Path.Combine(ComponentService, "Blobs"); } }
        public string AutoRouting { get { return Path.Combine(AdvisoryService, "AutoRouting"); }}
        public string AutoRoutingGraph { get { return Path.Combine(AutoRouting, "Graph.bin"); } }
        public string GreenLogsPath { get { return Path.Combine(_root, "GreenLogsApplication"); } }
        public string GreenLogsInstallerPath { get { return Path.Combine(_root, "GreenLogsApplication\\Installable\\GreenLogsInstaller"); }}
        public string GreenLogsApplicationPath { get { return Path.Combine(GreenLogsPath, "web"); } }
        #endregion

        #region Files
        public string GreenLogsInstallerFile { get { return Path.Combine(GreenLogsInstallerPath, "GreenLogsInstaller.exe"); } }
        public string GreenLogsVersionFile { get { return Path.Combine(GreenLogsApplicationPath, "release.json"); } }
        public string RouteDb3File { get { return Path.Combine(RoutesDb, "Routes.db3"); } }
        public string LogFile { get { return Path.Combine(Log, "Debug.log"); } }
        public string ConfigFileSubpath { get { return Path.Combine(ConfigSubpath, "NavBoxConfig.xml"); } }
        public string ConfigFileBackupSubpath { get { return Path.Combine(ConfigSubpath, "NavBoxConfigBackup.xml"); } }
        public string ConfigFileBackup { get { return Path.Combine(_root, ConfigFileBackupSubpath); } }
        public string ConfigFile { get { return Path.Combine(_root, ConfigFileSubpath); } }
        public string MailConfigFile { get { return Path.Combine(Config, "MailConfig.xml"); } }
        public string WorkerFile { get { return Path.Combine(Executables, "NavBoxWorker.dll"); } }
        public string EncDBGuard { get { return Path.Combine(ENC, "DBGuard"); } }
        public string CommandLog { get { return Path.Combine(Log, "Command.log"); } }
        public string WebDavLog { get { return Path.Combine(Log, "WebDav.log"); } }
        public string OSRSimulationParameters { get { return Path.Combine(OSRParametersPath, "simulationparameters.json"); } }
        public string OSRShipInfo { get { return Path.Combine(OSRParametersPath, "shipinfo.json"); } }
        public string ApiCredentials { get { return Path.Combine(Executables, "APICredentials"); } }
        public string RouteConverter { get { return Path.Combine(NavStation, "route_planning_converter.exe"); } }
        public string OSRShipinfoParameters { get { return Path.Combine(OSRParametersPath, "shipinfo.json"); } }
        public string ReportsCommandFile { get { return Path.Combine(ReportPath, "report.bin"); } }
        public string ActiveRoutePP
        {
            get { return Path.Combine(Executables, "ActivePassage.info"); }
        }
        public string AutomationDb { get { return Path.Combine(_root, @"Automation\Automation.db3"); } }
        public string DataLayersDb { get { return Path.Combine(DataLayersPath, "DataLayers.db3"); } }
        public string DataLayersCheckFile { get { return Path.Combine(DataLayersPath, "DataLayersCheck.bin"); } }
        public string AdvisoryServiceDb
        {
            get { return Path.Combine( AdvisoryService, "AdvisoryDatabase.db" ); }
        }
        public string ComponentServiceDb
        {
            get { return Path.Combine(ComponentService, "ComponentService.db3"); }
        }
        public string ComponentServiceLog { get { return Path.Combine(Log, "ComponentService.log"); } }
        public string Nauplius
        {
            get { return Path.Combine( Executables, "Nauplius.dll"); }
        }

        public string RestApi
        {
            get { return Path.Combine( Core, "API.exe" ); }
        }
        public string CommunicationBus
        {
#if DEBUG
            get { return @"..\..\..\CommunicationBus\bin\Debug\CommunicationBus.exe"; }
#else
            get { return Path.Combine( Core, "CommunicationBus.exe" ); }
#endif
        }
        public string AdvisoryServiceManager
        {
#if DEBUG
            get { return @"..\..\..\AdvisoryServiceManager\bin\Debug\AdvisoryServiceManager.exe"; }
#else
            get { return Path.Combine( Core, "AdvisoryServiceManager.exe"); }
#endif
        }
        public string ComponentServiceManager
        {
#if DEBUG
            get { return @"..\..\..\ComponentServiceManager\bin\Debug\ComponentServiceManager.exe"; }
#else
            get { return Path.Combine(Core, "ComponentServiceManager.exe"); }
#endif
        }

        public string MessageStorageDb
        {
            get { return Path.Combine(MessageStoragePath, "MessageStorage.db3"); }
        }

        public string PositionDatabase
        {
            get { return Path.Combine( _root, @"Positions\Track.db" ); }
        }
        #endregion

        public static readonly Locator Instance = new Locator();

        //Stop creating new instances, use the singleton pattern instead!
        //This constructor should be made private
        static Locator()
        {
            if (File.Exists(_settingsFile))
            {
                string[] settings = File.ReadAllLines(_settingsFile);
                if (settings.Length > 0)
                {
                    _root = settings[0];
                    return;
                }
            }
            _root = "C:\\Navtor";
        }

        public void SetAlternativeRootFolder(string newRootFolder)
        {
            _root = newRootFolder.Trim().TrimEnd('\\');
            if (File.Exists(_settingsFile))
                File.Delete(_settingsFile);
            File.WriteAllLines(_settingsFile, new[] { _root });
        }

        public string OSRVoyageParameters()
        {
            var reg = new System.Text.RegularExpressions.Regex(@"^[\d]+$");
            var files = Directory.GetFiles(OSRVoyagePath, "*.json").Where(f => reg.IsMatch(Path.GetFileNameWithoutExtension(f))).ToArray();
            Array.Reverse(files);
            if (files.Length > 0)
                return Path.Combine(OSRParametersPath, files[0]);

            return string.Empty;

        }

    }
}
