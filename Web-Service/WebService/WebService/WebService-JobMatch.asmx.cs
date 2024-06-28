using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.Sql;
using System.Data.SqlClient;

namespace WebService
{
    /// <summary>
    /// Descripción breve de WebService_JobMatch
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService_JobMatch : System.Web.Services.WebService
    {
        //cambiar la cadena conexion segun la base
        //cadena local
        private static SqlConnection con = new SqlConnection(@"Data Source=(localdb)\SQLMATEO;Initial Catalog=JobMatch;Integrated Security=True");
        //cade de conexion con somee
        //private static SqlConnection con = new SqlConnection(@"workstation id=JobMatch.mssql.somee.com;packet size=4096;user id=SrTeo201_SQLLogin_1;pwd=9rcknmwx7c;data source=JobMatch.mssql.somee.com;persist security info=False;initial catalog=JobMatch;TrustServerCertificate=True");

        //login
        [WebMethod]
        public string Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
                    {
                        return "Llenar Credenciales";
                    }
                    else if (string.IsNullOrEmpty(email))
                    {
                        return "Llenar email";
                    }
                    else
                    {
                        return "Llenar contraseña";
                    }
                }

                con.Open();

                // Verificar si el email existe en la base de datos
                string queryEmailCheck = "SELECT COUNT(*) FROM Usuarios WHERE email = @Email";
                SqlCommand cmdEmailCheck = new SqlCommand(queryEmailCheck, con);
                cmdEmailCheck.Parameters.AddWithValue("@Email", email);
                int emailC = (int)cmdEmailCheck.ExecuteScalar();

                if (emailC == 0)
                {
                    con.Close();
                    return "Credenciales no registradas";
                }

                // Verificar si la contraseña coincide con el email
                string query = "SELECT email, contraseña, tipo_usuario FROM Usuarios WHERE email = @Email";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", email);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedPassword = reader["contraseña"].ToString();
                    string tipoUsuario = reader["tipo_usuario"].ToString();
                    con.Close();

                    if (storedPassword != password)
                    {
                        return "Contraseña incorrecta";
                    }

