/*
 * Practica 07
 * Steven Silva
 * Fecha de realización: 31 de mayo de 2026
 * Fecha de entrega: 03 de junio de 2026        
 * Resultados:
 * Este archivo contiene la interfaz principal del cliente.
 * El cliente permite ingresar datos, conectarse al servidor y enviar solicitudes.
 * Como modificación de la práctica, el cliente ya no construye directamente todo el proceso
 * de comunicación, sino que utiliza la clase Protocolo para ejecutar el método HazOperacion.
 */

using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;
// Se usa un alias para poder trabajar con la clase Protocolo
// sin confundirla con el nombre del proyecto Protocolo.
using GestorProtocolo = Protocolo.Protocolo;
namespace Cliente
{
    public partial class FrmValidador : Form
    {
        // Se crea una instancia de la clase Protocolo.
        // El cliente usará esta clase para enviar pedidos y recibir respuestas del servidor.
        private GestorProtocolo protocolo = new GestorProtocolo();
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se puedo establecer conexión " + ex.Message,
                    "ERROR");
            }
            finally 
            {
                flujo?.Close();
                remoto?.Close();
            }

            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;
            if (usuario == "" || contraseña == "")
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña",
                    "ADVERTENCIA");
                return;
            }

            // Se envía el comando INGRESO usando la clase Protocolo.
            // Ya no se crea manualmente un objeto Pedido en el formulario.
            Respuesta respuesta = HazOperacion("INGRESO", usuario, contraseña);

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales",
                    "ERROR");
                txtUsuario.Focus();
            }
        }

        // Este método reemplaza la forma anterior de enviar pedidos desde el cliente.
        // Ahora el cliente solo envía el comando y los parámetros,
        // mientras que la clase Protocolo se encarga de crear el Pedido,
        // enviarlo al servidor y recibir la Respuesta.
        private Respuesta HazOperacion(string comando, params string[] parametros)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();

                Respuesta respuesta = protocolo.HazOperacion(flujo, comando, parametros);

                return respuesta;
            }
            // Se controla el error en caso de que el cliente no pueda conectarse al servidor.
            catch (SocketException ex)
            {
                MessageBox.Show("Error al intentar transmitir: " + ex.Message, "ERROR");
                return null;
            }
            // Se cierran los recursos de red para evitar conexiones abiertas innecesarias.
            finally
            {
                flujo?.Close();
                remoto?.Close();
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Se envía el comando CALCULO usando la clase Protocolo.
            // Los datos del vehículo se pasan como parámetros.
            Respuesta respuesta = HazOperacion("CALCULO", modelo, marca, placa);

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje,
                    "INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                switch (resultado)
                {
                    case 0b00100000:
                        chkLunes.Checked = true;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00010000:
                        chkMartes.Checked = true;
                        chkLunes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00001000:
                        chkMiercoles.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000100:
                        chkJueves.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000010:
                        chkViernes.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        break;
                    default:
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                }
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            // Se envía el comando CONTADOR usando la clase Protocolo.
            // Este comando permite consultar cuántas solicitudes ha realizado el cliente.
            Respuesta respuesta = HazOperacion("CONTADOR", "consulta");

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");

            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0],
                    "INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (flujo != null)
                flujo.Close();
            if (remoto != null)
                if (remoto.Connected)
                    remoto.Close();
        }
    }
}
