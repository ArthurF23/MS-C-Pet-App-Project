//same program but uses SQL server
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;

// https://learn.microsoft.com/en-us/training/modules/challenge-project-work-variable-data-c-sharp/2-prepare
// https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand?view=dotnet-plat-ext-8.0

//Main program class
class Program
{
    public class SQL_CMDS
    {
        //reduce repetative code with the SQL_BACKEND... Each function that uses sql can use the backend to
        //call the reader and get the output from there. Reduces lines of code too.
        static class SQL_BACKEND
        {
            //connection info stored here so if needed it can changed instead of it being constant
            public class CONNECTION_INFO
            {
                public static string dataSource = "";
                public static string userID = "";
                public static string password = "";
                public static string initialCatalog = "PetDatabase";
                public static bool trustCert = true;
            }

            //Build connection to SQL server.
            static public SqlConnection BuildConnection()
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = CONNECTION_INFO.dataSource;
                builder.UserID = CONNECTION_INFO.userID;
                builder.Password = CONNECTION_INFO.password;
                builder.InitialCatalog = CONNECTION_INFO.initialCatalog;
                builder.TrustServerCertificate = CONNECTION_INFO.trustCert;

                SqlConnection connection = new SqlConnection(builder.ConnectionString);

                return connection;
            }

            //Make connection and execute a query on the SQL database.
            static public SqlDataReader Execute_Command(string sqlCommand)
            {
                SqlConnection connection = BuildConnection();
                connection.Open();

                SqlCommand command = new SqlCommand(sqlCommand, connection);
                return command.ExecuteReader();
            }

            //Execute command with parameters
            static public SqlDataReader Execute_Command(string sqlCommand, params SqlParameter[] parameters)
            {
                SqlConnection connection = BuildConnection();
                connection.Open();

                SqlCommand command = new SqlCommand(sqlCommand, connection);
                command.Parameters.AddRange(parameters);
                return command.ExecuteReader();
            }
        }

