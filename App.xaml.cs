using System.Globalization;
using System.Windows;

namespace SchedulingDesktopWGU
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var dict = new ResourceDictionary();

            switch (culture)
            {
                case "es":
                    dict.Source = new System.Uri("Resources/Strings.es.xaml", System.UriKind.Relative);
                    break;
                default:
                    dict.Source = new System.Uri("Resources/Strings.en.xaml", System.UriKind.Relative);
                    break;
            }

            Resources.MergedDictionaries.Add(dict);
        }
    }
}
