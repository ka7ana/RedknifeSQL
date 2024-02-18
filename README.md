# RedknifeSQL

A C# console utility for interacting with MSSQL servers

RedknifeSQL is a simple menu-based console application to aid with pen-testing, specifically whilst dealing with Microsoft SQL servers in an AD environment.

The application prompts the user for the server hostname and database name to connect to. At present, only Windows authentication is supported.

## Command line arguments

The following command line options are supported:

- `--debug` - run RedknifeSQL in debug mode (outputs all SQL queries/statements executed via options)
- `--server` - set the server hostname to connect to on startup (saves having to explicitly connect via option)
- `--db` - set the database name to connect to on startup (saves having to explicitly connect via option)
- `--script` - the path of a text file containing a SQL script to execute against the connected server (requires both `server` and `db` to be provided as args). This is for non-interactive mode.
- `--output` - the path of a file that output will be redirected to. Useful for non-interactive mode.

## Interactive mode

RedknifeSQL interactive mode outputs a numeric menu to the user and loops until the user quits. The options are as follows:

1. `Connect to server` - initiates a connection to a specific server and database - the user is prompted for these values
2. `Get current user information` - Outputs information about the current user database connection - the `SYSTEM_USER` and `USER_NAME()` values, as well as whether the user has the `public` and `sysadmin` roles.
3. `Get impersonatable roles` - gets the MSSQL server principals that can be impersonated by the current user.
4. `Connect to UNC path` - attempts to execute connect to the provided UNC path by executing the `xp_dirtree` procedure. Handy for grabbing NetNTLM hashes.
5. `Set 'EXECUTE AS LOGIN' value` - sets the `EXECUTE AS LOGIN` value
6. `Set 'EXECUTE AS USER' value` - sets the `EXECUTE AS USER` value
7. `Check if xp_cmdshell enabled` - checks whether `xp_cmdshell` is enabled on the server
8. `Set xp_cmdshell status` - attempts to enable/disable `xp_cmdshell` on the connected server. Also enables `show advanced options`. Requires appropriate permissions.
9. `Execute xp_cmdshell` - attempts to execute `xp_cmdshell` on the connected server. Requires `xp_cmdshell` to be enabled, and the appropriate permissions.
10. `Execute cmd via sp_OACreate/sp_OAMethod` - enables `Ole Automation Procedures` and attempts to use `sp_oacreate` to execute `wscript.shell`. Then calls `sp_oamethod` with the user provided shell command.
11. `List TRUSTWORTHY databases` - lists the system databases with the `is_trustworthy_on` flag set to `1` and the owner of the database
12. `Create assembly stored procedure` - uses `CREATE ASSEMBLY` to create a new DLL-based stored procedure for command execution. Enables `show advanced options`, `clr enabled` and disables `clr strict security`.
13. `Execute cmd via assembly stored procedure` - attempts to call the `cmdExecute` stored procedure (created by option `12`) with the user-provided command.
14. `Get linked SQL server info` - get information about the configured linked SQL servers, including usernames, roles and which user has the linked server connection
15. `Call xp_cmdshell on linked server` - attempts to call `xp_cmdshell` on a linked server. Note that this can be chained via serval linked servers by supplying the hostname as a comma-separated string (i.e. hostA,hostB,hostC). This option generates the appropriate `EXEC (...) AT [server]` query depending on the number of hosts supplied. This option also attempts to enable `show advanced options` on the target server.
16. `PrivEsc via callback xp_cmdshell from linked server` - calls `xp_cmdshell` on a linked server to gain priv esc on current server (in cases where current user does not have `xp_cmdshell` permissions on the current server)
17. `Execute SQL Command` - execute an arbitrary SQL command provided by the user. Formats the output as a table.
18. `Get server principals` - lists the `user`, `pricipal_id`, `type_desc` and `default_database_name` from `sys.server_principals`.

### Exiting RedknifeSQL

Entering option value `q`, `quit` or `exit` (irrespective of case) will exit RedknifeSQL and close the current DB connection (if one exists).

## Non-interactive (script mode)

In script mode, RedknifeSQL reads an input file (specified by the `--script` argument) containing SQL statements to execute. Note that the `--server` and `--db` command line args must be supplied in order to specify which server and database to connect to.

Upon connecting to the specified database, RedknifeSQL automatically executes the following options:

- `Get user information` (option 2)
- `Get impersonatable roles` (option 3)
- `Get server principals` (option 18)
- `Check if xp_cmdshell enabled` (option 7)
- `List TRUSTWORTHY databases` (option 11)
- `Get linked SQL server info` (option 14)

Once these options have been executed, RedknifeSQL attempts to execute the script/statements as specified in the `--script` argument. 

## RedknifeSQL as an InstallUtil Uninstall

RedknifeSQL can also be run as an InstallUtil Uninstall process, i.e.:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U C:\path\to\RedknifeSQL.exe
```

When run as an uninstaller, you can provide the arguments to RedknifeSQL as a text file named `RedknifeSQL.txt` (located in the same directory as `RedknifeSQL.exe`) - these will be parsed and passed to RedknifeSQL as if they had been specified at the command line directly.

**Quick note**: when running as an uninstaller, the presence of RedknifeSQL.txt is required. However, if no args are specified in the file, RedknifeSQL will drop into interactive mode, which is quite handy :). I should get round to fixing this i.e. allow it to drop into interactive mode by default if no RedknifeSQL.txt file present.

