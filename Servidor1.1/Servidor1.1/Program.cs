using System.Net.Sockets;
using System.Net;
using System.Text;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ServidorChat
{
    private TcpListener tcpListener;
    private List<TcpClient> listaClientes;
    private bool isServerRunning;
    private Thread serverInputThread;
    private string logFileName;
    private int logFileCounter;
    private int connectedClients;
    private int totalClientesDesconectados;

    public ServidorChat()
    {
        this.tcpListener = new TcpListener(IPAddress.Any, 1234);
        this.listaClientes = new List<TcpClient>();
        isServerRunning = true;
        logFileCounter = 1;
        connectedClients = 0;
        totalClientesDesconectados = 0;

        serverInputThread = new Thread(ServerInput);
        serverInputThread.Start();

        IniciarServidor();
    }

    private void IniciarServidor()
    {
        this.tcpListener.Start();
        Console.WriteLine("Servidor iniciado.");

        while (isServerRunning)
        {
            try
            {
                Console.WriteLine("Esperando la conexión de un cliente...");
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Console.WriteLine("Cliente conectado.");

                Thread clientThread = new Thread(() => HandleClientComm(client));
                clientThread.Start();
                this.listaClientes.Add(client);
                Interlocked.Increment(ref connectedClients);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    Console.WriteLine("El servidor se detuvo.");
                }
                else
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        // Al salir del bucle while, es porque el servidor se detiene
        CerrarArchivoActual();
    }

    private void CerrarArchivoActual()
    {
        if (logFileName != null)
        {
            using (StreamWriter serverStreamWriter = new StreamWriter(logFileName, true))
            {
                serverStreamWriter.WriteLine("---- Sesión Finalizada ----");
                serverStreamWriter.Close(); // Cerrar el StreamWriter después de escribir
            }
            logFileName = null;
        }
    }

    private void ServerInput()
    {
        while (isServerRunning)
        {
            string serverMessage = Console.ReadLine();

            if (serverMessage.ToLower() == "exit")
            {
                isServerRunning = false;
                DetenerServidor();
                break;
            }

            EnviarMensajeATodosDesdeServidor(serverMessage);
            RegistrarMensajeEnArchivo($"Servidor: {serverMessage}");
        }
    }

    private void HandleClientComm(object client)
    {
        TcpClient tcpClient = (TcpClient)client;

        try
        {
            using (NetworkStream clientStream = tcpClient.GetStream())
            {
                byte[] message = new byte[4096];
                while (isServerRunning)
                {
                    int bytesRead = clientStream.Read(message, 0, message.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Cliente desconectado.");
                        tcpClient.Close();
                        this.listaClientes.Remove(tcpClient);
                        Interlocked.Decrement(ref connectedClients);
                        totalClientesDesconectados++;

                        if (connectedClients == 0 && isServerRunning)
                        {
                            CerrarArchivoActual();
                        }

                        break;
                    }

                    string dataReceived = Encoding.UTF8.GetString(message, 0, bytesRead);
                    Console.WriteLine("Mensaje recibido: " + dataReceived);

                    EnviarMensajeATodosDesdeCliente(dataReceived, tcpClient);
                    RegistrarMensajeEnArchivo($"Cliente: {dataReceived}");
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error de lectura/escritura: " + ex.Message);
            tcpClient.Close();
            this.listaClientes.Remove(tcpClient);
            Interlocked.Decrement(ref connectedClients);
            totalClientesDesconectados++;

            if (connectedClients == 0 && isServerRunning)
            {
                CerrarArchivoActual();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    private void EnviarMensajeATodosDesdeServidor(string mensaje)
    {
        foreach (TcpClient cliente in listaClientes)
        {
            try
            {
                EnviarMensajeAlCliente(cliente, $"Servidor: {mensaje}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar mensaje a todos los clientes: " + ex.Message);
            }
        }
    }

    private void EnviarMensajeATodosDesdeCliente(string mensaje, TcpClient senderClient)
    {
        foreach (TcpClient cliente in listaClientes)
        {
            if (cliente != senderClient)
            {
                try
                {
                    EnviarMensajeAlCliente(cliente, $"Cliente {mensaje}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al enviar mensaje a todos los clientes: " + ex.Message);
                }
            }
        }
    }

    private void EnviarMensajeAlCliente(TcpClient cliente, string mensaje)
    {
        try
        {
            NetworkStream stream = cliente.GetStream();
            byte[] sendBytes = Encoding.UTF8.GetBytes(mensaje);
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al enviar mensaje al cliente: " + ex.Message);
        }
    }

    private void RegistrarMensajeEnArchivo(string mensaje)
    {
        lock (this)
        {
            if (!isServerRunning)
            {
                CerrarArchivoActual();
            }

            if (connectedClients == 0 && logFileName != null)
            {
                CerrarArchivoActual();
            }

            if (logFileName == null)
            {
                logFileName = GenerateLogFileName(); // Generar un nuevo nombre de archivo si no hay uno actual
            }

            if (logFileName != null)
            {
                using (StreamWriter serverStreamWriter = new StreamWriter(logFileName, true))
                {
                    serverStreamWriter.WriteLine(mensaje);
                    serverStreamWriter.Flush();
                }
            }
        }
    }

    private string GenerateLogFileName()
    {
        string fileName = $"Chat_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{logFileCounter}.txt";
        logFileCounter++;
        return fileName;
    }

    private void DetenerServidor()
    {
        isServerRunning = false;

        foreach (TcpClient cliente in listaClientes)
        {
            cliente.Close();
        }

        if (logFileName != null)
        {
            CerrarArchivoActual();
        }

        tcpListener.Stop();

        LimpiarPantallaEsperarConexion();
    }

    private void LimpiarPantallaEsperarConexion()
    {
        Console.Clear();
        Console.WriteLine("Esperando la conexión de un cliente...");
    }

    static void Main(string[] args)
    {
        ServidorChat server = new ServidorChat();
    }
}