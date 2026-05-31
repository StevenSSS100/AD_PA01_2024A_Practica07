/*
 * Practica 07
 * Steven Silva
 * Fecha de realización: 31 de mayo de 2026
 * Fecha de entrega: 03 de junio de 2026        
 * Resultados:
 * Este archivo contiene las clases Pedido, Respuesta y Protocolo.
 * La clase Protocolo centraliza la comunicación entre cliente y servidor.
 * En esta clase se implementaron los métodos HazOperacion y ResolverPedido,
 * para que el cliente y el servidor ya no trabajen directamente con Pedido y Respuesta.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Protocolo
{
    // Se mantiene la clase Pedido porque representa la solicitud que el cliente envía al servidor.
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        public static Pedido Procesar(string mensaje)
        {
            string[] partes = mensaje.Trim().Split(' ');

            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    // Se mantiene la clase Respuesta porque representa el mensaje que el servidor devuelve al cliente.
    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public static Respuesta Procesar(string mensaje)
        {
            string[] partes = mensaje.Trim().Split(' ');

            return new Respuesta
            {
                Estado = partes[0],
                Mensaje = string.Join(" ", partes.Skip(1).ToArray())
            };
        }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    // Se creó la clase Protocolo para centralizar la lógica de comunicación.
    // Esta clase usa Pedido y Respuesta internamente, pero el cliente y el servidor
    // ya no necesitan trabajar directamente con esas clases.
    public class Protocolo
    {
        private Dictionary<string, int> listadoClientes;
        private object bloqueo;

        public Protocolo()
        {
            listadoClientes = new Dictionary<string, int>();
            bloqueo = new object();
        }

        // Este método fue movido desde el cliente hacia la clase Protocolo.
        // Su función es crear un Pedido, enviarlo por el NetworkStream y recibir una Respuesta.
        public Respuesta HazOperacion(NetworkStream flujo, string comando, params string[] parametros)
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

        // Este método fue movido desde el servidor hacia la clase Protocolo.
        // Su función es interpretar el mensaje recibido y devolver la respuesta correspondiente.
        public Respuesta ResolverPedido(string mensajeRx, string direccionCliente)
        {
            Pedido pedido = Pedido.Procesar(mensajeRx);

            Respuesta respuesta = new Respuesta
            {
                Estado = "NOK",
                Mensaje = "Comando no reconocido"
            };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    respuesta = ResolverIngreso(pedido);
                    break;

                case "CALCULO":
                    respuesta = ResolverCalculo(pedido, direccionCliente);
                    break;

                case "CONTADOR":
                    respuesta = ResolverContador(direccionCliente);
                    break;
            }

            return respuesta;
        }

        // Este método resuelve el ingreso del usuario.
        // Se valida si el usuario y la contraseña coinciden con los datos permitidos.
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

        // Este método resuelve el cálculo relacionado con la placa del vehículo.
        // Primero valida la placa y luego obtiene el indicador del día correspondiente.
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
                Mensaje = $"{placa} {indicadorDia}"
            };
        }

        // Este método permite consultar cuántas solicitudes ha realizado un cliente.
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
                    return 0b00100000; // Lunes

                case 3:
                case 4:
                    return 0b00010000; // Martes

                case 5:
                case 6:
                    return 0b00001000; // Miércoles

                case 7:
                case 8:
                    return 0b00000100; // Jueves

                case 9:
                case 0:
                    return 0b00000010; // Viernes

                default:
                    return 0;
            }
        }

        // Se usa lock para evitar problemas cuando varios clientes acceden al contador al mismo tiempo.
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