/*
 * Practica 07
 * Steven Silva
 * Fecha de realización: 31 de mayo de 2026
 * Fecha de entrega: 03 de junio de 2026
 * Resultados:
 * Este archivo contiene la lógica principal del servidor.
 * El servidor recibe solicitudes por sockets y utiliza la clase Protocolo
 * para procesar cada mensaje recibido y generar la respuesta correspondiente.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Protocolo;
using GestorProtocolo = Protocolo.Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;

        // Instancia compartida para resolver los pedidos recibidos por los clientes.
        private static readonly GestorProtocolo protocolo = new GestorProtocolo();

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();

                Console.WriteLine("Servidor inició en el puerto 8080...");

                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();

                    Console.WriteLine(
                        "Cliente conectado, puerto: {0}",
                        cliente.Client.RemoteEndPoint.ToString());

                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(
                    "Error de socket al iniciar el servidor: " + ex.Message);
            }
            finally
            {
                if (escuchador != null)
                {
                    escuchador.Stop();
                }
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream();

                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx = Encoding.UTF8.GetString(
                        bufferRx,
                        0,
                        bytesRx);

                    Console.WriteLine("Se recibió: " + mensajeRx);

                    string direccionCliente =
                        ((IPEndPoint)cliente.Client.RemoteEndPoint).Address.ToString();

                    // El servidor delega el procesamiento del mensaje a la clase Protocolo.
                    Respuesta respuesta = protocolo.ResolverPedido(
                        mensajeRx,
                        direccionCliente);

                    Console.WriteLine("Se envió: " + respuesta);

                    byte[] bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(
                    "Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                if (flujo != null)
                {
                    flujo.Close();
                }

                if (cliente != null)
                {
                    cliente.Close();
                }
            }
        }
    }
}