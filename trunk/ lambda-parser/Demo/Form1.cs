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

            // test GeneralEventHandling
            GeneralEventHandling.NewAttachGeneralHandler(this, this.GetType().GetEvent("MouseClick"));

            // test DynamicQuery
            string[] arr = { "ABC", "ADE", "BCD", "DEF" };
            var query = arr.AsQueryable().Where("StartsWith(\"A\")");
            MessageBox.Show(query.Count().ToString());
        }
    }
}
