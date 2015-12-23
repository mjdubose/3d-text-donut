using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace _3dtextdonut
{
    public partial class Form1 : Form
    {
        private const int Screensize = 50;
        private const int Screensizey = 50;

        private const double ThetaSpacing = 0.07;

        private const double PhiSpacing = 0.02;

        private const double R1 = 1.0;
        private const double R2 = 2.0;
        private const double K2 = 5.0;
        private const double Pi = Math.PI;
        private const double K1 = Screensizey*K2*3/(8*(R1 + R2));

        private readonly char[] _charvalues = new char[12];

        public Form1()
        {
            InitializeComponent();
            bgWorker.DoWork += BgWorkerDoWork;
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            try
            {
                if (textBox1.InvokeRequired)
                {
                    var d = new SetTextCallback(SetText);
                    Invoke(d, text);
                }
                else
                {
                    textBox1.Text = text;
                    Refresh();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void RenderFrame(double a, double b)
        {
            var cosA = Math.Cos(a);
            var sinA = Math.Sin(a);
            var cosB = Math.Cos(b);
            var sinB = Math.Sin(b);

            var output = new char[Screensizey, Screensize];

            for (var x = 0; x < Screensize; x++)
            {
                for (var y = 0; y < Screensizey; y++)
                {
                    output[y, x] = ' ';
                }
            }

            var zbuffer = new double[Screensizey, Screensize];
            for (var x = 0; x < Screensize; x++)
            {
                for (var y = 0; y < Screensizey; y++)
                {
                    zbuffer[y, x] = 0;
                }
            }

            for (double theta = 0; theta < 2*Pi; theta += ThetaSpacing)
            {
                var costheta = Math.Cos(theta);
                var sintheta = Math.Sin(theta);

                for (double phi = 0; phi < 2*Pi; phi += PhiSpacing)
                {
                    var cosphi = Math.Cos(phi);
                    var sinphi = Math.Sin(phi);

                    var circlex = R2 + R1*costheta;
                    var circley = R1*sintheta;

                    var x = circlex*(cosB*cosphi + sinA*sinB*sinphi) - circley*cosA*sinB;
                    var y = circlex*(sinB*cosphi - sinA*cosB*sinphi) + circley*cosA*cosB;
                    var z = K2 + cosA*circlex*sinphi + circley*sinA;
                    var ooz = 1/z;

                    var xp = (int) (Screensize/2 + K1*ooz*x);
                    var yp = (int) (Screensizey/2 - K1*ooz*y);

                    var l = cosphi*costheta*sinB - cosA*costheta*sinphi - sinA*sintheta +
                            cosB*(cosA*sintheta - costheta*sinA*sinphi);


                    if (l < 0) continue;
                    if (ooz < zbuffer[yp, xp]) continue;
                    zbuffer[yp, xp] = ooz;
                    var luminanceIndex = (int) (l*8);

                    output[yp, xp] = _charvalues[luminanceIndex];
                }
            }

            var displayvalues = new StringBuilder();
            //  string displayvalues = string.Empty;
            var endstring = 0;
            var endoflinecount = Screensize - 1;
            foreach (var h in output)
            {
                if (endstring == endoflinecount)
                {
                    displayvalues.Append(Environment.NewLine);
                    endoflinecount += Screensize;
                    endstring++;
                }
                else
                {
                    displayvalues.Append(h);
                    endstring++;
                }
            }

            SetText(displayvalues.ToString());
        }

        private void BtnDrawClick(object sender, EventArgs e)
        {
            if (bgWorker.IsBusy != true)
            {
                bgWorker.RunWorkerAsync();
            }

            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }

        private void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _charvalues[0] = '.';
            _charvalues[1] = ',';
            _charvalues[2] = '-';
            _charvalues[3] = '~';
            _charvalues[4] = ':';
            _charvalues[5] = ';';
            _charvalues[6] = '!';
            _charvalues[7] = '=';
            _charvalues[8] = '*';
            _charvalues[9] = '#';
            _charvalues[10] = '$';
            _charvalues[11] = '@';


            for (double q = 1; q < 100;)
            {
                for (double q1 = 1; q1 < 360;)
                {
                    RenderFrame(q1, q);

                    q1 = q1 + .001;
                }
                q = q + .05;
            }
        }

        private delegate void SetTextCallback(string text);
    }
}