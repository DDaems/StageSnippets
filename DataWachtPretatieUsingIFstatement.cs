using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

public static class DataWachtPrestatie { 
    public class Syncronise { 

        // DataWachtPrestatie.Syncronise.WachtDienstenNaarWachtbord
        public static void WachtDienstenNaarWachtbord(DateTime StartDate, DateTime EndDate, out string ErrorMessage) {
            ErrorMessage = "";
            int DepartmentUnitID = 6;
            int Division = 1;
            int affected = 0;
            Dictionary<int, int> ConvertID = GetPersonIdsTranslationDictionaryFromDivision(Division);
            Dictionary<string, string> DutyTimes = getDutyTimesWachtbordForDutyUnit(DepartmentUnitID);

            try
            {
                using (SqlConnection informalendar = new SqlConnection("Integrated Security=SSPI;Initial Catalog=informalendar"))
                {
                    using (SqlConnection wachtbord = new SqlConnection("Integrated Security=SSPI;Initial Catalog=wachtbord"))
                    {

                        informalendar.Open();
                        wachtbord.Open();
                        SqlCommand SelectCommand = new SqlCommand("SELECT * FROM Wachtprestaties", informalendar);         
                        

                        using (SqlDataReader reader = SelectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Thursday)
                                {
                                    DateTime endDate = date.DayOfWeek == DayOfWeek.Monday ?
                                                                            date.AddDays(3):
                                                                            date.AddDays(4);

                                    string query = "INSERT INTO " +
                                                   "DutyShifts(DutyDepartmentID, StartDate, EndDate, StartTime, EndTime, PersonID) " +
                                                   "VALUES(@DutyDepartmentID, @StartDate, @EndDate, @StartTime, @EndTime, @PersonID)";
                                    SqlCommand InserCommand = new SqlCommand(query, wachtbord);
                                        InserCommand.Parameters.AddWithValue("@DutyDepartmentID", SqlDbType.Int).Value = DepartmentUnitID;
                                        InserCommand.Parameters.AddWithValue("@StartDate", SqlDbType.DateTime).Value = date.ToString("yyyy/MM/dd");
                                        InserCommand.Parameters.AddWithValue("@EndDate", SqlDbType.DateTime).Value = endDate.ToString("yyyy/MM/dd");
                                        InserCommand.Parameters.AddWithValue("@StartTime", SqlDbType.NVarChar).Value = DutyTimes["StartTime"];
                                        InserCommand.Parameters.AddWithValue("@EndTime", SqlDbType.NVarChar).Value = DutyTimes["EndTime"];
                                        InserCommand.Parameters.AddWithValue("@PersonID", SqlDbType.Int).Value = ConvertID[(int)reader["PersonID"]];

                                try
                                {
                                int Affected = InserCommand.ExecuteNonQuery();
                                if (Affected > 0) affected++;
                                }
                                catch(Exception e)
                                {
                                ErrorMessage = e.Message;
                                throw new Exception(e.Message);
                                }
                              }
   
                            }
                        }
                    }
                }
            }catch(Exception e)
            {
                ErrorMessage = e.Message;
            }
            MessageBox.Show("Affected: " + affected);

        }
    }
    
    private static Dictionary<string, string> getDutyTimesWachtbordForDutyUnit(int v)
    {
        var DutyTimes = new Dictionary<string, string>();

        DateTime dt = DateTime.Now;

        DutyTimes.Add("StartTime", dt.ToString("HH:mm"));
        DutyTimes.Add("EndTime", dt.ToString("HH:mm"));
        return DutyTimes;
    }
    public static Dictionary<int, int> GetPersonIdsTranslationDictionaryFromDivision(int v)
    {
        #region create Dictionary

        var dictionary = new Dictionary<int, int>();
        dictionary.Add(2, 20);
        dictionary.Add(1, 10);
        dictionary.Add(3, 30);
        dictionary.Add(4, 40);
        dictionary.Add(5, 50);
        dictionary.Add(6, 60);
        #endregion
        return dictionary;
    }
}
