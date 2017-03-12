using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Utils
{
    public static class DbUtils
    {
        /// <summary>
        /// Get's a ready connection to the database
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetConnection(){
            MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["db_connection"]);
            connection.Open();
            return connection;
        }

        public static MySqlCommand GetCommand(String storedProc, MySqlConnection connection, MySqlTransaction transaction = null)
        {
            MySqlCommand command = new MySqlCommand(storedProc, connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            return command;
        }
    }
}