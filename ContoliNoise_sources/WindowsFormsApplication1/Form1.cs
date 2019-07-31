using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;

//http://www.sepcot.com/blog/2006/08/PDN-PerlinNoise2d

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        public System.Random r;
        public long Iterations = 0;
        public bool Noise3dSTOP = true;

        delegate float f(float x1, float y1);

        public Form1()
        {
            InitializeComponent();
        }


#region PERLIN
        private static uint GetUint(uint x, uint y)
        {
            uint m_w = 43;//x * 7 + y * 17 + x * y + 1; //x * 43 + 1;    /* must not be zero, nor 0x464fffff */
            uint m_z = x * y * 57 + y^2 + x * 7 +1 ;    /* must not be zero, nor 0x9068ffff */

            m_z = 36969 * (m_z & 65535) + (m_z >> 16);
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);
            return (m_z << 16) + m_w;
        }

        float pseudoRandom(uint x, uint y)
        {
            uint u = GetUint(x, y);
            return (u + 1.0f) * 2.328306435454494e-10f;
        }

        float noise(float x, float y,float w,float h)
        {
            return pseudoRandom((uint)(x ), (uint)(y ));
        }

        float SmoothNoise_1(float x, float y,float w,float h)
        {
            float corners = (noise(x - 1, y - 1, w, h)
                             + noise(x + 1, y - 1, w, h)
                             + noise(x - 1, y + 1, w, h)
                             + noise(x + 1, y + 1, w, h))
                / 16;
            float sides = (noise(x - 1, y, w, h) + noise(x + 1, y, w, h) + noise(x, y - 1, w, h) + noise(x, y + 1, w, h)) / 8;
            float center = noise(x, y, w, h) / 4;
 
            return (corners + sides + center);
        }

        //Cosine_Interpolate
        float Interpolate(float a, float b, float x)
        {
            float ft = x * 3.1415927f;
            float f = (1 - (float)Math.Cos(ft)) * .5f;

            return a * (1 - f) + b * f;
        }

        float InterpolatedNoise_1(float x, float y, float w , float h)
        {
            int integer_X = (int)x;
            float fractional_X = x - integer_X;

            int integer_Y = (int)y;
            float fractional_Y = y - integer_Y;


            int integer_X1 = integer_X + 1;
            if (integer_X1 >= w)
                         integer_X1 = 0;

            int integer_Y1 = integer_Y + 1;
            if (integer_Y1 >= h)
                    integer_Y1 = 0;


            float v1 = SmoothNoise_1(integer_X, integer_Y,w,h);
            float v2 = SmoothNoise_1(integer_X1, integer_Y, w, h);
            float v3 = SmoothNoise_1(integer_X, integer_Y1, w, h);
            float v4 = SmoothNoise_1(integer_X1, integer_Y1, w, h);

            float i1 = Interpolate(v1, v2, fractional_X);
            float i2 = Interpolate(v3, v4, fractional_X);

            return Interpolate(i1, i2, fractional_Y);

        }


        float PerlinNoise_2D(float x, float y, int W , int H)
        {
            float total = 0;
            float p = 0.1f;//persistence;
            double n = 4; //Number_Of_Octaves - 1;

            for (uint i = 0; i <= n; i++)
            {

                float frequency = 2 ^ i;
                float amplitude = (float)System.Math.Pow(p, i);

                //total = total + SmoothNoise_1(x * frequency, y * frequency) * amplitude;
                total = total + InterpolatedNoise_1(x * frequency, y * frequency, W*frequency,H*frequency ) * amplitude;
            }

            return total;

        }


        double PerlinNoise2dBIS(int x, int y, int W, int H)
        {
            double total = 0.0;

            float x_frequency = .03f; // USER ADJUSTABLE .015f;
            float y_frequency = .03f; // USER ADJUSTABLE .015f;
            float persistence = .5f;  // USER ADJUSTABLE .65f
            float octaves = 8f;    // USER ADJUSTABLE
            float amplitude = 1f;    // USER ADJUSTABLE


            x_frequency = float.Parse(Xfreq.Text);
            y_frequency = float.Parse(Yfreq.Text);
            persistence = float.Parse(pers.Text);
            octaves = float.Parse(oct.Text);
            amplitude = float.Parse(amp.Text);



            for (int lcv = 0; lcv < octaves; lcv++)
            {
                total = total + InterpolatedNoise_1(x * x_frequency, y * y_frequency, W*x_frequency, H*y_frequency) * amplitude;
                x_frequency = x_frequency * 2;
                y_frequency = y_frequency * 2;
                amplitude = amplitude * persistence;
            }

            double cloudCoverage = 0; // USER ADJUSTABLE
            double cloudDensity = 1; // USER ADJUSTABLE

            total = (total - cloudCoverage) * cloudDensity;

            if (total < 0) 
                total = 0.0;
            if (total > 1)
                total = 1.0; //black

            return total;
        }



        private void button1_Click(object sender, EventArgs e)
        {


            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            
            for (int i = 0; i < pictureBox1.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Height; j++)
                {
                    //double a = pseudoRandom(i, j);
                    //double a = PerlinNoise_2D(i, j);
                    
                    double a;
                    if (!seamless.Checked)
                    {
                        a = PerlinNoise2dBIS(i, j, pictureBox1.Width, pictureBox1.Height);
                    }
                    else {
                        /* Seamlessly tiling noise ? http://webstaff.itn.liu.se/~stegu/TNM022-2005/perlinnoiselinks/perlin-noise-math-faq.html#toc-tile */
                        int w = pictureBox1.Width + 1;
                        int h = pictureBox1.Height + 1;
                        a = (PerlinNoise2dBIS(i, j, pictureBox1.Width, pictureBox1.Height) * (w - i) * (h - j)
                                 + PerlinNoise2dBIS(w - i, j, pictureBox1.Width, pictureBox1.Height) * (i) * (h - j)
                                 + PerlinNoise2dBIS(w - i, h - j, pictureBox1.Width, pictureBox1.Height) * (i) * (j)
                                 + PerlinNoise2dBIS(i, h - j, pictureBox1.Width, pictureBox1.Height) * (w - i) * (j)
                                 ) / (w * h)
                                 ;
                    }

                    
                    
                    
                    int c = (int)((1f-a) * 255);

                    //c = Math.Abs(c);
                    //if (c > 255)
                    //    c = 255;
                    //if (c > 70 )
                    //    c = (int)(c * 1.7f);

                    //a = Math.Round(a, 3);
                    Color clr;
                    if (grayScale.Checked)
                        clr = Color.FromArgb(c,c,c);
                    else
                        clr = Color.FromArgb(c, 255, 255, 255);

                    //Color clr = Color.FromArgb(c, 255, 255, 255);
 //                   ((Bitmap)pictureBox1.Image).SetPixel((int)i, (int)j, clr);
                    tmp.SetPixel((int)i, (int)j, clr);
                    //pictureBox1.Refresh();

                }
            }

            pictureBox1.Image = tmp;
            pictureBox1.Refresh();

            //double a = pseudoRandom(238, 24);

            pictureBox2.Image = tmp;
            pictureBox3.Image = tmp;
            pictureBox4.Image = tmp;
            pictureBox5.Image = tmp;

        }
