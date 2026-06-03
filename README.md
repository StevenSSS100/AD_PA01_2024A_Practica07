# Práctica 07 - Gestión de Versiones e Integración Continua

## Datos generales

**Nombre:** Steven Silva
**Asignatura:** Aplicaciones Distribuidas
**Práctica:** Práctica 07
**Tema:** Gestión de Versiones e Integración Continua con GitHub
**Fecha de realización:** 31 de mayo de 2026
**Fecha de entrega:** 03 de junio de 2026

---

## Descripción del proyecto

Este repositorio contiene el proyecto modificado de la Prueba Acumulativa 01 de Aplicaciones Distribuidas. El sistema está formado por tres partes principales: un cliente, un servidor y un proyecto de protocolo compartido.

La práctica consistió en clonar el proyecto base, revisar su funcionamiento, realizar modificaciones en el código y cargar los cambios en GitHub usando control de versiones. Además, se reorganizó parte de la lógica de comunicación para que el cliente y el servidor utilicen una clase central llamada `Protocolo`.

---

## Objetivo de la práctica

Aplicar GitHub como herramienta para la gestión de versiones en un proyecto desarrollado en Visual Studio, realizando cambios en el código fuente, registrándolos mediante commits y publicándolos en un repositorio remoto.

---

## Tecnologías utilizadas

* C#
* Visual Studio 2022
* Git
* GitHub
* Sockets TCP
* Programación cliente-servidor

---

## Estructura del proyecto

```text
AD_PA01_2024A
│
├── Cliente
│   └── Contiene la interfaz gráfica y el envío de solicitudes al servidor.
│
├── Servidor
│   └── Recibe los mensajes enviados por el cliente y devuelve una respuesta.
│
├── Protocolo
│   └── Contiene las clases Pedido, Respuesta y Protocolo.
│
└── PruebaAcumulativa01_2024A.sln
    └── Solución principal del proyecto.
```

---

## Cambios realizados

* Se agregó un encabezado en los archivos modificados.
* Se colocaron comentarios en las partes principales del código.
* Se creó la clase `Protocolo` dentro del proyecto `Protocolo`.
* Se trasladó el método `HazOperacion` hacia la clase `Protocolo`.
* Se trasladó el método `ResolverPedido` hacia la clase `Protocolo`.
* Se modificó el cliente para que utilice la clase `Protocolo`.
* Se modificó el servidor para que utilice la clase `Protocolo`.
* Se corrigió el mensaje del puerto del servidor para que indique correctamente el puerto `8080`.
* Se limpió el código eliminando espacios, variables y elementos innecesarios.

---

## Funcionamiento general

El cliente envía solicitudes al servidor mediante sockets TCP. Estas solicitudes son procesadas por la clase `Protocolo`, la cual se encarga de interpretar los comandos y generar las respuestas correspondientes.

De esta forma, el cliente ya no construye directamente toda la lógica del pedido y el servidor ya no resuelve directamente cada operación dentro de su propia clase. La comunicación queda organizada en un solo lugar, haciendo que el código sea más claro y fácil de mantener.

---

## Cómo ejecutar el proyecto

1. Clonar el repositorio:

```bash
git clone https://github.com/StevenSSS100/AD_PA01_2024A_Practica07.git
```

2. Abrir la solución en Visual Studio:

```text
PruebaAcumulativa01_2024A.sln
```

3. Ejecutar primero el proyecto `Servidor`.

4. Verificar que el servidor muestre el siguiente mensaje:

```text
Servidor inició en el puerto 8080...
```

5. Ejecutar el proyecto `Cliente`.

6. Ingresar las credenciales de prueba:

```text
Usuario: root
Contraseña: admin20
```

7. Realizar una consulta de placa desde la interfaz del cliente.

---

## Autor

Steven Silva

---

## Nota

Este proyecto fue desarrollado con fines académicos para la asignatura Aplicaciones Distribuidas. Los cambios realizados corresponden a la Práctica 07, enfocada en el uso de GitHub, control de versiones y organización del código cliente-servidor.
