using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace image_marker
{
    public partial class Main : Form
    {
        Dictionary<String, List<BoundingBox>> _all_data = new Dictionary<string, List<BoundingBox>>();
        public static String image_folder = "training_images";
        String annotation_file = "generated_annotation.txt";
        String classes_file = "default_classes.txt";


        int current_index = 0;
        Image current_image = null;

        double scale = 1;

        BoundingBox current_editing = null;

        public Main()
        {
            InitializeComponent();

            button1.ForeColor = Color.Red;
        }


        /// <summary>
        /// pre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (_all_data.Count > 0)
            {
                if (current_index > 0)
                {
                    current_index--;

                    label1.Text = (current_index + 1) + "/" + _all_data.Count;
                    textBox2.Text = _all_data.Keys.ToList()[current_index];


                    SwitchImage();

                }
            }
        }


        /// <summary>
        /// next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (_all_data.Count > 0)
            {
                if (current_index < _all_data.Count - 1)
                {
                    current_index++;

                    label1.Text = (current_index + 1) + "/" + _all_data.Count;
                    textBox2.Text = _all_data.Keys.ToList()[current_index];


                    SwitchImage();
                }
            }
        }

        /// <summary>
        /// select folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog f = new FolderBrowserDialog())
            {
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    String base_path = f.SelectedPath;

                    // load classes from default_classes.txt
                    if (!File.Exists(base_path + "\\" + classes_file))
                    {
                        MessageBox.Show("MUST exists default_classes.txt file!");
                        return;
                    }

                    if(!Utils.LoadClassesData(base_path + "\\" + classes_file))
                    {
                        MessageBox.Show("no data in default_classes.txt file!");
                        return;
                    }

                    // load images
                    if (!Directory.Exists(base_path + "\\" + image_folder))
                    {
                        MessageBox.Show("MUST exists training_images folder!");
                        return;
                    }

                    var d = Utils.LoadFromDir(base_path + "\\" + image_folder);

                    if (d.Count == 0)
                    {
                        MessageBox.Show("no images in training_images folder!");
                        return;
                    }

                    _all_data.Clear();
                    foreach (KeyValuePair<String, List<BoundingBox>> p in d)
                    {
                        _all_data.Add(p.Key, p.Value);
                    }

                    // load from annotation file if exists
                    if (File.Exists(base_path + "\\" + annotation_file))
                    {
                        var d2 = Utils.LoadFromAnnotationFile(base_path + "\\" + annotation_file);

                        foreach (KeyValuePair<String, List<BoundingBox>> p in d2)
                        {
                            // make sure the image exists
                            if (!_all_data.ContainsKey(p.Key))
                            {
                                continue;
                            }

                            p.Value.ForEach((item) => { _all_data[p.Key].Add(item); });
                        }
                    }


                    // show first image
                    current_index = 0;
                    textBox1.Text = base_path;
                    button1.Text = "dataset folder";
                    button1.ForeColor = Color.Black;
                    label1.Text = (current_index + 1) + "/" + _all_data.Count;
                    textBox2.Text = _all_data.Keys.ToList()[current_index];

                    SwitchImage();


                    numericUpDown1.Maximum = _all_data.Count;
                }
            }
        }

        /// <summary>
        /// show folder structure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            using (FolderStructureDialog f = new FolderStructureDialog())
            {
                f.ShowDialog();
            }
        }

        /// <summary>
        /// resize image_panel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            SwitchImage();
        }


        /// <summary>
        /// 
        /// </summary>
        private void SwitchImage()
        {
            if (_all_data.Count == 0)
                return;
            Image image = null;
            try
            {
                image = Image.FromFile(textBox1.Text + "\\" + _all_data.Keys.ToList()[current_index]);
            }
            catch
            {
                return;
            }

            double scale_ui = (double)image_container.Width / image_container.Height;
            double scale_im = (double)image.Width / image.Height;

            if (scale_ui > scale_im)
            {
                if (image_container.Height < image.Height)
                {
                    image_panel.Height = image_container.Height;
                    image_panel.Width = (int)(image.Width * ((double)image_panel.Height / image.Height));

                    image_panel.Left = (image_container.Width - image_panel.Width) / 2;
                    image_panel.Top = 0;
                }
                else
                {
                    image_panel.Height = image.Height;
                    image_panel.Width = image.Width;

                    image_panel.Left = (image_container.Width - image_panel.Width) / 2;
                    image_panel.Top = (image_container.Height - image_panel.Height) / 2;
                }
            }
            else
            {
                if (image_container.Width < image.Width)
                {
                    image_panel.Width = image_container.Width;
                    image_panel.Height = (int)(image.Height * ((double)image_panel.Width / image.Width));

                    image_panel.Top = (image_container.Height - image_panel.Height) / 2;
                    image_panel.Left = 0;
                }
                else
                {
                    image_panel.Width = image.Width;
                    image_panel.Height = image.Height;

                    image_panel.Left = (image_container.Width - image_panel.Width) / 2;
                    image_panel.Top = (image_container.Height - image_panel.Height) / 2;
                }
            }
            scale = (double)image.Width / image_panel.Width;
            if (current_image != null)
            {
                current_image.Dispose();
                current_image = null;
            }
            current_image = image;

            listBox1.Items.Clear();

            _all_data[textBox2.Text].ForEach((a) => 
            {
                listBox1.Items.Add(a);
            });

            image_panel.Invalidate();
        }

        Point mouse_point = new Point(0, 0);
        /// <summary>
        /// image panel paint
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_panel_Paint(object sender, PaintEventArgs e)
        {
            if (current_image != null)
            {
                e.Graphics.DrawImage(current_image, new Rectangle(0, 0, image_panel.Width, image_panel.Height));
            }


            if (mouse_point.X != 0 && mouse_point.Y != 0)
            {
                using (Pen p = new Pen(Brushes.Blue, 2))
                {
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(p, new Point(0, mouse_point.Y), new Point(image_panel.Width, mouse_point.Y));
                    e.Graphics.DrawLine(p, new Point(mouse_point.X, 0), new Point(mouse_point.X, image_panel.Height));
                }
            }

            if (_all_data.ContainsKey(textBox2.Text))
            {
                _all_data[textBox2.Text].ForEach((a) => 
                {
                    a.Draw(e.Graphics, scale);
                });
            }

            if (current_editing != null)
            {
                current_editing.Draw(e.Graphics, scale);
            }
        }

        /// <summary>
        /// mouse down to start create bounding box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (_all_data.Count == 0)
            {
                return;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                current_editing = new BoundingBox() { IsSelected = true, Image_Path = textBox2.Text, Class_ID = -1, Region = new Rectangle(new Point((int)(e.Location.X * scale), (int)(e.Location.Y * scale)), new Size(1,1)) };

                image_panel.Invalidate();
            }
        }

        /// <summary>
        /// mouse move to resize the bounding box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && current_editing != null)
            {
                if ((int)(e.Location.X * scale) > current_editing.Region.Left && (int)(e.Location.Y * scale) > current_editing.Region.Top)
                {
                    current_editing.Region = new RectangleF(current_editing.Region.Left, 
                        current_editing.Region.Top,
                        (float)(e.Location.X * scale) - current_editing.Region.Left,
                        (float)(e.Location.Y * scale) - current_editing.Region.Top);   

                    image_panel.Invalidate();
                }
            }

            if (_all_data != null && _all_data.Count > 0)
            {
                mouse_point = e.Location;
                image_panel.Invalidate();
            }
        }

        /// <summary>
        /// mouse up to end create bounding box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (current_editing != null)
            {
                using (AddBBoxDialog a = new AddBBoxDialog(current_editing))
                {
                    if (a.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _all_data[textBox2.Text].Add(current_editing);
                        listBox1.Items.Add(current_editing);

                        listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    }

                    current_editing = null;

                    image_panel.Invalidate();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_panel_MouseLeave(object sender, EventArgs e)
        {
            mouse_point = new Point(0, 0);
            image_panel.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _all_data[textBox2.Text].ForEach((a) => 
            {
                if (a == listBox1.SelectedItem)
                {
                    a.IsSelected = true;
                }
                else
                {
                    a.IsSelected = false;
                }
            });

            image_panel.Invalidate();
        }

        /// <summary>
        /// double click to delete bounding box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                if (MessageBox.Show("delete this bounding box?", "delete confirm", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    _all_data[textBox2.Text].Remove(listBox1.SelectedItem as BoundingBox);
                    listBox1.Items.Remove(listBox1.SelectedItem);
                }
            }
        }

        /// <summary>
        /// save to annotation file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            if (_all_data.Count != 0)
            {
                string path = textBox1.Text + "\\" + annotation_file;

                int count = Utils.SaveToAnnotation(_all_data, path);

                MessageBox.Show("saved total " + count + " lines!");
            }
        }

        /// <summary>
        /// jump to index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value <= _all_data.Count)
            {
                current_index = (int)numericUpDown1.Value - 1;

                label1.Text = (current_index + 1) + "/" + _all_data.Count;
                textBox2.Text = _all_data.Keys.ToList()[current_index];

                SwitchImage();
            }
            else
            {
                MessageBox.Show("out of index!");
            }
        }
    }
}
