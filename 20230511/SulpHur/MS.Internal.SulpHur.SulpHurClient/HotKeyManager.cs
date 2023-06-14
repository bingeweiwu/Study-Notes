using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public sealed class HotKeyManager : IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (KeyPressed != null)
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }
        /// <summary>
        /// Store registered hotkey\hanlder pair
        /// </summary>
        private class HotKey
        {
            public int Id { get; set; }
            public ModifierKeys Modifier { get; set; }
            public Keys Key { get; set; }
            public EventHandler<KeyPressedEventArgs> Handler{ get; set; }

            public HotKey(int id, ModifierKeys modifier, Keys key, EventHandler<KeyPressedEventArgs> handler)
            {
                this.Id = id;
                this.Modifier = modifier;
                this.Key = key;
                this.Handler = handler;
            }
        }

        // static variable
        private static HotKeyManager instance;
        // member
        private Window _window = new Window();
        private int _currentId;
        private List<HotKey> hotKeyList = new List<HotKey>();
        private bool isDisposed = false;

        public static HotKeyManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HotKeyManager();
                }
                return instance;
            }
        }
        private HotKeyManager()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate(object sender, KeyPressedEventArgs args)
            {
                List<HotKey> triggeredHotKeyList = hotKeyList.FindAll(item => item.Key == args.Key && item.Modifier == args.Modifier);
                foreach (var hotKey in triggeredHotKeyList)
                {
                    hotKey.Handler(sender, args);
                }

                // notification event
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                throw new InvalidOperationException("Couldn't register the hot key.");
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        /// <param name="handler">Handler for the registered hotkey.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key, EventHandler<KeyPressedEventArgs> handler)
        {
            RegisterHotKey(modifier, key);

            // add to hotkey list
            hotKeyList.Add(new HotKey(_currentId, modifier, key, handler));
        }

        ~HotKeyManager()
        {
            this.Dispose(false);
        }
        #region IDisposable Members
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    hotKeyList.Clear();
                    hotKeyList = null;
                }

                // unregister all the registered hot keys.
                for (int i = _currentId; i > 0; i--)
                {
                    UnregisterHotKey(_window.Handle, i);
                }
                // dispose the inner native window.
                _window.Dispose();
                instance = null;

                this.isDisposed = true;
            }
        }
        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Keys _key;

        public KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

}
