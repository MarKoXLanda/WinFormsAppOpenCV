using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenCvSharp;

namespace WinFormsAppOpenCV
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer relojAnimacion;
        private VideoCapture video;
        private Mat frame;

        
        int columnas = 160;
        int filas = 120;
        int tamañoPixel = 9;

        int offsetX = 0;
        int offsetY = 0;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            int anchoDibujo = columnas * tamañoPixel;
            int altoDibujo = filas * tamañoPixel;
            offsetX = (Screen.PrimaryScreen.Bounds.Width - anchoDibujo) / 2;
            offsetY = (Screen.PrimaryScreen.Bounds.Height - altoDibujo) / 2;

            PrepararVideo();
            IniciarAnimacion();
        }

        private void PrepararVideo()
        {
            video = new VideoCapture("badapple.mp4");
            frame = new Mat();
        }

        private void IniciarAnimacion()
        {
            relojAnimacion = new System.Windows.Forms.Timer();
            //16 60fps
            // Forzamos el reloj a 45 FPS (1000 ms / 45 frames = 22)
            relojAnimacion.Interval = 31;

            relojAnimacion.Tick += RelojAnimacion_Tick;
            relojAnimacion.Start();
        }

        private void RelojAnimacion_Tick(object sender, EventArgs e)
        {
            if (video == null || !video.IsOpened()) return;

            video.Read(frame);
            if (frame.Empty())
            {
                video.Set(VideoCaptureProperties.PosFrames, 0);
                return;
            }

            Mat frameReducido = new Mat();
            Cv2.Resize(frame, frameReducido, new OpenCvSharp.Size(columnas, filas));
            Cv2.CvtColor(frameReducido, frameReducido, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(frameReducido, frameReducido, 127, 255, ThresholdTypes.Binary);

            GraphicsPath rutaPoligonos = new GraphicsPath();

            
            for (int y = 0; y < filas; y++)
            {
                int inicioX = -1; // Marcador para saber donde empieza una linea blanca

                for (int x = 0; x < columnas; x++)
                {
                    byte valorPixel = frameReducido.At<byte>(y, x);

                    if (valorPixel > 0) // Es BLANCO
                    {
                        if (inicioX == -1)
                        {
                            inicioX = x; // Registramos el inicio de la linea blanca
                        }
                    }
                    else // Es NEGRO
                    {
                        if (inicioX != -1) // Si veniamos dibujando una linea blanca, la cerramos
                        {
                            int anchoLinea = x - inicioX;
                            rutaPoligonos.AddRectangle(new Rectangle(
                                offsetX + (inicioX * tamañoPixel),
                                offsetY + (y * tamañoPixel),
                                anchoLinea * tamañoPixel, // Ancho combinado
                                tamañoPixel));

                            inicioX = -1; // Reiniciamos el marcador
                        }
                    }
                }

                // Caso especial: si la línea blanca llego hasta el final del borde derecho de la pantalla
                if (inicioX != -1)
                {
                    int anchoLinea = columnas - inicioX;
                    rutaPoligonos.AddRectangle(new Rectangle(
                        offsetX + (inicioX * tamañoPixel),
                        offsetY + (y * tamañoPixel),
                        anchoLinea * tamañoPixel,
                        tamañoPixel));
                }
            }

            // Liberamos la región vieja ANTES de aplicar la nueva (previene las fugas de memoria)
            if (this.Region != null)
            {
                this.Region.Dispose();
            }

            this.Region = new Region(rutaPoligonos);

            // Liberamos memoria de objetos que ya no necesitamos en este frame
            rutaPoligonos.Dispose();
            frameReducido.Dispose();
        }
    }
}