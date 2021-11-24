using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

public static class DataWachtPrestatie { 
    public class Syncronise { 
        public static int WachtDienstenNaarWachtbord(DateTime StartDate, DateTime EndDate, out string ErrorMessage) {
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

                        informalendar.Open();

                        SqlCommand SelectCommand = new SqlCommand("SELECT * FROM Wachtprestaties", informalendar);         
                        

                        using (SqlDataReader reader = SelectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime date = (DateTime)reader["Date"];
                                DateTime endDate = date;
                                switch (date.DayOfWeek)
                                {
                                    case DayOfWeek.Monday:
                                        endDate = date.AddDays(3);
                                        break;
                                    case DayOfWeek.Thursday:
                                        endDate = date.AddDays(4);
                                        break;
                                    default:
                                        continue;
                                }

                                bool succes = insertIntoWachtbord(DepartmentUnitID, ConvertID[(int)reader["PersonID"]], DutyTimes["StartTime"], DutyTimes["EndTime"], date, endDate, out ErrorMessage);
                                if (succes) affected++;
                            }
                        
                    }
                }
            }catch(Exception e)
            {
                ErrorMessage = e.Message;
            }
            
            return affected;

        }

        private static bool insertIntoWachtbord(int DepartmentUnitID, int PersonID, string StartTime, string EndTime, DateTime StartDate, DateTime EndDate, out string ErrorMessage)
        {
            ErrorMessage = "";
            using (SqlConnection wachtbord = new SqlConnection("Integrated Security=SSPI;Initial Catalog=wachtbord"))
            {

                string query = "INSERT INTO " +
                                    "DutyShifts(DutyDepartmentID, StartDate, EndDate, StartTime, EndTime, PersonID) " +
                                    "VALUES(@DutyDepartmentID, @StartDate, @EndDate, @StartTime, @EndTime, @PersonID)";

                SqlCommand InserCommand = new SqlCommand(query, wachtbord);
                InserCommand.Parameters.AddWithValue("@DutyDepartmentID", SqlDbType.Int).Value = DepartmentUnitID;
                InserCommand.Parameters.AddWithValue("@StartDate", SqlDbType.DateTime).Value = StartDate.ToString("yyyy/MM/dd");
                InserCommand.Parameters.AddWithValue("@EndDate", SqlDbType.DateTime).Value = EndDate.ToString("yyyy/MM/dd");
                InserCommand.Parameters.AddWithValue("@StartTime", SqlDbType.NVarChar).Value = StartTime;
                InserCommand.Parameters.AddWithValue("@EndTime", SqlDbType.NVarChar).Value = EndTime;
                InserCommand.Parameters.AddWithValue("@PersonID", SqlDbType.Int).Value = PersonID;

                try
                {
                    wachtbord.Open();
                    int affected = InserCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    ErrorMessage = e.Message;
                    return false;
                }
            } 
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
