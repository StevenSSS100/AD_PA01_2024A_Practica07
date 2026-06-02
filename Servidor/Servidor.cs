/*
 * Practica 07
 * Steven Silva
 * Fecha de realización: 31 de mayo de 2026
 * Fecha de entrega: 03 de junio de 2026        
 * Resultados:
 * Este archivo contiene la lógica principal del servidor.
 * El servidor recibe las solicitudes enviadas por el cliente mediante sockets.
 * Como modificación de la práctica, el servidor ya no resuelve directamente los pedidos,
 * sino que utiliza la clase Protocolo para procesar el mensaje recibido y generar la respuesta.
 */

using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;
// Se usa un alias para evitar confusión entre el nombre del proyecto Protocolo
// y la nueva clase Protocolo creada dentro de ese proyecto.
using GestorProtocolo = Protocolo.Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;

        // Se crea una instancia de la clase Protocolo.
        // Esta instancia será utilizada por el servidor para resolver los pedidos recibidos.
        private static GestorProtocolo protocolo = new GestorProtocolo();

        

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                // Se corrigió el mensaje mostrado en consola.
                // El servidor trabaja en el puerto 8080, por lo tanto el mensaje debe indicar ese puerto.
                Console.WriteLine("Servidor inició en el puerto 8080...");
                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " +
                    ex.Message);
            }
            finally 
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;
            try
            {
                flujo = cliente.GetStream();
                byte[] bufferTx;
                // El servidor recibe el mensaje enviado por el cliente en formato de texto.
                // Luego este mensaje será enviado a la clase Protocolo para ser procesado.
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx =
                        Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    // modificacion: se agrega la impresión del mensaje recibido para facilitar la depuración y seguimiento de las solicitudes de los clientes.
                    Console.WriteLine("Se recibió: " + mensajeRx);

                    string direccionCliente =
                        ((IPEndPoint)cliente.Client.RemoteEndPoint).Address.ToString();

                    // Antes el servidor resolvía directamente el pedido.
                    // Ahora se envía el mensaje recibido a la clase Protocolo,
                    // que se encarga de interpretar el comando y generar la respuesta.
                    Respuesta respuesta = protocolo.ResolverPedido(mensajeRx, direccionCliente);

                    Console.WriteLine("Se envió: " + respuesta);

                    // La respuesta generada por la clase Protocolo se convierte a texto
                    // y se envía nuevamente al cliente mediante el flujo de red.
                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }


        

    }
}
