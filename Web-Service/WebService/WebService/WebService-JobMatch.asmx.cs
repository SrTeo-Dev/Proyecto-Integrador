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
        private static SqlConnection con = new SqlConnection(@"Data Source=(localdb)\SQLMATEO;Initial Catalog=JobMatch;Integrated Security=True");
        //cambiar la cadena conexion segun la base

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
                        return "Bienvenido, CANDIDATO";
                    }
                    else if (tipoUsuario == "EMPRESA")
                    {
                        return "Bienvenido, EMPRESA";
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

        [WebMethod]
        public string Registro()
        {
            return "0";
        }
    }
}
