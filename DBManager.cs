using System;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

public static class DBManager {
    public static MySqlConnection connection;
    public static MySqlCommand command;
    public static MySqlDataReader reader;

    public static void Conn() {
        try {
            connection = new MySqlConnection("SERVER=127.0.0.1;PORT=3306;DATABASE=userdata;UID=root;PWD=supersecret123;SslMode=none;");
            connection.Open();
        } catch(SqlException e) {
            Console.WriteLine("Error at Conn(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static void NonQuery(String queryString) {
        try {
            Conn();
            command = new MySqlCommand(queryString, connection);
            command.ExecuteNonQuery();
            connection.Close();
        } catch (SqlException e) {
            Console.WriteLine("Error at NonQuery(): " + e.StackTrace + ". Reason: " + e.Message);
        }
    }

    public static string[] Query(String queryString, string[] parameters) {
        string[] response = new string[100];
        try {
            Conn();
            command = new MySqlCommand(queryString, connection);
            for (int j = 0; j < parameters.Length; j++)
            {
                command.Parameters.AddWithValue("@" + j, parameters[j]);
            }
            reader = command.ExecuteReader();
            int i = 0;
            while (reader.Read())
            {
                response[i] = reader[i].ToString();
                i++;
            }
            connection.Close();
        } catch (SqlException e) {
            Console.WriteLine("Error at Query(): " + e.StackTrace + ". Reason: " + e.Message);
        }
        return response;
    }
}
