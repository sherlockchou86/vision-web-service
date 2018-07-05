using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace image_marker
{
    public partial class AddBBoxDialog : Form
    {
        BoundingBox bbox;

        public AddBBoxDialog(BoundingBox bbox)
        {
            InitializeComponent();

            this.bbox = bbox;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Text = comboBox1.SelectedIndex.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                bbox.Class_ID = comboBox1.SelectedIndex;
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                MessageBox.Show("select class index!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void AddBBoxDialog_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(Utils.Classes.ToArray());

            textBox1.Text = bbox.Region.Left + "," + bbox.Region.Top + "," + bbox.Region.Right + "," + bbox.Region.Bottom;

            textBox2.Text = bbox.Image_Path;
        }
    }
}
