using Gemelo.Components.Common.Base;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.TouchBeamer.Code.Settings
{
    public class ViewportSettings
    {
        public int MarginLeft { get; set; }

        public int MarginTop { get; set; }

        public int MarginRight { get; set; }

        public int MarginBottom { get; set; }

        public static ViewportSettings From(Viewbox viewbox)
        {
            Thickness margin = viewbox.Margin;
            return new ViewportSettings
            {
                MarginLeft = MathEx.RoundToInt(margin.Left),
                MarginTop = MathEx.RoundToInt(margin.Top),
                MarginRight = MathEx.RoundToInt(margin.Right),
                MarginBottom = MathEx.RoundToInt(margin.Bottom),
            };
        }

        public static bool TryFromJsonFile(string filePath, out ViewportSettings result)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return TryFromJson(json, out result);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static bool TryFromJson(string json, out ViewportSettings result)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                result = JsonConvert.DeserializeObject<ViewportSettings>(value: json, settings: settings);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public void ApplyTo(Viewbox viewbox)
        {
            viewbox.Margin = new Thickness(
                left: MarginLeft,
                top: MarginTop,
                right: MarginRight,
                bottom: MarginBottom);
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
            };
            return JsonConvert.SerializeObject(value: this, formatting: Formatting.None, settings: settings);
        }

        public void ToJsonFile(string viewportFilePath)
        {
            string json = ToJson();
            File.WriteAllText(viewportFilePath, json);
        }
    }
}
