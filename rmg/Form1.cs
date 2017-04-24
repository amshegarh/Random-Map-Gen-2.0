using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rmg
{

    public partial class Form1 : Form
    {
        Height_map h;
        public Dictionary<string, string> args = new Dictionary<string, string>(50)
        {
            { "seed","42" },
            { "width","1024" },
            { "height","512" },
            { "maxLevel","512" },
            { "roughness","1" },
            { "waterLevel", "150" },
            { "mountLevel","400" },
            { "waterColorLow", "#000044"},
            { "waterColorHigh", "#0000FF"},
            { "landColorLow", "#008000"},
            { "landColorHigh", "#F0D000"},
            { "mountColorLow", "#F0D000"},
            { "mountColorHigh", "#EBEBEB"},

        };

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            h.generate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            h = new Height_map(this);
            foreach (KeyValuePair<string,string>row in args)
            {
                dataGridView1.Rows.Add(row.Key, row.Value );
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            args[Convert.ToString(dataGridView1[0, e.RowIndex].Value)] = Convert.ToString(dataGridView1[1, e.RowIndex].Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            h.draw();
        }
    }

    public class Height_map
    {
        Form1 f;
        public int[,] map;
        int maxHeight;
        int sizex;
        int sizey;


        public int mod(int a, int b)
        {
            return ((a % b) + b) % b;
        }

        public int get_rand(int step, Random r)
        {
            return r.Next(-step / 4, step / 4);
        }

        public Height_map(Form1 f)
        {
            this.f = f;
        }

        public bool generate()
        {
            int seed;
            int max;
            int sizex;
            int sizey;
            double roughness;
            try
            {
                seed = int.Parse(f.args["seed"]);
                this.sizex = Convert.ToInt32(Math.Pow(2, Math.Round(Math.Log(int.Parse(f.args["width"]), 2))));
                this.sizey = Convert.ToInt32(Math.Pow(2, Math.Round(Math.Log(int.Parse(f.args["height"]), 2))));
                max = int.Parse(f.args["maxLevel"]);
                this.maxHeight = max;
                roughness = float.Parse(f.args["roughness"]);
            }
            catch
            {
                MessageBox.Show("Invalid gen parametr(s)");
                return false;
            }
            Random R = new Random(seed);
            sizex = this.sizex;
            sizey = this.sizey;
            map = new int[sizex, sizey];
            int xamount = 2;
            int yamount = 1;
            int dist = sizey;
            if (sizex >= sizey)
            {
                xamount = sizex / sizey;
                yamount = 1;
                dist = sizey;
            }
            else
            {
                yamount = sizey / sizex;
                xamount = 1;
                dist = sizex;
            }
            for (int x = 0; x < sizex; x += dist)
                for (int y = 0; y < sizey; y += dist)
                    map[x, y] = R.Next(max);
            while (true)
            {
                for (int csx = 0; csx < xamount; csx++)
                    for (int csy = 0; csy < yamount; csy++)
                    {
                        int cx = csx * dist + dist / 2;
                        int cy = csy * dist + dist / 2;
                        int sum = 0;
                        foreach (int itx in new int[2] { -dist / 2, dist / 2 })
                            foreach (int ity in new int[2] { -dist / 2, dist / 2 })
                                sum += map[mod(cx + itx, sizex), mod(cy + ity, sizey)];
                        map[cx, cy] = sum / 4 + get_rand((int)((float)dist * roughness), R);
                    }
                for (int csx = 0; csx < xamount; csx++)
                    for (int csy = 0; csy < yamount; csy++)
                    {
                        int cx = csx * dist + dist / 2;
                        int cy = csy * dist + dist / 2;
                        foreach (int itx in new int[3] { -dist / 2, 0, dist / 2 })
                            foreach (int ity in new int[3] { -dist / 2, 0, dist / 2 })
                            {
                                if ((itx != 0 && ity != 0) || (itx == 0 && ity == 0)) continue;
                                int sum = 0;
                                foreach (int ittx in new int[3] { -dist / 2, 0, dist / 2 })
                                    foreach (int itty in new int[3] { -dist / 2, 0, dist / 2 })
                                    {
                                        if ((ittx != 0 && itty != 0) || (ittx == 0 && itty == 0)) continue;
                                        sum += map[mod(cx + itx + ittx, sizex), mod(cy + ity + itty, sizey)];
                                    }
                                map[mod(cx + itx, sizex), mod(cy + ity, sizey)] = sum / 4 + get_rand((int)((float)dist*roughness), R);
                            }
                    }
                if (dist <= 2) break;
                xamount *= 2;
                yamount *= 2;
                dist /= 2;
            }
            normalize();
            return true;
        }

        public void draw()
        {
            if (map == null)
            {
                MessageBox.Show("Nothing do draw. Generate map first");
                return;
            }
            Bitmap b = new Bitmap(map.GetLength(0),map.GetLength(1));
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, b.PixelFormat);

            int PixelSize = 4;

            int waterlevel;
            int mountlevel;
            Color wlow;
            Color whigh;
            Color llow;
            Color lhigh;
            Color mlow;
            Color mhigh;

            try
            {
                waterlevel = int.Parse(f.args["waterLevel"]);
                mountlevel = int.Parse(f.args["mountLevel"]);
                wlow = System.Drawing.ColorTranslator.FromHtml(f.args["waterColorLow"]);
                whigh = System.Drawing.ColorTranslator.FromHtml(f.args["waterColorHigh"]);
                llow = System.Drawing.ColorTranslator.FromHtml(f.args["landColorLow"]);
                lhigh = System.Drawing.ColorTranslator.FromHtml(f.args["landColorHigh"]);
                mlow = System.Drawing.ColorTranslator.FromHtml(f.args["mountColorLow"]);
                mhigh = System.Drawing.ColorTranslator.FromHtml(f.args["mountColorHigh"]);
            }
            catch
            {
                MessageBox.Show("Generation success but invalid draw parametr(s)");
                return;
            }
            unsafe
            {
                byte pr = 0, pg = 0, pb = 0;
                double grad;
                for (int y = 0; y < bmd.Height; y++)
                {
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);

                    for (int x = 0; x < bmd.Width; x++)
                    {
                        if (map[x,y] <= waterlevel)
                        {
                            grad = ((float)map[x, y] / (float)waterlevel);
                            pr = (byte)((float)(1 - grad) * (float)wlow.R + grad * (float)whigh.R);
                            pg = (byte)((float)(1 - grad) * (float)wlow.G + grad * (float)whigh.G);
                            pb = (byte)((float)(1 - grad) * (float)wlow.B + grad * (float)whigh.B);
                        }
                        else if (map[x, y] <= mountlevel)
                        {
                            grad = ((float)map[x, y] - (float)(waterlevel + 1)) / (float)(mountlevel - (waterlevel + 1));
                            pr = (byte)((float)llow.R + grad * (float)(lhigh.R - llow.R));
                            pg = (byte)((float)llow.G + grad * (float)(lhigh.G - llow.G));
                            pb = (byte)((float)llow.B + grad * (float)(lhigh.B - llow.B));
                        }
                        else
                        {
                            grad = ((float)map[x, y] - (float)(mountlevel + 1)) / (float)maxHeight;
                            pr = (byte)((float)mlow.R + grad * (float)(mhigh.R - mlow.R));
                            pg = (byte)((float)mlow.G + grad * (float)(mhigh.G - mlow.G));
                            pb = (byte)((float)mlow.B + grad * (float)(mhigh.B - mlow.B));
                        }

                        row[x * PixelSize] = pb;
                        row[x * PixelSize + 1] = pg;
                        row[x * PixelSize + 2] = pr;
                        row[x * PixelSize + 3] = 255;
                    }
                }
            }
            b.UnlockBits(bmd);
            Form2 f2 = new Form2(b);
            f2.Show();
        }

        public void normalize()
        {
            int max = Int32.MinValue;
            int minHeight = map[0, 0];
            for (int i = 0; i < sizex; i++)
                for (int j = 0; j < sizey; j++)
                {
                    if (map[i, j] < minHeight)
                        minHeight = map[i, j];
                    if (map[i, j] > max)
                        max = map[i, j];
                }
            max -= minHeight;
            for (int i = 0; i < sizex; i++)
                for (int j = 0; j < sizey; j++)
                {
                    map[i, j] -= minHeight;
                    map[i, j] = Convert.ToInt32(Math.Round((float)map[i, j] / ((float)max / (float)maxHeight)));
                }
        }

    };
}