#endregion 


# region CONTOLI NOISE

        private void printQuadOLD(Bitmap tmp, int wid, int hei, int x, int y, int boxSize, int deepness, int delta)
        {
            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = 0; i < boxSize; i++)
                {

                    for (int j = 0; j < boxSize; j++)
                    //for (int j = 0; j < 1; j++)
                    {

                        // ROTAZIONE
                        //p'x = cos(theta) * (px-ox) - sin(theta) * (py-oy) + ox
                        //p'y = sin(theta) * (px-ox) + cos(theta) * (py-oy) + oy
                        //double theta = r.NextDouble()*90;
                        /*
                        double theta = 45;
                        int pixXR = (int)(Math.Cos(theta) * (x + i) - Math.Sin(theta) * (y + j));
                        int pixYR = (int)(Math.Sin(theta) * (x + i) + Math.Cos(theta) * (y + j));
                        int pixX = (pixXR+wid) % wid; // % --> seamless
                        int pixY = (pixYR+hei) % hei; //% --> seamless
                        pixX = Math.Abs(pixX);
                        pixY = Math.Abs(pixY);
                        */

                        int pixX = (x + i) % wid; // % --> seamless
                        int pixY = (y + j) % hei; //% --> seamless
                        Color c = tmp.GetPixel(pixX, pixY);
                        //c.R = c.R + 1;
                        int alfa = c.A + delta;
                        if (alfa > 255)
                            alfa = 255;

                        //Color clr = Color.FromArgb(alfa, c.R , c.G , c.B);
                        Color clr = Color.FromArgb(alfa, 255, 255, 255);
                        //Color clr = Color.FromArgb(255, alfa, alfa, alfa);




                        tmp.SetPixel(pixX, pixY, clr);
                        //tmp.SetPixel(pixX, pixY, Color.Red);
                        //System.Random r = new System.Random((int)DateTime.Now.Ticks);

                        Iterations++;

                    }
                    int xx = r.Next(x, x + boxSize);
                    int yy = r.Next(y, y + boxSize);

                    printQuadOLD(tmp, wid, hei, xx, yy, boxSize / 2, --deepness, delta);

                }
            }

        }

        private void printQuad3D(float[, ,] Values, int wid, int hei, int zlvl, int x, int y, int z, int boxSize, int deepness, int delta)
        {

            if (boxSize > wid)
                boxSize = wid;
            if (boxSize > hei)
                boxSize = wid;
            //if (boxSize > zlvl)
            //    zlvl = wid;


            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = -boxSize / 2; i < boxSize / 2; i++)
                {
                    for (int j = -boxSize / 2; j < boxSize / 2; j++)
                    {
                        for (int k = -boxSize / 2; k < boxSize / 2; k++)
                        {

                            /*seamless management start*/
                            int pixX = (x + i) % wid;
                            int pixY = (y + j) % hei;
                            int pixZ = (z + k) % zlvl;
                            if (pixX < 0)
                                pixX = wid + pixX;
                            if (pixY < 0)
                                pixY = wid + pixY;
                            if (pixZ < 0)
                                pixZ = zlvl + pixZ;
                            /*seamless management end*/

                            Values[pixX, pixY, pixZ] = Values[pixX, pixY, pixZ] + delta; // add value

                            Iterations++;
                        }
                    }

                    int xx = r.Next(x - (int)(boxSize * 0.5), x + (int)(boxSize * 0.5));
                    int yy = r.Next(y - (int)(boxSize * 0.5), y + (int)(boxSize * 0.5));
                    int zz = r.Next(z - (int)(boxSize * 0.5), z + (int)(boxSize * 0.5));

                    printQuad3D(Values, wid, hei,zlvl, xx, yy,zz, boxSize / 2, --deepness, delta);
                }
            }

        }


        private void printCirc3D(float[, ,] Values, int wid, int hei, int zlvl, int x, int y, int z, int boxSize, int deepness, int delta)
        {

            if (boxSize > wid)
                boxSize = wid;
            if (boxSize > hei)
                boxSize = wid;
            //if (boxSize > zlvl)
            //    zlvl = wid;


            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = -boxSize / 2; i < boxSize / 2; i++)
                {
                    for (int j = -boxSize / 2; j < boxSize / 2; j++)
                    {
                        for (int k = -boxSize / 2; k < boxSize / 2; k++)
                        {

                            if (Math.Pow(i, 2) + Math.Pow(j, 2) + Math.Pow(k, 2) <= Math.Pow(boxSize / 2, 2))
                            {

                                /*seamless management start*/
                                int pixX = (x + i) % wid;
                                int pixY = (y + j) % hei;
                                int pixZ = (z + k) % zlvl;
                                if (pixX < 0)
                                    pixX = wid + pixX;
                                if (pixY < 0)
                                    pixY = wid + pixY;
                                if (pixZ < 0)
                                    pixZ = zlvl + pixZ;
                                /*seamless management end*/

                                Values[pixX, pixY, pixZ] = Values[pixX, pixY, pixZ] + delta; // add value

                                Iterations++;
                            }
                        }
                    }

                    int xx = r.Next(x - (int)(boxSize * 0.5), x + (int)(boxSize * 0.5));
                    int yy = r.Next(y - (int)(boxSize * 0.5), y + (int)(boxSize * 0.5));
                    int zz = r.Next(z - (int)(boxSize * 0.5), z + (int)(boxSize * 0.5));

                    printCirc3D(Values, wid, hei, zlvl, xx, yy, zz, boxSize / 2, --deepness, delta);
                }
            }

        }


        private void printQuad(float [,] Values, int wid, int hei,int x, int y, int boxSize, int deepness, int delta) {

            if (animateChk.Checked)
            {
                animate(Values);
            }

            if (boxSize > wid )
                boxSize= wid;
            if (boxSize > hei )
                boxSize= wid;

            if (deepness > 0 && boxSize>=1)
            {
                for (int i = -boxSize/2; i < boxSize/2; i++)
                {
                    for (int j = -boxSize / 2; j < boxSize / 2; j++)
                    {
                            /*seamless management start*/
                            int pixX = (x + i) % wid;
                            int pixY = (y + j) % hei;
                            if (pixX < 0)
                                pixX = wid + pixX;
                            if (pixY < 0)
                                pixY = wid + pixY;
                            /*seamless management end*/
                            
                            Values[pixX, pixY] = Values[pixX, pixY] + delta; // add value

                            Iterations++;
                    }
                    //int xx = r.Next(x, x + boxSize);
                    //int yy = r.Next(y, y + boxSize);

                    //int xx = r.Next(x - boxSize / 2, x + boxSize / 2);
                    //int yy = r.Next(y - boxSize / 2, y + boxSize / 2);

                    int xx = r.Next(x - (int)(boxSize * 0.5), x + (int)(boxSize * 0.5));
                    int yy = r.Next(y - (int)(boxSize * 0.5), y + (int)(boxSize * 0.5));


                    printQuad(Values, wid, hei, xx, yy, boxSize / 2, --deepness, delta);
                }
            }

        }


        private void printQuad1D(float[] Values, int wid, int x, int boxSize, int deepness, int delta)
        {

            if (boxSize > wid)
                boxSize = wid;

            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = -boxSize / 2; i < boxSize / 2; i++)
                {
                    /*seamless management start*/
                    int pixX = (x + i) % wid;
                    if (pixX < 0)
                        pixX = wid + pixX;
                    /*seamless management end*/
                    Values[pixX] = Values[pixX] + delta; // add value
                    Iterations++;
                    int xx = r.Next(x - (int)(boxSize * 0.5), x + (int)(boxSize * 0.5));
                    printQuad1D(Values, wid, xx, boxSize / 2, --deepness, delta);
                }
            }

        }


        private void printFunc(float[,] Values, int wid, int hei, int x, int y, int boxSize, int deepness, int delta, f func)
        {

            if (animateChk.Checked)
            {
                animate(Values);
            }

            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = -boxSize/2; i < boxSize/2; i++)
                {
                    for (int j = -boxSize / 2; j < boxSize/2; j++)
                    {

                        //scaling  -1 .. +1
                        float fx = i / (boxSize / 2f);
                        float fy = j / (boxSize / 2f);

                        float a = 0f;
                        if (func(fx, fy) != float.NaN)
                        {
                            a = ((float)delta
                                * func(fx, fy)
                                );
                        }

                        if (a > 0f)
                        {
                            
                            int pixX = (x + i) % wid; // % --> seamless
                            int pixY = (y + j) % hei; //% --> seamless

                            if (pixX < 0)
                                pixX = wid + pixX;
                            if (pixY < 0)
                                pixY = hei + pixY;


                            Values[pixX, pixY] = Values[pixX, pixY] + a;
                        }
                        Iterations++;

                    }
                    int xx = r.Next(x - boxSize/2, x + boxSize/2);
                    int yy = r.Next(y - boxSize / 2, y + boxSize/2);

                    printFunc(Values, wid, hei, xx, yy, boxSize / 2, --deepness, delta, func);

                }
            }

        }

        private void printCrates(float[,]Values,int wid, int hei, int x, int y, int boxSize, int deepness, int delta)
        {

            if (animateChk.Checked)
            {
                animate(Values);
            }

            if (boxSize > wid)
                boxSize = wid;
            if (boxSize > hei)
                boxSize = wid;

            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = 0; i < boxSize; i++)
                {
                    for (int j = 0; j < boxSize; j++)
                    {
                        if (Math.Pow(i - boxSize / 2, 2) + Math.Pow(j - boxSize / 2, 2) <= Math.Pow(boxSize / 2, 2)
                            &&
                            Math.Pow(i - boxSize / 2, 2) + Math.Pow(j - boxSize / 2, 2) >= Math.Pow((boxSize -2) / 2, 2)
                            )
                        {
                            
                            
                            int noise = r.Next(1, 1);
                            for (int I_noise = 0; I_noise < noise; I_noise++)
                            {
                                int pixX = (x + i+I_noise) % wid; // % --> seamless
                                int pixY = (y + j+I_noise) % hei; //% --> seamless

                                Values[pixX, pixY] = Values[pixX, pixY] + delta*10f;

                                Iterations++;
                            }
                        }

                    }
                    int xx = r.Next(x, x + boxSize);
                    int yy = r.Next(y, y + boxSize);

                    printCrates(Values, wid, hei, xx, yy, boxSize / 4, --deepness, delta);
                    

                }
            }

        }

        private void printCirc(float[,] Values, int wid, int hei, int x, int y, int boxSize, int deepness, int delta)
        {

            if (animateChk.Checked)
            {
                animate(Values);
            }

            if (boxSize > wid)
                boxSize = wid;
            if (boxSize > hei)
                boxSize = wid;

            if (deepness > 0 && boxSize >= 1)
            {
                for (int i = -boxSize/2; i < boxSize/2; i++)
                {
                    for (int j = -boxSize/2; j < boxSize/2; j++)
                    {
                        //if ( Math.Pow(i - boxSize / 2,2)  + Math.Pow(j - boxSize / 2 ,2) <= Math.Pow(boxSize / 2,2) 
                        if ( Math.Pow(i ,2)  + Math.Pow(j  ,2) <= Math.Pow(boxSize / 2,2) 
                            )
                        {

                            int pixX = (x + i) % wid; // % --> seamless
                            int pixY = (y + j) % hei; //% --> seamless
                            if (pixX < 0)
                                pixX = wid + pixX;
                            if (pixY < 0)
                                pixY = wid + pixY;

                            Values[pixX, pixY] = Values[pixX, pixY] + delta;

                            Iterations++;
                        }

                    }
                    //int xx = r.Next(x, x + boxSize);
                    //int yy = r.Next(y, y + boxSize);
                    int xx = r.Next(x - boxSize / 2, x + boxSize / 2);
                    int yy = r.Next(y - boxSize / 2, y + boxSize / 2);

                    printCirc(Values, wid, hei, xx, yy, boxSize / 2, --deepness, delta);

                }
            }

        }

