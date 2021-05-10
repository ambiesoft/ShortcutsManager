using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace Ambiesoft.ShortcutsManager
{
    class ComboItem 
    {
        private string _name;
        public ComboItem(string name)
        {
            _name = name;
        }
        public ComboItem(Keys k)
        {
            _name = (string)keyConverter.ConvertTo(k, typeof(string));
        }

        private static KeysConverter keyConverter = new KeysConverter();
        public Keys KeyCode
        {
            get
            {
                return (Keys)keyConverter.ConvertFrom(_name);
            }
        }
        public static bool IsAvailable(Keys k)
        {
            return !string.IsNullOrEmpty(keyConverter.ConvertToString(k));
        }
        public override string ToString()
        {
            return _name;
        }



        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to ThreeDPoint return false:
            ComboItem p = obj as ComboItem;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return _name.Equals(p._name);
        }

        public bool Equals(ComboItem p)
        {
            // Return true if the fields match:
            return _name.Equals(p._name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _name.GetHashCode();
        }


    }
}
