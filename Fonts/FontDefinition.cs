using System;
using System.Text;
using Microsoft.SPOT;
using netduino.helpers.Imaging;
using netduino.helpers.Helpers;
namespace netduino.helpers.Fonts {
    public abstract class FontDefinition {
        abstract public FontInfo GetFontInfo();
        public string GetFontName() {
            var name = GetType().ToString();
            return name.Substring(name.LastIndexOf('.') + 1);
        }
    }
}
