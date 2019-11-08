using System.Collections.Generic;
using System.Linq;
using PX.TaxProvider;

namespace Monster.TaxProvider
{
    public class ProviderSettings
    {
        public const string SETTING_EXTTAXREPORTER = "EXTTAXREPORTER";
        public const string SETTING_EXTERNALTAXID = "EXTERNALTAXID";
        public const string SETTING_LOGLEVEL = "LOGLEVEL";


        public static List<ITaxProviderSetting> Defaults =>
            new List<ITaxProviderSetting>()
            {
                new TaxProviderSetting(
                    LogicAutomatedTaxProvider.TaxProviderID,
                    SETTING_EXTTAXREPORTER,
                    1,
                    "External Tax Reporting Provider",
                    string.Empty,
                    TaxProviderSettingControlType.Text),

                new TaxProviderSetting(
                    LogicAutomatedTaxProvider.TaxProviderID,
                    SETTING_EXTERNALTAXID,
                    1,
                    "External Tax ID (in Acumatica)",
                    string.Empty,
                    TaxProviderSettingControlType.Text),

                new TaxProviderSetting(
                    LogicAutomatedTaxProvider.TaxProviderID,
                    SETTING_LOGLEVEL,
                    1,
                    "Logging Level",
                    string.Empty,
                    TaxProviderSettingControlType.Text),
            };
    }

    public static class ProviderSettingsExtensions
    {
        public static ITaxProviderSetting Setting(this List<ITaxProviderSetting> settings, string settingId)
        {
            return settings.FirstOrDefault(x => x.SettingID == settingId);
        }

        public static bool DebugLoggingEnabled(this List<ITaxProviderSetting> settings)
        {
            var setting = settings.Setting(ProviderSettings.SETTING_LOGLEVEL);
            return setting != null && setting.Value.ToUpper() == "DEBUG_ENABLED";
        }
    }
}