                    if (tipoUsuario == "CANDIDATO")
                    {
                        return "1";
                    }
                    else if (tipoUsuario == "EMPRESA")
                    {
                        return "2";
                    }
                    else
                    {
                        return "Tipo de usuario no reconocido";
                    }
                }
                else
                {
                    con.Close();
                    return "Credenciales no válidas";
                }
            }
            catch (Exception ex)
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                return "Error: " + ex.Message;
            }
        }
        //registro de Candidato y usuario
        [WebMethod]
        public string RegistroCandidato(string nombre, string email, string contraseña, string ubicacion)
        {
            // Validaciones de campos vacíos
            if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(contraseña) && string.IsNullOrWhiteSpace(ubicacion))
            {
                return "Llenar todas las credenciales";
            }
            else if (string.IsNullOrWhiteSpace(nombre))
            {
                return "El nombre es obligatorio";
            }
            else if (string.IsNullOrWhiteSpace(email))
            {
                return "El email es obligatorio";
            }
            else if (string.IsNullOrWhiteSpace(contraseña))
            {
                return "La contraseña es obligatoria";
            }
            else if (string.IsNullOrWhiteSpace(ubicacion))
            {
                return "La ubicación es obligatoria";
            }

            try
            {
                con.Open();

                // Verificar si el correo electrónico ya está registrado
                string queryEmailCheck = "SELECT COUNT(*) FROM Usuarios WHERE email = @Email";
                SqlCommand cmdEmailCheck = new SqlCommand(queryEmailCheck, con);
                cmdEmailCheck.Parameters.AddWithValue("@Email", email);
                int emailCount = (int)cmdEmailCheck.ExecuteScalar();

                if (emailCount > 0)
                {
                    con.Close();
                    return "El correo electrónico ya está registrado.";
                }

                // Iniciar una transacción
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // Paso 1: Insertar el usuario en la tabla Usuarios
                    string queryUsuario = "INSERT INTO Usuarios (nombre, email, contraseña, tipo_usuario) VALUES (@Nombre, @Email, @Contraseña, 'CANDIDATO'); SELECT SCOPE_IDENTITY();";
                    SqlCommand cmdUsuario = new SqlCommand(queryUsuario, con, transaction);
                    cmdUsuario.Parameters.AddWithValue("@Nombre", nombre);
                    cmdUsuario.Parameters.AddWithValue("@Email", email);
                    cmdUsuario.Parameters.AddWithValue("@Contraseña", contraseña);

                    // Ejecutar la consulta y obtener el usuario_id generado
                    int usuarioId = Convert.ToInt32(cmdUsuario.ExecuteScalar());

                    // Paso 2: Insertar el perfil del candidato en la tabla Perfiles_Candidatos
                    string queryCandidato = "INSERT INTO Perfiles_Candidatos (usuario_id, ubicacion) VALUES (@UsuarioId, @Ubicacion)";
                    SqlCommand cmdCandidato = new SqlCommand(queryCandidato, con, transaction);
                    cmdCandidato.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmdCandidato.Parameters.AddWithValue("@Ubicacion", ubicacion);

                    cmdCandidato.ExecuteNonQuery();

                    // Confirmar la transacción
                    transaction.Commit();

                    con.Close();

                    return "Registro de candidato exitoso";
                }
                catch (Exception)
                {
                    // Revertir la transacción en caso de error
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                return "Error: " + ex.Message;
            }
        }

        //registro de Enpresa y usuario
        [WebMethod]
        public string RegistroEmpresa(string nombre, string email, string contraseña, string nombreEmpresa)
        {
            // Validaciones de campos vacíos
            if (string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(contraseña) && string.IsNullOrEmpty(nombreEmpresa))
            {
                return "Todos los campos obligatorios deben ser llenados.";
            }
            else if (string.IsNullOrWhiteSpace(nombre))
            {
                return "El nombre es obligatorio";
            }
            else if (string.IsNullOrWhiteSpace(email))
            {
                return "El email es obligatorio";
            }
            else if (string.IsNullOrWhiteSpace(contraseña))
            {
                return "La contraseña es obligatoria";
            }
            else if (string.IsNullOrWhiteSpace(nombreEmpresa))
            {
                return "El nombre de la empresa es obligatorio";
            }

            try
            {
                con.Open();

                // Verificar si el correo electrónico ya está registrado
                string queryEmailCheck = "SELECT COUNT(*) FROM Usuarios WHERE email = @Email";
                SqlCommand cmdEmailCheck = new SqlCommand(queryEmailCheck, con);
                cmdEmailCheck.Parameters.AddWithValue("@Email", email);
                int emailCount = (int)cmdEmailCheck.ExecuteScalar();

                if (emailCount > 0)
                {
                    con.Close();
                    return "El correo electrónico ya está registrado.";
                }

                // Iniciar una transacción
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // Paso 1: Insertar el usuario en la tabla Usuarios
                    string queryUsuario = "INSERT INTO Usuarios (nombre, email, contraseña, tipo_usuario) VALUES (@Nombre, @Email, @Contraseña, 'EMPRESA'); SELECT SCOPE_IDENTITY();";
                    SqlCommand cmdUsuario = new SqlCommand(queryUsuario, con, transaction);
                    cmdUsuario.Parameters.AddWithValue("@Nombre", nombre);
                    cmdUsuario.Parameters.AddWithValue("@Email", email);
                    cmdUsuario.Parameters.AddWithValue("@Contraseña", contraseña);

                    // Ejecutar la consulta y obtener el usuario_id generado
                    int usuarioId = Convert.ToInt32(cmdUsuario.ExecuteScalar());

                    // Paso 2: Insertar la empresa en la tabla Empresas
                    string queryEmpresa = "INSERT INTO Empresas (usuario_id, nombre_empresa) VALUES (@UsuarioId, @NombreEmpresa)";
                    SqlCommand cmdEmpresa = new SqlCommand(queryEmpresa, con, transaction);
                    cmdEmpresa.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmdEmpresa.Parameters.AddWithValue("@NombreEmpresa", nombreEmpresa);

                    cmdEmpresa.ExecuteNonQuery();

                    // Confirmar la transacción
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Revertir la transacción en caso de error
                    transaction.Rollback();
                    throw new Exception("Error durante el registro: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }

                return "Registro de empresa exitoso";
            }
            catch (Exception ex)
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
                return "Error: " + ex.Message;
            }
        }


    }
}
