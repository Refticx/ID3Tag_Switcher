
using System;
using System.Windows.Forms;

namespace trackID3TagSwitcher
{
    public partial class DebugText : Form
    {
        public DebugText( )
        {
            InitializeComponent( );
        }
        private void AccountForm_Load( object sender , EventArgs e )
        {

        }

        private void button1_Click( object sender , EventArgs e )
        {
            string txt1 = this.richTextBox1.Text;
            string txt2 = String.Empty;
            for ( int i = 0 ; i < txt1.Length ; i++ )
            {
                txt2 += (char)( (int)txt1[i] + this.numericUpDown1.Value );
            }
            this.richTextBox2.Text = txt2;
        }
    }
}
