#if !DEPLOY
//using ComponentFactory.Krypton.Toolkit;
//using System.Drawing;
//using System.Windows;
//using System.Windows.Forms;

////Seb add
////https://github.com/File-New-Project/EarTrumpet/blob/master/EarTrumpet/Services/ThemeService.cs
//namespace ComponentFactory.Krypton.Toolkit
//{
//    public class ThemeService
//    {
//        public static bool IsWindowTransparencyEnabled
//        {
//            get { return !SystemInformation.HighContrast && UserSystemPreferencesService.IsTransparencyEnabled; }
//        }

//        //public static void UpdateThemeResources(ResourceDictionary dictionary)
//        //{
//        //    dictionary["WindowBackground"] = new SolidColorBrush(GetWindowBackgroundColor());

//        //    SetBrush(dictionary, "WindowForeground", "ImmersiveApplicationTextDarkTheme");
//        //    ReplaceBrush(dictionary, "CottonSwabSliderThumb", "ImmersiveSystemAccent");
//        //    ReplaceBrush(dictionary, "CottonSwabSliderTrackBackground", SystemParameters.HighContrast ? "ImmersiveSystemAccentLight1" : "ImmersiveControlDarkSliderTrackBackgroundRest");
//        //    SetBrushWithOpacity(dictionary, "CottonSwabSliderTrackBackgroundHover", SystemParameters.HighContrast ? "ImmersiveSystemAccentLight1" : "ImmersiveDarkBaseMediumHigh", SystemParameters.HighContrast ? 1.0 : 0.25);
//        //    SetBrush(dictionary, "CottonSwabSliderTrackBackgroundPressed", "ImmersiveControlDarkSliderTrackBackgroundRest");
//        //    ReplaceBrush(dictionary, "CottonSwabSliderTrackFill", "ImmersiveSystemAccentLight1");
//        //    SetBrush(dictionary, "CottonSwabSliderThumbHover", "ImmersiveControlDarkSliderThumbHover");
//        //    SetBrush(dictionary, "CottonSwabSliderThumbPressed", "ImmersiveControlDarkSliderThumbHover");
//        //}

//        public static Color GetWindowBackgroundColor()
//        {
//            string resource;
//            if (SystemInformation.HighContrast)
//            {
//                resource = "ImmersiveApplicationBackground";
//            }
//            else if (UserSystemPreferencesService.UseAccentColor)
//            {
//                resource = IsWindowTransparencyEnabled ? "ImmersiveSystemAccentDark2" : "ImmersiveSystemAccentDark1";
//            }
//            else
//            {
//                resource = "ImmersiveDarkChromeMedium";
//            }

//            var color = AccentColorService.GetColorByTypeName(resource);
//            //return color;
//            //color.A = (byte)(IsWindowTransparencyEnabled ? 190 : 255);
//            return Color.FromArgb(IsWindowTransparencyEnabled ? 190 : 255, color.R, color.G, color.B);
//        }

//        //private static void SetBrushWithOpacity(ResourceDictionary dictionary, string name, string immersiveAccentName, double opacity)
//        //{
//        //    var color = AccentColorService.GetColorByTypeName(immersiveAccentName);
//        //    color.A = (byte) (opacity*255);
//        //    ((SolidColorBrush) dictionary[name]).Color = color;
//        //}

//        //private static void SetBrush(ResourceDictionary dictionary, string name, string immersiveAccentName)
//        //{
//        //    SetBrushWithOpacity(dictionary, name, immersiveAccentName, 1.0);
//        //}

//        //private static void ReplaceBrush(ResourceDictionary dictionary, string name, string immersiveAccentName)
//        //{
//        //    dictionary[name] = new SolidColorBrush(AccentColorService.GetColorByTypeName(immersiveAccentName));
//        //}
//    }
//}
#endif