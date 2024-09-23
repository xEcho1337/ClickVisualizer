using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ClickVisualizer
{
    public partial class MainForm : Form
    {
        private List<double> CurrentDelays;
        private string CurrentFile;

        public MainForm()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisX.Title = "Index";
            chart1.ChartAreas[0].AxisY.Title = "Delay";
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 5000;
            chart1.ChartAreas[0].AxisY.Minimum = -5;
            chart1.ChartAreas[0].AxisY.Interval = 0.2;
            chart1.ChartAreas[0].AxisY.Maximum = 5;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;

            chart1.MouseWheel += new MouseEventHandler(chart1_MouseWheel);
            CheckForIllegalCrossThreadCalls = false;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    CurrentFile = filePath;

                    LoadDataFromFile(filePath);

                    this.Text = $"ClickVisualizer - Viewing {filePath}";
                }
            }
        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                var chart = (Chart)sender;
                var xAxis = chart.ChartAreas[0].AxisX;
                var yAxis = chart.ChartAreas[0].AxisY;

                if (e.Delta < 0)
                {
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0)
                {
                    var posXStart = xAxis.ScaleView.Position;
                    var posXEnd = xAxis.ScaleView.Position + xAxis.ScaleView.Size;
                    var posYStart = yAxis.ScaleView.Position;
                    var posYEnd = yAxis.ScaleView.Position + yAxis.ScaleView.Size;

                    xAxis.ScaleView.Zoom(posXStart + (posXEnd - posXStart) / 4, posXEnd - (posXEnd - posXStart) / 4);
                    yAxis.ScaleView.Zoom(posYStart + (posYEnd - posYStart) / 4, posYEnd - (posYEnd - posYStart) / 4);
                }
            }
            catch { }
        }

        private void LoadDataFromFile(string filePath)
        {
            try
            {
                Series pointSeries = new Series("Delays")
                {
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = MarkerStyle.Circle
                };

                string[] lines = File.ReadAllLines(filePath);
                var delays = new double[lines.Length];

                for (int i = 0; i < lines.Length && i <= 5000; i++)
                {
                    if (double.TryParse(lines[i], out double delay))
                    {
                        delays[i] = delay;
                    }
                }

                CurrentDelays = new List<double>(delays);
                Task.Run(RecalculateCollection);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante la lettura del file: " + ex.Message);
            }
        }

        private void RecalculateCollection()
        {
            int intervalSize = trackBar1.Value;

            trackBar1.Enabled = false;

            Series skewnessSeries = CreateSeries("Skewness", Color.Pink);
            Series stdSeries = CreateSeries("Standard Deviation", Color.Red);
            Series entropySeries = CreateSeries("Entropy", Color.Green);
            Series autocorrSeries = CreateSeries("Autocorrelation", Color.Purple);
            Series kurtosisSeries = CreateSeries("Kurtosis", Color.Orange);
            Series giniSeries = CreateSeries("Gini Coefficient", Color.Blue);
            Series covSeries = CreateSeries("Coeff. of variation", Color.BlueViolet);
            Series cpsSeries = CreateSeries("CPS", Color.Aqua);

            var delays = new List<double>(CurrentDelays);

            for (int i = 0; i < Math.Min(delays.Count, 5000); i += intervalSize)
            {
                int end = Math.Min(i + intervalSize, delays.Count);
                var interval = delays.Skip(i).Take(end - i);

                if (interval.Count() != intervalSize) continue;

                double entropy = Features.CalculateEntropy(interval);
                double autocorrelation = Features.CalculateAutocorrelation(interval);
                double kurtosis = Features.CalculateKurtosis(interval);
                double giniCoefficient = Features.CalculateGiniCoefficient(interval);
                double skewness = Features.CalculateSkewness(interval);

                double stdDev = Features.CalculateStandardDeviation(interval);
                double mean = Features.CalculateMean(interval);

                double coeffOfVariation = stdDev / mean;
                int xPosition = i + intervalSize / 2;

                cpsSeries.Points.AddXY(xPosition, 1000 / mean);
                stdSeries.Points.AddXY(xPosition, stdDev);

                skewnessSeries.Points.AddXY(xPosition, skewness);
                autocorrSeries.Points.AddXY(xPosition, autocorrelation);
                entropySeries.Points.AddXY(xPosition, entropy);
                kurtosisSeries.Points.AddXY(xPosition, kurtosis);
                covSeries.Points.AddXY(xPosition, coeffOfVariation);
                giniSeries.Points.AddXY(xPosition, giniCoefficient);
            }

            chart1.Series.Clear();
            chart1.Series.Add(skewnessSeries);

            if (checkBox1.Checked)
            {
                chart1.Series.Add(stdSeries);
            }

            if (checkBox2.Checked)
            {
                chart1.Series.Add(autocorrSeries);
            }

            if (checkBox3.Checked)
            {
                chart1.Series.Add(cpsSeries);
            }

            chart1.Series.Add(covSeries);
            chart1.Series.Add(entropySeries);
            chart1.Series.Add(giniSeries);
            
            trackBar1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(chart1.Width, chart1.Height))
                {
                    chart1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "PNG Image|*.png";
                        saveFileDialog.Title = "Save Chart Image";
                        saveFileDialog.FileName = "ChartScreenshot.png";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            bmp.Save(saveFileDialog.FileName, ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving screenshot: " + ex.Message);
            }
        }

        private Series CreateSeries(string name, Color color)
        {
            return new Series(name)
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = color
            };
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(chart1.Width, chart1.Height))
                {
                    chart1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

                    Clipboard.SetImage(bmp);
                    MessageBox.Show("Chart copied to clipboard.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error copying chart to clipboard: " + ex.Message);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = "Samples: " + trackBar1.Value;
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            label1.Text = "Samples: " + trackBar1.Value;

            if (CurrentFile == null || CurrentDelays == null) return;

            Task.Run(RecalculateCollection);
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            var chartArea = chart1.ChartAreas[0];

            if (CurrentFile == null || CurrentDelays == null) return;

            double width = chart1.Width;
            double height = chart1.Height;

            double minX = Math.Max(e.X, 0);
            double minY = Math.Max(e.Y, 0);

            var xValue = chartArea.AxisX.PixelPositionToValue(Math.Min(minX, width - 1));
            var yValue = chartArea.AxisY.PixelPositionToValue(Math.Min(minY, height - 1));

            toolTip1.SetToolTip(chart1, $"X: {xValue:F2}, Y: {yValue:F2}");
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            label2.Text = $"Minimum: {numericUpDown1.Value}";

            if (numericUpDown2.Value < numericUpDown1.Value)
            {
                MessageBox.Show("The minimum value must be lower than the maximum value!");
                numericUpDown1.Value = -5;
            }

            chart1.ChartAreas[0].AxisY.Minimum = (double) numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            label3.Text = $"Maximum: {numericUpDown2.Value}";

            if (numericUpDown2.Value < numericUpDown1.Value)
            {
                MessageBox.Show("The maximum value must be greater than the minimum value!");
                numericUpDown2.Value = 5;
            }

            chart1.ChartAreas[0].AxisY.Maximum = (double) numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            label4.Text = $"Step: {numericUpDown3.Value}";

            chart1.ChartAreas[0].AxisY.Interval = (double) numericUpDown3.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Task.Run(RecalculateCollection);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Task.Run(RecalculateCollection);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Task.Run(RecalculateCollection);
        }
    }
}
