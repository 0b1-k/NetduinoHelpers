using System;
using System.Collections;

namespace netduino.helpers.Imaging {
    public class Composition {
        private readonly ArrayList _missiles = new ArrayList();
        private byte[] _frameCache;
        private int _frameCacheX;
        private int _frameCacheY;

        public Composition(Bitmap background) {
            Background = background;
        }

        public Composition(byte[] background, int width, int height)
            : this(new Bitmap(background, width, height)) {}

        public Bitmap Background { get; private set; }

        public PlayerMissile this[string name] {
            get {
                foreach (PlayerMissile missile in _missiles) {
                    if (missile.Name == name) return missile;
                }
                return null;
            }
        }

        public event CoincEventHandler Coinc;

        protected virtual bool OnCoinc(object sender, PlayerMissile missile1, PlayerMissile missile2) {
            if (Coinc != null) {
                Coinc(this, missile1, missile2);
            }
            return false;
        }

        public PlayerMissile AddMissile(
            string name,
            int x = 0,
            int y = 0) {
            var missile = new PlayerMissile(name, x, y, this);
            _missiles.Add(missile);
            ClearCache();
            return missile;
        }

        public PlayerMissile AddMissile(PlayerMissile missile) {
            if (missile == null) throw new ArgumentNullException("missile");
            _missiles.Add(missile);
            if (missile.Owner != this) missile.Owner = this;
            ClearCache();
            return missile;
        }

        private void ClearCache() {
            _frameCache = null;
        }

        public void RemoveMissile(string name) {
            foreach (PlayerMissile missile in _missiles) {
                if (missile.Name == name) _missiles.Remove(missile);
                missile.Owner = null;
            }
            ClearCache();
        }

        public byte[] GetFrame(int offsetX, int offsetY) {
            if (_frameCache != null && offsetX == _frameCacheX && offsetY == _frameCacheY) {
                return _frameCache;
            }

            CheckForCollisions();

            _frameCache = Background.GetFrame(offsetX, offsetY);
            _frameCacheX = offsetX;
            _frameCacheY = offsetY;

            foreach (PlayerMissile missile in _missiles) {
                if (!missile.IsVisible) continue;
                var relX = missile.X - offsetX;
                var relY = missile.Y - offsetY;
                if (relY < 0 || relX < 0 || relX >= Bitmap.FrameSize || relY >= Bitmap.FrameSize) continue;
                _frameCache[relY] |= Bitmap.ShiftMasks[relX];
            }
            
            return _frameCache;
        }

        private void CheckForCollisions() {
            for (var i = 0; i < _missiles.Count; i++) {
                var missile1 = (PlayerMissile)_missiles[i];
                if (missile1.IsEnemy || !missile1.IsVisible) continue;
                for (var j = 0; j < _missiles.Count; j++) {
                    var missile2 = (PlayerMissile)_missiles[j];
                    if (!missile2.IsEnemy || !missile2.IsVisible) continue;
                    if (missile1.X != missile2.X || missile1.Y != missile2.Y) continue;
                    if (OnCoinc(this, missile1, missile2)) return;
                }
            }
        }

        public void NotifyChange() {
            ClearCache();
        }
    }
}
