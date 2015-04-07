namespace Nwazet.Go.Fonts {
    public abstract class FontDefinition {
        abstract public FontInfo GetFontInfo();
        public string GetFontName() {
            var name = GetType().ToString();
            return name.Substring(name.LastIndexOf('.') + 1);
        }
    }
}
