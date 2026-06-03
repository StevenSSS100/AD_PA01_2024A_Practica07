/*
 * Practica 07
 * Steven Silva
 * Fecha de realización: 31 de mayo de 2026
 * Fecha de entrega: 03 de junio de 2026
 * Resultados:
 * Este archivo contiene las clases Pedido, Respuesta y Protocolo.
 * La clase Protocolo centraliza la comunicación entre cliente y servidor.
 * Aquí se implementan los métodos HazOperacion y ResolverPedido para que
 * el cliente y el servidor no trabajen directamente toda la lógica del pedido.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Protocolo
{
    // Representa la solicitud que el cliente envía al servidor.
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            string[] partes = mensaje.Trim().Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Comando, string.Join(" ", Parametros));
        }
    }

    // Representa la respuesta que el servidor devuelve al cliente.
    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public static Respuesta Procesar(string mensaje)
        {
            string[] partes = mensaje.Trim().Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            return new Respuesta
            {
                Estado = partes[0],
                Mensaje = string.Join(" ", partes.Skip(1).ToArray())
            };
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Estado, Mensaje);
        }
    }

    // Centraliza las operaciones de comunicación entre cliente y servidor.
    public class Protocolo
    {
        private readonly Dictionary<string, int> listadoClientes;
        private readonly object bloqueo;

        public Protocolo()
        {
            listadoClientes = new Dictionary<string, int>();
            bloqueo = new object();
        }

        // Crea un pedido, lo envía al servidor y procesa la respuesta recibida.
        public Respuesta HazOperacion(
            NetworkStream flujo,
            string comando,
            params string[] parametros)
        {
            if (flujo == null || !flujo.CanRead || !flujo.CanWrite)
            {
                return new Respuesta
                {
                    Estado = "NOK",
                    Mensaje = "No hay conexión con el servidor"
                };
            }

            Pedido pedido = new Pedido
            {
                Comando = comando,
                Parametros = parametros
            };

            byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());
            flujo.Write(bufferTx, 0, bufferTx.Length);

            byte[] bufferRx = new byte[1024];
            int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
            string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

            return Respuesta.Procesar(mensajeRx);
        }

        // Interpreta el pedido recibido por el servidor y genera una respuesta.
        public Respuesta ResolverPedido(string mensajeRx, string direccionCliente)
        {
            Pedido pedido = Pedido.Procesar(mensajeRx);

            switch (pedido.Comando)
            {
                case "INGRESO":
                    return ResolverIngreso(pedido);

                case "CALCULO":
                    return ResolverCalculo(pedido, direccionCliente);

                case "CONTADOR":
                    return ResolverContador(direccionCliente);

                default:
                    return new Respuesta
                    {
                        Estado = "NOK",
                        Mensaje = "Comando no reconocido"
                    };
            }
        }

        // Valida las credenciales ingresadas desde el cliente.
        private Respuesta ResolverIngreso(Pedido pedido)
        {
            if (pedido.Parametros.Length == 2 &&
                pedido.Parametros[0] == "root" &&
                pedido.Parametros[1] == "admin20")
            {
                return new Respuesta
                {
                    Estado = "OK",
                    Mensaje = "ACCESO_CONCEDIDO"
                };
            }

            return new Respuesta
            {
                Estado = "NOK",
                Mensaje = "ACCESO_NEGADO"
            };
        }

        // Valida los datos del vehículo y calcula el día correspondiente según la placa.
        private Respuesta ResolverCalculo(Pedido pedido, string direccionCliente)
        {
            if (pedido.Parametros.Length != 3)
            {
                return new Respuesta
                {
                    Estado = "NOK",
                    Mensaje = "Faltan datos del vehículo"
                };
            }

            string modelo = pedido.Parametros[0];
            string marca = pedido.Parametros[1];
            string placa = pedido.Parametros[2].ToUpper();

            if (string.IsNullOrWhiteSpace(modelo) ||
                string.IsNullOrWhiteSpace(marca) ||
                string.IsNullOrWhiteSpace(placa))
            {
                return new Respuesta
                {
                    Estado = "NOK",
                    Mensaje = "No se permiten campos vacíos"
                };
            }

            if (!ValidarPlaca(placa))
            {
                return new Respuesta
                {
                    Estado = "NOK",
                    Mensaje = "Placa no válida"
                };
            }

            byte indicadorDia = ObtenerIndicadorDia(placa);
            ContadorCliente(direccionCliente);

            return new Respuesta
            {
                Estado = "OK",
                Mensaje = string.Format("{0} {1}", placa, indicadorDia)
            };
        }

        // Devuelve el número de consultas realizadas por un cliente.
        private Respuesta ResolverContador(string direccionCliente)
        {
            lock (bloqueo)
            {
                if (listadoClientes.ContainsKey(direccionCliente))
                {
                    return new Respuesta
                    {
                        Estado = "OK",
                        Mensaje = listadoClientes[direccionCliente].ToString()
                    };
                }
            }

            return new Respuesta
            {
                Estado = "NOK",
                Mensaje = "No hay solicitudes previas"
            };
        }

        private bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        private byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));

            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000;

                case 3:
                case 4:
                    return 0b00010000;

                case 5:
                case 6:
                    return 0b00001000;

                case 7:
                case 8:
                    return 0b00000100;

                case 9:
                case 0:
                    return 0b00000010;

                default:
                    return 0;
            }
        }

        // Controla el contador de solicitudes evitando conflictos entre clientes.
        private void ContadorCliente(string direccionCliente)
        {
            lock (bloqueo)
            {
                if (listadoClientes.ContainsKey(direccionCliente))
                {
                    listadoClientes[direccionCliente]++;
                }
                else
                {
                    listadoClientes[direccionCliente] = 1;
                }
            }
        }
    }
}