#endregion

        private void Crates(float[,]Values, int wid, int hei, int x, int y, int fromP, int toP,int fromR, int toR)
        {
                double power = r.Next(fromP, toP);
                int radius = r.Next(fromR, toR);
                for (int i = 0; i < wid; i++)
                {
                    for (int j = 0; j < hei; j++)
                    {

                        Double Dist1X = Math.Abs((i - x));
                        Double Dist1Y = Math.Abs((j - y));
                        
                        Double Dist2X = wid - Dist1X; 
                        Double Dist2Y = hei - Dist1Y;
                        /*to grant seamless I take the min between distX and wid-distX
                         |                       |
                         |                       |     ----------- = Dist1X
                         |...i-----------X.......|     ..........  = Dist2X
                         |                       |
                         */
                        Dist1X = Math.Min(Dist1X, Dist2X);
                        /*to grant seamless I take the min between distY and hei-distY*/
                        Dist1Y = Math.Min(Dist1Y, Dist2Y);
                        
                        double Dist = Math.Sqrt(Math.Pow(Dist1X,2) + Math.Pow(Dist1Y,2));
                        if (Dist < 0.001) //avoid division by zero
                            Dist = 0.001;
                       
                        
                        // to make inside of crate:
                        if (Dist < radius)
                            //Dist = radius-Dist;
                            Dist = 20;
                        
                        int pixX = (i ) ; 
                        int pixY = (j ) ;

                        //Values[pixX, pixY] = Values[pixX, pixY] + (int)(power / Dist);
                        Values[pixX, pixY] = Values[pixX, pixY] + (int)(power * radius / Dist);


                        Iterations++;
                     }




                }


        }


        private void Starfield(float[,] Values, int wid, int hei, int x, int y, int fromP, int toP, int fromR, int toR)
        {
            double power = r.Next(fromP, toP);
            int radius = r.Next(fromR, toR);
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {

                    Double Dist1X = Math.Abs((i - x));
                    Double Dist1Y = Math.Abs((j - y));

                    Double Dist2X = wid - Dist1X;
                    Double Dist2Y = hei - Dist1Y;
                    /*to grant seamless I take the min between distX and wid-distX
                     |                       |
                     |                       |     ----------- = Dist1X
                     |...i-----------X.......|     ..........  = Dist2X
                     |                       |
                     */
                    Dist1X = Math.Min(Dist1X, Dist2X);
                    /*to grant seamless I take the min between distY and hei-distY*/
                    Dist1Y = Math.Min(Dist1Y, Dist2Y);

                    double Dist = Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2));
                    if (Dist < 0.001) //avoid division by zero
                        Dist = 0.001;

                    int pixX = (i);
                    int pixY = (j);

                    
                    Values[pixX, pixY] = Values[pixX, pixY] + (int)(power * radius  / Dist);


                    Iterations++;
                }




            }


        }


        private void landscape1(float[,] Values, int wid, int hei, int x, int y, int fromP, int toP, int fromR, int toR)
        {
            double power = r.Next(fromP, toP);
            int radius = r.Next(fromR, toR);
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {

                    Double Dist1X = Math.Abs((i - x));
                    Double Dist1Y = Math.Abs((j - y));

                    Double Dist2X = wid - Dist1X;
                    Double Dist2Y = hei - Dist1Y;
                    /*to grant seamless I take the min between distX and wid-distX
                     |                       |
                     |                       |     ----------- = Dist1X
                     |...i-----------X.......|     ..........  = Dist2X
                     |                       |
                     */
                    Dist1X = Math.Min(Dist1X, Dist2X);
                    /*to grant seamless I take the min between distY and hei-distY*/
                    Dist1Y = Math.Min(Dist1Y, Dist2Y);

                    double Dist = Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2));
                    if (Dist < 0.001) //avoid division by zero
                        Dist = 0.001;

                    int pixX = (i);
                    int pixY = (j);


                    Values[pixX, pixY] = Values[pixX, pixY] + (int)(power * radius / Math.Log(Dist+1));


                    Iterations++;
                }




            }


        }


        private void Voronoi(float[,] Values, int wid, int hei, int[,] points)
        {            
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {
                    float minDist = Math.Min(wid,hei);
                    int minV = 0;
                    for (int p = 0; p < points.GetLength(0); p++) {
                        int pX = points[p, 0];
                        int pY = points[p, 1];
                        int pV = points[p, 2]; //0..255
                        Double Dist1X = Math.Abs((i - pX));
                        Double Dist1Y = Math.Abs((j - pY));
                        Double Dist2X = wid - Dist1X;
                        Double Dist2Y = hei - Dist1Y;
                        /*to grant seamless I take the min between distX and wid-distX
                         |                       |
                         |                       |     ----------- = Dist1X
                         |...i-----------X.......|     ..........  = Dist2X
                         |                       |
                         */
                        Dist1X = Math.Min(Dist1X, Dist2X);
                        /*to grant seamless I take the min between distY and hei-distY*/
                        Dist1Y = Math.Min(Dist1Y, Dist2Y);

                        float dist = (float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2));//metric
                        //METRICS EXAMPLES
                        //float dist = 1f/(float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2));//metric
                        //float dist = (float)(Dist1X + Dist1Y);//Taxicab metric //http://en.wikipedia.org/wiki/Taxicab_geometry
                        //float dist = (float)(Math.Sin(64*Dist1X/wid) + Dist1Y + Math.Sin(64*Dist1Y/hei) + Dist1X);
                        //float dist = (float)Math.Abs(Math.Log(Dist1X / Dist1Y));
                        //float dist = (float)Math.Max(Dist1X , Dist1Y); //chessboard distance
                        //float dist = (float)Math.Min(Dist1X, Dist1Y);                        
                        //double z = (Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2));
                        //float dist = (float)Math.Log((1 / z) + Math.Sqrt(1+1/(z*z))  );                       
                        if (dist < minDist) {
                            minDist = dist;
                            minV = pV;
                        }
                    }
                    Values[i, j] = minV;
                    Iterations++;
                }
            }
        }


        private void Voronoi2(float[,] Values, int wid, int hei, int[,] points)
        {

            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {
                    float minDist = Math.Min(wid, hei);
                    int nMINS = points.GetLength(0)/2;
                    int[] mins = new int[nMINS];
                    int minV1 = 0; //store lat two min value .. 
                    //int minV2 = 0;
                    //int minV3 = 0;
                    //int minV4 = 0;
                    for (int p = 0; p < points.GetLength(0); p++)
                    {
                        int pX = points[p, 0];
                        int pY = points[p, 1];
                        int pV = points[p, 2]; //0..255


                        Double Dist1X = Math.Abs((i - pX));
                        Double Dist1Y = Math.Abs((j - pY));
                        Double Dist2X = wid - Dist1X;
                        Double Dist2Y = hei - Dist1Y;
                        /*to grant seamless I take the min between distX and wid-distX
                         |                       |
                         |                       |     ----------- = Dist1X
                         |...i-----------X.......|     ..........  = Dist2X
                         |                       |
                         */
                        Dist1X = Math.Min(Dist1X, Dist2X);
                        /*to grant seamless I take the min between distY and hei-distY*/
                        Dist1Y = Math.Min(Dist1Y, Dist2Y);
                        float dist = (float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2))
                            ;

                        //float dist = (float)Math.Sqrt( (pX - i) * (pX - i) + (pY - j) * (pY - j) );
                        if (dist < minDist)
                        {

                            minDist = dist;
                            minV1 = pV;
                            for (int m = 1; m < nMINS; m++)
                            {
                                mins[m - 1] = mins[m];
                            }
                            mins[nMINS - 1] = minV1;
                        }


                    }
                    float tot = 0;
                    for (int m = 0; m < nMINS; m++)
                    {
                        tot = tot + mins[m];
                    }

                    Values[i, j] = (tot) / nMINS;
                    Iterations++;
                }

            }
        }


        private void Voronoi3(float[,] Values, int wid, int hei, int[,] points, int minDelta)
        {
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {
                    float minDist = 999999999f;
                    float minDist2 = 999999999f;
                    int minV = 0;
                    for (int p = 0; p < points.GetLength(0); p++)
                    {
                        int pX = points[p, 0];
                        int pY = points[p, 1];
                        int pV = points[p, 2]; //0..255
                        Double Dist1X = Math.Abs((i - pX));
                        Double Dist1Y = Math.Abs((j - pY));
                        Double Dist2X = wid - Dist1X;
                        Double Dist2Y = hei - Dist1Y;
                        /*to grant seamless I take the min between distX and wid-distX
                         |                       |
                         |                       |     ----------- = Dist1X
                         |...i-----------X.......|     ..........  = Dist2X
                         |                       |
                         */
                        Dist1X = Math.Min(Dist1X, Dist2X);
                        /*to grant seamless I take the min between distY and hei-distY*/
                        Dist1Y = Math.Min(Dist1Y, Dist2Y);


                        float dist = (float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2)); //euclidian metric

                        //float dist = (float)(Dist1X + Dist1Y);//Taxicab metric //http://en.wikipedia.org/wiki/Taxicab_geometry
                        
                        //to make it ondulated
                        //1.
                        //dist = dist + (float)Math.Sin(Dist1X * 0.15) * (float)Math.Cos(Dist1Y * 0.15);
                        //2.
                        //dist = dist + (float)Math.Cos(Dist1Y * 0.15);
                        //dist = dist + (float)Math.Sin(Dist1Y * 0.15);

                        // strange effects:
                        // dist = dist % 100;
                        // dist = dist + dist % 100;

                        if (dist <= minDist)
                        {
                            minDist2 = minDist;
                            minDist = dist;
                            minV = pV;
                        }
                        else { 
                                if (dist <= minDist2)
                                    minDist2 = dist;
                             }
                    }

                    if ((minDist2 - minDist < 1f) )
                        Values[i, j] = 0;
                    else 
                        if (minDist2 - minDist < (float)minDelta)
                            Values[i, j] = ((minDist2 - minDist) / (float)minDelta) * 255f;
                        else
                            Values[i, j] = 255;
                        //Values[i, j] = (minDist2 - minDist);
                        //Values[i, j] = minV;
                    Iterations++;
                }

            }

        }


        private void Voronoi3_3D(float[,,] Values, int wid, int hei,int zlvl, int[,] points, int minDelta)
        {
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {
                    for (int k = 0; k < zlvl; k++)
                    {

                        float minDist = 999999999f;
                        float minDist2 = 999999999f;
                        int minV = 0;
                        for (int p = 0; p < points.GetLength(0); p++)
                        {
                            int pX = points[p, 0];
                            int pY = points[p, 1];
                            int pZ = points[p, 2];
                            int pV = points[p, 3]; //0..255
                            Double Dist1X = Math.Abs((i - pX));
                            Double Dist1Y = Math.Abs((j - pY));
                            Double Dist1Z = Math.Abs((k - pZ));
                            Double Dist2X = wid - Dist1X;
                            Double Dist2Y = hei - Dist1Y;
                            Double Dist2Z = zlvl - Dist1Z;
                            /*to grant seamless I take the min between distX and wid-distX
                             |                       |
                             |                       |     ----------- = Dist1X
                             |...i-----------X.......|     ..........  = Dist2X
                             |                       |
                             */
                            Dist1X = Math.Min(Dist1X, Dist2X);
                            /*to grant seamless I take the min between distY and hei-distY*/
                            Dist1Y = Math.Min(Dist1Y, Dist2Y);
                            Dist1Z = Math.Min(Dist1Z, Dist2Z);


                            float dist = (float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2) + Math.Pow(Dist1Z, 2)); //euclidian metric

                            //float dist = (float)(Dist1X + Dist1Y);//Taxicab metric //http://en.wikipedia.org/wiki/Taxicab_geometry

                            //to make it ondulated
                            //1.
                            //dist = dist + (float)Math.Sin(Dist1X * 0.15) * (float)Math.Cos(Dist1Y * 0.15);
                            //2.
                            //dist = dist + (float)Math.Cos(Dist1Y * 0.15);
                            //dist = dist + (float)Math.Sin(Dist1Y * 0.15);

                            // strange effects:
                            // dist = dist % 100;
                            // dist = dist + dist % 100;

                            if (dist <= minDist)
                            {
                                minDist2 = minDist;
                                minDist = dist;
                                minV = pV;
                            }
                            else
                            {
                                if (dist <= minDist2)
                                    minDist2 = dist;
                            }
                        }

                        if ((minDist2 - minDist < 1f))
                            Values[i, j,k] = 0;
                        else
                            if (minDist2 - minDist < (float)minDelta)
                                Values[i, j,k] = ((minDist2 - minDist) / (float)minDelta) * 255f;
                            else
                                Values[i, j,k] = 255;
                        //Values[i, j] = (minDist2 - minDist);
                        //Values[i, j] = minV;
                        Iterations++;
                    }
                }

            }

        }



        private void Voronoi4(float[,] Values, int wid, int hei, int[,] points, int minDelta)
        {
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {
                    float minDist = 999999999f;
                    float minDist2 = 999999999f;
                    float minDist3 = 999999999f;
                    int minV = 0;
                    for (int p = 0; p < points.GetLength(0); p++)
                    {
                        int pX = points[p, 0];
                        int pY = points[p, 1];
                        int pV = points[p, 2]; //0..255
                        Double Dist1X = Math.Abs((i - pX));
                        Double Dist1Y = Math.Abs((j - pY));
                        Double Dist2X = wid - Dist1X;
                        Double Dist2Y = hei - Dist1Y;
                        /*to grant seamless I take the min between distX and wid-distX
                         |                       |
                         |                       |     ----------- = Dist1X
                         |...i-----------X.......|     ..........  = Dist2X
                         |                       |
                         */
                        Dist1X = Math.Min(Dist1X, Dist2X);
                        /*to grant seamless I take the min between distY and hei-distY*/
                        Dist1Y = Math.Min(Dist1Y, Dist2Y);


                        float dist = (float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2)); //euclidian metric
                        //float dist = (float)(Dist1X + Dist1Y);//Taxicab metric //http://en.wikipedia.org/wiki/Taxicab_geometry

                        if (dist <= minDist)
                        {
                            minDist3 = minDist2;
                            minDist2 = minDist;
                            minDist = dist;
                            minV = pV;
                        }
                        else
                        {
                            if (dist <= minDist2)
                            {
                                minDist3 = minDist2;
                                minDist2 = dist;
                            }
                            else {
                                if (dist <= minDist3) {
                                    minDist3 = dist;
                                }
                            }
                        }
                    }

                    if (((minDist2 - minDist) + (minDist3 - minDist) < 1f))
                        Values[i, j] = 0;
                    else
                        if ((minDist2 - minDist) + (minDist3 - minDist) < (float)minDelta)
                            Values[i, j] = (((minDist2 - minDist) + (minDist3 - minDist)) / (float)minDelta) * 255f;
                        else
                            Values[i, j] = 255;
                    //Values[i, j] = (minDist2 - minDist);
                    //Values[i, j] = minV;
                    Iterations++;
                }

            }

        }


        private void Voronoi4_3D(float[,,] Values, int wid, int hei,int zlvl, int[,] points, int minDelta)
        {
            for (int i = 0; i < wid; i++)
            {
                for (int j = 0; j < hei; j++)
                {
                    for (int k = 0; k < zlvl; k++)
                    {

                        float minDist = 999999999f;
                        float minDist2 = 999999999f;
                        float minDist3 = 999999999f;
                        int minV = 0;
                        for (int p = 0; p < points.GetLength(0); p++)
                        {
                            int pX = points[p, 0];
                            int pY = points[p, 1];
                            int pZ = points[p, 2];
                            int pV = points[p, 3]; //0..255
                            Double Dist1X = Math.Abs((i - pX));
                            Double Dist1Y = Math.Abs((j - pY));
                            Double Dist1Z = Math.Abs((k - pZ));
                            Double Dist2X = wid - Dist1X;
                            Double Dist2Y = hei - Dist1Y;
                            Double Dist2Z = zlvl - Dist1Z;
                            /*to grant seamless I take the min between distX and wid-distX
                             |                       |
                             |                       |     ----------- = Dist1X
                             |...i-----------X.......|     ..........  = Dist2X
                             |                       |
                             */
                            Dist1X = Math.Min(Dist1X, Dist2X);
                            /*to grant seamless I take the min between distY and hei-distY*/
                            Dist1Y = Math.Min(Dist1Y, Dist2Y);
                            Dist1Z = Math.Min(Dist1Z, Dist2Z);


                            float dist = (float)Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2) + Math.Pow(Dist1Z, 2)); //euclidian metric
                            //float dist = (float)(Dist1X + Dist1Y);//Taxicab metric //http://en.wikipedia.org/wiki/Taxicab_geometry

                            if (dist <= minDist)
                            {
                                minDist3 = minDist2;
                                minDist2 = minDist;
                                minDist = dist;
                                minV = pV;
                            }
                            else
                            {
                                if (dist <= minDist2)
                                {
                                    minDist3 = minDist2;
                                    minDist2 = dist;
                                }
                                else
                                {
                                    if (dist <= minDist3)
                                    {
                                        minDist3 = dist;
                                    }
                                }
                            }
                        }

                        if (((minDist2 - minDist) + (minDist3 - minDist) < 1f))
                            Values[i, j,k] = 0;
                        else
                            if ((minDist2 - minDist) + (minDist3 - minDist) < (float)minDelta)
                                Values[i, j,k] = (((minDist2 - minDist) + (minDist3 - minDist)) / (float)minDelta) * 255f;
                            else
                                Values[i, j,k] = 255;
                        //Values[i, j] = (minDist2 - minDist);
                        //Values[i, j] = minV;
                        Iterations++;
                    }
                }

            }

        }




        private void Crates2(float[,] Values, int wid, int hei, int x, int y, int fromP, int toP, int fromR, int toR)
        {
            
            double power = r.Next(fromP, toP);
            int radius = r.Next(fromR, toR);
            int d = radius * 4;

            for (int i = x-d; i < x+d; i++)
            {
                for (int j = y-d; j < y+d; j++)
                {

                    int I = i % wid;
                    int J = j % hei;

                    if (I < 0)
                        I = wid + I;
                    if (J < 0)
                        J = hei + J;



                    Double Dist1X = Math.Abs((I - x));
                    Double Dist1Y = Math.Abs((J - y));

                    Double Dist2X = wid - Dist1X;
                    Double Dist2Y = hei - Dist1Y;
                    /*to grant seamless I take the min between distX and wid-distX
                     |                       |
                     |                       |     ----------- = Dist1X
                     |...i-----------X.......|     ..........  = Dist2X
                     |                       |
                     */
                    Dist1X = Math.Min(Dist1X, Dist2X);
                    /*to grant seamless I take the min between distY and hei-distY*/
                    Dist1Y = Math.Min(Dist1Y, Dist2Y);

                    double Dist = Math.Sqrt(Math.Pow(Dist1X, 2) + Math.Pow(Dist1Y, 2));
                    if (Dist < 0.001) //avoid division by zero
                        Dist = 0.001;

                    int pixX = I;
                    int pixY = J;

                    int deltaA=0;
                    if (Dist <= radius)
                        deltaA = (int)(power * ((Dist * Dist) / (radius * radius)));
                        //deltaA = (int)(power * Dist);
                    else if (Dist < d)
                            deltaA = (int)(power * (((d-Dist) * (d-Dist)) / (radius * radius)));

                    Values[pixX, pixY] = Values[pixX, pixY] + deltaA;


                    Iterations++;
                }




            }


        }


        private void updBitmap(float[,] Values, Bitmap tmp, bool greyscale)
        {
            updBitmap(Values, tmp, greyscale, 0f, 255f);
        }

        private void updBitmap(float[,] Values, Bitmap tmp, bool greyscale, float minLimit, float maxLimit)
        {
            for (int i = 0; i < Values.GetLength(0); i++)
            {
                for (int j = 0; j < Values.GetLength(1); j++)
                {
                    float v = Values[i, j];

                    if (v < minLimit)
                        v = 0;

                    if (v > maxLimit)
                        v = 255f;

                    Color clr;
                    if (greyscale)
                        clr = Color.FromArgb((int)v, (int)v, (int)v);
                    else
                        clr = Color.FromArgb((int)v, 255, 255, 255);
                    tmp.SetPixel(i, j, clr);

                }
            }

        }

        private void animateBitmap(float[,,] Values, Bitmap tmp, bool greyscale)
        {

            //for (int k = 0; k < Values.GetLength(2); k++) //z is time
            int k= 0;
            while (!Noise3dSTOP) 
            {


                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        float v = Values[i, j,k] ;

                        if (v < 0)
                            v = 0;

                        if (v > 255f)
                            v = 255f;

                        Color clr;
                        if (greyscale)
                            clr = Color.FromArgb((int)v, (int)v, (int)v);
                        else
                            clr = Color.FromArgb((int)v, 255, 255, 255);
                        tmp.SetPixel(i, j, clr);

                    }
                }

                //refresh 
                pictureBox1.Image = tmp;
                pictureBox1.Refresh();

                pictureBox2.Image = tmp;
                pictureBox3.Image = tmp;
                pictureBox4.Image = tmp;
                pictureBox5.Image = tmp;
                pictureBox2.Refresh();
                pictureBox3.Refresh();
                pictureBox4.Refresh();
                pictureBox5.Refresh();

                Application.DoEvents();

                k = k + 1;
                k = k % (Values.GetLength(2));

                FrameLbl.Text = "Frame: " + k.ToString() + "/" + Values.GetLength(2).ToString();

                if (Noise3dSTOP) {
                    Noise3dSTOP = false;
                    return;
                } 
            }
        }


        private void updBitmapScaled(float[,] Values, Bitmap tmp, bool greyscale)
        {
            float max = 0f;
            float min = 255f;
            for (int i = 0; i < Values.GetLength(0); i++)
            {
                for (int j = 0; j < Values.GetLength(1); j++)
                {
                    if (Values[i, j] > max)
                        max = Values[i, j];
                    if (Values[i, j] < min)
                        min = Values[i, j];
                }
            }

            float d = max - min;

            for (int i = 0; i < Values.GetLength(0); i++)
            {
                for (int j = 0; j < Values.GetLength(1); j++)
                {
                    float v = (Values[i, j]/d) *255f;

                    if (v < 0)
                        v = 0;

                    if (v > 255f)
                        v = 255f;

                    Color clr;
                    if (greyscale)
                        clr = Color.FromArgb((int)v, (int)v, (int)v);
                    else
                        clr = Color.FromArgb((int)v, 255, 255, 255);
                    tmp.SetPixel(i, j, clr);

                }
            }

        }


        private void animate(float[,] Values) {

            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            updBitmap(Values, tmp, grayScale.Checked);

            pictureBox1.Image = tmp;
            pictureBox1.Refresh();
            this.Refresh();
            Application.DoEvents();

        }

        private Random getrRandom()
        {


            if (randomChk.Checked)
            {
                int seed = (int)DateTime.Now.Ticks;
                seedTxt.Text = seed.ToString();
                return new Random(seed);
            }
            else
            {
                int seed;
                if (int.TryParse(seedTxt.Text, out seed))
                {
                    return new Random(seed);
                }
                else
                {
                    seed = (int)DateTime.Now.Ticks;
                    seedTxt.Text = seed.ToString();
                    return new Random(seed);
                }

            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            DateTime t = new DateTime();
            t = DateTime.Now;
            
            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            float[,] Values = new float[pictureBox1.Width, pictureBox1.Height];


            int wid = pictureBox1.Width;
            int hei = pictureBox1.Height;

            int N_Repetitions = 20;
            int box_size = 100;
            int Godeep = 9;
            
            if (delta.Text == "")
                delta.Text = "1";
            int d = int.Parse(delta.Text);

            if (Repetitions.Text == "")
                Repetitions.Text = N_Repetitions.ToString();
            N_Repetitions = int.Parse(Repetitions.Text);

            if (Recursion.Text == "")
                Recursion.Text = Godeep.ToString();
            Godeep = int.Parse(Recursion.Text);

            if (BoxPerc.Text == "")
                BoxPerc.Text = "25";
            box_size = (int)(Math.Min(wid, hei) * float.Parse(BoxPerc.Text)/100f);

            Iterations = 0;

            bool useValueArray = false;

            //r = new System.Random((int)DateTime.Now.Ticks);
            r = getrRandom();

            for (int nr = 0; nr < N_Repetitions; nr++) {

                //get a random point
                int x = r.Next(0, wid - 1);
                int y = r.Next(0, hei - 1);



                switch (FunctionCbo.Text.ToUpper())
                {
                    case "QUAD":
                        printQuad(Values, wid, hei, x, y, box_size, Godeep, d);
                        useValueArray = true;                       
                        break;
                    case "CIRC":
                        printCirc(Values, wid, hei, x, y, box_size, Godeep, d);
                        useValueArray = true;                       
                        break;
                    case "CRATES":                        
                        printCrates(Values, wid, hei, x, y, r.Next(box_size/5,box_size), Godeep, d);
                        useValueArray = true;  
                        //Crates(tmp, wid, hei, x, y, r.Next(box_size / 5, box_size), Godeep, d);
                        break;

                    case "FUNC":

                        // use lambda function
                        //TORUS
                        f torus  = (x1,y1) => (float)Math.Sqrt((0.4f * 0.4f - Math.Pow((0.6f - Math.Sqrt(x1 * x1 + y1 * y1)), 2)))
                            /0.4f;
                        //0.4 is the tube radius. I divide by 0.4 to scale 0 <= z <= 1
                        //0.6 is the torus radius , distance from origin


                        f paraboloid =  (x1,y1) => 1f + (float) ((x1*x1)/1f + (y1*y1)/1f) * -1f;

                        f test = (x1, y1) => 1f - (float)(Math.Pow(Math.Sin(4 * x1), 2) + Math.Pow(Math.Sin(4 * y1), 2));


                        printFunc(Values, wid, hei, x, y, box_size, Godeep, d, torus);
                        useValueArray = true;                       
                        break;

                        

                    default:

                        break;
                }

                //

            
            }

            if (useValueArray)
            {
                if (scaledChk.Checked)
                    updBitmapScaled(Values, tmp, grayScale.Checked);
                else
                    updBitmap(Values, tmp, grayScale.Checked, float.Parse(minLimitTxt.Text), float.Parse(maxLimitTxt.Text));
            }


            pictureBox1.Image = tmp;
            pictureBox1.Refresh();

            //double a = pseudoRandom(238, 24);

            pictureBox2.Image = tmp;
            pictureBox3.Image = tmp;
            pictureBox4.Image = tmp;
            pictureBox5.Image = tmp;

            IterationsTxt.Text = Iterations.ToString();
            timeTxt.Text = ( DateTime.Now-t).TotalSeconds.ToString();
        }


        private void button3_Click(object sender, EventArgs e)
        {

            DateTime t = new DateTime();
            t = DateTime.Now;

            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            float[,] Values = new float[pictureBox1.Width, pictureBox1.Height];

            int wid = pictureBox1.Width;
            int hei = pictureBox1.Height;

            int N_Repetitions = 10;
            N_Repetitions = int.Parse(pointNumTxt.Text);

            bool useValueArray = false;

            Iterations = 0;

            //r = new System.Random((int)DateTime.Now.Ticks);
            r = getrRandom();

            for (int nr = 0; nr < N_Repetitions; nr++)
            {

                //get a random point
                int x = r.Next(0, wid - 1);
                int y = r.Next(0, hei - 1);

                int fromP = int.Parse(PowerFromTxt.Text);
                int toP = int.Parse(PowerToTxt.Text);
                int fromR = int.Parse(CrSizeFrom.Text);
                int toR = int.Parse(CrSizeTo.Text);


                switch (cratesFunctionCbo.Text.ToUpper())
                {
                    case "CRATES":
                        Crates(Values, wid, hei, x, y, fromP, toP, fromR, toR);
                        useValueArray = true;
                        break;
                    case "CRATES2":
                        Crates2(Values, wid, hei, x, y, fromP, toP, fromR, toR);
                        useValueArray = true;
                        break;

                    case "STARFIELD":
                        Starfield(Values, wid, hei, x, y, fromP, toP, fromR, toR);
                        useValueArray = true;
                        break;

                    case "LANDSCAPE1":
                        landscape1(Values, wid, hei, x, y, fromP, toP, fromR, toR);
                        useValueArray = true;
                        break;


                    default:

                        break;
                }

                //

            }

            if (useValueArray)
            {
                if (scaledChk.Checked)
                    updBitmapScaled(Values, tmp, grayScale.Checked);
                else
                    updBitmap(Values, tmp, grayScale.Checked, float.Parse(minLimitTxt.Text), float.Parse(maxLimitTxt.Text));
            }

            pictureBox1.Image = tmp;
            pictureBox1.Refresh();

            //double a = pseudoRandom(238, 24);

            pictureBox2.Image = tmp;
            pictureBox3.Image = tmp;
            pictureBox4.Image = tmp;
            pictureBox5.Image = tmp;

            IterationsTxt.Text = Iterations.ToString();
            timeTxt.Text = (DateTime.Now - t).TotalSeconds.ToString();



        }

        private void button4_Click(object sender, EventArgs e)
        {



            DateTime t = new DateTime();
            t = DateTime.Now;

            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            float[,] Values = new float[pictureBox1.Width, pictureBox1.Height];

            int wid = pictureBox1.Width;
            int hei = pictureBox1.Height;

            int N_Repetitions = 10;
            N_Repetitions = int.Parse(pointsNumTxt.Text);

            int minDelta = 5;
            if (!int.TryParse(minDeltadTxt.Text, out minDelta))
                minDelta = 0;

            bool useValueArray = false;

            Iterations = 0;

            //r = new System.Random((int)DateTime.Now.Ticks);
            r = getrRandom();

            int[,] points = new int[N_Repetitions,3];

            for (int nr = 0; nr < N_Repetitions; nr++)
            { //valorize points
                points[nr, 0] = r.Next(0, wid - 1);
                points[nr, 1] = r.Next(0, hei - 1);
                points[nr, 2] = r.Next(0, 255);                        
                //

            }



            switch (VORcbo.Text.ToUpper())
            {
                case "VORONOI":
                    Voronoi(Values, wid, hei, points);
                    useValueArray = true;
                    break;
                case "VORONOI2":
                    Voronoi2(Values, wid, hei, points);
                    useValueArray = true;
                    break;
                case "VORONOI3":
                    Voronoi3(Values, wid, hei, points,minDelta);
                    useValueArray = true;
                    break;

                case "VORONOI4":
                    Voronoi4(Values, wid, hei, points, minDelta);
                    useValueArray = true;
                    break;


                default:
                    break;
            }


            if (useValueArray)
            {
                if (scaledChk.Checked)
                    updBitmapScaled(Values, tmp, grayScale.Checked);
                else
                    updBitmap(Values, tmp, grayScale.Checked, float.Parse(minLimitTxt.Text), float.Parse(maxLimitTxt.Text));
            }

            if (showpointsChk.Checked) {
                Color cc = Color.Red;
                for (int nr = 0; nr < N_Repetitions; nr++)
                {
                    //make the last points lighter // for test/debug purpose
                    int Dr = cc.R + nr * 5;
                    if (Dr > 255)
                        Dr = 255;
                    int Dg = cc.G + nr * 5;
                    if (Dg > 255)
                        Dg = 255;
                    int Db = cc.B + nr * 5;
                    if (Db > 255)
                        Db = 255;
                    Color c = Color.FromArgb(Dr, Dg, Db);
                    

                    tmp.SetPixel(points[nr, 0], points[nr, 1],c);
                    tmp.SetPixel(points[nr, 0]+1, points[nr, 1]+1, c);
                    tmp.SetPixel(points[nr, 0]+1, points[nr, 1], c);
                    tmp.SetPixel(points[nr, 0], points[nr, 1]+1, c);
                }
            }

            pictureBox1.Image = tmp;
            pictureBox1.Refresh();

            //double a = pseudoRandom(238, 24);

            pictureBox2.Image = tmp;
            pictureBox3.Image = tmp;
            pictureBox4.Image = tmp;
            pictureBox5.Image = tmp;

            IterationsTxt.Text = Iterations.ToString();
            timeTxt.Text = (DateTime.Now - t).TotalSeconds.ToString();



        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Noise3dSTOP = false;

            DateTime t = new DateTime();
            t = DateTime.Now;

            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            int wid = pictureBox1.Width;
            int hei = pictureBox1.Height;
            int zlvl = 128;

            int N_Repetitions = 20;
            int box_size = 100;
            int Godeep = 9;

            if (delta.Text == "")
                delta.Text = "1";
            int d = int.Parse(delta.Text);

            if (Repetitions.Text == "")
                Repetitions.Text = N_Repetitions.ToString();
            N_Repetitions = int.Parse(Repetitions.Text);

            if (Recursion.Text == "")
                Recursion.Text = Godeep.ToString();
            Godeep = int.Parse(Recursion.Text);

            if (BoxPerc.Text == "")
                BoxPerc.Text = "25";
            box_size = (int)(Math.Min(wid, hei) * float.Parse(BoxPerc.Text) / 100f);



            if (!int.TryParse(animFrameNrTxt.Text, out zlvl))
                zlvl = 128;

            if (box_size > zlvl)
            {
                zlvl = box_size;
                animFrameNrTxt.Text = zlvl.ToString();
            }

            float[, ,] Values = new float[wid, hei, zlvl];


            Iterations = 0;

            bool useValueArray = false;

            //r = new System.Random((int)DateTime.Now.Ticks);
            r = getrRandom();

            for (int nr = 0; nr < N_Repetitions; nr++)
            {

                //get a random point
                int x = r.Next(0, wid - 1);
                int y = r.Next(0, hei - 1);
                int z = r.Next(0, zlvl - 1);


                switch (FunctionCbo.Text.ToUpper())
                {
                    case "QUAD":
                        printQuad3D(Values, wid, hei,zlvl, x, y,z, box_size, Godeep, d);
                        useValueArray = true;
                        break;

                    case "CIRC":
                        printCirc3D(Values, wid, hei,zlvl, x, y,z, box_size, Godeep, d);;
                        useValueArray = true;
                        break;

                    default:

                        break;
                }

                //


            }

            IterationsTxt.Text = Iterations.ToString();
            timeTxt.Text = (DateTime.Now - t).TotalSeconds.ToString();

            this.Cursor = Cursors.Default;

            animateBitmap(Values, tmp, grayScale.Checked);

            pictureBox1.Image = tmp;
            pictureBox1.Refresh();

            //double a = pseudoRandom(238, 24);

            pictureBox2.Image = tmp;
            pictureBox3.Image = tmp;
            pictureBox4.Image = tmp;
            pictureBox5.Image = tmp;






        }

        private void button6_Click(object sender, EventArgs e)
        {
            Noise3dSTOP = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Noise3dSTOP = false;

            DateTime t = new DateTime();
            t = DateTime.Now;

            Bitmap tmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            int wid = pictureBox1.Width;
            int hei = pictureBox1.Height;
            int zlvl = 128;
            if (! int.TryParse(vorFrameTxt.Text, out zlvl))
                zlvl = 128;

            float[,,] Values = new float[wid,hei,zlvl];


            int N_Repetitions = 10;
            N_Repetitions = int.Parse(pointsNumTxt.Text);

            int minDelta = 5;
            if (!int.TryParse(minDeltadTxt.Text, out minDelta))
                minDelta = 0;

            bool useValueArray = false;

            Iterations = 0;

            //r = new System.Random((int)DateTime.Now.Ticks);
            r = getrRandom();

            int[,] points = new int[N_Repetitions, 4];

            for (int nr = 0; nr < N_Repetitions; nr++)
            { //valorize points
                points[nr, 0] = r.Next(0, wid - 1);
                points[nr, 1] = r.Next(0, hei - 1);
                points[nr, 2] = r.Next(0, zlvl - 1);
                points[nr, 3] = r.Next(0, 255);
                //
            }


            switch (VORcbo.Text.ToUpper())
            {
                case "VORONOI":
                case "VORONOI3" :
                case "VORONOI2" :
                    Voronoi3_3D(Values, wid, hei, zlvl, points, minDelta);
                    useValueArray = true;
                    break;
                case "VORONOI4":
                    Voronoi4_3D(Values, wid, hei, zlvl, points, minDelta);
                    useValueArray = true;
                    break;

                default :
                    break;

            }

            IterationsTxt.Text = Iterations.ToString();
            timeTxt.Text = (DateTime.Now - t).TotalSeconds.ToString();

            this.Cursor = Cursors.Default;

            animateBitmap(Values, tmp, grayScale.Checked);

            pictureBox1.Image = tmp;
            pictureBox1.Refresh();

            //double a = pseudoRandom(238, 24);

            pictureBox2.Image = tmp;
            pictureBox3.Image = tmp;
            pictureBox4.Image = tmp;
            pictureBox5.Image = tmp;




        }

        private void button7_Click(object sender, EventArgs e)
        {
            Noise3dSTOP = true;
        }




    }
}
