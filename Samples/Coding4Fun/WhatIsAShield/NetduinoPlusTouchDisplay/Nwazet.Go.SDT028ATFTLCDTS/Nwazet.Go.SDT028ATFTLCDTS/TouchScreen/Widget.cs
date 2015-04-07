using Nwazet.Go.Imaging;
namespace Nwazet.Go.Display.TouchScreen {
    public delegate void WidgetClickedHandler(VirtualCanvas canvas, Widget widget, TouchEvent touchEvent);
    public class Widget : IClickable {
        public int ID { get; set; }
        private bool _active;
        public bool Active {
            get {
                return _active;
            }
            set {
                _active = value;
                _clicked = false;
            }
        }
        private bool _clicked;
        public bool Clicked {
            get {
                return _clicked;
            }
            set {
                _clicked = value;
                if (_clicked) {
                    _active = false;
                }
            }
        }
        public bool Dirty { get; set; }
        public Widget() {
            Active = false;
            Dirty = true;
        }
        public ScreenArea Area;
        public void DefineClickableArea(ScreenArea area) {
            Active = true;
            Area = area;
        }
        public void OnClickEvent(TouchEvent touchEvent) {
            if (Active) {
                Clicked = Area.IsWithinArea(touchEvent.X, touchEvent.Y);
            }
        }
        public virtual void Draw(VirtualCanvas canvas) {
            if (!Dirty) return;
            if (Area.Radius == 0) {
                if (Clicked) {
                    canvas.DrawRectangleFilled(Area.X, Area.Y, Area.X + Area.Width - 1, Area.Y + Area.Height - 1, (ushort)BasicColor.Red);
                } else {
                    canvas.DrawRectangleFilled(Area.X, Area.Y, Area.X + Area.Width - 1, Area.Y + Area.Height - 1, (ushort)BasicColor.Black);
                }
            } else {
                if (Clicked) {
                    canvas.DrawCircleFilled(Area.X, Area.Y, Area.Radius, (ushort)BasicColor.Red);
                } else {
                    canvas.DrawCircleFilled(Area.X, Area.Y, Area.Radius, (ushort)BasicColor.Black);
                }
            }
            Dirty = false;
        }
    }
}
