namespace Nwazet.Go.Display.TouchScreen {
    public interface IClickable {
        int ID { get; set; }
        bool Active { get; set; }
        bool Clicked { get; set; }
        void DefineClickableArea(ScreenArea area);
        void OnClickEvent(TouchEvent touchEvent);
    }
}
