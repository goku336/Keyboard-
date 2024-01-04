using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardPOS
{
    public partial class FrmKeyboardPOS : Form
    {

        HashSet<Keys> onScreenKeys = new HashSet<Keys>(
    Enumerable.Range('A', 'Z' - 'A' + 1).Select(c => (Keys)c)
    .Concat(Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (Keys)c))
    .Concat(Enumerable.Range('0', '9' - '0' + 1).Select(c => (Keys)c))
    .Concat(new Keys[] { Keys.Enter, Keys.Back, Keys.CapsLock,Keys.Space,Keys.Tab,
                Keys.Oemcomma, Keys.OemPeriod, Keys.OemQuestion, Keys.Multiply,
                Keys.Subtract, Keys.Add ,Keys.Oem1,Keys.Oem102,Keys.Oem2,Keys.Oem2,Keys.Oem3,Keys.Oem4,Keys.Oem5,Keys.Oem6,Keys.Oem7,Keys.Oem8}) // Add your custom keys here
                );

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int VK_CAPITAL = 0x14;

        private bool capsLockEnabled = Control.IsKeyLocked(Keys.CapsLock);

        public FrmKeyboardPOS()
        {
            InitializeComponent();
            this.ActiveControl = txtValue;
            UpdateOnScreenKeyboardAppearance();
            ////this.KeyUp += FrmKeyboardPOS_KeyUp;
            ////this.KeyDown += FrmKeyboardPOS_KeyDown;


        }
        private void ToggleCapsLock()
        {           
            const uint KEYEVENTF_KEYDOWN = 0x0000;

            keybd_event(VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);        
        }

        private void BackToTextboxFocus()
        {
            txtValue.Focus();
            txtValue.SelectionStart = txtValue.Text.Length;
            txtValue.SelectionLength = 0;
        }

        private void SetOnScreenKeyboardCapsLockState(bool newState)
        {
            SendKeys.Send(newState ? "{CAPSLOCK}" : "+{CAPSLOCK}");
            // Send the appropriate simulated keystroke to the on-screen keyboard        
        }

        private void BtnCharacter_Click(object sender, EventArgs e)
        {
            SimpleButton btn = sender as SimpleButton;
            capsLockEnabled = Control.IsKeyLocked(Keys.CapsLock);
            txtValue.Text += capsLockEnabled ? btn.Text.ToUpper() : btn.Text.ToLower();
            BackToTextboxFocus();
        }

        private void BtnSpace_Click(object sender, EventArgs e)
        {
            txtValue.Text = txtValue.Text + " ";
            BackToTextboxFocus();
        }

        private void BtnBackspace_click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtValue.Text))
            {
                txtValue.Text = txtValue.Text.Substring(0, txtValue.Text.Length - 1);
                BackToTextboxFocus();
            }
        }

        private void BtnCapsLock_Click(object sender, EventArgs e)
        {
            capsLockEnabled = !capsLockEnabled;
            ToggleCapsLock();
            UpdateOnScreenKeyboardAppearance();
            // capsLockEnabled = Control.IsKeyLocked(Keys.CapsLock );
            SetOnScreenKeyboardCapsLockState(capsLockEnabled);
            BackToTextboxFocus();
        }

        private void UpdateOnScreenKeyboardAppearance()
        {
            BtnCapsLock.BackColor = capsLockEnabled ? Color.Orange : DefaultBackColor;
            BtnCapsLock.ForeColor = capsLockEnabled ? Color.White : DefaultForeColor;
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            txtValue.Text += Environment.NewLine;
            BackToTextboxFocus();
        }

        private void FrmKeyboardPOS_Load(object sender, EventArgs e)
        {
            //capsLockEnabled = Control.IsKeyLocked(Keys.CapsLock);
            UpdateOnScreenKeyboardAppearance();
            SetOnScreenKeyboardCapsLockState(capsLockEnabled);
            txtValue.Focus();
        }

        private bool IsOnScreenKey(Keys key)
        {
            return onScreenKeys.Contains(key);      
        }

        private void TxtValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Prevent physical keyboard keys from appearing in the text box
            // e.Handled = true;

            // Handle special keys that affect both physical and on-screen keyboards
            //if (IsOnScreenKey((Keys)e.KeyChar))
            //{
            //    char character = e.KeyChar;
            //    txtValue.Text += capsLockEnabled ? char.ToUpper(character) : char.ToLower(character);
            //    BackToTextboxFocus();
            //}
            capsLockEnabled = Control.IsKeyLocked(Keys.CapsLock);
            if (e.KeyChar == (char)Keys.Back)
            {
                // Handle the backspace key press here if needed
                // For example, remove the last character from the text box
                if (!string.IsNullOrEmpty(txtValue.Text))
                {
                    txtValue.Text = txtValue.Text.Substring(0, txtValue.Text.Length - 1);
                    BackToTextboxFocus();
                    UpdateOnScreenKeyboardAppearance();
                }

                e.Handled = true; // Prevent further processing of the backspace key
            }

           else if (IsOnScreenKey((Keys)e.KeyChar))
            {
                char character = e.KeyChar;
                if (capsLockEnabled)
                {
                    character = char.ToUpper(character);
                }
                else if (char.IsLetter(character) && !capsLockEnabled)
                {
                    character = char.ToLower(character);
                }

                txtValue.Text += character;
                BackToTextboxFocus();
                e.Handled = true;
                UpdateOnScreenKeyboardAppearance();
            }
            else if (IsBlockedCharacter(e.KeyChar))
            {
                e.Handled = true; // Block the Tab and Shift keys
            }
        }

        private bool IsBlockedCharacter(char character)
        {
            // Define a set of special characters that you want to block
            string blockedCharacters = "~!@#$%^&()_{}[]:\";'<>?\\|";

            return blockedCharacters.Contains(character);
        }

        private void FrmKeyboardPOS_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.CapsLock)
            {
                capsLockEnabled = !capsLockEnabled;
                UpdateOnScreenKeyboardAppearance();
                SetOnScreenKeyboardCapsLockState(capsLockEnabled);
                BackToTextboxFocus();
                e.Handled = true; // Prevent further processing of Caps Lock key
            }
        }

        private void FrmKeyboardPOS_KeyDown(object sender, KeyEventArgs e)
        {
             if (IsOnScreenKey(e.KeyCode))
            {
                char character = (char)e.KeyCode;
                if (capsLockEnabled)
                {
                    character = char.ToUpper(character);
                }
                else if (char.IsLetter(character) && !capsLockEnabled)
                {
                    character = char.ToLower(character);
                }

                txtValue.Text += character;
                BackToTextboxFocus();
                e.Handled = true;
                UpdateOnScreenKeyboardAppearance();
            }

        }

        private void Mouse_Enter(object sender, EventArgs e)
        {
            SimpleButton btn = sender as SimpleButton;
            btn.BackColor = Color.Purple;
            btn.ForeColor = Color.Red;
        }

        private void Mouse_Leave(object sender, EventArgs e)
        {
            SimpleButton btn = sender as SimpleButton;
            btn.BackColor = DefaultBackColor;
            btn.ForeColor = DefaultForeColor;                       
        }

    }
        
    }