        //Get all info on PetFiles database
        static public string GetAllPetFilesAndInfo(string sql)
        {
            string output = "";

            try
            {
                SqlDataReader reader = SQL_BACKEND.Execute_Command(sql);

                while (reader.Read())
                {
                    //Here we try to add the read value from the server to the string but if the value in the server is null then it will throw an error
                    //We try & catch like if else to prevent getting errors from null values from the server
                    try { output += reader.GetString(0) + " "; }
                    catch (SqlNullValueException e) { output += "NULL "; }

                    try { output += reader.GetString(1) + " "; }
                    catch (SqlNullValueException e) { output += "NULL "; }

                    try { output += reader.GetString(2) + " "; }
                    catch (SqlNullValueException e) { output += "NULL "; }

                    try { output += reader.GetInt32(3).ToString() + " "; }
                    catch (SqlNullValueException e) { output += "NULL "; }

                    try { output += reader.GetString(4) + " "; }
                    catch (SqlNullValueException e) { output += "NULL "; }

                    try { output += reader.GetString(5) + " "; }
                    catch (SqlNullValueException e) { output += "NULL "; }

                    try { output += '$' + reader.GetDecimal(6).ToString() + '\n'; }
                    catch { output += "NULL\n"; }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return output;
        }

        //Find matching data in Physical and Personality descriptions
        static public string FindMatchingCharacteristics(string desc)
        {
            string sql = "SELECT [ID], [SPECIES], [NICKNAME], [AGE], [PHYSICAL_DESCRIPTION], [PERSONALITY_DESCRIPTION] " +
                 "FROM [PetDatabase].[Pets].[Files] " +
                 "WHERE PHYSICAL_DESCRIPTION LIKE @Desc OR PERSONALITY_DESCRIPTION LIKE @Desc";

            SqlParameter parameter = new SqlParameter("@Desc", "%" + desc + "%");

            StringBuilder outputBuilder = new StringBuilder();

            try
            {
                using (SqlDataReader reader = SQL_BACKEND.Execute_Command(sql, parameter))
                {
                    while (reader.Read())
                    {
                        // Handle null values using DBNull.Value
                        outputBuilder.AppendFormat("{0} ", reader[0] == DBNull.Value ? "NULL" : reader.GetString(0));
                        outputBuilder.AppendFormat("{0} matches the description.\n", reader[2] == DBNull.Value ? "NULL" : reader.GetString(2));
                        outputBuilder.AppendFormat("    Physical Description: {0}\n", reader[4] == DBNull.Value ? "NULL" : reader.GetString(4));
                        outputBuilder.AppendFormat("    Personality Description: {0}\n", reader[5] == DBNull.Value ? "NULL" : reader.GetString(5));
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return outputBuilder.ToString();
        }

        //Create ID for pet by parsing all ids. Number will always increment.
        //ID = {first letter of species} + {number increment}
        static public string CreatePetID(string species)
        {
            string sql = "SELECT [ID] FROM [PetDatabase].[Pets].[Files]";
            string output = "";

            SqlDataReader reader = SQL_BACKEND.Execute_Command(sql);
            while (reader.Read())
            {
                //Here we try to add the read value from the server to the string but if the value in the server is null then it will throw an error
                //We try & catch like if else to prevent getting errors from null values from the server
                try { output = reader.GetString(0); }
                catch (SqlNullValueException e) { output = "NULL "; }
            }

            output = output.Remove(0, 1);
            int num = Int32.Parse(output) + 1;
            output = species[0] + num.ToString();
            return output;
        }

        //Add pet to database
        static public void AddPetToDatabase(string species, string nickname, string age, string physical_desc, string personal_desc, string sug_dono)
        {
            string id = CreatePetID(species);

            string sql = "INSERT INTO [PetDatabase].[Pets].[Files](ID, SPECIES, NICKNAME, AGE, PHYSICAL_DESCRIPTION, PERSONALITY_DESCRIPTION, SUGGESTED_DONATION) " +
                         "VALUES(@ID, @Species, @Nickname, @Age, @PhysicalDesc, @PersonalDesc, @SugDono)";

            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@ID", id),
            new SqlParameter("@Species", species),
            new SqlParameter("@Nickname", nickname),
            new SqlParameter("@Age", age),
            new SqlParameter("@PhysicalDesc", physical_desc),
            new SqlParameter("@PersonalDesc", personal_desc),
            new SqlParameter("@SugDono", sug_dono)
                };

                SQL_BACKEND.Execute_Command(sql, parameters);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    //Take input func that trims and lowercases every input
    private static void takeInput(ref string? input)
    {
        string? readResult = null; //Data entry for user input

        //Request input from user for the desired species
        while (readResult == null)
        {
            readResult = Console.ReadLine();
            if (readResult != null)
            {
                input = readResult.ToLower().Trim();
                Console.WriteLine();
            }
        }
    }

    //Main function
    public static void Main(string[] args)
    {
        ///////////////////////
        //Additional Defs    //
        ///////////////////////

        string? menuSelection = "";

        ///////////////////////
        //Console Loop       //
        ///////////////////////

        do
        {
            // NOTE: the Console.Clear method is throwing an exception in debug sessions
            Console.Clear();

            Console.WriteLine("Welcome to Hejlsberg's PetFriends app. Your main menu options are:");
            Console.WriteLine(" 1. List all of our current pet information");
            Console.WriteLine(" 2. Search pets via ID");
            Console.WriteLine(" 3. Search a species with specified characteristics");
            Console.WriteLine(" 4. Add animal to database");
            Console.WriteLine();
            Console.WriteLine("Enter your selection number (or type Exit to exit the program)");

            menuSelection = null;
            takeInput(ref menuSelection);

            //Switch case on user input
            switch (menuSelection)
            {
                //case 1, list all animals in the animals database along with all their information
                case "1":
                    Console.WriteLine(SQL_CMDS.GetAllPetFilesAndInfo("SELECT * FROM [PetDatabase].[Pets].[Files]"));

                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                    break;

                //Find via ID
                case "2":
                    Console.WriteLine("Please input ID");

                    string? IDref = "";
                    takeInput(ref IDref);

                    Console.WriteLine((IDref == null ? "NULL STR INPUTTED" : SQL_CMDS.GetAllPetFilesAndInfo("SELECT * FROM [PetDatabase].[Pets].[Files] WHERE ID = '" + IDref + "'")));

                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                    break;

                //Search for pet based on characteristic types
                case "3":
                    Console.WriteLine("Please input the characteristic you would like to search for");

                    string? charDesc = "";
                    takeInput(ref charDesc);

                    Console.WriteLine((charDesc == null ? "NULL STR INPUTTED" : SQL_CMDS.FindMatchingCharacteristics(charDesc)));

                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                    break;

                //Add animal to database
                case "4":
                    string? SPECIES = "", NICKNAME = "", AGE = "", PHYSICAL_DESCRIPTION = "", PERSONALITY_DESCRIPTION = "", SUGGESTED_DONATION = "";

                    Console.WriteLine("Please enter species");
                    takeInput(ref SPECIES);

                    Console.WriteLine("Please enter nickname");
                    takeInput(ref NICKNAME);

                    Console.WriteLine("Please enter age");
                    takeInput(ref AGE);

                    Console.WriteLine("Please enter physical description");
                    takeInput(ref PHYSICAL_DESCRIPTION);

                    Console.WriteLine("Please enter personality description");
                    takeInput(ref PERSONALITY_DESCRIPTION);

                    Console.WriteLine("Please enter suggested donation");
                    takeInput(ref SUGGESTED_DONATION);

                    if (SPECIES == null ||
                        NICKNAME == null ||
                        AGE == null ||
                        PHYSICAL_DESCRIPTION == null ||
                        PERSONALITY_DESCRIPTION == null ||
                        SUGGESTED_DONATION == null ||
                        SPECIES.Length == 0 ||
                        NICKNAME.Length == 0 ||
                        AGE.Length == 0 ||
                        PHYSICAL_DESCRIPTION.Length == 0 ||
                        PERSONALITY_DESCRIPTION.Length == 0 ||
                        SUGGESTED_DONATION.Length == 0
                        )
                    {
                        Console.WriteLine("Missing value entered. Please try again.");
                        break;
                    }
                    else
                    {
                        SQL_CMDS.AddPetToDatabase(SPECIES, NICKNAME, AGE, PHYSICAL_DESCRIPTION, PERSONALITY_DESCRIPTION, SUGGESTED_DONATION);
                    }

                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                    break;
            };
        } while (menuSelection != "exit");
    }
}
