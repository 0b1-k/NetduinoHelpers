using Nwazet.Go.Fonts;
using Nwazet.Go.Imaging;
namespace Nwazet.Go.Display.TouchScreen {
    public class ButtonWidget : Widget {
        public string Text { get; set; }
        public FontInfo FontInfo { get; set; }
        public ushort BorderColor { get; set; }
        public ushort FillColor { get; set; }
        public ushort FillColorClicked { get; set; }
        public ushort FontColor { get; set; }
        public ushort FontColorClicked { get; set; }
        public RoundedCornerStyle CornerStyle { get; set; }

        public ButtonWidget() {
            DefineClickableArea(new ScreenArea(0, 0, 0, 0));
            InitializeLookAndFeel();
        }
        public ButtonWidget(int x, int y, int width, int height, FontInfo fontInfo, string text) {
            FontInfo = fontInfo;
            Text = text;
            DefineClickableArea(new ScreenArea(x, y, width - 1, height - 1));
            InitializeLookAndFeel();
        }
        protected virtual void InitializeLookAndFeel() {
            BorderColor = (ushort)BasicColor.Black;
            FillColor = (ushort)BasicColor.White;
            FillColorClicked = (ushort)BasicColor.Black;
            FontColor = (ushort)BasicColor.Black;
            FontColorClicked = (ushort)BasicColor.White;
            CornerStyle = RoundedCornerStyle.All;
            if (Text == null) {
                Text = "?";
            }
            if (FontInfo == null) {
                FontInfo = new DejaVuSans9().GetFontInfo();
            }
        }
        public override void Draw(Imaging.VirtualCanvas canvas) {
            if (!Dirty) return;
            if (Clicked) {
                canvas.DrawButton(
                    Area.X, Area.Y,
                    Area.Width, Area.Height,
                    FontInfo.ID, FontInfo.Height,
                    BorderColor, FillColorClicked, FontColorClicked, Text);
            } else {
                canvas.DrawButton(
                    Area.X, Area.Y,
                    Area.Width, Area.Height,
                    FontInfo.ID, FontInfo.Height,
                    BorderColor, FillColor, FontColor, Text);
            }
            Dirty = false;
        }
    }
}
