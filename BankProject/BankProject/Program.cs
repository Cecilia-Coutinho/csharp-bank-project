﻿using Bytescout.Spreadsheet;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using static System.Console; //to simplify Console references
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.CodeDom;

namespace BankProject
{
    internal class Program
    {
        static BankUser[] bankUsers = {
            new BankUser("maria", "1234", new List<BankAccount>{new BankAccount("Savings", 1000), new BankAccount("Salary", 100) }),
            new BankUser("pedro", "1234", new List<BankAccount>{new BankAccount("Savings", 100) }),
            new BankUser("kalle", "1234", new List<BankAccount>{new BankAccount("Savings", 100) }),
            new BankUser("johan", "1234", new List < BankAccount >{new BankAccount("Salary", 0) }),
            new BankUser("matias", "1234", new List < BankAccount >{new BankAccount("Savings", 0), new BankAccount("Salary", 0) }) };

        //to get index of array of each user
        static int GetBankUserIndexByUserName(string? user)
        {
            for (int i = 0; i < bankUsers.Length; i++)
            {
                if (bankUsers[i].User == user)
                {
                    return i;
                }
            }

            return -1; //false
        }

        static void Main(string[] args)
        {
            LoadSpreadsheetData();
            RunSystem();
        }

        static void RunSystem()
        {
            Clear();
            PrintWelcome();
            Console.Write("\n\tUser: \n\t");
            string? user = Console.ReadLine()?.ToLower();

            int userIndex = GetBankUserIndexByUserName(user); //get user index

            bool accountFound = userIndex != -1;

            if (accountFound)
            {
                if (bankUsers[userIndex].LockAccountDateTime.AddMinutes(3) < DateTime.Now)
                {
                    CheckPassAndRunAccount(user, userIndex);
                }
                else
                {
                    // Block new login attempt after 3 minutes
                    Clear();
                    TimeSpan lockCounter = bankUsers[userIndex].LockAccountDateTime.AddMinutes(3) - DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\tMax failed attempts.\n".ToUpper());
                    Console.ResetColor();
                    Console.WriteLine($"\n\tPlease, wait! You can login again after {SetTimeFormat(lockCounter)}\n");
                    ReadLine();
                    RunSystem();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\tInvalid username\n".ToUpper());
                Console.ResetColor();
                Console.WriteLine($"\n\tPress ENTER to try again.\n");
                Console.ReadLine();
                RunSystem();
            }
        }

        static string SetTimeFormat(TimeSpan t)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)t.TotalHours,
                t.Minutes,
                t.Seconds);
        }

        static void PrintWelcome()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n\t============WELCOME TO BANK XZ!============\n");
            Console.ResetColor();

        }
        static Spreadsheet SaveSpreadsheet()
        {
            // Create new Spreadsheet
            Spreadsheet document = new Spreadsheet();
            document.Workbook.DefaultFont = new SpreadsheetFont("Arial", 10);

            // Add new worksheet
            Worksheet sheet = document.Workbook.Worksheets.Add("my_excel");

            int startRow = 0;

            //set titles in the sheet
            SetSpreadsheetTitles(sheet, startRow);

            //get and set data
            PrintWorksheetUserData(sheet, startRow);


            if (File.Exists(@".\writeExcelOutput.xlsx"))
            {
                File.Delete(@".\writeExcelOutput.xlsx");
            }
            document.SaveAs(@".\writeExcelOutput.xlsx");

            // AutoFit all columns
            sheet.Columns[0].AutoFit();
            sheet.Columns[1].AutoFit();
            sheet.Columns[2].AutoFit();
            sheet.Columns[3].AutoFit();

            document.Close();

            return document;
        }

        private static void AddAllBorders(Cell cell)
        {
            //add style spreadsheet
            cell.LeftBorderStyle = Bytescout.Spreadsheet.Constants.LineStyle.Thin;
            cell.RightBorderStyle = Bytescout.Spreadsheet.Constants.LineStyle.Thin;
            cell.TopBorderStyle = Bytescout.Spreadsheet.Constants.LineStyle.Thin;
            cell.BottomBorderStyle = Bytescout.Spreadsheet.Constants.LineStyle.Thin;
        }

        static void PrintWorksheetUserData(Worksheet sheet, int startRow)
        {
            //add data to spreadsheet
            for (int i = 0; i < bankUsers.Length; i++)
            {
                //add user index
                sheet.Cell((++startRow), 0).Value = GetBankUserIndexByUserName(bankUsers[i].User);
                AddAllBorders(sheet.Cell(startRow, 0));

                //add username
                sheet.Cell(startRow, 1).Value = bankUsers[i].User;
                sheet.Cell(startRow, 1).AlignmentHorizontal = Bytescout.Spreadsheet.Constants.AlignmentHorizontal.Right;
                AddAllBorders(sheet.Cell(startRow, 1));

                //add userPass
                sheet.Cell((startRow), 2).Value = bankUsers[i].Password;
                sheet.Cell(startRow, 2).AlignmentHorizontal = Bytescout.Spreadsheet.Constants.AlignmentHorizontal.Right;
                AddAllBorders(sheet.Cell(startRow, 2));

                //add bankAccount Data
                for (int j = 0; j < bankUsers[i].BankAccounts.Count; j++)
                {
                    string accountType = bankUsers[i].BankAccounts[j].AccountType;
                    float balance = bankUsers[i].BankAccounts[j].Balance;
                    if (accountType.Contains("Savings"))
                    {
                        sheet.Cell((startRow), 3).Value = balance;
                        sheet.Cell(startRow, 3).AlignmentHorizontal = Bytescout.Spreadsheet.Constants.AlignmentHorizontal.Right;
                        AddAllBorders(sheet.Cell(startRow, 3));
                    }
                    if (accountType.Contains("Salary"))
                    {
                        sheet.Cell((startRow), 4).Value = balance;
                        sheet.Cell(startRow, 4).AlignmentHorizontal = Bytescout.Spreadsheet.Constants.AlignmentHorizontal.Right;
                        AddAllBorders(sheet.Cell(startRow, 4));
                    }
                }
            }
        }

        static void SetSpreadsheetTitles(Worksheet sheet, int row)
        {
            //to set titles spreadsheet
            sheet.Cell((row), 0).Value = $"UserIndex".ToUpper();
            AddAllBorders(sheet.Cell(row, 0));
            sheet.Cell(row, 0).Font = new Font("Arial", 11, FontStyle.Bold);

            sheet.Cell((row), 1).Value = $"Username".ToUpper();
            AddAllBorders(sheet.Cell(row, 1));
            sheet.Cell(row, 1).Font = new Font("Arial", 11, FontStyle.Bold);

            sheet.Cell((row), 2).Value = $"UserPass".ToUpper();
            AddAllBorders(sheet.Cell(row, 2));
            sheet.Cell(row, 2).Font = new Font("Arial", 11, FontStyle.Bold);

            sheet.Cell((row), 3).Value = $"Savings".ToUpper();
            AddAllBorders(sheet.Cell(row, 3));
            sheet.Cell(row, 3).Font = new Font("Arial", 11, FontStyle.Bold);

            sheet.Cell((row), 4).Value = $"Salary".ToUpper();
            AddAllBorders(sheet.Cell(row, 4));
            sheet.Cell(row, 4).Font = new Font("Arial", 11, FontStyle.Bold);
        }

        static void LoadSpreadsheetData()
        {
            //to read document and save updated data

            Spreadsheet document = new Spreadsheet();
            document.LoadFromFile("writeExcelOutput.xlsx");

            // Get first worksheet
            Worksheet sheet = document.Workbook.Worksheets[0];

            //iterate with spreadsheet rows
            for (int rowIndex = 1; rowIndex < sheet.NotEmptyRowMax; rowIndex++)
            {
                //iterate with spreadsheet columns
                for (int columnIndex = 0; columnIndex < sheet.NotEmptyRowMax + 1; columnIndex++)
                {
                    //get data from titles
                    string columnTitle = sheet.Cell(0, columnIndex).ValueAsString;

                    //to update data
                    if (columnTitle == "USERNAME")
                    {
                        bankUsers[rowIndex - 1].User = sheet.Cell(rowIndex, columnIndex).ValueAsString;
                        bankUsers[rowIndex - 1].BankAccounts.Clear();
                    }
                    else if (columnTitle == "USERPASS")
                    {
                        bankUsers[rowIndex - 1].Password = sheet.Cell(rowIndex, columnIndex).ValueAsString;
                    }
                    else if (columnTitle == "SAVINGS")
                    {
                        bool hasSavings = sheet.Cell(rowIndex, columnIndex).ValueAsString.Length > 0;
                        if (hasSavings)
                        {
                            float savingsBalance = (float)sheet.Cell(rowIndex, columnIndex).ValueAsDouble;
                            bankUsers[rowIndex - 1].BankAccounts.Add(new BankAccount("Savings", savingsBalance));
                        }
                    }
                    else if (columnTitle == "SALARY")
                    {
                        bool hasSalary = sheet.Cell(rowIndex, columnIndex).ValueAsString.Length > 0;
                        if (hasSalary)
                        {
                            float salaryBalance = (float)sheet.Cell(rowIndex, columnIndex).ValueAsDouble;
                            bankUsers[rowIndex - 1].BankAccounts.Add(new BankAccount("Salary", salaryBalance));
                        }
                    }
                }
            }
            document.Dispose();
        }
        static void CheckPassAndRunAccount(string? user, int userIndex)
        {
            int maxInvalidPassAttempts = 3;
            int countPassAttempts;

            for (countPassAttempts = 1; countPassAttempts <= maxInvalidPassAttempts; countPassAttempts++)
            {
                Clear();

                PrintWelcome();
                Console.Write("\n\tPassword: \n\t");
                string? password = Console.ReadLine();

                if (bankUsers[userIndex].Password == password)
                {
                    RunMenu(user, password, userIndex);
                    break;
                }
                else
                {
                    //max attempts failed password

                    if (countPassAttempts < maxInvalidPassAttempts)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\tInvalid password!\n".ToUpper());
                        Console.ResetColor();
                        Console.WriteLine($"\n\tFailed Attempts: {countPassAttempts}" +
                            $"\n\tPress ENTER to try again.\n");
                        Console.ReadLine();
                    }
                    else
                    {
                        //get time to block account from loggin for 3 minutes
                        bankUsers[userIndex].LockAccountDateTime = DateTime.Now;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\tInvalid password!\n".ToUpper());
                        Console.ResetColor();
                        Console.WriteLine($"\n\tFailed Attempts: {countPassAttempts}. You can try again after 3 minutes");
                        Console.ReadLine();
                        RunSystem();
                    }
                }
            }
        }


        static void RunMenu(string? user, string? pass, int userIndex)
        {
            bool runLoginMenu = true;
            while (runLoginMenu)
            {
                Clear();
                string welcome1 = "\n\t============Welcome to your account============\n";

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(welcome1.ToUpper());
                Console.ResetColor();
                Console.WriteLine("\tPlease select one of the options below:");
                Console.WriteLine("\n\t1. View Accounts and Balance\n" +
                    "\n\t2. Make a Deposit\n" +
                    "\n\t3. Transfer Between Accounts\n" +
                    "\n\t4. Withdraw Money\n" +
                    "\n\t5. Create New Account\n" +
                    "\n\t6. Money Exchange Simulation\n" +
                    "\n\t7. Change Password\n" +
                    "\n\t8. Log Out\n");
                Console.Write("\t Select menu: ");

                int menuChoice;
                int.TryParse(Console.ReadLine(), out menuChoice);

                switch (menuChoice)
                {
                    case 1:
                        ViewAccountsAndBalance(userIndex);
                        GoBackMenuOptions();
                        break;
                    case 2:
                        MakeDeposit(userIndex);
                        GoBackMenuOptions();
                        break;
                    case 3:
                        TransferBtwAccounts(userIndex);
                        GoBackMenuOptions();
                        break;
                    case 4:
                        WithdrawMoney(userIndex);
                        GoBackMenuOptions();
                        break;
                    case 5:
                        CreateNewAccount(userIndex);
                        GoBackMenuOptions();
                        break;
                    case 6:
                        /*
                         * only does a simulation with a fixed exchange rate
                         * The method doesn'lockCounter do anything directly in the user account
                        */
                        MenuCurrencyConverterSimulation();
                        break;
                    case 7:
                        ChangePassword(userIndex);
                        GoBackMenuOptions();
                        break;
                    case 8:
                        SaveSpreadsheet();
                        Console.WriteLine("\n\tThanks for your visit!");
                        Thread.Sleep(1000);

                        RunSystem();
                        break;
                    default:
                        Console.Clear();
                        InvalidOption();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\tPlease, choose 1-5 from the menu\n");
                        Console.ResetColor();
                        GoBackMenuOptions();
                        break;
                }
            }
        }

        static void ViewAccountsAndBalance(int userIndex)
        {
            Clear();
            Console.WriteLine("\n=============BALANCE=============");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"\n\tHi {bankUsers[userIndex].User.ToUpper()}!\n");

            //to get user and bank account
            for (int i = 0; i < bankUsers[userIndex].BankAccounts.Count; i++)
            {
                ///show balance accounts and types
                var accountBalance = bankUsers[userIndex].BankAccounts[i].Balance;
                var accountType = bankUsers[userIndex].BankAccounts[i].AccountType;

                Console.WriteLine($"\n\t{accountType}: {accountBalance.ToString("c2", CultureInfo.CreateSpecificCulture("sv-SE"))}\n");

            }
            Console.ResetColor();
        }
        static void MakeDeposit(int userIndex)
        {
            Clear();
            Console.Write("\n\tHow much $$ do you want to deposit? ");
            string? deposit = Console.ReadLine();
            float depositAmount;
            float.TryParse(deposit, out depositAmount);

            int userChoiceAccount = SelectAccount(userIndex);
            int goBackOption = GetGoBackOption(userIndex);

            bool userChoiceAccountIsValid = userChoiceAccount != -1;


            if (depositAmount <= 0)
            {
                NegativeAmount();
            }
            else if (userChoiceAccountIsValid)
            {
                bankUsers[userIndex].BankAccounts[userChoiceAccount].Balance += depositAmount;
                ViewAccountsAndBalance(userIndex);
            }
        }

        static void WithdrawMoney(int userIndex)
        {
            Clear();
            Console.Write("How much $$ do you want to withdraw? ");
            string? withdraw = Console.ReadLine();
            float withdrawAmount;
            float.TryParse(withdraw, out withdrawAmount);

            int userChoiceAccount = SelectAccount(userIndex);
            int goBackOption = GetGoBackOption(userIndex);
            bool userChoiceAccountIsValid = userChoiceAccount != -1;

            if (withdrawAmount <= 0)
            {
                NegativeAmount();
            }
            else if (userChoiceAccountIsValid)
            {
                Console.Write("\n\tENTER your Password: \n\t");
                string? password = Console.ReadLine();


                if (bankUsers[userIndex].Password == password)
                {
                    var accountBalance = bankUsers[userIndex].BankAccounts[userChoiceAccount].Balance;

                    if (withdrawAmount <= accountBalance)
                    {
                        bankUsers[userIndex].BankAccounts[userChoiceAccount].Balance -= withdrawAmount;
                        ViewAccountsAndBalance(userIndex);
                    }
                }
                else
                {
                    Console.WriteLine("\n\tYou don't have enough money");
                }
            }
        }

        static void TransferBtwAccounts(int userIndex)
        {
            Clear();
            Console.Write("\n\tTo whom do you want to transfer?: ");
            string? transferTo = Console.ReadLine();

            int targetUserIndex = GetBankUserIndexByUserName(transferTo);

            bool accountTargetFound = targetUserIndex != -1;

            if (accountTargetFound)
            {
                Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n\tselect type of account to transfer from:\n");
                Console.ResetColor();

                int userChoiceAccount = SelectAccount(userIndex);
                //int goBackOption = GetGoBackOption(userIndex);

                Console.WriteLine($"\n\tselect type of account to transfer to:\n");

                int targetUserChoiceAccount = SelectAccount(targetUserIndex);

                bool invalidUserChoiceAccount = userChoiceAccount == -1;

                if (!invalidUserChoiceAccount)
                {
                    ForegroundColor = ConsoleColor.Green;
                    Console.Write("\n\tHow much $$ do you want to transfer? ");
                    string? transfer = Console.ReadLine();
                    ResetColor();
                    float transferAmount;
                    float.TryParse(transfer, out transferAmount);

                    if (transferAmount <= 0)
                    {
                        NegativeAmount();
                    }
                    else if (bankUsers[userIndex].BankAccounts[userChoiceAccount].Balance < transferAmount)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\tERROR! Not allowed. You don't have enough money");
                        ResetColor();
                        Console.WriteLine("\n\tPress ENTER to see your Balance");
                        Console.ReadLine();
                        ViewAccountsAndBalance(userIndex);

                    }
                    else
                    {

                        //add pin to confirm transaction

                        Console.Write("\n\tENTER your Password: \n\t");
                        string? password = Console.ReadLine();


                        if (bankUsers[userIndex].Password == password)
                        {
                            Clear();
                            //targetUserChoiceAccount = SelectAccount(targetUserIndex);
                            bool invalidTargetUserChoiceAccount = targetUserChoiceAccount == -1;

                            if (!invalidTargetUserChoiceAccount)
                            {
                                bankUsers[userIndex].BankAccounts[userChoiceAccount].Balance -= transferAmount;
                                bankUsers[targetUserIndex].BankAccounts[targetUserChoiceAccount].Balance += transferAmount;

                                ViewAccountsAndBalance(userIndex);
                            }

                        }
                        else
                        {
                            ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Transaction not allowed. Check your password!");
                            ResetColor();
                            //InvalidOption();
                        }
                    }

                }

                ////PRINT TO CHECK TRANSFER (both users)
                //for (int i = 0; i < bankUsers[userIndex].BankAccounts.Count; i++)
                //{
                //    //show balance accounts and types
                //    var accountBalance = bankUsers[userIndex].BankAccounts[i].Balance;
                //    var accountType = bankUsers[userIndex].BankAccounts[i].AccountType;

                //    var targetAccountBalance = bankUsers[targetUserIndex].BankAccounts[i].Balance;
                //    var targetAccountType = bankUsers[targetUserIndex].BankAccounts[i].AccountType;

                //    Console.WriteLine($"\n\tHi {bankUsers[userIndex].User.ToUpper()}!\n");
                //    Console.WriteLine($"\n\lockCounter{accountType}: {accountBalance.ToString("c2", CultureInfo.CreateSpecificCulture("sv-SE"))}\n");

                //    Console.WriteLine($"\n\tHi {bankUsers[targetUserIndex].User.ToUpper()}!\n");
                //    Console.WriteLine($"\n\lockCounter{targetAccountType}: {targetAccountBalance.ToString("c2", CultureInfo.CreateSpecificCulture("sv-SE"))}\n");

                //}
            }
            else
            {
                ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\tInvalid User".ToUpper());
                ResetColor();
            }
        }

        static void CreateNewAccount(int userIndex)
        {
            //all bank accounts
            List<string> allAccountTypes = new List<string>();
            allAccountTypes.Add("Savings");
            allAccountTypes.Add("Salary");

            Console.WriteLine($"\n\tselect account:\n");

            //get available accounts for creation
            for (int j = 0; j < allAccountTypes?.Count; j++)
            {
                var menuNumber = j;

                for (int i = 0; i < bankUsers[userIndex].BankAccounts.Count; i++)
                {
                    var accountType = bankUsers[userIndex].BankAccounts[i].AccountType;


                    if (allAccountTypes.Contains(accountType))
                    {
                        allAccountTypes.Remove(accountType);
                    }
                }

                //print menu with available accounts
                if (allAccountTypes.Count > 0)
                {
                    Clear();
                    Console.WriteLine($"\n\t{menuNumber + 1}. {allAccountTypes[j]}\n");

                    var goBackOption = GetGoBackOption(userIndex);

                    Console.WriteLine($"\n\t{goBackOption + 1}. Go Back\n");
                    Console.Write("\t");

                    //get menu choice
                    int userChoiceAccount;
                    int.TryParse(Console.ReadLine(), out userChoiceAccount);
                    bool userChoiceIsValid = Convert.ToBoolean(userChoiceAccount);
                    userChoiceAccount -= 1; //to get the correct index instead of the printed index

                    //add new account
                    if (userChoiceIsValid && userChoiceAccount != goBackOption)
                    {
                        for (int i = 0; i < allAccountTypes.Count; i++)
                        {
                            bankUsers[userIndex].BankAccounts.Add(new BankAccount(allAccountTypes[userChoiceAccount], 0));
                            ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n\t New account '{allAccountTypes[userChoiceAccount]}' successfully created!\t");
                            ResetColor();
                            Console.WriteLine("\n\t Press enter to check your accounts and balance\t");
                            ReadLine();
                            ViewAccountsAndBalance(userIndex);
                        }
                    }
                    else if (userChoiceAccount == goBackOption)
                    {
                        break;
                    }
                    else
                    {
                        InvalidOption();
                    }
                }
                else
                {
                    Clear();
                    ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\t You already have all available accounts\n");
                    ResetColor();
                    break;
                }
            }
        }
        //a

        static void ChangePassword(int userIndex)
        {
            Clear();
            Console.Write("\n\tType your new Password: ");
            string? passwordNewOne = Console.ReadLine();
            if (passwordNewOne != null)
            {
                bankUsers[userIndex].Password = passwordNewOne;
                SaveSpreadsheet();
                ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n\tthe password was successfully changed!\n".ToUpper());
                ResetColor();
            }
            else
            {
                ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\tERROR! Password could not be changed");
                ResetColor();
            }
        }

        static bool MenuCurrencyConverterSimulation()
        {
            Clear();
            Console.WriteLine("\n\tTo which currency do you want to convert?\n\tPlease select one of the options below:");
            Console.WriteLine("\n\t1. From SEK to Dollar\n" +
                "\n\t2. From SEK to Euro\n" +
                "\n\t3. Go Back to menu\n");
            Console.Write("\t Select menu: ");

            int menuChoice;
            int.TryParse(Console.ReadLine(), out menuChoice);

            switch (menuChoice)
            {
                case 1:
                    GetCurrencyConvertToDollar();
                    GoBackMenuOptions();
                    return true;
                case 2:
                    GetCurrencyConvertToEuro();
                    GoBackMenuOptions();
                    return true;
                case 3:
                    Console.WriteLine("\n\tThanks for your visit!");
                    Thread.Sleep(1000);
                    return false;
                default:
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\tPlease, choose 1-3 from the menu\n");
                    Console.ResetColor();
                    ReadLine();
                    MenuCurrencyConverterSimulation();
                    return true;
            }
        }

        static void GetCurrencyConvertToDollar()
        {
            Clear();
            Console.Write("\n\tHow much SEK do you want to Convert: ");
            float currencyAmount;
            float.TryParse(Console.ReadLine(), out currencyAmount);
            ConvertToDollar convertToDollar = new ConvertToDollar(currencyAmount);
            Console.WriteLine(convertToDollar.PrintCurrencyConverter(currencyAmount));
        }

        static void GetCurrencyConvertToEuro()
        {
            Clear();
            Console.Write("\n\tHow much SEK do you want to Convert: ");
            float currencyAmount;
            float.TryParse(Console.ReadLine(), out currencyAmount);
            ConvertToEuro convertToEuro = new ConvertToEuro(currencyAmount);
            Console.WriteLine(convertToEuro.PrintCurrencyConverter(currencyAmount));
        }

        static void GoBackMenuOptions()
        {
            Console.WriteLine("\n\tPress ENTER to go back to the menu.\n");
            Console.ReadLine();
        }

        ///static void NewBalance(int userIndex)
        //{
        //    Clear();
        //    Console.WriteLine("\n=============Balance Updated=============");
        //    ViewAccountsAndBalance(userIndex);
        //}

        static void InvalidOption()
        {
            ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\tERROR: Invalid Option!".ToUpper());
            ResetColor();
        }

        static void NegativeAmount()
        {
            ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\tERROR! Transaction not allowed." +
                "\n\tThe Amount needs to be valid or higher than 0,00".ToUpper());
            ResetColor();
        }

        static int SelectAccount(int userIndex)
        {
            Console.WriteLine($"\n\tselect account:\n");

            for (int i = 0; i < bankUsers[userIndex].BankAccounts.Count; i++)
            {
                var accountType = bankUsers[userIndex].BankAccounts[i].AccountType;
                var menuNumber = i;

                //to start menu choices with option 1 instead of 0
                Console.WriteLine($"\n\t{menuNumber + 1}. {accountType}\n");

            }
            var goBackOption = GetGoBackOption(userIndex);

            Console.WriteLine($"\n\t{goBackOption + 1}. Go Back\n");
            Console.Write("\t");

            int userChoiceAccount;
            bool userChoiceIsValid = int.TryParse(Console.ReadLine(), out userChoiceAccount);
            userChoiceAccount -= 1; //to get the correct index instead of the printed index

            if (userChoiceIsValid)
            {
                if (userChoiceAccount < bankUsers[userIndex].BankAccounts.Count)
                {
                    return userChoiceAccount;
                }
            }

            if (userChoiceAccount != goBackOption)
            {
                InvalidOption();
            }

            return -1;
        }

        static int GetGoBackOption(int userIndex)
        {
            return bankUsers[userIndex].BankAccounts.Count;
        }
    }
}
