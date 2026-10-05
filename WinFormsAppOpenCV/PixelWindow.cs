using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

public class PixelWindow : Form
{
    public PixelWindow()
    {
        // Quitamos los bordes y la barra superior para que sea un bloque puro
        this.FormBorderStyle = FormBorderStyle.None;

        // Hacemos que la ventana sea de color blanco puro
        this.BackColor = Color.White;

        // Evitamos que aparezca en la barra de tareas para no saturarla
        this.ShowInTaskbar = false;

        // Tamaño inicial pequeño
        this.Size = new Size(50, 50);

        // Inicia en el centro de la pantalla
        this.StartPosition = FormStartPosition.CenterScreen;
    }
}