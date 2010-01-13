using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            GeneralEventHandling.NewAttachGeneralHandler(this, this.GetType().GetEvent("MouseClick"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
