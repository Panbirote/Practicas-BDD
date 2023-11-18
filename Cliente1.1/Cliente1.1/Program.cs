using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class ClienteChat
{
    private TcpClient tcpClient;
    private NetworkStream clientStream;
    private string nombreCliente;

    private static int contadorClientes = 1;


    public ClienteChat()
    {
        this.nombreCliente = "Cliente" + contadorClientes++;
        ConectarAlServidor();
        // Iniciar el hilo para recibir mensajes del servidor
        Task.Run(() => RecibirMensajes());
    }

    private void ConectarAlServidor()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 1234);
            clientStream = tcpClient.GetStream();
            Console.WriteLine("Conexión establecida. Puedes enviar mensajes.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error de conexión: " + ex.Message);
        }
    }

    public void EnviarMensaje(string mensaje)
    {
        try
        {
            // Concatenar el nombre del cliente al mensaje antes de enviarlo al servidor
            mensaje = $"{this.nombreCliente}: {mensaje}";

            if (this.tcpClient != null && this.tcpClient.Connected && this.clientStream != null)
            {
                byte[] sendBytes = Encoding.UTF8.GetBytes(mensaje);
                this.clientStream.Write(sendBytes, 0, sendBytes.Length);
                this.clientStream.Flush();
            }
            else
            {
                Console.WriteLine("El cliente no está conectado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al enviar el mensaje: " + ex.Message);
        }
    }

    public void RecibirMensajes()
    {
        byte[] message = new byte[4096];
        int bytesRead;
        int intentosReconexion = 0;
        int maxIntentos = 5; // Número máximo de intentos de reconexión

        while (true)
        {
            try
            {
                if (this.tcpClient == null || !this.tcpClient.Connected || this.clientStream == null)
                {
                    ConectarAlServidor();
                    if (!this.tcpClient.Connected)
                    {
                        intentosReconexion++;
                        if (intentosReconexion >= maxIntentos)
                        {
                            Console.WriteLine("Se excedió el número máximo de intentos de reconexión.");
                            break;
                        }
                        Thread.Sleep(2000); // Espera 2 segundos antes de intentar reconectar
                        continue;
                    }
                    intentosReconexion = 0; // Reinicia el contador de intentos si la conexión se restablece
                }

                if (this.tcpClient != null && this.tcpClient.Connected && this.clientStream != null)
                {
                    bytesRead = this.clientStream.Read(message, 0, 4096);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Conexión perdida. Intentando reconectar...");
                        this.clientStream.Dispose();
                        this.tcpClient.Close();
                        Thread.Sleep(2000); // Espera 2 segundos antes de intentar reconectar
                        continue;
                    }

                    string dataReceived = Encoding.UTF8.GetString(message, 0, bytesRead);
                    Console.WriteLine("Mensaje del servidor: " + dataReceived);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la recepción de mensajes: " + ex.Message);
            }
        }
    }

    private void CerrarConexion()
    {
        try
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al cerrar la conexión: " + ex.Message);
        }
    }

    static void Main(string[] args)
    {
        ClienteChat cliente = new ClienteChat();

        Console.WriteLine("Escribe un mensaje para enviar al servidor (Escribe 'exit' para salir):");

        string mensaje;
        while (true)
        {
            mensaje = Console.ReadLine();

            cliente.EnviarMensaje(mensaje);

            if (mensaje.ToLower() == "exit")
            {
                cliente.CerrarConexion();
                break;
            }
        }
    }
}