﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flashcards.Data;
using Flashcards.DTOs;
using Flashcards.Modes;
using Flashcards.View;
using Microsoft.Data.SqlClient;

namespace Flashcards.Controller
{
    internal class CardController
    {
        static string? connectionString = ConfigHelper.GetConnectionString();
        public static void View(string StackName)
        {
            TableVisualisationEngine.ShowCardTable(GetTheCards(StackName));          
        }

        public static void Insert(string front, string back, string stackId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var tabtableCmd = connection.CreateCommand();
                tabtableCmd.CommandText = $"INSERT INTO cards (Front, Back, Stack_id) VALUES (N'{front}', N'{back}', '{stackId}');";

                tabtableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void Update(string cardId)
        {
            int isExists = 1;
            bool validInput = true;
            int Id;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int dataID = CardMapper.GetCardDataID(cardId);

                do
                {
                    if (dataID == 0) isExists = 0;

                    if (isExists == 0) Console.WriteLine($"Card-{cardId} doesn't exists. Please enter another stack.");
                    else if (validInput == false) Console.WriteLine("INVALID INPUT, please enter a integer.");

                    if (isExists == 0 || validInput == false)
                    {
                        Console.WriteLine("Please enter the new id");
                        validInput = int.TryParse(Console.ReadLine(), out Id);
                    }

                    var sqlQuery = connection.CreateCommand();
                    sqlQuery.CommandText = $"IF EXISTS (SELECT 1 FROM dbo.cards WHERE CardId = {dataID}) SELECT 1 ELSE SELECT 0;";
                    isExists = (int)sqlQuery.ExecuteScalar();

                } while (isExists == 0 || validInput == false);

                Console.WriteLine("Please the front");
                string front = Console.ReadLine();

                if (front == "0") return;

                Console.WriteLine("Please the back");
                string back = Console.ReadLine();

                if (back == "0") return;

                var tableComd = connection.CreateCommand();
                tableComd.CommandText = $"UPDATE dbo.cards SET Front = '{front}', Back = '{back}' WHERE CardId = {dataID}";

                tableComd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void Detele(string cardId)
        {
            int isExists = 1;
            bool validInput = true;
            int Id;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int dataID = CardMapper.GetCardDataID(cardId);
                do
                {
                    if (dataID == 0) isExists = 0;
                    if (isExists == 0) Console.WriteLine($"Card-{cardId} doesn't exists. Please enter another stack.");
                    else if (validInput == false) Console.WriteLine("INVALID INPUT, please enter a integer.");

                    if (isExists == 0 || validInput == false)
                    {
                        Console.WriteLine("Please enter the new id");
                        validInput = int.TryParse(Console.ReadLine(), out Id);
                     }

                    var sqlQuery = connection.CreateCommand();
                    sqlQuery.CommandText = $"IF EXISTS (SELECT 1 FROM dbo.cards WHERE CardId = {dataID}) SELECT 1 ELSE SELECT 0;";
                    isExists = (int)sqlQuery.ExecuteScalar();

                } while (isExists == 0);

                var tabtableCmd = connection.CreateCommand();
                tabtableCmd.CommandText = $"DELETE FROM cards WHERE CardId = {dataID};";

                tabtableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static List<CardDTO> Study(string StackName)
        {
            return GetTheCards(StackName);
        }

        public static List<CardDTO> GetTheCards(string StackName)
        {
            string StackId = StackController.GetStackId(StackName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<CardSession> tableCard = new List<CardSession>();

                var tableComd = connection.CreateCommand();

                tableComd.CommandText = $"SELECT CardId, Front, Back, Stack_id FROM dbo.cards WHERE Stack_id = {StackId};";

                SqlDataReader reader = tableComd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableCard.Add(new CardSession
                        {
                            Id = reader.GetInt32(0),
                            Front = reader.GetString(1),
                            Back = reader.GetString(2),
                            StackId = reader.GetInt32(3),
                        });
                    }
                }

                reader.Close();
                List<CardDTO> cardDTOs = CardMapper.TransitionToDTO(tableCard);
                return cardDTOs;
            }
        }

    }
}
