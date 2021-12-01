using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private string Errors;

        public static class DataWachtPrestatie {
            public class Syncronise {

                static int DepartmentUnitID = 6;
                static int Division = 1;

                public static Dictionary<int, string> wachtbordPersonen;
                public static Dictionary<string, string> DutyTimes;


                public static void Generate(DateTime StartDate, DateTime EndDate, out string ErrorMessage)
                {
                    ErrorMessage = "";
                    wachtbordPersonen = getPersonIdsFromWachtbord(DepartmentUnitID);
                    DutyTimes = getDutyTimesWachtbord(DepartmentUnitID);


                    DataTable dataTable = selectWachtdienstenInformalendar(StartDate, EndDate, out ErrorMessage);
                    List<DataRow> queue = new List<DataRow>();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (queue.Count == 0) {
                            queue.Add(row);
                            continue;
                        }

                        DataRow last = queue.Last();
                        if (last["ShortName"] == row["ShortName"]) {
                            queue.Add(row);
                            continue;
                        }

                        bool succes = insertIntoWachtbord(queue, out ErrorMessage);

                        if (!succes) {
                            MessageBox.Show(ErrorMessage);
                            return;
                        }
                        queue.Clear();
                        queue.Add(row);
                    }

                    if (queue.Count > 0) {
                        bool succes = insertIntoWachtbord(queue, out ErrorMessage);        
                    }

                }
            


                public static DataTable selectWachtdienstenInformalendar(DateTime StartDate, DateTime EndDate, out string ErrorMessage) {
                    ErrorMessage = "";

                    DataTable dataTable = new DataTable();

                    using (SqlConnection informalendar = new SqlConnection("Integrated Security=SSPI;Initial Catalog=informalendar"))
                    {

                        string query = $"SELECT * FROM Wachtprestaties w INNER JOIN Persons p on p.ID = w.personID " +
                                        $"WHERE Division = @Division AND Date >= CAST(@StartDate as DateTime) AND Date <= CAST(@EndDate as DateTime) " +
                                        $"ORDER BY Date";
                        SqlCommand SelectCommand = new SqlCommand(query, informalendar);
                                   SelectCommand.Parameters.AddWithValue("@Division", SqlDbType.Int).Value = Division;
                                   SelectCommand.Parameters.AddWithValue("@StartDate", SqlDbType.DateTime).Value = StartDate;
                                   SelectCommand.Parameters.AddWithValue("@EndDate", SqlDbType.DateTime).Value = EndDate;


                        informalendar.Open();
                            SqlDataAdapter dataAdapter = new SqlDataAdapter(SelectCommand);
                                           dataAdapter.Fill(dataTable);

                        return dataTable;
                    }

                }

                private static bool insertIntoWachtbord(List<DataRow> queue, out string ErrorMessage)
                {

                    DataRow last = queue.Last();
                    DataRow first = queue.First();
                    int personID = wachtbordPersonen.FirstOrDefault(entry => Equals(entry.Value, last["ShortName"].ToString().Trim())).Key;

                    bool succes = insertIntoWachtbord(DepartmentUnitID, personID, DutyTimes["StartTime"], DutyTimes["EndTime"],
                                                      (DateTime)first["date"], (DateTime)last["date"], out ErrorMessage);

                    return succes;
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


            private static Dictionary<string, string> getDutyTimesWachtbord(int v)
            {
                var DutyTimes = new Dictionary<string, string>();

                DateTime dt = DateTime.Now;

                DutyTimes.Add("StartTime", dt.ToString("HH:mm"));
                DutyTimes.Add("EndTime", dt.ToString("HH:mm"));
                return DutyTimes;
            }
            public static Dictionary<int, string> getPersonIdsFromWachtbord(int v)
            {
                #region create Dictionary

                var dictionary = new Dictionary<int, string>();
                dictionary.Add(1, "DDaems");
                dictionary.Add(2, "SDeWilde");
                dictionary.Add(5, "HKarl");
                dictionary.Add(6, "Mark");

                #endregion
                return dictionary;
            }
        }

        public Form1()
        {
            InitializeComponent();


            //    ErrorMessage += $"{DepartmentUnitID} : " +
            //$"{date.ToString("yyyy/MM/dd")} - {endDate.ToString("yyyy/MM/dd")} " +
            //$"| {DutyTimes["StartTime"]} - {DutyTimes["EndTime"]} " +
            //$"= {ConvertID[(int)reader["PersonID"]]}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            var StartDate = DateTime.Now.AddMonths(-1);
            var EndDate = DateTime.Now.AddMonths(1);


            DataWachtPrestatie.Syncronise.Generate(StartDate, EndDate, out Errors);
        }

        internal Stack<string> generatePersonsStack(List<string> persons, int length)
        {
            if (persons.Count % 2 == 0)
            {
                persons = ReverseBy1AndAtToSelf(persons);
            }

            Stack<string> stack = new Stack<string>();

            List<string> clonePersons = persons.ToList();
            for (int i = 0; i < length; i++)
            {
                if (clonePersons.Count == 0) clonePersons = persons.ToList();
                stack.Push(clonePersons[0]);
                clonePersons.RemoveAt(0);
            }
            stack = Reverse(stack);
            return stack;
        }
        public List<string> ReverseBy1AndAtToSelf(List<string> persons)
        {
            List<string> clonePersons = persons.ToList();
            clonePersons.Reverse<string>();
            for (int i = 0; i < persons.Count; i++)
            {
                if (i % 2 != 0)
                {
                    clonePersons.Reverse(i - 1, 2);
                }
            }
            persons.AddRange(clonePersons);
            return persons;

        }
        static Stack<string> Reverse(Stack<string> stack)
        {
            Stack<string> temp = new Stack<string>();
            while (stack.Count > 0)
            {
                temp.Push(stack.Pop());
            }
            return temp;
        }
        private List<DateTime> createDatesCollection(DateTime start, DateTime end)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime currentDate = start;

            while (currentDate <= end)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
//            MessageBox.Show(dates.Count.ToString());
            return dates;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            DateTime startDatum = dateTimePicker1.Value;
            DateTime eindDatum = dateTimePicker2.Value;


            List<DateTime> dateCollection = createDatesCollection(startDatum, eindDatum);

            List<string> persons = new List<string>() { "Nick", "Geert", "Stefan", "Dieter", "Karl" };

            Stack<string> personsStack = generatePersonsStack(persons, dateCollection.Count);

            string currentPerson = personsStack.Pop();
            string output = "";
            foreach (DateTime date in dateCollection)
            {
                if (date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Thursday) currentPerson = personsStack.Pop();
                output += date.DayOfWeek + " " + date.ToShortDateString() + ": " + currentPerson + Environment.NewLine;
            }
            MessageBox.Show(output);
        }

    }
}
