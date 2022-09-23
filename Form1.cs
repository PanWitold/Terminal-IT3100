using System;
using System.Globalization; // needed?
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using Calib;


namespace terminal
{
    public partial class Onufry : Form
    {
        string TM_SERIES;   // A-F
        bool debug = false; // if debugging  <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< DEBUGGING ON TERMINAL     <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        bool month_is_closed = false;

        string BMP_filename_ticket_single_O = @"\FlashDisk\TM\ticket_prize_list\jedn_O.bmp";
        string BMP_filename_ticket_single_OK = @"\FlashDisk\TM\ticket_prize_list\jedn_OK.bmp";
        string BMP_filename_ticket_single_line_O =  @"\FlashDisk\TM\ticket_prize_list\odc_O.bmp";
        string BMP_filename_ticket_single_line_OK = @"\FlashDisk\TM\ticket_prize_list\odc_OK.bmp";
        string BMP_filename_ticket_family_O =  @"\FlashDisk\TM\ticket_prize_list\rodzinny_O.bmp";
        string BMP_filename_ticket_family_OK = @"\FlashDisk\TM\ticket_prize_list\rodzinny_OK.bmp";
        string BMP_filename_ticket_special_O =  @"\FlashDisk\TM\ticket_prize_list\taniogzubowy_O.bmp";
        string BMP_filename_ticket_special_OK = @"\FlashDisk\TM\ticket_prize_list\taniogzubowy_OK.bmp";
        string BMP_filename_ticket_monthly = @"\FlashDisk\TM\ticket_prize_list\miesieczny.bmp";
        string BMP_filename_ticket_bike= @"\FlashDisk\TM\ticket_prize_list\rower.bmp";

        string CSV_filename_counters =    @"\FlashDisk\TM\Data\tm_variables";
        string CSV_filename_series_logs = @"\FlashDisk\TM\Data\tm_series_logs";
        string CSV_filename_logs;
        string CSV_filename_tickets_series;
        string CSV_filename_tickets_month = "";
        string CSV_filename_tickets_month_SD;
        string CSV_filename_canceled_SD;
        string cancel_filename_flash = "";

        char CSV_delimiter = ';';
        int counter_series = 1;
        bool bool_open_cash = false;
        int counter_tickets = 1; 
        string curr_user = "";
        string choosen_start = "";
        string choosen_destination = "";
        string train_type;
        int relief = 0;
        double ticket_prize = 0.0;
        double relief_prize = 0.0;
        double normal_ticket_prize = 0; // only to generate at CSV file
        double family_ticket_prize = 0; // -||-

        bool was_printed = false;
        bool bool_ticket_T = false;  //bilety wybrane
        bool bool_ticket_TP = false;
        bool bool_ticket_month = false;
        bool bool_ticket_bike = false;
        bool bool_ticket_group = false;
        bool bool_is_special = false;
        bool bool_KDR = false;
        bool extra_prize = false;  
        int extra_prize_money = 5;  // in cash(PLN) - change this variable if wrong
        int credentials_type;

        // var niezbedne do ref aby mieæ cenê biletu do zwrotu
        string canceled_ticket_number;
        DateTime cancel_ticket_date;
        double cancel_value;
        string canceled_ticket_info_line;
        List<int> canceled_tickets = new List<int>();
        // var do wydruku zamkniecia zmiany
        string data_series_start;
        string data_series_stop;
        double ticket_cash_people;
        double ticket_cash_bike;
        double ticket_cash_extra;
        double cancel_ticket_cash_people = 0;
        double cancel_ticket_cash_bike = 0;
        double cancel_ticket_cash_extra = 0;
        double ticket_cashless_pay; // podany przez usera
        double earn_summary;
        double return_summary;
        int ticket_first = 2147483646;
        int ticket_last = 0;
        //needed for printer
        string strInputCharANK = ((char)27).ToString() + "Y" + ((char)1).ToString();
        string strBuff = ((char)32).ToString();
        string strCR = ((char)13).ToString();	// Set Carriage Return code
        // arrays 
        string[] TM_devices = new string[6] // serial_number : number of TM
        {
            "8C9B02716AAAA1:A",
            "861B01310AAAA1:B",
            "8C9B02864AAAA1:C",
            "861C01710AAAA1:D",
            "8C9B02946AAAA1:E",
            "8C9802110AAAA1:F",
        };

        string[] users_list = new string[16]
        {
        "1111:1470",   // first is superuser - could be changed
        "01:1986",
        "02:1994",
        "03:1999",
        "04:1999",
        "05:1993",
        "06:1980",
        "07:1998",
        "08:1996",
        "09:1974",
        "10:2010",
        "11:2011",
        "12:2012",
        "13:2013",
        "14:2014",
        "15:2015",
        };

        string[] stations_1 = new string[8]
        {
            "Œroda Wlkp.Miasto",
            "Œroda Wlkp. W¹sk",  
            "S³upia Wielka",
            "Annopole",
            "P³aczki",
            "Œnieciska",
            "Polwica",
            "Zaniemyœl",
        };

        string[] stations_2 = new string[8] // must have the same stations as the stations_1
        {
            "Œroda Wlkp.Miasto",
            "Œroda Wlkp. W¹sk",  
            "S³upia Wielka",
            "Annopole",
            "P³aczki",
            "Œnieciska",
            "Polwica",
            "Zaniemyœl",
        };

        public void Led_loop()
        {
            while (true)
            {
                Thread.Sleep(10000);
                string batt_status = BatteryInfo.GetSystemPowerStatus().ToString();
                if (batt_status == "VeryHigh")
                {
                    Calib.SystemLibNet.Api.SysSetLED(Calib.SystemLibNet.Def.LED_OFF,0,0,0);
                }
                else if (batt_status == "High")
                {
                    Calib.SystemLibNet.Api.SysSetLED(Calib.SystemLibNet.Def.LED_GREEN, 2, 5, 30);
                }
                else if (batt_status == "Medium")
                {
                    Calib.SystemLibNet.Api.SysSetLED(Calib.SystemLibNet.Def.LED_ORANGE, 5, 15, 15);
                }
                else
                {
                    Calib.SystemLibNet.Api.SysSetLED(Calib.SystemLibNet.Def.LED_RED, 5, 25, 5);
                }
            }
        }
        private void GoFullscreen() // truly fullscreen
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
        }
        private void ShowErrorMessage(string pszCaption, int nStatus)
        {
            //string szTitle = "PrinterLib Error";
            string szMsg;
            //error message
            switch (nStatus)
            {
                case PrinterLibNet.Def.PRN_NORMAL:
                    return;

                case PrinterLibNet.Def.PRN_NOTOPEN:
                    szMsg = "PRN_NOTOPEN";
                    break;

                case PrinterLibNet.Def.PRN_DRIVER_NOTEXIST:
                    szMsg = "PRN_DRIVER_NOTEXIST";
                    break;

                case PrinterLibNet.Def.PRN_ALREADY_OPEN:
                    szMsg = "PRN_ALREADY_OPEN";
                    break;

                case PrinterLibNet.Def.PRN_NOTFOUND:
                    szMsg = "Def.PRN_NOTFOUND";
                    break;

                case PrinterLibNet.Def.PRN_NOTCHANGE:
                    szMsg = "PRN_NOTCHANGE";
                    break;

                case PrinterLibNet.Def.PRN_FILE_NOTEXIST:
                    szMsg = "PRN_FILE_NOTEXIST";
                    break;

                case PrinterLibNet.Def.PRN_FILEFORMAT_ERROR:
                    szMsg = "PRN_FILEFORMAT_ERROR";
                    break;

                case PrinterLibNet.Def.PRN_FILEOPEN_ERROR:
                    szMsg = "PRN_FILEOPEN_ERROR";
                    break;

                case PrinterLibNet.Def.PRN_PARAMETER_ERROR:
                    szMsg = "PRN_PARAMETER_ERROR";
                    break;

                case PrinterLibNet.Def.PRN_HARDWARE_ERROR:
                    szMsg = "PRN_HARDWARE_ERROR";
                    break;

                case PrinterLibNet.Def.PRN_PLATEN_OPEN:
                    szMsg = "PRN_PLATEN_OPEN";
                    break;

                case PrinterLibNet.Def.PRN_PAPER_END:
                    szMsg = "RN_PAPER_END";
                    break;

                case PrinterLibNet.Def.PRN_AUTOLOADING:
                    szMsg = "PRN_AUTOLOADING";
                    break;

                case PrinterLibNet.Def.FUNCTION_UNSUPPORT:
                    szMsg = "FUNCTION_UNSUPPORT";
                    break;

                default:
                    szMsg = "Unknown Status";
                    break;
            }
            // display messagebox
            MessageBox.Show(szMsg, pszCaption);
            string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "error", curr_user, counter_series.ToString(), "Printer error " + szMsg);
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        public bool is_printer_ok()
        {
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();
            nRet = PrinterLibNet.Api.PRNOpen();
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                return false;
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                return false;
            }
            //close printer driver
            nRet = PrinterLibNet.Api.PRNClose();
            return true;
        }
        private int print_screen() // by printer | -1 - was printed | -2 - error 
        {
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();

            nRet = PrinterLibNet.Api.PRNOpen();
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                was_printed = true;
                return -2;  // error from printer
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                was_printed = true;
                return -2;  // error from printer
            }
            //print out display screen
            if (!was_printed)
            {
                if(!debug)
                {
                nRet = PrinterLibNet.Api.PRNPrintScreen();
                PrinterLibNet.Api.PRNTextOut(1, strCR);
                PrinterLibNet.Api.PRNTextOut(0, "\n");
                ShowErrorMessage("PRNPrintScreen", nRet);
                }
                was_printed = true;
                string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "Print_Screen");
                saveToFile(CSV_filename_logs, tempstring, false);
            }
            else
            {
                PrinterLibNet.Api.PRNClose();
                was_printed = true;
                return -1;
            }

            //close printer driver
            nRet = PrinterLibNet.Api.PRNClose();
            ShowErrorMessage("PRNClose", nRet);
            return 1; // all ok
        }
        public void print_canceled_ticket()
        {
            string strFont = ((char)27).ToString() + "F" + ((char)4).ToString();
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();

            nRet = PrinterLibNet.Api.PRNOpen();
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                return;
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                return;
            }
            PrinterLibNet.Api.PRNTextOut(1, strFont); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "Towarzystwo Przyjaciol \n            Kolejki Sredzkiej BANA\nul. Dworcowa 3\n63-000 Sroda Wielkopolska\nNIP 786-169-82-53\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "Anulowanie biletu\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "Bilet nr       " + canceled_ticket_number + "\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "Do zwrotu:     " + String.Format("{0:0.00}", cancel_value) + " PLN\n\n");
            //print out carriage return
            nRet = PrinterLibNet.Api.PRNTextOut(1, strCR);

            //close printer driver
            nRet = PrinterLibNet.Api.PRNClose();
            ShowErrorMessage("PRNClose", nRet);
            string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "Wydrukowano anulowanie biletu");
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        public void print_credentials(int type) // 1 = all_return, 2 = partial_return, 3 = delayed
        {
            string strFont = ((char)27).ToString() + "F" + ((char)4).ToString();
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();

            nRet = PrinterLibNet.Api.PRNOpen();
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                return;
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                return;
            }
            if (was_printed)
            {
                return;
            }
            PrinterLibNet.Api.PRNTextOut(1, strFont); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "Towarzystwo Przyjaciol \n            Kolejki Sredzkiej BANA\nul. Dworcowa 3\n63-000 Sroda Wielkopolska\nNIP 786-169-82-53\n");
            PrinterLibNet.Api.PRNTextOut(0, "Poswiadczenie\n");
            switch (type)
            {
                case 1:
                    PrinterLibNet.Api.PRNTextOut(0, "Bilet nr: " + this.textBox7.Text + this.textBox8.Text + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Calkowicie niewykorzystany\n");
                    PrinterLibNet.Api.PRNTextOut(0, "W dniu: " +DateTime.Now.ToString("yyyy-MM-dd")+ "\n");
                    if (comboBox3.SelectedItem.ToString() == "Wydano nowy bilet numer:")
                    {
                        PrinterLibNet.Api.PRNTextOut(0, "Wydano nowy bilet numer:" + this.textBox9.Text + this.textBox10.Text + "\n");
                    }
                    else 
                    {
                        PrinterLibNet.Api.PRNTextOut(0, "Nie wydano nowego biletu\n");
                    }
                    break;
                case 2:
                    PrinterLibNet.Api.PRNTextOut(0, "Bilet nr: " + this.textBox7.Text + this.textBox8.Text + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Niewykorzystany przez osob:\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Normalny: " + this.credentials_numeric_norm.Value.ToString() + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Ulgowy: " + this.credentials_numeric_relief.Value.ToString() + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Rower: " + this.credentials_numeric_bike.Value.ToString() + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Rodzinny: " + this.credentials_numeric_family.Value.ToString() + "\n\n");
                    break;
                case 3:
                    PrinterLibNet.Api.PRNTextOut(0, "Pociag nr   " + "" + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "W dniu: " + DateTime.Now.ToString("yyyy-MM-dd") + "\n");
                    PrinterLibNet.Api.PRNTextOut(0, "Opozniony o: " + this.textBox10.Text+ " minut.\n");
                    break;
                default:
                    break;
            }

            PrinterLibNet.Api.PRNTextOut(0, "Nr sluzbowy: K" + curr_user + "\n\n\n");
            
            //print out carriage return
            nRet = PrinterLibNet.Api.PRNTextOut(1, strCR);

            //close printer driver
            nRet = PrinterLibNet.Api.PRNClose();
            ShowErrorMessage("PRNClose", nRet);
            was_printed = true;
        }
        public int print_from_bitmap(string filename)
        {
            if(!File.Exists(filename))  // file dont exists
            {
                return -3;
            }
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();

            nRet = PrinterLibNet.Api.PRNOpen();
            int all_ok;
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                PrinterLibNet.Api.PRNClose();
                ShowErrorMessage("PRNClose", nRet);
                return -2;  // error from printer
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                PrinterLibNet.Api.PRNClose();
                ShowErrorMessage("PRNClose", nRet);
                return -2;  // error from printer
            }
            //print out display screen
            if (!was_printed)
            {
                nRet = PrinterLibNet.Api.PRNBMPOut(filename);
                if (nRet == PrinterLibNet.Def.PRN_NORMAL)
                {
                    PrinterLibNet.Api.PRNTextOut(1, strCR);
                    PrinterLibNet.Api.PRNTextOut(0, "\n\n\n");
                    was_printed = true;
                    all_ok = 1;
                }
                else
                {
                    ShowErrorMessage("Printing..", nRet);
                    all_ok = -1;
                }
            }
            else
            {
                PrinterLibNet.Api.PRNClose();
                all_ok = -1;
            }


            //close printer driver
            PrinterLibNet.Api.PRNClose();

              return all_ok;
        }
        public void print_close_selling() 
        {
            // vars
            string strFont = ((char)27).ToString() + "F" + ((char)4).ToString();

            int ticket_counter = ticket_last - ticket_first +1;
            string ticket_1 = TM_SERIES + ticket_first.ToString();
            string ticket_2 = TM_SERIES + ticket_last.ToString();

            double summing_return = return_summary + ticket_cashless_pay + (earn_summary - return_summary - ticket_cashless_pay);
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();

            nRet = PrinterLibNet.Api.PRNOpen();
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                return;
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                return;
            }

            PrinterLibNet.Api.PRNTextOut(1, strFont); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "Towarzystwo Przyjaciol \n            Kolejki Sredzkiej BANA\nul. Dworcowa 3\n63-000 Sroda Wielkopolska\nNIP 786-169-82-53\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "O" + ((char)0).ToString()); // BOLD
            PrinterLibNet.Api.PRNTextOut(0, "Zamkniecie zmiany nr       " + (counter_series -1).ToString() + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "O" + ((char)1).ToString()); // BOLD
            PrinterLibNet.Api.PRNTextOut(0, "Terminal mobilny           " + TM_SERIES + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Otwarcie zmiany    " + data_series_start + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Zamkniecie zmiany  " + data_series_stop + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Wydrukowano        " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "PRZYCHODY                  [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "Przejazd osob              " + String.Format("{0:0.00}", ticket_cash_people) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Przewoz roweru             " + String.Format("{0:0.00}", ticket_cash_bike) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)3).ToString());    //mniejsza czcionka
            PrinterLibNet.Api.PRNTextOut(0, "Opl. za wyd. bil. z PTU 8%          " + String.Format("{0:0.00}", ticket_cash_extra) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)4).ToString());    //pierwotna czcionka
            PrinterLibNet.Api.PRNTextOut(0, "RAZEM PRZYCHOD             " + String.Format("{0:0.00}", earn_summary) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)3).ToString());    //mniejsza czcionka
            PrinterLibNet.Api.PRNTextOut(0, "w tym: \n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)4).ToString());    //pierwotna czcionka
            PrinterLibNet.Api.PRNTextOut(0, "PTU A (23%)                " + String.Format("{0:0.00}", (ticket_cash_bike*23)/123) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "PTU B (8%)                 " + String.Format("{0:0.00}", ((ticket_cash_people+ticket_cash_extra)*8)/108) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "ROZCHODY                   [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "Przejazd osob              " + String.Format("{0:0.00}", cancel_ticket_cash_people) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Przewoz roweru             " + String.Format("{0:0.00}", cancel_ticket_cash_bike) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)3).ToString());    //mniejsza czcionka
            PrinterLibNet.Api.PRNTextOut(0, "Opl. za wyd. bil. z PTU 8%          " + String.Format("{0:0.00}", cancel_ticket_cash_extra) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Platnosci bezgotowkowe              " + String.Format("{0:0.00}", ticket_cashless_pay) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Gotowka do kasy kond                " + String.Format("{0:0.00}", (earn_summary-return_summary-ticket_cashless_pay)) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)4).ToString()+"\n");    //pierwotna czcionka
            PrinterLibNet.Api.PRNTextOut(0, "RAZEM ROZCHOD              " + String.Format("{0:0.00}", summing_return) + "\n");

            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)3).ToString());    //mniejsza czcionka
            PrinterLibNet.Api.PRNTextOut(0, "w tym: \n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "F" + ((char)4).ToString());    //pierwotna czcionka
            PrinterLibNet.Api.PRNTextOut(0, "PTU A (23%)                " + String.Format("{0:0.00}", ((cancel_ticket_cash_bike*23)/123)) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "PTU B (8%)                 " + String.Format("{0:0.00}", ((cancel_ticket_cash_extra+cancel_ticket_cash_people)*8)/108) + "\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "Bilety wydane              " + ticket_counter.ToString() + " szt.\n");
            PrinterLibNet.Api.PRNTextOut(0, "od numeru                  " + ticket_1 + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "do numeru                  " + ticket_2 + "\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "Zdaje nr sluzbowy:  K" + curr_user + "\n\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "Przyjmuje:        \n\n\n\n\n");

            //print out carriage return
            nRet = PrinterLibNet.Api.PRNTextOut(1, strCR);

            //close printer driver
            nRet = PrinterLibNet.Api.PRNClose();
            ShowErrorMessage("PRNClose", nRet);
        }
        private void rebuild_ticket_panel() // undo changes at panel to print tickets 
        {
            this.label34.Visible = true;
            this.label35.Visible = true;
            this.label36.Visible = true;
            this.label37.Visible = true;
            this.label38.Visible = true;
            this.label39.Visible = true;
            this.label40.Visible = true;
            this.label41.Visible = true;
            this.paper_ticket_single_RODZ.Visible = true;
            this.paper_ticket_single_DATA.Visible = true;
            this.label42.Visible = false;
            this.label45.Visible = true;
            this.label62.Visible = false;
            this.label46.Visible = true;
            this.label63.Visible = false;

            this.paper_ticket_single_RODZ.Text = "";
            this.paper_ticket_single_TYPE.Text = "";
            this.paper_ticket_single_DATA.Text = "";
            this.paper_ticket_single_START.Text = "";
            this.paper_ticket_single_DESTINATION.Text = "";
            this.label38.Text = "RODZ:";
            this.label32.Text = "wa¿ny w dniu:";
            this.label42.Text = "........................................................";
            this.label52.Text = "w tym PTU 8% PLN";
            this.paper_ticket_single_ULG.Text = "";
            this.paper_ticket_single_ZNIZKA.Text = "";
            this.paper_ticket_single_NORM.Text = "";
            this.paper_ticket_single_POC.Text = "";
            this.panel_cash_get_money.Text = "";
            this.label60.Text = "";
        }
        public void clear_var_after_close()
        {
            ticket_cash_people = 0;
            ticket_cash_bike = 0;
            ticket_cash_extra = 0;
            cancel_ticket_cash_people = 0;
            cancel_ticket_cash_bike = 0;
            cancel_ticket_cash_extra = 0;
            ticket_cashless_pay = 0; // podany przez usera
            earn_summary = 0;
            return_summary = 0;
            ticket_first = 2147483646;
            ticket_last = 0;
            canceled_ticket_number = "";
            cancel_value = 0;
            canceled_ticket_info_line = "";
            canceled_tickets.Clear();
            normal_ticket_prize = 0;
            family_ticket_prize = 0;
            this.summing_cash_input.Text = "";


        }
        public void wrong_date()
        {
            Thread.Sleep(5000);
            MessageBox.Show("Data terminala musi byæ ustawiona na poprawn¹!", "B³êdny czas!");
        }       // one time thread

        // file operations
        public string vars_ToCSV(char CSV_delimiter, params string[] args)  //all vars gets as string type
        {
            string main_string = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                {
                    main_string += args[i];
                }
                else
                {
                    main_string += (CSV_delimiter + args[i]);
                }
            }
            return main_string;
        }
        public bool saveToFile(string file, string content, bool recreate)
        {
            try
            {
                if (recreate)
                {
                    FileStream fm = new FileStream(file, FileMode.Create, FileAccess.Write);
                    StreamWriter f = new StreamWriter(fm);
                    f.BaseStream.Seek(0, SeekOrigin.End);
                    f.WriteLine("{0}", content);
                    f.Close();
                }
                else
                {
                    FileStream fm = new FileStream(file, FileMode.Append, FileAccess.Write);
                    StreamWriter f = new StreamWriter(fm);
                    f.BaseStream.Seek(0, SeekOrigin.End);
                    f.WriteLine("{0}", content);
                    f.Close();
                }
                
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }
        public bool readVarfromCSV(string file, char CSV_delimiter) // using only at logging
        {
            // TODO write new  without var file // remember to change file name at beginning
            
            try // first version

            {
                string line;
                FileStream fm = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    counter_tickets = int.Parse(words[0]);
                    counter_series = int.Parse(words[1]);
                    bool_open_cash = bool.Parse(words[2]);
                }
                f.Close();
                return true;
            }
            catch (Exception)
            {
                //MessageBox.Show("Plik ze zmiennymi nie istnieje!\n\n"+e, file);
                return false;
            }
             /*
            try // tickets series & series
            {
                string line;
                FileStream fm = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    counter_tickets = int.Parse(words[3]);
                    counter_series = int.Parse(words[2]);
                }
                counter_tickets += 1;
                f.Close();
            }
            catch (Exception)
            {
                //MessageBox.Show("Plik ze zmiennymi nie istnieje!\n\n"+e, file);
                return false;
            }
            try // read isopen series (if closed series + 1)
            {
                string line;
                FileStream fm = new FileStream(logs_file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    if(words[3] == "close"){
                        bool_open_cash = false;
                    }
                    else if (words[3] == "open")
                    {
                        bool_open_cash = true;
                    }
                }
                if (!bool_open_cash)
                {
                    counter_series += 1;
                }
                counter_tickets += 1;
                f.Close();
            }
            catch (Exception)
            {
                //MessageBox.Show("Plik ze zmiennymi nie istnieje!\n\n"+e, file);
                return false;
            }
            return true;
            */
        }
        public bool look_for_tickets_in_series(string month_filename, char CSV_delimiter, int curr_session)
        {
            bool return_status = true;
            try
            {
                string line;
                FileStream fm = new FileStream(month_filename, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);

                    if (int.Parse(words[2]) == curr_session)
                    {
                        return_status = true;
                        break;
                    }
                    else
                    {
                        return_status = false;
                    }
                }
                f.Close();
                return return_status;
            }
            catch (Exception e)
            {
                MessageBox.Show("Plik ze zmiennymi nie istnieje!\n\n"+e, month_filename);
                return return_status;
            }
        }
        public string get_name_last_series_to_close_month(string tm_series, char delimiter) //using only at closing month if app is freshy open without a logging
        {
            try
            {
                string line;
                DateTime last_date = DateTime.Now;
                string filename;
                FileStream fm = new FileStream(tm_series, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(delimiter);
                    last_date = DateTime.Parse(words[0]);
                }
                f.Close();
                filename = last_date.ToString("yyyy-MM")+".csv";
                return filename;
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d przy pobieraniu zmiennych\n"+e);
                return "";
            }
        }
        // pobiera info o anulowanym bilecie, w refach data i cena,na return linia do zapisu do plikow
        public string ticket_cancel_read_from_series(string file, char CSV_delimiter, int ticket_nr, ref DateTime ticket_data, ref double ticket_prize)    // needed for cancelling
        {
            try
            {
                string line;

                FileStream fm = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);

                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    if (words[3] == ticket_nr.ToString())
                    {
                        //MessageBox.Show(line);    //all info about ticket to return
                        ticket_data = DateTime.Parse(words[1]);
                        ticket_prize = double.Parse(words[19]);
                        break;
                    }
                }
                f.Close();
                return line;
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d odczytywania danych z plików na karcie!\n\n" + e.Message, file);
                return "";
            }
        }
        public bool dates_read_from_series(string file, char CSV_delimiter, int series, ref string data_series_start, ref string data_series_stop)    // series_logs
        {
            try
            {
                string line;
                if (!File.Exists(file))
                {
                    MessageBox.Show("Brak plików o zmianie!", file);
                    return false;
                }
                FileStream fm = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);

                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    if (words[1] == series.ToString() && words[3] == "open")
                    {
                        data_series_start = DateTime.Parse(words[0]).ToString("yyyy-MM-dd HH:mm");
                    }
                    else if (words[1] == series.ToString() && words[3] == "close")
                    {
                        data_series_stop = DateTime.Parse(words[0]).ToString("yyyy-MM-dd HH:mm");
                    }
                }

                if (data_series_start != "" && data_series_stop != "")
                {
                    f.Close();
                    return true;
                }
                else
                {
                    f.Close();
                    return false;
                }
                
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d odczytywania danych z plików na karcie!\n\n" + e.Message, file);
                return false;
            }
        }
        public bool gen_selling_from_sold(string file, char CSV_delimiter, ref double cash_people, ref double cash_bike, ref double cash_additional)
        {
            try
            {
                string line;
                FileStream fm = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);

                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    //Console.WriteLine(line);

                    double cash_pln;
                    double cash_extra;

                    if (words[19] == "0.00")
                    {
                        cash_pln = 0;
                    }
                    else
                    {
                        cash_pln = double.Parse(words[19]);
                    }
                    if (words[8] == "0.00")
                    {
                        cash_extra = 0;
                    }
                    else
                    {
                        cash_extra = double.Parse(words[8]);
                    }
                    //MessageBox.Show(cash_pln + " " + cash_extra);

                    if (words[12] == "True")    //bike
                    {
                        if (words[7] == "True") //if extra money
                        {

                            cash_bike = cash_bike + cash_pln - cash_extra;
                        }
                        else //else
                        {
                            cash_bike = cash_bike + cash_pln;
                        }
                    }
                    else // any other option is traveling people
                    {
                        if (words[7] == "True") //if extra money
                        {
                            cash_people = cash_people + cash_pln - cash_extra;
                        }
                        else //else
                        {
                            cash_people = cash_people + cash_pln;
                        }
                    }
                    if (words[7] == "True")
                    {
                        cash_additional = cash_additional + cash_extra;
                    }
                }

                f.Close();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d odczytywania danych z plików na karcie!\n\n" + e.Message, file);
                return false;
            }
        }
        public void get_ticket_numbers(string file, char CSV_delimiter, ref int first, ref int last)    // using to cancel ticket
        {
            try
            {
                string line;

                first = 2147483646;
                last = 0;

                FileStream fm = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);

                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(CSV_delimiter);
                    if (int.Parse(words[3]) < first)
                    {
                        first = int.Parse(words[3]);
                    }
                    if (int.Parse(words[3]) > last)
                    {
                        last = int.Parse(words[3]);
                    }
                }
                f.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d odczytywania danych z plików na karcie!\n\n" + e.Message, file);
            }
        }
        public double[] get_billings_at_closing(string filename, char delimiter)
        {
            double normal_counter = 0;
            double normal_cash = 0;
            double relief_33_counter = 0;
            double relief_33_cash = 0;
            double kdr_counter = 0;
            double kdr_cash = 0;
            double relief_37_counter = 0;
            double relief_37_cash = 0;
            double relief_49_counter = 0;
            double relief_49_cash = 0;
            double relief_51_counter = 0;
            double relief_51_cash = 0;
            double relief_78_counter = 0;
            double relief_78_cash = 0;
            double relief_93_counter = 0;
            double relief_93_cash = 0;
            double relief_95_counter = 0;
            double relief_95_cash = 0;
            double relief_100_counter = 0;
            double relief_100_cash = 0;
            double group_guide_counter = 0;
            double group_guide_cash = 0;
            double family_counter = 0;
            double family_cash = 0;
            //monthly
            double normal_month_counter = 0;
            double normal_month_cash = 0;
            double month_33_counter = 0;
            double month_33_cash = 0;
            double month_37_counter = 0;
            double month_37_cash = 0;
            double month_kdr_counter = 0;
            double month_kdr_cash = 0;
            double month_49_counter = 0;
            double month_49_cash = 0;
            double month_51_counter = 0;
            double month_51_cash = 0;
            double month_78_counter = 0;
            double month_78_cash = 0;
            double month_93_counter = 0;
            double month_93_cash = 0;
            double bike_counter = 0;
            double bike_cash = 0;
            double extra_money_counter = 0;
            double extra_money_cash = 0;

            FileStream fm = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader f = new StreamReader(fm);
            string line;
            string ticket_normal;
            string ticket_relief;
            string ticket_family;

            while ((line = f.ReadLine()) != null)
            {
                string[] words = line.Split(delimiter);
                ticket_family = words[18];
                ticket_normal = words[15];
                ticket_relief = words[16];

                if (ticket_normal == "")
                {
                    ticket_normal = "0";
                }
                if (ticket_family == "")
                {
                    ticket_family = "0";
                }

                if (bool.Parse(words[11]) == false && int.Parse(ticket_normal) > 0) //normal ticket
                {
                    normal_counter += double.Parse(ticket_normal);
                    normal_cash += (double.Parse(ticket_normal) * double.Parse(words[22]));
                }

                if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 33)         //33
                {
                    relief_33_counter += double.Parse(ticket_relief);
                    relief_33_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21]))); // how many sold * (normal prize - relief prize) 
                }
                else if (bool.Parse(words[11]) == false && words[6] == "True")          // KDR
                {
                    kdr_counter += double.Parse(ticket_relief);
                    kdr_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 37)   // 37
                {
                    relief_37_counter += double.Parse(ticket_relief);
                    relief_37_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 49)
                {
                    relief_49_counter += double.Parse(ticket_relief);
                    relief_49_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 51)
                {
                    relief_51_counter += double.Parse(ticket_relief);
                    relief_51_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 78)
                {
                    relief_78_counter += double.Parse(ticket_relief);
                    relief_78_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 93)
                {
                    relief_93_counter += double.Parse(ticket_relief);
                    relief_93_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 95)
                {
                    relief_95_counter += double.Parse(ticket_relief);
                    relief_95_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 100)
                {
                    relief_100_counter += double.Parse(ticket_relief);
                    relief_100_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));  
                }

                if (bool.Parse(words[11]) == false && words[13] == "True" && int.Parse(ticket_family) > 0)     // group guide
                {
                    group_guide_counter += double.Parse(ticket_family);
                }
                else if (bool.Parse(words[11]) == false && words[13] == "False" && int.Parse(ticket_family) > 0)     // family
                {
                    family_counter += double.Parse(ticket_family);
                    family_cash += ((double.Parse(ticket_family)/2) * double.Parse(words[23]));
                }
                // monthly 
                if (bool.Parse(words[11]) == true && int.Parse(ticket_normal) > 0) //normal ticket
                {
                    normal_month_counter += double.Parse(ticket_normal);
                    normal_month_cash += (double.Parse(ticket_normal) * double.Parse(words[22]));
                }
                if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 33)         //33
                {
                    month_33_counter += double.Parse(ticket_relief);
                    month_33_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21]))); // (normal prize - relief prize) * how many sold
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 37)         //37
                {
                    month_37_counter += double.Parse(ticket_relief);
                    month_37_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21]))); // (normal prize - relief prize) * how many sold
                }
                else if (bool.Parse(words[11]) == true && words[6] == "True")          // KDR
                {
                    month_kdr_counter += double.Parse(ticket_relief);
                    month_kdr_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 49)
                {
                    month_49_counter += double.Parse(ticket_relief);
                    month_49_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 51)
                {
                    month_51_counter += double.Parse(ticket_relief);
                    month_51_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 78)
                {
                    month_78_counter += double.Parse(ticket_relief);
                    month_78_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 93)
                {
                    month_93_counter += double.Parse(ticket_relief);
                    month_93_cash += (double.Parse(ticket_relief) * (double.Parse(words[22]) - double.Parse(words[21])));
                }
                if (words[12] == "True") // bike
                {
                    bike_counter += double.Parse(ticket_relief);
                    bike_cash += double.Parse(words[22]) * double.Parse(ticket_relief) ; // normal prize * how many sold
                }
                if (words[7] == "True") // extra cash
                {
                    extra_money_counter += 1;
                    extra_money_cash += double.Parse(words[8]);
                }
            }
            f.Close();

            double[] list = new double[44]{
                normal_counter, normal_cash, relief_33_counter, relief_33_cash, kdr_counter, kdr_cash, relief_37_counter, relief_37_cash, relief_49_counter, relief_49_cash, relief_51_counter, relief_51_cash, relief_78_counter, relief_78_cash, relief_93_counter, relief_93_cash, relief_95_counter, relief_95_cash, relief_100_counter, relief_100_cash, group_guide_counter, group_guide_cash, family_counter, family_cash, normal_month_counter, normal_month_cash, month_33_counter, month_33_cash, month_37_counter, month_37_cash, month_kdr_counter, month_kdr_cash, month_49_counter, month_49_cash, month_51_counter, month_51_cash, month_78_counter, month_78_cash, month_93_counter, month_93_cash, bike_counter, bike_cash, extra_money_counter, extra_money_cash
                };
            return list;
        } //using by close_month_of_selling() method
        public double[] get_money_to_refund(string filename, char delimiter)
        {
            double relief_33_counter = 0;
            double relief_33_cash = 0;
            double kdr_counter = 0;
            double kdr_cash = 0;
            double relief_37_counter = 0;
            double relief_37_cash = 0;
            double relief_49_counter = 0;
            double relief_49_cash = 0;
            double relief_51_counter = 0;
            double relief_51_cash = 0;
            double relief_78_counter = 0;
            double relief_78_cash = 0;
            double relief_93_counter = 0;
            double relief_93_cash = 0;
            double relief_95_counter = 0;
            double relief_95_cash = 0;
            double relief_100_counter = 0;
            double relief_100_cash = 0;
            //monthly
            double month_33_counter = 0;
            double month_33_cash = 0;
            double month_37_counter = 0;
            double month_37_cash = 0;
            double month_kdr_counter = 0;
            double month_kdr_cash = 0;
            double month_49_counter = 0;
            double month_49_cash = 0;
            double month_51_counter = 0;
            double month_51_cash = 0;
            double month_78_counter = 0;
            double month_78_cash = 0;
            double month_93_counter = 0;
            double month_93_cash = 0;

            FileStream fm = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader f = new StreamReader(fm);
            string line;
            string ticket_normal;
            string ticket_relief;
            string ticket_family;

            while ((line = f.ReadLine()) != null)
            {
                string[] words = line.Split(delimiter);
                ticket_family = words[18];
                ticket_normal = words[15];
                ticket_relief = words[16];

                if (ticket_normal == "")
                {
                    ticket_normal = "0";
                }
                if (ticket_family == "")
                {
                    ticket_family = "0";
                }

                if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 33)         //33
                {
                    relief_33_counter += double.Parse(ticket_relief);
                    relief_33_cash += double.Parse(ticket_relief) * double.Parse(words[21]); // how many sold * (normal prize - relief prize) 
                }
                else if (bool.Parse(words[11]) == false && words[6] == "True")          // KDR
                {
                    kdr_counter += double.Parse(ticket_relief);
                    kdr_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 37)   // 37
                {
                    relief_37_counter += double.Parse(ticket_relief);
                    relief_37_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 49)
                {
                    relief_49_counter += double.Parse(ticket_relief);
                    relief_49_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 51)
                {
                    relief_51_counter += double.Parse(ticket_relief);
                    relief_51_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 78)
                {
                    relief_78_counter += double.Parse(ticket_relief);
                    relief_78_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 93)
                {
                    relief_93_counter += double.Parse(ticket_relief);
                    relief_93_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 95)
                {
                    relief_95_counter += double.Parse(ticket_relief);
                    relief_95_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == false && int.Parse(words[5]) == 100)
                {
                    relief_100_counter += double.Parse(ticket_relief);
                    relief_100_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }

                // monthly 
                
                if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 33)         //33
                {
                    month_33_counter += double.Parse(ticket_relief);
                    month_33_cash += double.Parse(ticket_relief) * double.Parse(words[21]); // (normal prize - relief prize) * how many sold
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 37)         //37
                {
                    month_37_counter += double.Parse(ticket_relief);
                    month_37_cash += double.Parse(ticket_relief) * double.Parse(words[21]); // (normal prize - relief prize) * how many sold
                }
                else if (bool.Parse(words[11]) == true && words[6] == "True")          // KDR
                {
                    month_kdr_counter += double.Parse(ticket_relief);
                    month_kdr_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 49)
                {
                    month_49_counter += double.Parse(ticket_relief);
                    month_49_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 51)
                {
                    month_51_counter += double.Parse(ticket_relief);
                    month_51_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 78)
                {
                    month_78_counter += double.Parse(ticket_relief);
                    month_78_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
                else if (bool.Parse(words[11]) == true && int.Parse(words[5]) == 93)
                {
                    month_93_counter += double.Parse(ticket_relief);
                    month_93_cash += double.Parse(ticket_relief) * double.Parse(words[21]);
                }
            }
            f.Close();

            double[] list = new double[32]{
                relief_33_counter, relief_33_cash, kdr_counter, kdr_cash, relief_37_counter, relief_37_cash, relief_49_counter, relief_49_cash, relief_51_counter, relief_51_cash, relief_78_counter, relief_78_cash, relief_93_counter, relief_93_cash, relief_95_counter, relief_95_cash, relief_100_counter, relief_100_cash, month_33_counter, month_33_cash, month_37_counter, month_37_cash,month_kdr_counter, month_kdr_cash, month_49_counter, month_49_cash, month_51_counter, month_51_cash, month_78_counter, month_78_cash, month_93_counter, month_93_cash
                };
            return list;
        }
        public bool verify_is_month_closed(string filename_logs, char delimiter)// if false - could open new
        {
            bool is_closed = false;
            DateTime month_closed;
            try
            {
                FileStream fm = new FileStream(filename_logs, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
            
                string line;
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(delimiter);
                    if (words[3] == "open" || words[3] == "close" || words[3] == "renew_month")
                    {
                        is_closed = false;
                    }
                    else if (words[3] == "close_month")
                    {
                        is_closed = true;
                        month_closed = DateTime.Parse(words[0]);
                        if (month_closed.ToString("yyyy-MM") != DateTime.Now.ToString("yyyy-MM"))
                        {
                            is_closed = false;
                        }
                    }
                }
                f.Close();
            }
            catch (FileNotFoundException)
            {
                return false; // if file isnt exists month weren't open
            }
                return is_closed;
        }
        public bool verify_previous_month_closed(string filename_logs, char delimiter) //if true -OK
        {
            bool cash_is_closed = true;
            bool month_is_closed = true;
            DateTime curr_month = DateTime.Now;
            DateTime last_month_open = DateTime.Parse("01/01/2001 00:00:00");
            DateTime last_month_close = DateTime.Parse("01/01/2001 00:00:00");
            string line;
            try
            {
                FileStream fm = new FileStream(filename_logs, FileMode.Open, FileAccess.Read);
                StreamReader f = new StreamReader(fm);
                while ((line = f.ReadLine()) !=  null)
                {
                    string[] words = line.Split(delimiter);

                    if (last_month_open.ToString("yyyy-MM") == "0001-01")   // on init
                    {
                        last_month_open = DateTime.Parse(words[0]);
                    }// if init

                    if ((words[3] == "open" || words[3] == "renew_month") && cash_is_closed)
                    {
                        cash_is_closed = false;
                        month_is_closed = false;
                        last_month_open = DateTime.Parse(words[0]);
                    }
                    else if (words[3] == "close_month" && last_month_open.ToString("yyyy-MM") == (DateTime.Parse(words[0])).ToString("yyyy-MM")) // datetime is set 01.01.0001 00:00
                    {
                        last_month_close = DateTime.Parse(words[0]);
                        month_is_closed = true;
                    }
                }
                f.Close();

                if (last_month_open.ToString("yyyy-MM") == DateTime.Now.ToString("yyyy-MM"))
                {
                    return true;
                }
                return month_is_closed;
            }
            catch (FileNotFoundException)
            {
                return true; // if file isnt exists - not previous month
            }
        }
        public bool close_month_of_selling(char delimiter, string filename_open_cash, string filename_canceled, string tm_logs) // print close month
        {
            string info = "";
            string type = "error";
            int nr_open_cash = 0;
            int nr_close_cash = 0;
            double month_cashless_pay = 0;

            // TODO sprawdzenie regionu - jeœli nie polski - wywalaæ b³¹d!

            DateTime date_open_cash = DateTime.Now;
            DateTime date_close_cash = DateTime.Now;
            // get number of open and close
            string line;
            FileStream fm = new FileStream(filename_open_cash, FileMode.Open, FileAccess.Read);
            StreamReader f = new StreamReader(fm);

            while ((line = f.ReadLine()) != null)
            {
                string[] words = line.Split(delimiter);
                if (nr_open_cash == 0)
                {
                    nr_open_cash = int.Parse(words[2]);
                }
                else
                {
                    nr_close_cash = int.Parse(words[2]);
                }
            }
            f.Close();
            // get datatime of open and close
            try
            {
                fm = new FileStream(tm_logs, FileMode.Open, FileAccess.Read);
                f = new StreamReader(fm);
                while ((line = f.ReadLine()) != null)
                {
                    string[] words = line.Split(delimiter);

                    if (nr_open_cash == int.Parse(words[1]) && words[3] == "open")
                    {
                        date_open_cash = DateTime.Parse(words[0]);
                    }
                    if (nr_close_cash == int.Parse(words[1]) && words[3] == "close")
                    {
                        date_close_cash = DateTime.Parse(words[0]);
                    }
                    // get all cashless money
                    if (int.Parse(words[1]) >= nr_open_cash && int.Parse(words[1]) <= nr_close_cash)
                    {
                        if (words[3] == "close")
                        {
                            month_cashless_pay += double.Parse(words[4]);
                        }
                    }
                }
                
                f.Close();
            }
            catch (Exception)
            {
                if (nr_open_cash == 0 || nr_close_cash == 0)
                {
                    type = "error";
                    info = "B³¹d w odczytywaniu numeru zmiany";
                    DialogResult dialogresult = MessageBox.Show(info);
                    string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "zamkniecie miesiaca" + info);
                    saveToFile(CSV_filename_logs, tempstring, false);
                    return false;
                }
            }
            
            double[] list_money ={ };
            double[] list_money_lost = new double[50];
            try
            {
                list_money = get_billings_at_closing(filename_open_cash, CSV_delimiter);
                if (File.Exists(filename_canceled))
                {
                    list_money_lost = get_billings_at_closing(filename_canceled, CSV_delimiter);
                }
                else
                {
                    for (int i = 0; i < 50; i++)    // if there wasnt canceled tickets
                        list_money_lost[i] = 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d:\n"+e, "get_billings");
                info = "B³¹d w odczytywaniu pieniedzy";
                //DialogResult dialogresult = MessageBox.Show(info);
                string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "zamkniecie miesiaca" + info);
                saveToFile(CSV_filename_logs, tempstring, false);
                return false;
            }


            string strFont = ((char)27).ToString() + "F" + ((char)4).ToString();
            string str_3_font = ((char)27).ToString() + "F" + ((char)3).ToString();
            int nRet;
            int dwPaperType = new int();
            int dwDepth = new int();
            int dwSpeed = new int();
            int dwAutoloading = new int();
            int dwLoadingValue = new int();
            int dwPreHeat = new int();
            int dwPrintContinuation = new int();

            nRet = PrinterLibNet.Api.PRNOpen();
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNOpen", nRet);
                info = "B³¹d na linii TM - drukarka";
                DialogResult dialogresult = MessageBox.Show(info);
                string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "zamkniecie miesiaca" + info);
                saveToFile(CSV_filename_logs, tempstring, false);
                return false;
            }
            //check printer state
            nRet = PrinterLibNet.Api.PRNGetPrinterProperty(ref dwPaperType, ref dwDepth, ref dwSpeed, ref dwAutoloading, ref dwLoadingValue, ref dwPreHeat, ref dwPrintContinuation);
            if (nRet != PrinterLibNet.Def.PRN_NORMAL)
            {
                ShowErrorMessage("PRNGetPrinterProperty", nRet);
                info = "B³¹d na linii TM - drukarka - pobieranie zmiennych";
                DialogResult dialogresult = MessageBox.Show(info);
                string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "zamkniecie miesiaca" + info);
                saveToFile(CSV_filename_logs, tempstring, false);
                return false;
            }
            double income = 0;
            double income_PTU_B = 0;
            double expenditure = 0;
            double expenditure_PTU_B = 0;
            double summary = 0;
            
            for (int i = 1; i < 44; i=i+2)
                income += list_money[i];
            for (int i = 1; i < 44; i=i+2)
                expenditure += list_money_lost[i];
            for (int i = 1; i < 44; i= i+2)
                income_PTU_B += list_money[i];
            for (int i = 1; i < 44; i = i + 2)
                expenditure_PTU_B += list_money_lost[i];
            summary = income - expenditure;
            income_PTU_B = ((income_PTU_B - list_money[41]) * 8)/108; // all without a bike
            expenditure_PTU_B = ((expenditure_PTU_B - list_money_lost[41]) * 8)/108;

            PrinterLibNet.Api.PRNTextOut(1, strFont); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "Towarzystwo Przyjaciol \n            Kolejki Sredzkiej BANA\nul. Dworcowa 3\n63-000 Sroda Wielkopolska\nNIP 786-169-82-53\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "O" + ((char)0).ToString()); // BOLD
            PrinterLibNet.Api.PRNTextOut(0, "Zamkniecie miesiaca:     " + date_close_cash.ToString("MM/yyyy") + "\n");
            PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "O" + ((char)1).ToString()); // BOLD
            PrinterLibNet.Api.PRNTextOut(0, "Terminal mobilny:        " + TM_SERIES + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Otwarcie zmiany " + nr_open_cash  + " " + date_open_cash.ToString("yyyy-MM-dd HH:mm") + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Zamkniecie      " + nr_close_cash + " " + date_close_cash.ToString("yyyy-MM-dd HH:mm") + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Wydrukowano        " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "PRZYCHODY\nBilety jednorazowe\n");
            PrinterLibNet.Api.PRNTextOut(0, "w tym:       [szt.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "normalne      " + list_money[0] + "          " + String.Format("{0:0.00}", list_money[1]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "z ulga:\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + list_money[2] + "          " + String.Format("{0:0.00}", list_money[3]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR37%        " + list_money[4] + "          " + String.Format("{0:0.00}", list_money[5]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + list_money[6] + "          " + String.Format("{0:0.00}", list_money[7]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + list_money[8] + "          " + String.Format("{0:0.00}", list_money[9]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + list_money[10] + "          " + String.Format("{0:0.00}", list_money[11]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + list_money[12] + "          " + String.Format("{0:0.00}", list_money[13]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + list_money[14] + "          " + String.Format("{0:0.00}", list_money[15]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   95%        " + list_money[16] + "          " + String.Format("{0:0.00}", list_money[17]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "  100%        " + list_money[18] + "          " + String.Format("{0:0.00}", list_money[19]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "przewodnik    " + list_money[20] + "          " + String.Format("{0:0.00}", list_money[21]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "rodzinne      " + list_money[22] + "          " + String.Format("{0:0.00}", list_money[23]) + "\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "Bilety miesieczne\n");
            PrinterLibNet.Api.PRNTextOut(0, "w tym:       [szt.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "normalne      " + list_money[24] + "          " + String.Format("{0:0.00}", list_money[25]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "z ulga:\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + list_money[26] + "          " + String.Format("{0:0.00}", list_money[27]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + list_money[28] + "          " + String.Format("{0:0.00}", list_money[29]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR49%        " + list_money[30] + "          " + String.Format("{0:0.00}", list_money[31]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + list_money[32] + "          " + String.Format("{0:0.00}", list_money[33]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + list_money[34] + "          " + String.Format("{0:0.00}", list_money[35]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + list_money[36] + "          " + String.Format("{0:0.00}", list_money[37]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + list_money[38] + "          " + String.Format("{0:0.00}", list_money[39]) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "przewoz roweru     " + list_money[40] + "             " + String.Format("{0:0.00}", list_money[41]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "opl. za wyd bil z ptu 8%:  " + list_money[42] + "     " + String.Format("{0:0.00}", list_money[43]) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, strFont) ;
            PrinterLibNet.Api.PRNTextOut(0, "RAZEM PRZYCHOD           " + String.Format("{0:0.00}", income) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "w tym:\n");
            PrinterLibNet.Api.PRNTextOut(1, strFont);
            PrinterLibNet.Api.PRNTextOut(0, "PTU A (23%)             " + String.Format("{0:0.00}", (list_money[41] * 23)/123) + "\n");  // 23% only with bike
            PrinterLibNet.Api.PRNTextOut(0, "PTU B (8%)              " + String.Format("{0:0.00}", income_PTU_B) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "ROZCHODY\nBilety jednorazowe\n");
            PrinterLibNet.Api.PRNTextOut(0, "w tym:       [szt.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "normalne      " + list_money_lost[0] + "          " + String.Format("{0:0.00}", list_money_lost[1]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "z ulga:\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + list_money_lost[2] + "          " + String.Format("{0:0.00}", list_money_lost[3]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR37%        " + list_money_lost[4] + "          " + String.Format("{0:0.00}", list_money_lost[5]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + list_money_lost[6] + "          " + String.Format("{0:0.00}", list_money_lost[7]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + list_money_lost[8] + "          " + String.Format("{0:0.00}", list_money_lost[9]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + list_money_lost[10] + "          " + String.Format("{0:0.00}", list_money_lost[11]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + list_money_lost[12] + "          " + String.Format("{0:0.00}", list_money_lost[13]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + list_money_lost[14] + "          " + String.Format("{0:0.00}", list_money_lost[15]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   95%        " + list_money_lost[16] + "          " + String.Format("{0:0.00}", list_money_lost[17]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "  100%        " + list_money_lost[18] + "          " + String.Format("{0:0.00}", list_money_lost[19]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "przewodnik    " + list_money_lost[20] + "          " + String.Format("{0:0.00}", list_money_lost[21]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "rodzinne      " + list_money_lost[22] + "          " + String.Format("{0:0.00}", list_money_lost[23]) + "\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "Bilety miesieczne\n");
            PrinterLibNet.Api.PRNTextOut(0, "w tym:       [szt.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "normalne      " + list_money_lost[24] + "          " + String.Format("{0:0.00}", list_money_lost[25]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "z ulga:\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + list_money_lost[26] + "          " + String.Format("{0:0.00}", list_money_lost[27]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + list_money_lost[28] + "          " + String.Format("{0:0.00}", list_money_lost[29]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR49%        " + list_money_lost[30] + "          " + String.Format("{0:0.00}", list_money_lost[31]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + list_money_lost[32] + "          " + String.Format("{0:0.00}", list_money_lost[33]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + list_money_lost[34] + "          " + String.Format("{0:0.00}", list_money_lost[35]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + list_money_lost[36] + "          " + String.Format("{0:0.00}", list_money_lost[37]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + list_money_lost[38] + "          " + String.Format("{0:0.00}", list_money_lost[39]) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "przewoz roweru     " + list_money_lost[40] + "             " + String.Format("{0:0.00}", list_money_lost[41]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "opl. za wyd bil z ptu 8%:  " + list_money_lost[42] + "     " + String.Format("{0:0.00}", list_money_lost[43]) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, strFont);
            PrinterLibNet.Api.PRNTextOut(0, "RAZEM ROZCHOD           " + String.Format("{0:0.00}", expenditure) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "w tym:\n");
            PrinterLibNet.Api.PRNTextOut(1, strFont);
            PrinterLibNet.Api.PRNTextOut(0, "PTU A (23%)             " + String.Format("{0:0.00}", (list_money_lost[41] * 23)/123) + "\n");  // 23% only with bike
            PrinterLibNet.Api.PRNTextOut(0, "PTU B (8%)              " + String.Format("{0:0.00}", expenditure_PTU_B) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "SALDO\nBilety jednorazowe\n");
            PrinterLibNet.Api.PRNTextOut(0, "w tym:       [szt.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "normalne      " + (list_money[0] - list_money_lost[0]) + "          " + String.Format("{0:0.00}", list_money[1] - list_money_lost[1]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "z ulga:\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + (list_money[2] - list_money_lost[2]) + "          " + String.Format("{0:0.00}", list_money[3]-list_money_lost[3]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR37%        " + (list_money[4] - list_money_lost[4]) + "          " + String.Format("{0:0.00}", list_money[5]-list_money_lost[5]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + (list_money[6] - list_money_lost[6])+ "          " + String.Format("{0:0.00}", list_money[7]-list_money_lost[7]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + (list_money[8] - list_money_lost[8])+ "          " + String.Format("{0:0.00}", list_money[9]-list_money_lost[9]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + (list_money[10] - list_money_lost[10])+ "          " + String.Format("{0:0.00}", list_money[11]-list_money_lost[11]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + (list_money[12] - list_money_lost[12])+ "          " + String.Format("{0:0.00}", list_money[13]-list_money_lost[13]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + (list_money[14] - list_money_lost[14])+ "          " + String.Format("{0:0.00}", list_money[15]-list_money_lost[15]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   95%        " + (list_money[16] - list_money_lost[16])+ "          " + String.Format("{0:0.00}", list_money[17]-list_money_lost[17]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "  100%        " + (list_money[18] - list_money_lost[18])+ "          " + String.Format("{0:0.00}", list_money[19]-list_money_lost[19]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "przewodnik    " + (list_money[20] - list_money_lost[20])+ "          " + String.Format("{0:0.00}", list_money[21]-list_money_lost[21]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "rodzinne      " + (list_money[22] - list_money_lost[22])+ "          " + String.Format("{0:0.00}", list_money[23]-list_money_lost[23]) + "\n\n");
            PrinterLibNet.Api.PRNTextOut(0, "Bilety miesieczne\n");
            PrinterLibNet.Api.PRNTextOut(0, "w tym:       [szt.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "normalne      " + (list_money[24]-list_money_lost[24]) + "          " + String.Format("{0:0.00}", list_money[25]-list_money_lost[25]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "z ulga:\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + (list_money[26]-list_money_lost[26]) + "          " + String.Format("{0:0.00}", list_money[27]-list_money_lost[27]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + (list_money[28]-list_money_lost[28]) + "          " + String.Format("{0:0.00}", list_money[29]-list_money_lost[29]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR49%        " + (list_money[30]-list_money_lost[30]) + "          " + String.Format("{0:0.00}", list_money[31]-list_money_lost[31]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + (list_money[32]-list_money_lost[32]) + "          " + String.Format("{0:0.00}", list_money[33]-list_money_lost[33]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + (list_money[34]-list_money_lost[34]) + "          " + String.Format("{0:0.00}", list_money[35]-list_money_lost[35]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + (list_money[36]-list_money_lost[36]) + "          " + String.Format("{0:0.00}", list_money[37]-list_money_lost[37]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + (list_money[38]-list_money_lost[38]) + "          " + String.Format("{0:0.00}", list_money[39]-list_money_lost[39]) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "przewoz roweru     " + (list_money[40]-list_money_lost[40]) + "             " + String.Format("{0:0.00}", list_money[41]-list_money_lost[41]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "opl. za wyd bil z ptu 8%:  " + (list_money[42]-list_money_lost[42]) + "     " + String.Format("{0:0.00}", list_money[43]-list_money_lost[43]) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, strFont);
            PrinterLibNet.Api.PRNTextOut(0, "RAZEM SALDO           " + String.Format("{0:0.00}", income - expenditure) + "\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "w tym:\n");
            PrinterLibNet.Api.PRNTextOut(1, strFont);
            PrinterLibNet.Api.PRNTextOut(0, "PTU A (23%)             " + String.Format("{0:0.00}", (list_money[41] * 23) / 123 - (list_money_lost[41] * 23) / 123) + "\n");  // 23% only with bike
            PrinterLibNet.Api.PRNTextOut(0, "PTU B (8%)              " + String.Format("{0:0.00}", income_PTU_B - expenditure_PTU_B) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "Kasjer rachunkozdawca:\n\n\n\n\n");
            PrinterLibNet.Api.PRNTextOut(1, str_3_font); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "               Podpis uzytkownika\n");
            PrinterLibNet.Api.PRNTextOut(0, "___________________________________________\n\n");
            //print out carriage return
            nRet = PrinterLibNet.Api.PRNTextOut(1, strCR);

            //end of first raport
            MessageBox.Show("Zaraz nast¹pi wydruk dotacji.", "Oderwij taœmê");

            double refund_single_ticket_counter = 0;
            double refund_single_ticket_cash = 0;
            double refund_monthly_ticket_counter = 0;
            double refund_monthly_ticket_cash = 0;
            double[] list_money_refund ={ };
            double[] list_money_refund_lost = new double[50];
            
            try
            {
                list_money_refund = get_money_to_refund(filename_open_cash, CSV_delimiter);
                if (File.Exists(filename_canceled))
                {
                    list_money_refund_lost = get_money_to_refund(filename_canceled, CSV_delimiter);
                }
                else
                {
                    for (int i = 0; i < 50; i++)    // if there wasnt canceled tickets
                        list_money_refund_lost[i] = 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("B³¹d:\n"+e, "get_billings");
                return false;
            }
            for (int i = 0; i < 18; i = i + 2)
            {
                refund_single_ticket_counter = refund_single_ticket_counter + list_money_refund[i] - list_money_refund_lost[i];
                refund_single_ticket_cash = refund_single_ticket_cash + list_money_refund[i + 1] - list_money_refund_lost[i + 1];
            }
            for (int i = 18; i < 32; i = i + 2)
            {
                refund_monthly_ticket_counter = refund_monthly_ticket_counter + list_money_refund[i] - list_money_refund_lost[i];
                refund_monthly_ticket_cash = refund_monthly_ticket_cash + list_money_refund[i + 1] - list_money_refund_lost[i + 1];
            }
            

            PrinterLibNet.Api.PRNTextOut(1, strFont); //set font size
            PrinterLibNet.Api.PRNTextOut(0, "Towarzystwo Przyjaciol \n            Kolejki Sredzkiej BANA\nul. Dworcowa 3\n63-000 Sroda Wielkopolska\nNIP 786-169-82-53\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            //PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "O" + ((char)0).ToString()); // BOLD
            PrinterLibNet.Api.PRNTextOut(0, "Terminal mobilny:    " + TM_SERIES + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "Za miesiac:          " + date_close_cash.ToString("MM/yyyy") + "\n");
            //PrinterLibNet.Api.PRNTextOut(1, ((char)27).ToString() + "O" + ((char)1).ToString()); // BOLD

            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "    WARTOSC UTRACONYCH WPLYWOW\n");
            PrinterLibNet.Api.PRNTextOut(0, "      TARYFOWYCH Z TYTULU ULG\n");
            PrinterLibNet.Api.PRNTextOut(0, "PRZEJAZDOWYCH SKORYGOWANA O ZWROTY\n");
            PrinterLibNet.Api.PRNTextOut(0, "         NIEWYKORZYSTANYCH\n");
            PrinterLibNet.Api.PRNTextOut(0, "                I\n");
            PrinterLibNet.Api.PRNTextOut(0, "        ANULOWANYCH BILETOW\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "Bilety jednorazowe\n");
            PrinterLibNet.Api.PRNTextOut(0, "Ulga:       [L. os.]     [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + (list_money_refund[0] - list_money_refund_lost[0]) + "         " + String.Format("{0:0.00}", list_money_refund[1] - list_money_refund_lost[1]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR37%        " + (list_money_refund[2] - list_money_refund_lost[2]) + "         " + String.Format("{0:0.00}", list_money_refund[3] - list_money_refund_lost[3]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + (list_money_refund[4] - list_money_refund_lost[4]) + "         " + String.Format("{0:0.00}", list_money_refund[5] - list_money_refund_lost[5]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + (list_money_refund[6] - list_money_refund_lost[6]) + "         " + String.Format("{0:0.00}", list_money_refund[7] - list_money_refund_lost[7]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + (list_money_refund[8] - list_money_refund_lost[8]) + "         " + String.Format("{0:0.00}", list_money_refund[9] - list_money_refund_lost[9]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + (list_money_refund[10] - list_money_refund_lost[10]) + "         " + String.Format("{0:0.00}", list_money_refund[11] - list_money_refund_lost[11]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + (list_money_refund[12] - list_money_refund_lost[12]) + "         " + String.Format("{0:0.00}", list_money_refund[13] - list_money_refund_lost[13]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   95%        " + (list_money_refund[14] - list_money_refund_lost[14]) + "         " + String.Format("{0:0.00}", list_money_refund[15] - list_money_refund_lost[15]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "  100%        " + (list_money_refund[16] - list_money_refund_lost[16]) + "         " + String.Format("{0:0.00}", list_money_refund[17] - list_money_refund_lost[17]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, " RAZEM:       " + refund_single_ticket_counter + "         " + String.Format("{0:0.00}", refund_single_ticket_cash) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "____________________________________");
            PrinterLibNet.Api.PRNTextOut(0, "Bilety miesieczne\n");
            PrinterLibNet.Api.PRNTextOut(0, "Ulga:       [L. os.]      [PLN]\n");
            PrinterLibNet.Api.PRNTextOut(0, "   33%        " + (list_money_refund[18] - list_money_refund_lost[18]) + "         " + String.Format("{0:0.00}", list_money_refund[19] - list_money_refund_lost[19]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   37%        " + (list_money_refund[20] - list_money_refund_lost[20]) + "         " + String.Format("{0:0.00}", list_money_refund[21] - list_money_refund_lost[21]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "KDR49%        " + (list_money_refund[22] - list_money_refund_lost[22]) + "         " + String.Format("{0:0.00}", list_money_refund[23] - list_money_refund_lost[23]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   49%        " + (list_money_refund[24] - list_money_refund_lost[24]) + "         " + String.Format("{0:0.00}", list_money_refund[25] - list_money_refund_lost[25]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   51%        " + (list_money_refund[26] - list_money_refund_lost[26]) + "         " + String.Format("{0:0.00}", list_money_refund[27] - list_money_refund_lost[27]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   78%        " + (list_money_refund[28] - list_money_refund_lost[28]) + "         " + String.Format("{0:0.00}", list_money_refund[29] - list_money_refund_lost[29]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, "   93%        " + (list_money_refund[30] - list_money_refund_lost[30]) + "         " + String.Format("{0:0.00}", list_money_refund[31] - list_money_refund_lost[31]) + "\n");
            PrinterLibNet.Api.PRNTextOut(0, " RAZEM:       " + refund_monthly_ticket_counter + "         " + String.Format("{0:0.00}", refund_monthly_ticket_cash) + "\n\n\n");

            //close printer driver
            nRet = PrinterLibNet.Api.PRNClose();
            ShowErrorMessage("PRNClose", nRet);

            // save info about closing month
            string temp_string_close = vars_ToCSV(delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter_series.ToString(), curr_user, "close_month");
            if (!saveToFile(CSV_filename_series_logs, temp_string_close, false))
            {
                MessageBox.Show("Nie uda³o siê zapisaæ informacji o zamkniêciu zmiany.\nZrestartuj terminal i spróbuj ponownie zamkn¹æ miesi¹c.","B³¹d");
            }
            type = "info";
            info = "zamknieto";
            string tempstringCSV = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "zamkniecie miesiaca" + info);
            saveToFile(CSV_filename_logs, tempstringCSV, false);

            return true;
        }

        public Onufry()
        {
            InitializeComponent();
            user_name.Focus();
            stations_start.DataSource = stations_1;         // start stations to single
            comboBox2.DataSource = stations_1;              //                   group tickets
            stations_destination.DataSource = stations_2;   //  destination stations in single
            comboBox1.DataSource = stations_2;              //                       and group tickets
            GoFullscreen();
            Calib.SystemLibNet.Api.SysDisableCardDetect(0); // disable detecting PCIExpress Card

            if (!readVarfromCSV(CSV_filename_counters, CSV_delimiter))   // get last saved counters of ticket & cash series
            {
                MessageBox.Show("Wyst¹pi³ problem z dostêpem do plików. Aplikacja nie bêdzie dzia³a³a poprawnie!", "B³¹d I/O");
            }

            if (debug)
            {
                Calib.SystemLibNet.Api.SysAudioOff();
                Calib.SystemLibNet.Api.SysKeyBackLightOn();
            }
            else // for production
            {
                get_number_TM();  //validate TM
                Calib.SystemLibNet.Api.SysAudioOff();
                Calib.SystemLibNet.Api.SysKeyBackLightOn();
            }

            // TODO
            //Thread thr = new Thread(watch_time); 
            //thr.IsBackground = true;    // if main process is closed thread will be killed
            //thr.Start();
            ThreadStart childref = new ThreadStart(Led_loop);
            Thread childThread = new Thread(childref);
            childThread.Start();

        }

        private void watch_time()
        {
            while (true)
            {
                Thread.Sleep(60000); // refresh datetime in every minute
                this.date_updater();
            }
        }    // works in loop at minute frequency
        private void date_updater()
        {
            this.dateTimePicker1.Value = DateTime.Now;
        }

        private void battery_status()
        {
            //TODO
        }


        private void get_number_TM()
        {
            Char[] pdwDevID = new char[14];
            Calib.SystemLibNet.Api.SysGetDeviceIDCode(pdwDevID);
            string name = new string(pdwDevID);

            for (int i = 0; i < TM_devices.Length; i++)
            {
                string[] words = TM_devices[i].Split(':');
                string TM_ID = words[0];
                string TM_NR = words[1];

                if (name == TM_ID)
                {
                    TM_SERIES = TM_NR;
                    return;
                }
            }
            MessageBox.Show("Numery seryjne TM nie zgadzaj¹ siê", "B³¹d");
            this.button1.Enabled = false;
        }   
        private void panel1_GotFocus(object sender, EventArgs e)
        {
            this.user_name.Focus();
        }
        private void enter_pressed(object sender, KeyPressEventArgs e)  // get react for enter pressed
        {
            if (e.KeyChar == (char)13)
                this.user_passwd.Focus();
            this.dateTimePicker1.Value = DateTime.Now;
        }
        private void enter_on_passwd(object sender, KeyPressEventArgs e)    //same here
        {
            if (e.KeyChar == (char)13 && this.button1.Enabled)
                this.button1_Click(sender, e);
        } 
        private void button1_Click(object sender, EventArgs e)  // login_button clicked - validate
        {
            bool logged = false;
            string name = this.user_name.Text;
            string passwd = this.user_passwd.Text;
            string temp_string = vars_ToCSV(CSV_delimiter, counter_tickets.ToString(), counter_series.ToString(), bool_open_cash.ToString());
            if (!saveToFile(CSV_filename_counters, temp_string, true))
            {
                MessageBox.Show("B³¹d zapisu otwarcia zmiany!", "B³¹d I/O");
            }
            for (int i = 0; i < users_list.Length; i++)
            {
                if (this.dateTimePicker1.Value < DateTime.Parse("2022-01-01") && i != 0)  // look for date at start and disable logging
                {
                    MessageBox.Show("Data terminala musi byæ ponownie ustawiona na poprawn¹!\nZaloguj siê jako admin i zmieñ czas!", "B³êdny czas!");
                    return;
                }
                string[] words  = users_list[i].Split(':');
                string u_name = words[0];
                string u_passwd = words[1];
                if (name == u_name && passwd == u_passwd)
                {
                    if (bool_open_cash) // if cash is open!
                    {
                        this.label1.Text = "Otwarta kasa";
                        this.label2.Text = "Koontynuuj zmianê nr: ";
                    }
                    else
                    {
                        this.label1.Text = "Otwarcie zmiany";
                        this.label2.Text = "Czy chcesz otworzyæ zmianê nr: ";
                    }

                    
                    this.button3.Visible = true;
                    button5.Visible = false;    
                    exit_app.Visible = false;
                    this.label1.Visible = true;
                    if (verify_is_month_closed(CSV_filename_series_logs, CSV_delimiter))    // if month is closed
                    {
                        month_is_closed = true;
                        this.button3.Enabled = false;
                        this.label2.Text = "Miesi¹c zosta³ ju¿ zamkniêty";
                        this.open_cash.Visible = false;
                        this.button5.Text = "Wznów miesi¹c";
                    }
                    else
                    {
                        month_is_closed = false;
                        this.button3.Enabled = true;
                        this.open_cash.Visible = true;
                        this.label2.Text = "Aktualna zmiana:";
                        this.button5.Text = "Zakoñczenie miesi¹ca";
                    }
                    if (i == 0)                 // superuser
                    {
                        this.label1.Visible = false;
                        this.button3.Visible = false;
                        button5.Visible = true; // buttons only for special user
                        exit_app.Visible = true;
                    }
                    
                    this.open_cash.Text = counter_series.ToString();    // nr series
                    logged = true;
                    this.curr_user = name;
                    panel1.Visible = false;
                    panel2.Visible = true;
                    this.clear();
                    break;
                }

            }
            if (!logged)
            {
                DialogResult dialogresult = MessageBox.Show("NIE uda³o siê zalogowaæ.");
                this.user_passwd.Text = ""; //resetuje has³o
                this.user_passwd.Focus();
            }
        }
        private void exit_app_Click(object sender, EventArgs e) // close app
        {
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)  // shutdown clicked
        {
            if (!debug)
            {
                Calib.SystemLibNet.Api.SysPowerOff();
            }
            else
            {
                this.Close();
            }
        }
        private void button4_Click(object sender, EventArgs e)  //logout clicked
        {
            panel1.Visible = true;
            panel2.Visible = false;
            curr_user = "";
        }
        private void button3_Click(object sender, EventArgs e)  // open/continue selling 
        {
            if (!verify_previous_month_closed(CSV_filename_series_logs, CSV_delimiter) && !bool_open_cash)
            {
                MessageBox.Show("Poprzedni miesi¹c nie zosta³ zamkniêty!","Brak mo¿liwoœci otwarcia");
                return;
            }
            panel2.Visible = false;
            panel3.Visible = true;
            if (!bool_open_cash)    // otwarcie nowej zmiany
            {
                string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter_series.ToString(), curr_user, "open");
                saveToFile(CSV_filename_series_logs, temp_string, false);
                bool_open_cash = true;
                temp_string = vars_ToCSV(CSV_delimiter, counter_tickets.ToString(), counter_series.ToString(), bool_open_cash.ToString());
                if (!saveToFile(CSV_filename_counters, temp_string, true))
                {
                    MessageBox.Show("B³¹d zapisu otwarcia zmiany!", "B³¹d I/O");
                }
            }

            CSV_filename_tickets_series = "\\SD Card\\zmiana_" + counter_series.ToString() + ".csv";
            CSV_filename_tickets_month = "\\FlashDisk\\TM\\Data\\" + DateTime.Now.ToString("yyyy-MM") + ".csv";
            CSV_filename_tickets_month_SD = "\\SD Card\\" + DateTime.Now.ToString("yyyy-MM") + ".csv";
            CSV_filename_canceled_SD = "\\SD Card\\anulowane_" + counter_series.ToString() + ".csv";
            cancel_filename_flash = "\\FlashDisk\\TM\\Data\\anulowane_" + DateTime.Now.ToString("yyyy-MM") + ".csv";
            CSV_filename_logs = "\\FlashDisk\\TM\\Data\\logs_" + DateTime.Now.ToString("yyyy-MM") + ".csv";

            string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),"info", curr_user, counter_series.ToString(), "zalogowano i otwarcie/kontynuacja zmiany");
            saveToFile(CSV_filename_logs, tempstring, false);

        }
        private void button5_Click(object sender, EventArgs e)  // close(submit) month
        {
            if (bool_open_cash)
            {
                MessageBox.Show("Nie mo¿na zrobiæ podsumowania miesi¹ca bez zakoñczenia bie¿¹cej zmiany!!", "ZAMKNIJ ZMIANÊ");
                return;
            }
            if (!month_is_closed)   // if false on button "zakoñczenie miesi¹ca"
            {
                if (CSV_filename_tickets_month == "")
                {
                    string series_filename = get_name_last_series_to_close_month(CSV_filename_series_logs, CSV_delimiter);
                    if (series_filename == "")
                    {
                        return;
                    }
                    CSV_filename_tickets_month = "\\FlashDisk\\TM\\Data\\" + series_filename;
                    cancel_filename_flash =      "\\FlashDisk\\TM\\Data\\anulowane_" + series_filename;
                }
                if (!File.Exists(CSV_filename_tickets_month))
                {
                    MessageBox.Show("Nie da sie zamkn¹æ miesi¹ca, który nie istnieje!", "Brak historii");
                    return;
                }
                // 
                if (!close_month_of_selling(CSV_delimiter, CSV_filename_tickets_month, cancel_filename_flash, CSV_filename_series_logs))
                {
                    MessageBox.Show("Nie uda³o siê zamkn¹æ miesi¹ca.","B³¹d.");
                }
            }
            else // if true on button "wznów miesi¹c"
            {
                string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter_series.ToString(), curr_user, "renew_month");
                saveToFile(CSV_filename_series_logs, temp_string, false);
                MessageBox.Show("Poprzedni wydruk zamkniêcia miesi¹ca w tym momencie sta³ siê nieaktualny.", "Wznowiono.");
            }
            this.panel2.Visible = false;
            this.panel1.Visible = true;
            //MessageBox.Show("Modu³ jeszcze nie zosta³ zaimplementowany", " W trakcie realizacji");
        }
        private void panel6_GotFocus(object sender, EventArgs e)    //panel with fv
        {
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(fv_enter_pressed);
            this.textBox1.Text = "";
            this.textBox3.Text = "";
        }

        // -------------------- 
        private void clear()    //clear visibles and variables
        {
            user_name.Text = "";
            user_passwd.Text = "";
            choosen_start = "";
            choosen_destination = "";

            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            numericUpDown4.Value = 0;
            numericUpDown5.Value = 0;
            numericUpDown6.Value = 0;

            bool_ticket_T = false;  
            bool_ticket_TP = false;
            bool_ticket_month = false;
            bool_ticket_bike = false;
            bool_ticket_group = false;
            bool_KDR = false;
            extra_prize = false;
            bool_is_special = false;
            relief = 0;
            single_ticket_clicked(); // reset default colour at single ticket try

            ticket_cash_people = 0;
            ticket_cash_bike = 0;
            ticket_cash_extra = 0;
            cancel_ticket_cash_people = 0;
            cancel_ticket_cash_bike = 0;
            cancel_ticket_cash_extra = 0;
            ticket_first = 2147483646;
            ticket_last = 0;
            return_summary = 0;
            earn_summary = 0;
        }
        private void single_ticket_clicked() //RESET choosen relief - and ONLY visibles
        {
            this.button16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.button20.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.button32.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.label74.Visible = false;
            this.label72.Visible = false;
            this.label73.Visible = false;
            this.button7.BackColor = Color.Transparent;
            this.button8.BackColor = Color.Transparent;
            this.button9.BackColor = Color.Transparent;
            this.button10.BackColor = Color.Transparent;
            this.button11.BackColor = Color.Transparent;
            this.button12.BackColor = Color.Transparent;
            this.button13.BackColor = Color.Transparent;
            this.button14.BackColor = Color.Transparent;
            this.button15.BackColor = Color.Transparent;
            this.button41.BackColor = Color.Transparent;
            this.button40.BackColor = Color.Transparent;
            this.button39.BackColor = Color.Transparent;
            this.button38.BackColor = Color.Transparent;
            this.button37.BackColor = Color.Transparent;
            this.button36.BackColor = Color.Transparent;
            this.button35.BackColor = Color.Transparent;
            this.button34.BackColor = Color.Transparent;
            this.button33.BackColor = Color.Transparent;
            this.button29.BackColor = Color.Transparent;
            this.button28.BackColor = Color.Transparent;
            this.button27.BackColor = Color.Transparent;
            this.button26.BackColor = Color.Transparent;
            this.button25.BackColor = Color.Transparent;
            this.button24.BackColor = Color.Transparent;
            this.button23.BackColor = Color.Transparent;
        }
        // --------------------

        private void ticket_single_Click(object sender, EventArgs e)    // choosen single ticket from main menu
        {
            panel3.Visible = false; //ekran wyboru biletów
            panel6_single.Visible = true; //ekran wyboru dot. bil. jednorazowego
            ticket_type.Text = "T";
            bool_ticket_T = true;
            was_printed = false;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "bilet TAM");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void ticket_return_Click(object sender, EventArgs e)    // choosen return ticket from main menu
        {
            panel3.Visible = false; //ekran wyboru biletów
            panel6_single.Visible = true; //ekran wyboru dot. bil. jednorazowego
            ticket_type.Text = "T/P";
            bool_ticket_TP = true;
            was_printed = false;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "bilet T/P");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void ticket_month_Click(object sender, EventArgs e)     // choosen monthly ticket from main menu
        {
            panel3.Visible = false; //ekran wyboru biletów
            panel6_monthly.Visible = true; // miesieczny
            bool_ticket_month = true;
            was_printed = false;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "miesieczny");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void ticket_bike_Click(object sender, EventArgs e)      // choosen bike ticket from main menu
        {
            panel3.Visible = false; //ekran wyboru biletów
            panel7.Visible = true; // rower
            bool_ticket_bike = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "rower");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void ticket_group_Click(object sender, EventArgs e)     // choosen group ticket from main menu
        {
            panel3.Visible = false; //ekran wyboru biletów
            panel6_group.Visible = true; // grupa
            this.bool_ticket_group = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "grupa");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void ticket_options_Click(object sender, EventArgs e)
        {
            panel3.Visible = false; //ekran wyboru biletów
            panel4.Visible = true; // menu opcji
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "opcje");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void ticket_logout_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            panel3.Visible = false;
            curr_user = "";
            if (bool_open_cash)
            {
                MessageBox.Show("Jeœli zakoñczono sprzeda¿ biletów proszê pamiêtaæ o zamkniêciu zmiany", "Zmiana jest nadal otwarta");
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "wyloguj");
            saveToFile(CSV_filename_logs, temp_string, false);
            this.dateTimePicker1.Value = DateTime.Now;
        }
        private void opt_back_Click(object sender, EventArgs e)
        {
            this.panel4.Visible = false;     // cofniecie do wyboru biletów
            this.panel3.Visible = true;
            this.panel9.Visible = false;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - z menu opcji");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void opt_credentials_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;    
            panel5.Visible = true;      // credentials panel
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "poswiadczenia");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button17_Click(object sender, EventArgs e) // go to menu from single ticket
        {
            panel3.Visible = true;
            panel6_single.Visible = false;      // panel poœwiadczeñ
            this.clear();
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - bilet T/TP");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button19_Click(object sender, EventArgs e) // go to menu from monthly ticket
        {
            panel3.Visible = true;
            panel6_monthly.Visible = false;
            this.clear();
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - bilet miesieczny");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button46_Click(object sender, EventArgs e) // go to menu from bike ticket
        {
            panel3.Visible = true;
            panel7.Visible = false;
            this.clear();
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - bilet na rower");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button31_Click(object sender, EventArgs e) // go to menu from groupticket
        {
            panel3.Visible = true;
            panel6_group.Visible = false;
            this.clear();
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - bilet grupowy");
            saveToFile(CSV_filename_logs, temp_string, false);
            string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - bilet grupowy");
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        private void button21_Click(object sender, EventArgs e) // group ticket - normal button
        {
            if (!(this.button21.BackColor == System.Drawing.Color.Lime))
            {
                single_ticket_clicked();
                this.reliefs_size_monthly.Enabled = false;
                relief = 0;
                this.button21.BackColor = System.Drawing.Color.Lime;
                this.button22.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            }
        }
        private void button22_Click(object sender, EventArgs e) // group ticket - relief button
        {
            if (!(this.button22.BackColor == System.Drawing.Color.Lime))
            {
                this.reliefs_size_monthly.Enabled = true;
                this.button22.BackColor = System.Drawing.Color.Lime;
                this.button21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            }
        }
        private void credentials_back_Click(object sender, EventArgs e) // menu
        {
            this.panel4.Visible = true;
            this.panel5.Visible = false;
            this.panel9.Visible = false;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - z menu poswiadczen");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)    // enable reliefs_size at single
        {
            if (numericUpDown2.Value >= 1)
            {
                reliefs_size.Enabled = true;
            }
            else
            {
                reliefs_size.Enabled = false;
                relief = 0;
                single_ticket_clicked();
            }
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)    // enable reliefs_size at group
        {
            if (numericUpDown5.Value >= 1)
            {
                reliefs_size_group.Enabled = true;
            }
            else
            {
                reliefs_size_group.Enabled = false;
                relief = 0;
                single_ticket_clicked();
            }
        }
        // reliefs buttons
        private void button7_Click(object sender, EventArgs e)  // single 33
        {
            single_ticket_clicked();
            this.button7.BackColor = System.Drawing.Color.Lime;
            relief = 33;
        }
        private void button8_Click(object sender, EventArgs e)  // single 37
        {
            single_ticket_clicked();
            this.button8.BackColor = System.Drawing.Color.Lime;
            relief = 37;
        }
        private void button9_Click(object sender, EventArgs e)  // single KDR
        {
            single_ticket_clicked();
            this.button9.BackColor = System.Drawing.Color.Lime;
            relief = 37; // KDR
            this.bool_KDR = true;
        }
        private void button10_Click(object sender, EventArgs e) // single 49
        {
            single_ticket_clicked();
            this.button10.BackColor = System.Drawing.Color.Lime;
            relief = 49;
        }
        private void button11_Click(object sender, EventArgs e) // single 51
        {
            single_ticket_clicked();
            this.button11.BackColor = System.Drawing.Color.Lime;
            relief = 51;
        }
        private void button12_Click(object sender, EventArgs e) // single 78
        {
            single_ticket_clicked();
            this.button12.BackColor = System.Drawing.Color.Lime;
            relief = 78;
        }
        private void button13_Click(object sender, EventArgs e) // single 93
        {
            single_ticket_clicked();
            this.button13.BackColor = System.Drawing.Color.Lime;
            relief = 93;
        }
        private void button14_Click(object sender, EventArgs e) // single 95
        {
            single_ticket_clicked();
            this.button14.BackColor = System.Drawing.Color.Lime;
            relief = 95;
        }
        private void button15_Click(object sender, EventArgs e) // single 100
        {
            single_ticket_clicked();
            this.button15.BackColor = System.Drawing.Color.Lime;
            relief = 100;
        }
        // group tickets
        private void button41_Click(object sender, EventArgs e) // group 33
        {
            single_ticket_clicked();
            this.button41.BackColor = System.Drawing.Color.Lime;
            relief = 33;
        }
        private void button40_Click(object sender, EventArgs e) // group 37
        {
            single_ticket_clicked();
            this.button40.BackColor = System.Drawing.Color.Lime;
            relief = 37;
        }
        private void button39_Click(object sender, EventArgs e) // group KDR
        {
            single_ticket_clicked();
            this.button39.BackColor = System.Drawing.Color.Lime;
            relief = 37; // KDR
            this.bool_KDR = true;
        }
        private void button38_Click(object sender, EventArgs e) // group 49
        {
            single_ticket_clicked();
            this.button38.BackColor = System.Drawing.Color.Lime;
            relief = 49;
        }
        private void button37_Click(object sender, EventArgs e) // group 51
        {
            single_ticket_clicked();
            this.button37.BackColor = System.Drawing.Color.Lime;
            relief = 51;
        }
        private void button36_Click(object sender, EventArgs e) // group 78
        {
            single_ticket_clicked();
            this.button36.BackColor = System.Drawing.Color.Lime;
            relief = 78;
        }
        private void button35_Click(object sender, EventArgs e) // group 93
        {
            single_ticket_clicked();
            this.button35.BackColor = System.Drawing.Color.Lime;
            relief = 93;
        }
        private void button34_Click(object sender, EventArgs e) // group 95
        {
            single_ticket_clicked();
            this.button34.BackColor = System.Drawing.Color.Lime;
            relief = 95;
        }
        private void button33_Click(object sender, EventArgs e) // group 100
        {
            single_ticket_clicked();
            this.button33.BackColor = System.Drawing.Color.Lime;
            relief = 100;
        }
        // monthly tickets
        private void button29_Click(object sender, EventArgs e) // monthly 33
        {
            single_ticket_clicked();
            this.button29.BackColor = System.Drawing.Color.Lime;
            relief = 33;
        }
        private void button28_Click(object sender, EventArgs e) // monthly 37
        {
            single_ticket_clicked();
            this.button28.BackColor = System.Drawing.Color.Lime;
            relief = 37;
        }
        private void button27_Click(object sender, EventArgs e) // monthly 49
        {
            single_ticket_clicked();
            this.button27.BackColor = System.Drawing.Color.Lime;
            relief = 49;
        }
        private void button26_Click(object sender, EventArgs e) // monthly KDR
        {
            single_ticket_clicked();
            this.button26.BackColor = System.Drawing.Color.Lime;
            relief = 49;    // KDR
            this.bool_KDR = true;
        }
        private void button25_Click(object sender, EventArgs e) // monthly 51
        {
            single_ticket_clicked();
            this.button25.BackColor = System.Drawing.Color.Lime;
            relief = 51;
        }
        private void button24_Click(object sender, EventArgs e) // monthly 78
        {
            single_ticket_clicked();
            this.button24.BackColor = System.Drawing.Color.Lime;
            relief = 78;
        }
        private void button23_Click(object sender, EventArgs e) // monthly 93
        {
            single_ticket_clicked();
            this.button23.BackColor = System.Drawing.Color.Lime;
            relief = 93;
        }
        // only to design
        private void stations_revert_Click(object sender, EventArgs e)  // revert stations single
        {
            if (this.stations_start.DataSource == stations_1)
            {
                this.stations_start.DataSource = stations_2;
                this.stations_destination.DataSource = stations_1;
            }
            else
            {
                this.stations_start.DataSource = stations_1;
                this.stations_destination.DataSource = stations_2;
            }
        }
        private void button42_Click(object sender, EventArgs e)         // revert stations group
        {
            if (this.comboBox1.DataSource == stations_1)
            {
                this.comboBox1.DataSource = stations_2;
                this.comboBox2.DataSource = stations_1;
            }
            else
            {
                this.comboBox1.DataSource = stations_1;
                this.comboBox2.DataSource = stations_2;
            }
        }
        private void button16_Click(object sender, EventArgs e) // extra payment to single ticket
        {
            if (!(this.button16.BackColor == System.Drawing.Color.Red))
            {
                this.button16.BackColor = System.Drawing.Color.Red;
                extra_prize = true;
            }
            else
            {
                this.button16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
                extra_prize = false;
            }
        }
        private void button32_Click(object sender, EventArgs e) // extra payment to group ticket
        {
            if (!(this.button32.BackColor == System.Drawing.Color.Red))
            {
                this.button32.BackColor = System.Drawing.Color.Red;
                extra_prize = true;
            }
            else
            {
                this.button32.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
                extra_prize = false;
            }
        }
        private void button20_Click(object sender, EventArgs e) // extra payment to monthly ticket
        {
            if (!(this.button20.BackColor == System.Drawing.Color.Red))
            {
                this.button20.BackColor = System.Drawing.Color.Red;
                extra_prize = true;
            }
            else
            {
                this.button20.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
                extra_prize = false;
            }
        }
        private void button43_Click(object sender, EventArgs e)         // back from cash menu
        {
            this.panel_cash.Visible = false;
            if (bool_ticket_T || bool_ticket_TP)
            {
                this.panel6_single.Visible = true;
            }
            else if (bool_ticket_month)
            {
                this.panel6_monthly.Visible = true;
            }
            else if (bool_ticket_group)
            {
                this.panel6_group.Visible = true;
            }
            else if (bool_ticket_bike)
            {
                this.panel7.Visible = true;
            }
            else
            {
                this.panel3.Visible = true; // if something wrong with vars
            }
        }
        private void panel_cash_focus(object sender, EventArgs e)       //only focus at textbox
        {
            this.panel_cash_get_money.Focus();
        }
        private void refresh_money_to_pay(object sender, EventArgs e)
        {
            try
            {
                string get_money_string = this.panel_cash_get_money.Text;//.Replace(",", ".");
                if (get_money_string == "")
                { 
                    get_money_string = "0"; 
                }
                double money = Convert.ToDouble(get_money_string, System.Globalization.CultureInfo.InvariantCulture);
                this.label60.Text = (String.Format("{0:0.00}", (money - ticket_prize))).ToString() + " PLN";
            }
            catch { }   // dont crash if convert from nothing or text
        }
        private void opt_fv_Click(object sender, EventArgs e)
        {
            this.panel4.Focus();
            this.panel6.Visible = true;
            this.panel4.Visible = false;
            this.panel6.Focus();
            was_printed = false;
            this.KeyPreview = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "dane do fv");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        // after clicked go to money_validator
        private void button18_Click(object sender, EventArgs e) // confirm with single
        {
            string info;
            string type = "info";
            if (stations_start.SelectedItem.ToString() == stations_destination.SelectedItem.ToString())
            {
                type = "warning";
                info = "Stacja docelowa musi byæ ró¿na od stacji pocz¹tkowej";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else if (numericUpDown1.Value == 0 && numericUpDown2.Value == 0 && numericUpDown3.Value == 0)
            {
                type = "warning";
                info = "Musi byæ wybrany co najmniej jeden rodzaj biletu";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else if ((numericUpDown2.Value >= 1) && relief == 0)
            {
                type = "warning";
                info = "Nie wybrano zni¿ki";
                DialogResult dialogresult = MessageBox.Show(info);
            }/*
            else if((numericUpDown3.Value == 1) || (numericUpDown3.Value == 3))
            {
                type = "warning";
                info = "Wprowadzono niepoprawn¹ wartoœæ!\nPoprawne wartoœci to: 0,2,4";
                DialogResult dialogresult = MessageBox.Show(info);
            }*/
            else
            {
                if (radioButton2.Checked)
                {
                    bool_is_special = true;
                }
                else
                {
                    bool_is_special = false;
                }

                info = "OK";
                this.panel6_single.Visible = false;
                choosen_start = stations_start.Text.ToString();
                choosen_destination = stations_destination.Text.ToString();
                this.money_validator();
                this.refresh_money_to_pay(sender, e);
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "Bilet zatwierdz - "+info);
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button30_Click(object sender, EventArgs e) // confirm with group
        {
            string info = "";
            string type = "warning";
            if (comboBox2.SelectedItem.ToString() == comboBox1.SelectedItem.ToString())
            {
                info = "Stacja docelowa musi byæ ró¿na od stacji pocz¹tkowej";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else if (numericUpDown6.Value == 0 && numericUpDown5.Value == 0 && numericUpDown4.Value == 0)
            {
                info = "Musi byæ wybrany co najmniej jeden rodzaj biletu";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else if ((numericUpDown5.Value >= 1) && relief == 0)
            {
                info = "Nie wybrano zni¿ki";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else
            {
                if (radioButton3.Checked)
                {
                    bool_is_special = true;
                }
                else
                {
                    bool_is_special = false;
                }
                type = "info";
                this.panel6_group.Visible = false;
                choosen_start = this.comboBox2.Text.ToString();
                choosen_destination = this.comboBox1.Text.ToString();
                this.money_validator();
                this.refresh_money_to_pay(sender, e);
            }
            string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "zatwierdz - bilet grupowy " + info);
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        private void button6_Click(object sender, EventArgs e)  // confirm with monthly
        {
            string info = "";
            string type = "warning";
            if (this.reliefs_size_monthly.Enabled && relief == 0)
            {
                info = "Nie wybrano zni¿ki";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else
            {
                type = "info";
                this.panel6_monthly.Visible = false;
                choosen_start = stations_start.Text.ToString();
                choosen_destination = stations_destination.Text.ToString();
                this.money_validator();
                this.refresh_money_to_pay(sender, e);
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "cofnij - bilet miesieczny "+info);
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void bike_clicked(object sender, EventArgs e)   // confirm with bike
        {
            string info = "";
            string type = "warning";
            if (numericUpDown7.Value == 0)
            {
                info = "Wybierz co najmniej jeden rower";
                DialogResult dialogresult = MessageBox.Show(info);
            }
            else
            {
                type = "info";
                this.panel7.Visible = false;
                this.money_validator();
                this.refresh_money_to_pay(sender, e);
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "cofnij - bilet na rower");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        // then
        private void money_validator() // panel_cash
        {
            double to_pay = 0;
            double to_pay_all = 0; 
            this.panel_cash.Visible = true;
            this.panel_cash.Focus();
            this.rebuild_ticket_panel(); // renew ticket proportions

            
            if (bool_ticket_T)
            {
                train_type = "T";
            }
            else if (bool_ticket_TP) 
            { 
                train_type = "TP"; 
            }
            else if (bool_ticket_month)
            { 
                train_type = "monthly";
            }
            else if (bool_ticket_bike)
            { 
                train_type = "bike";
            }
            else if (bool_ticket_group) // group ticket costs like a single ticket
            { 
                train_type = "T";
            }

            if (numericUpDown1.Value > 0) // normal t or tp
            {
                to_pay = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, 0, ref relief_prize);
                to_pay_all = to_pay_all + (to_pay * Decimal.ToDouble(numericUpDown1.Value));
            }
            if (numericUpDown6.Value > 0) // group normal
            {
                to_pay = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, 0, ref relief_prize);
                to_pay_all = to_pay_all + (to_pay * Decimal.ToDouble(numericUpDown6.Value));
            }
            if (numericUpDown2.Value > 0) // normal t or tp
            {
                to_pay = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, relief, ref relief_prize);
                to_pay_all = to_pay_all + (to_pay * Decimal.ToDouble(numericUpDown2.Value));
            }
            if (numericUpDown5.Value > 0) // group relief
            {
                to_pay = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, relief, ref relief_prize);
                to_pay_all = to_pay_all + (to_pay * Decimal.ToDouble(numericUpDown5.Value));
            }
            if (bool_ticket_month) // monthly
            {
                to_pay_all = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, relief, ref relief_prize);
            }
            if (bool_ticket_bike) // bike
            {
                to_pay = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, 0, ref relief_prize);
                to_pay_all = to_pay_all + (to_pay * Decimal.ToDouble(numericUpDown7.Value));
            }
            if (numericUpDown3.Value > 0) // family ticket
            {
                to_pay = gen_ticket_prize(choosen_start, choosen_destination, train_type, bool_is_special, -1, ref relief_prize); // -1 family
                family_ticket_prize = to_pay;
                to_pay_all = to_pay_all + (to_pay * Decimal.ToDouble((numericUpDown3.Value)/2));
            }

            this.panel_cash_money.Text = String.Format("{0:0.00}", to_pay_all) + " PLN";
            if (extra_prize)
            {
                this.label55.Text = (String.Format("{0:0.00}", extra_prize_money) + " PLN");
                to_pay_all += extra_prize_money;
            }
            else
            {
                this.label55.Text = ("0 PLN");
            }
            this.label57.Text = (String.Format("{0:0.00}", to_pay_all) + " PLN");
            ticket_prize = to_pay_all;

        }
        private void cash_ticket(object sender, EventArgs e) //submitted cash 
        {
            // generate bottom of the ticket
            this.paper_ticket_single_HOUR.Text = DateTime.Now.ToString("HH:mm");
            this.paper_ticket_single_USER.Text = "K" + curr_user;
            this.paper_ticket_single_ZMIANA.Text = counter_series.ToString();
            this.paper_ticket_single_SERIA.Text = TM_SERIES;
            this.paper_ticket_single_TICKETNR.Text = counter_tickets.ToString();

            this.paper_ticket_single_PLN.Text = String.Format("{0:0.00}", ticket_prize);
            this.paper_ticket_single_PTU.Text = String.Format("{0:0.00}", (ticket_prize * 8)/108);
            this.panel_cash.Visible = false;
            //and the rest
            if (bool_ticket_T || bool_ticket_TP)
            {
                this.single_ticket_clicked(true);
            }
            else if (bool_ticket_group)
            {
                this.single_ticket_clicked(false);
            }
            else if (bool_ticket_month)
            {
                this.monthly_ticket_clicked();
            }
            else if (bool_ticket_bike)
            {
                this.bike_ticket_clicked();
                this.paper_ticket_single_PTU.Text = String.Format("{0:0.00}", (ticket_prize * 23)/123);
            }
            
        }
        // generate tickets
        private void single_ticket_clicked(bool type)   // true for single ticket false for group
        {
            this.panel8.Visible = true;
            was_printed = false;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(fv_enter_pressed);  // get enter pressed or not
            this.paper_ticket_single_START.Text = stations_start.Text;
            this.paper_ticket_single_DESTINATION.Text = stations_destination.Text;
            was_printed = false;

            this.paper_ticket_single_DATA.Text = DateTime.Now.ToString("yyyy-MM-dd");   

            if (!type)          //group
            {
                this.paper_ticket_single_TYPE.Text = "GRUPOWY";
                this.label38.Text = "BEZP£:";
                if (radioButton4.Checked)
                {
                    this.paper_ticket_single_POC.Text = "O";
                }
                else
                {
                    this.paper_ticket_single_POC.Text = "OK";
                }
                this.paper_ticket_single_NORM.Text = numericUpDown6.Value.ToString();
                this.paper_ticket_single_ULG.Text = numericUpDown5.Value.ToString();
                if (bool_KDR)
                {
                    this.paper_ticket_single_ZNIZKA.Text = "KDR";
                }
                else if (!bool_KDR)
                {
                    this.paper_ticket_single_ZNIZKA.Text = relief.ToString() + "%";
                }
                this.paper_ticket_single_RODZ.Text = numericUpDown4.Value.ToString();
            }
            else  // single ticket
            {
                if (radioButton1.Checked)
                {
                    this.paper_ticket_single_POC.Text = "O";
                }
                else
                {
                    this.paper_ticket_single_POC.Text = "OK";
                }
                this.paper_ticket_single_NORM.Text = numericUpDown1.Value.ToString();
                this.paper_ticket_single_ULG.Text = numericUpDown2.Value.ToString();
                if (bool_KDR)
                {
                    this.paper_ticket_single_ZNIZKA.Text = "KDR";
                }
                else if (!bool_KDR)
                {
                    this.paper_ticket_single_ZNIZKA.Text = relief.ToString() + "%";
                }
                this.paper_ticket_single_RODZ.Text = numericUpDown3.Value.ToString();
                if (bool_ticket_TP)
                {
                    //this.paper_ticket_single_TYPE.Text = "GRUPOWY";
                    this.label74.Visible = true; // >
                    this.label72.Visible = true; // from
                    this.label73.Visible = true; // to
                    this.label72.Text = choosen_destination;
                    this.label73.Text = choosen_start;
                }
                // ticket name+ gzubowy
                if (bool_ticket_T)
                {
                    if ((choosen_start == stations_1[0] || choosen_start == stations_1[1] || choosen_start == stations_1[7]) && choosen_destination == stations_1[3])
                    {
                        this.paper_ticket_single_TYPE.Text = "tanioGzubowy TAM";
                    }
                    else if ((choosen_destination == stations_1[0] || choosen_destination == stations_1[1] || choosen_destination == stations_1[7]) && choosen_start == stations_1[3])
                    {
                        this.paper_ticket_single_TYPE.Text = "tanioGzubowy TAM";
                    }
                    else
                    {
                        this.paper_ticket_single_TYPE.Text = "jednorazowy TAM";
                    }
                }
                else if (bool_ticket_TP)
                {
                    if ((choosen_start == stations_1[0] || choosen_start == stations_1[1] || choosen_start == stations_1[7]) && choosen_destination == stations_1[3])
                    {
                        this.paper_ticket_single_TYPE.Text = "tanioGzubowy T/P";
                    }
                    else if ((choosen_destination == stations_1[0] || choosen_destination == stations_1[1] || choosen_destination == stations_1[7]) && choosen_start == stations_1[3])
                    {
                        this.paper_ticket_single_TYPE.Text = "tanioGzubowy T/P";
                    }
                    else
                    {
                        this.paper_ticket_single_TYPE.Text = "jednorazowy T/P";
                    }
                }


            }
        }
        private void monthly_ticket_clicked()
        {
            this.panel6_monthly.Visible = false;
            this.panel8.Visible = true;
            was_printed = false;
            this.KeyPreview = true;
            int actual_year = int.Parse(DateTime.Now.ToString("yyyy")); // for DaysInMonth
            int actual_month = int.Parse(DateTime.Now.ToString("MM"));  // for DaysInMonth

            this.KeyDown += new KeyEventHandler(fv_enter_pressed);  // get enter pressed or not

            this.paper_ticket_single_POC.Text = "*";
            this.label42.Visible = true; // place for name
            this.label38.Visible = false;
            this.label62.Visible = true;
            this.label46.Visible = false;
            this.paper_ticket_single_RODZ.Visible = false;
            this.paper_ticket_single_DATA.Visible = false;
            this.paper_ticket_single_TYPE.Text = "SIECIOWY MIESIÊCZNY IMIENNY";				
            this.label32.Text = "Imiê i nazwisko:";
            this.paper_ticket_single_START.Text = DateTime.Now.ToString("yyyy-MM-dd");      // date at monthly ticket from 
            this.paper_ticket_single_DESTINATION.Text = DateTime.Now.AddDays(DateTime.DaysInMonth(actual_year, actual_month)).ToString("yyyy-MM-dd");
            this.paper_ticket_single_PLN.Text = String.Format("{0:0.00}", ticket_prize);
            this.paper_ticket_single_PTU.Text = String.Format("{0:0.00}", ((ticket_prize * 8)/108));
            if (relief > 0)
            {
                this.paper_ticket_single_ULG.Text = "1";
                this.paper_ticket_single_ZNIZKA.Text = relief.ToString() + "%";
            }
            else
            {
                this.paper_ticket_single_NORM.Text = "1";
            }
        }
        private void bike_ticket_clicked()
        {
            this.panel8.Visible = true;
            was_printed = false;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(fv_enter_pressed);  // get enter pressed or not
            this.label34.Visible = false;
            this.label35.Visible = false;
            this.label36.Visible = false;
            this.label37.Visible = false;
            this.label38.Visible = false;
            this.label39.Visible = false;
            this.label40.Visible = false;
            this.label41.Visible = false;
            this.label42.Visible = true;
            this.label44.Visible = false;
            this.label45.Visible = false;
            this.label63.Visible = true;

            this.label42.Text = "wa¿ny w jedn¹ stronê ³¹cznie z biletem na przejazd";
            this.paper_ticket_single_ULG.Text = this.numericUpDown7.Value.ToString();
            this.paper_ticket_single_TYPE.Text = "NA PRZEWÓZ ROWERU";
            this.paper_ticket_single_DATA.Text = DateTime.Now.ToString("yyyy-MM-dd");
            this.label52.Text = "w tym PTU 23% PLN";
            
        }
         
        //print all tickets
        private void fv_enter_pressed(object sender, KeyEventArgs e)    // run print_screen() and back to menu - also to print credentials
        {
            if (e.KeyCode == Keys.Enter && !was_printed && panel8.Visible)  // only to tickets
            {
                bool printer_status = is_printer_ok();
                if (printer_status)
                {
                    if (print_screen() > 0)
                    {
                        string temp_string = vars_ToCSV(CSV_delimiter, TM_SERIES, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter_series.ToString(), counter_tickets.ToString(), curr_user, relief.ToString(), bool_KDR.ToString(), extra_prize.ToString(), extra_prize_money.ToString(), bool_ticket_T.ToString(), bool_ticket_TP.ToString(), bool_ticket_month.ToString(), bool_ticket_bike.ToString(), bool_ticket_group.ToString(), this.paper_ticket_single_POC.Text, paper_ticket_single_NORM.Text, paper_ticket_single_ULG.Text, paper_ticket_single_ZNIZKA.Text, paper_ticket_single_RODZ.Text, paper_ticket_single_PLN.Text.Replace('.', ','), paper_ticket_single_PTU.Text.Replace('.', ','), relief_prize.ToString().Replace('.', ','), normal_ticket_prize.ToString().Replace('.', ','), family_ticket_prize.ToString().Replace('.', ','), choosen_start, choosen_destination);
                        if (this.panel8.Visible)    //tickets panel
                        {
                            if (!saveToFile(CSV_filename_tickets_series, temp_string, false))
                            {
                                MessageBox.Show("B³¹d zapisu!\n"+CSV_filename_tickets_series);
                            }
                            if (!saveToFile(CSV_filename_tickets_month, temp_string, false))
                            {
                                MessageBox.Show("B³¹d zapisu!\n"+CSV_filename_tickets_month);
                            }
                            if (!saveToFile(CSV_filename_tickets_month_SD, temp_string, false))
                            {
                                MessageBox.Show("B³¹d zapisu!\n"+ CSV_filename_tickets_month_SD);
                            }
                            temp_string = vars_ToCSV(CSV_delimiter, counter_tickets.ToString(), counter_series.ToString(), bool_open_cash.ToString());
                            if (!saveToFile(CSV_filename_counters, temp_string, true))
                            {
                                MessageBox.Show("B³¹d zapisu!", "B³¹d I/O");
                            }
                        }
                        this.panel6.Visible = false;    // panel with fv
                        this.panel8.Visible = false;    // panel with others
                        this.panel3.Visible = true;     // main panel 
                        this.panel4.Visible = false;
                        was_printed = true;
                        clear();
                        counter_tickets += 1;
                    }
                    else // in case of trouble reset printer driver
                    {
                        MessageBox.Show("Wyst¹pi³ b³¹d z wydrukowaniem. Spróbuj ponownie");
                        PrinterLibNet.Api.PRNClose();
                    }
                }
                else
                {
                    MessageBox.Show("B³¹d drukarki");
                }
            }
            else if(!was_printed && panel8.Visible)  // ticket if not ENTER
            {
                this.panel6.Visible = false;    // panel with fv
                this.panel8.Visible = false;    // panel with generated ticket
                this.panel3.Visible = false;    // main panel

                if (bool_ticket_group)
                {
                    this.panel6_group.Visible = true;
                }
                else if (bool_ticket_T || bool_ticket_TP)
                {
                    this.panel6_single.Visible = true;
                }
                else if (bool_ticket_month)
                {
                    this.panel6_monthly.Visible = true;
                }
                else if (bool_ticket_bike)
                {
                    this.panel7.Visible = true;
                }
                else
                {
                    this.panel3.Visible = true;     // main panel 
                }
            }
            else if (e.KeyCode == Keys.C && this.panel6.Visible) // fv panel
            {
                this.panel6.Visible = false;
                this.panel5.Visible = true;
                this.credentials_rebuild(sender, e);
            }
            else if (e.KeyCode == Keys.Enter && this.panel6.Visible)
            {
                print_screen();
                this.panel6.Visible = false;
                this.panel3.Visible = true;
            }
            else if (this.panel6.Visible)
            {
                return;
            }
            this.KeyPreview = false;
        }
        private void credentials_key_pressed(object sender, KeyEventArgs e)    // not in used! could be ereased
        {
            if (e.KeyCode == Keys.Enter && !was_printed)
            {
                was_printed = true;
                print_screen();
                this.panel9.Visible = false;
                this.panel4.Visible = true;
                this.KeyPreview = false;
            }
            else if (e.KeyCode == Keys.C)
            {
                this.panel9.Visible = false;
                this.panel5.Visible = true;
                this.KeyPreview = false;
                this.credentials_rebuild(sender, e);
                this.KeyPreview = false;
            }
        }

        // THE MOST IMPORTANT PART!!!
        private double gen_ticket_prize(string station_start, string station_destination, string type, bool is_special, int relief_size, ref double relief_money)  // relief_size == -1 - family ticket
        {
            // Œroda Wlkp Miasto
            Dictionary<string, int> sroda_miasto_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> sroda_miasto_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> sroda_miasto_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> sroda_miasto_norm_tp_ok = new Dictionary<string, int>();
            // Œroda Wlkp W¹sk.
            Dictionary<string, int> sroda_wask_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> sroda_wask_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> sroda_wask_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> sroda_wask_norm_tp_ok = new Dictionary<string, int>();
            // S³upia Wielka
            Dictionary<string, int> slupia_wielka_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> slupia_wielka_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> slupia_wielka_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> slupia_wielka_norm_tp_ok = new Dictionary<string, int>();
            // Annopole
            Dictionary<string, int> annopole_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> annopole_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> annopole_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> annopole_norm_tp_ok = new Dictionary<string, int>();
            // P³aczki
            Dictionary<string, int> placzki_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> placzki_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> placzki_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> placzki_norm_tp_ok = new Dictionary<string, int>();
            // Œnieciska
            Dictionary<string, int> snieciska_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> snieciska_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> snieciska_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> snieciska_norm_tp_ok = new Dictionary<string, int>();
            // Polwica
            Dictionary<string, int> polwica_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> polwica_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> polwica_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> polwica_norm_tp_ok = new Dictionary<string, int>();
            // Zaniemyœl
            Dictionary<string, int> zaniemysl_norm_t = new Dictionary<string, int>();
            Dictionary<string, int> zaniemysl_norm_tp = new Dictionary<string, int>();
            Dictionary<string, int> zaniemysl_norm_t_ok = new Dictionary<string, int>();
            Dictionary<string, int> zaniemysl_norm_tp_ok = new Dictionary<string, int>();


            // Œroda Wlkp Miasto
            sroda_miasto_norm_t.Add(stations_1[1], 7);
            sroda_miasto_norm_t.Add(stations_1[2], 7);
            sroda_miasto_norm_t.Add(stations_1[3], 10);
            sroda_miasto_norm_t.Add(stations_1[4], 21);
            sroda_miasto_norm_t.Add(stations_1[5], 28);
            sroda_miasto_norm_t.Add(stations_1[6], 30);
            sroda_miasto_norm_t.Add(stations_1[7], 30);     // T
            sroda_miasto_norm_tp.Add(stations_1[1], 14);
            sroda_miasto_norm_tp.Add(stations_1[2], 14);
            sroda_miasto_norm_tp.Add(stations_1[3], 20);
            sroda_miasto_norm_tp.Add(stations_1[4], 40);
            sroda_miasto_norm_tp.Add(stations_1[5], 40);
            sroda_miasto_norm_tp.Add(stations_1[6], 40);
            sroda_miasto_norm_tp.Add(stations_1[7], 40);    // TP
            sroda_miasto_norm_t_ok.Add(stations_1[1], 10);
            sroda_miasto_norm_t_ok.Add(stations_1[2], 10);
            sroda_miasto_norm_t_ok.Add(stations_1[3], 20);
            sroda_miasto_norm_t_ok.Add(stations_1[4], 30);
            sroda_miasto_norm_t_ok.Add(stations_1[5], 40);
            sroda_miasto_norm_t_ok.Add(stations_1[6], 40);
            sroda_miasto_norm_t_ok.Add(stations_1[7], 40);  // T OK
            sroda_miasto_norm_tp_ok.Add(stations_1[1], 20);
            sroda_miasto_norm_tp_ok.Add(stations_1[2], 20);
            sroda_miasto_norm_tp_ok.Add(stations_1[3], 40);
            sroda_miasto_norm_tp_ok.Add(stations_1[4], 50);
            sroda_miasto_norm_tp_ok.Add(stations_1[5], 50);
            sroda_miasto_norm_tp_ok.Add(stations_1[6], 50);
            sroda_miasto_norm_tp_ok.Add(stations_1[7], 50); // TP OK
            //Œroda Wlkp W¹sk.
            sroda_wask_norm_t.Add(stations_1[0], 7);
            sroda_wask_norm_t.Add(stations_1[2], 7);
            sroda_wask_norm_t.Add(stations_1[3], 10);
            sroda_wask_norm_t.Add(stations_1[4], 21);
            sroda_wask_norm_t.Add(stations_1[5], 28);
            sroda_wask_norm_t.Add(stations_1[6], 30);
            sroda_wask_norm_t.Add(stations_1[7], 30);   // T
            sroda_wask_norm_tp.Add(stations_1[0], 14);
            sroda_wask_norm_tp.Add(stations_1[2], 14);
            sroda_wask_norm_tp.Add(stations_1[3], 20);
            sroda_wask_norm_tp.Add(stations_1[4], 40);
            sroda_wask_norm_tp.Add(stations_1[5], 40);
            sroda_wask_norm_tp.Add(stations_1[6], 40);
            sroda_wask_norm_tp.Add(stations_1[7], 40);  // tp
            sroda_wask_norm_t_ok.Add(stations_1[0], 10);
            sroda_wask_norm_t_ok.Add(stations_1[2], 10);
            sroda_wask_norm_t_ok.Add(stations_1[3], 20);
            sroda_wask_norm_t_ok.Add(stations_1[4], 30);
            sroda_wask_norm_t_ok.Add(stations_1[5], 40);
            sroda_wask_norm_t_ok.Add(stations_1[6], 40);
            sroda_wask_norm_t_ok.Add(stations_1[7], 40);    // T OK
            sroda_wask_norm_tp_ok.Add(stations_1[0], 20);
            sroda_wask_norm_tp_ok.Add(stations_1[2], 20);
            sroda_wask_norm_tp_ok.Add(stations_1[3], 40);
            sroda_wask_norm_tp_ok.Add(stations_1[4], 50);
            sroda_wask_norm_tp_ok.Add(stations_1[5], 50);
            sroda_wask_norm_tp_ok.Add(stations_1[6], 50);
            sroda_wask_norm_tp_ok.Add(stations_1[7], 50);
            // S³upia Wielka
            slupia_wielka_norm_t.Add(stations_1[0], 7);
            slupia_wielka_norm_t.Add(stations_1[1], 7);
            slupia_wielka_norm_t.Add(stations_1[3], 7);
            slupia_wielka_norm_t.Add(stations_1[4], 14);
            slupia_wielka_norm_t.Add(stations_1[5], 21);
            slupia_wielka_norm_t.Add(stations_1[6], 28);
            slupia_wielka_norm_t.Add(stations_1[7], 30);    // t
            slupia_wielka_norm_tp.Add(stations_1[0], 14);
            slupia_wielka_norm_tp.Add(stations_1[1], 14);
            slupia_wielka_norm_tp.Add(stations_1[3], 14);
            slupia_wielka_norm_tp.Add(stations_1[4], 28);
            slupia_wielka_norm_tp.Add(stations_1[5], 40);
            slupia_wielka_norm_tp.Add(stations_1[6], 40);
            slupia_wielka_norm_tp.Add(stations_1[7], 40);   // tp
            slupia_wielka_norm_t_ok.Add(stations_1[0], 10);
            slupia_wielka_norm_t_ok.Add(stations_1[1], 10);
            slupia_wielka_norm_t_ok.Add(stations_1[3], 10);
            slupia_wielka_norm_t_ok.Add(stations_1[4], 20);
            slupia_wielka_norm_t_ok.Add(stations_1[5], 30);
            slupia_wielka_norm_t_ok.Add(stations_1[6], 40);
            slupia_wielka_norm_t_ok.Add(stations_1[7], 40);   // t ok
            slupia_wielka_norm_tp_ok.Add(stations_1[0], 20);
            slupia_wielka_norm_tp_ok.Add(stations_1[1], 20);
            slupia_wielka_norm_tp_ok.Add(stations_1[3], 20);
            slupia_wielka_norm_tp_ok.Add(stations_1[4], 40);
            slupia_wielka_norm_tp_ok.Add(stations_1[5], 50);
            slupia_wielka_norm_tp_ok.Add(stations_1[6], 50);
            slupia_wielka_norm_tp_ok.Add(stations_1[7], 50);   // t ok
            //Annopole
            annopole_norm_t.Add(stations_1[0], 10);
            annopole_norm_t.Add(stations_1[1], 10);
            annopole_norm_t.Add(stations_1[2], 7);
            annopole_norm_t.Add(stations_1[4], 7);
            annopole_norm_t.Add(stations_1[5], 14);
            annopole_norm_t.Add(stations_1[6], 21);
            annopole_norm_t.Add(stations_1[7], 23);    // t
            annopole_norm_tp.Add(stations_1[0], 20);
            annopole_norm_tp.Add(stations_1[1], 20);
            annopole_norm_tp.Add(stations_1[2], 14);
            annopole_norm_tp.Add(stations_1[4], 14);
            annopole_norm_tp.Add(stations_1[5], 28);
            annopole_norm_tp.Add(stations_1[6], 40);
            annopole_norm_tp.Add(stations_1[7], 30);    // tp
            annopole_norm_t_ok.Add(stations_1[0], 20);
            annopole_norm_t_ok.Add(stations_1[1], 20);
            annopole_norm_t_ok.Add(stations_1[2], 10);
            annopole_norm_t_ok.Add(stations_1[4], 10);
            annopole_norm_t_ok.Add(stations_1[5], 20);
            annopole_norm_t_ok.Add(stations_1[6], 30);
            annopole_norm_t_ok.Add(stations_1[7], 40);    // t ok
            annopole_norm_tp_ok.Add(stations_1[0], 40);
            annopole_norm_tp_ok.Add(stations_1[1], 40);
            annopole_norm_tp_ok.Add(stations_1[2], 20);
            annopole_norm_tp_ok.Add(stations_1[4], 20);
            annopole_norm_tp_ok.Add(stations_1[5], 40);
            annopole_norm_tp_ok.Add(stations_1[6], 50);
            annopole_norm_tp_ok.Add(stations_1[7], 50);    // tp ok
            //P³aczki
            placzki_norm_t.Add(stations_1[0], 21);
            placzki_norm_t.Add(stations_1[1], 21);
            placzki_norm_t.Add(stations_1[2], 14);
            placzki_norm_t.Add(stations_1[3], 7);
            placzki_norm_t.Add(stations_1[5], 7);
            placzki_norm_t.Add(stations_1[6], 14);
            placzki_norm_t.Add(stations_1[7], 21);    // t
            placzki_norm_tp.Add(stations_1[0], 40);
            placzki_norm_tp.Add(stations_1[1], 40);
            placzki_norm_tp.Add(stations_1[2], 28);
            placzki_norm_tp.Add(stations_1[3], 14);
            placzki_norm_tp.Add(stations_1[5], 14);
            placzki_norm_tp.Add(stations_1[6], 28);
            placzki_norm_tp.Add(stations_1[7], 40);    // tp
            placzki_norm_t_ok.Add(stations_1[0], 30);
            placzki_norm_t_ok.Add(stations_1[1], 30);
            placzki_norm_t_ok.Add(stations_1[2], 20);
            placzki_norm_t_ok.Add(stations_1[3], 10);
            placzki_norm_t_ok.Add(stations_1[5], 10);
            placzki_norm_t_ok.Add(stations_1[6], 20);
            placzki_norm_t_ok.Add(stations_1[7], 30);    // t ok
            placzki_norm_tp_ok.Add(stations_1[0], 50);
            placzki_norm_tp_ok.Add(stations_1[1], 50);
            placzki_norm_tp_ok.Add(stations_1[2], 40);
            placzki_norm_tp_ok.Add(stations_1[3], 20);
            placzki_norm_tp_ok.Add(stations_1[5], 20);
            placzki_norm_tp_ok.Add(stations_1[6], 40);
            placzki_norm_tp_ok.Add(stations_1[7], 50);    // tp ok
            //Œnieciska
            snieciska_norm_t.Add(stations_1[0], 28);
            snieciska_norm_t.Add(stations_1[1], 28);
            snieciska_norm_t.Add(stations_1[2], 21);
            snieciska_norm_t.Add(stations_1[3], 14);
            snieciska_norm_t.Add(stations_1[4], 7);
            snieciska_norm_t.Add(stations_1[6], 7);
            snieciska_norm_t.Add(stations_1[7], 14);    // t
            snieciska_norm_tp.Add(stations_1[0], 40);
            snieciska_norm_tp.Add(stations_1[1], 40);
            snieciska_norm_tp.Add(stations_1[2], 40);
            snieciska_norm_tp.Add(stations_1[3], 28);
            snieciska_norm_tp.Add(stations_1[4], 14);
            snieciska_norm_tp.Add(stations_1[6], 14);
            snieciska_norm_tp.Add(stations_1[7], 28);    // tp
            snieciska_norm_t_ok.Add(stations_1[0], 40);
            snieciska_norm_t_ok.Add(stations_1[1], 40);
            snieciska_norm_t_ok.Add(stations_1[2], 30);
            snieciska_norm_t_ok.Add(stations_1[3], 20);
            snieciska_norm_t_ok.Add(stations_1[4], 10);
            snieciska_norm_t_ok.Add(stations_1[6], 10);
            snieciska_norm_t_ok.Add(stations_1[7], 20);    // t ok
            snieciska_norm_tp_ok.Add(stations_1[0], 50);
            snieciska_norm_tp_ok.Add(stations_1[1], 50);
            snieciska_norm_tp_ok.Add(stations_1[2], 50);
            snieciska_norm_tp_ok.Add(stations_1[3], 40);
            snieciska_norm_tp_ok.Add(stations_1[4], 20);
            snieciska_norm_tp_ok.Add(stations_1[6], 20);
            snieciska_norm_tp_ok.Add(stations_1[7], 40);    // tp ok
            //Polwica
            polwica_norm_t.Add(stations_1[0], 30);
            polwica_norm_t.Add(stations_1[1], 30);
            polwica_norm_t.Add(stations_1[2], 28);
            polwica_norm_t.Add(stations_1[3], 21);
            polwica_norm_t.Add(stations_1[4], 14);
            polwica_norm_t.Add(stations_1[5], 7);
            polwica_norm_t.Add(stations_1[7], 7);    // t
            polwica_norm_tp.Add(stations_1[0], 40);
            polwica_norm_tp.Add(stations_1[1], 40);
            polwica_norm_tp.Add(stations_1[2], 40);
            polwica_norm_tp.Add(stations_1[3], 40);
            polwica_norm_tp.Add(stations_1[4], 28);
            polwica_norm_tp.Add(stations_1[5], 14);
            polwica_norm_tp.Add(stations_1[7], 14);    // tp
            polwica_norm_t_ok.Add(stations_1[0], 40);
            polwica_norm_t_ok.Add(stations_1[1], 40);
            polwica_norm_t_ok.Add(stations_1[2], 40);
            polwica_norm_t_ok.Add(stations_1[3], 30);
            polwica_norm_t_ok.Add(stations_1[4], 20);
            polwica_norm_t_ok.Add(stations_1[5], 10);
            polwica_norm_t_ok.Add(stations_1[7], 20);    // t ok
            polwica_norm_tp_ok.Add(stations_1[0], 50);
            polwica_norm_tp_ok.Add(stations_1[1], 50);
            polwica_norm_tp_ok.Add(stations_1[2], 50);
            polwica_norm_tp_ok.Add(stations_1[3], 50);
            polwica_norm_tp_ok.Add(stations_1[4], 40);
            polwica_norm_tp_ok.Add(stations_1[5], 20);
            polwica_norm_tp_ok.Add(stations_1[7], 20);    // tp ok
            //Zaniemyœl
            zaniemysl_norm_t.Add(stations_1[0], 30);
            zaniemysl_norm_t.Add(stations_1[1], 30);
            zaniemysl_norm_t.Add(stations_1[2], 30);
            zaniemysl_norm_t.Add(stations_1[3], 23);
            zaniemysl_norm_t.Add(stations_1[4], 21);
            zaniemysl_norm_t.Add(stations_1[5], 14);
            zaniemysl_norm_t.Add(stations_1[6], 7);    // t
            zaniemysl_norm_tp.Add(stations_1[0], 40);
            zaniemysl_norm_tp.Add(stations_1[1], 40);
            zaniemysl_norm_tp.Add(stations_1[2], 40);
            zaniemysl_norm_tp.Add(stations_1[3], 30);
            zaniemysl_norm_tp.Add(stations_1[4], 40);
            zaniemysl_norm_tp.Add(stations_1[5], 28);
            zaniemysl_norm_tp.Add(stations_1[6], 14);    // tp
            zaniemysl_norm_t_ok.Add(stations_1[0], 40);
            zaniemysl_norm_t_ok.Add(stations_1[1], 40);
            zaniemysl_norm_t_ok.Add(stations_1[2], 40);
            zaniemysl_norm_t_ok.Add(stations_1[3], 40);
            zaniemysl_norm_t_ok.Add(stations_1[4], 30);
            zaniemysl_norm_t_ok.Add(stations_1[5], 20);
            zaniemysl_norm_t_ok.Add(stations_1[6], 10);    // t ok
            zaniemysl_norm_tp_ok.Add(stations_1[0], 50);
            zaniemysl_norm_tp_ok.Add(stations_1[1], 50);
            zaniemysl_norm_tp_ok.Add(stations_1[2], 50);
            zaniemysl_norm_tp_ok.Add(stations_1[3], 50);
            zaniemysl_norm_tp_ok.Add(stations_1[4], 50);
            zaniemysl_norm_tp_ok.Add(stations_1[5], 40);
            zaniemysl_norm_tp_ok.Add(stations_1[6], 20);    // t ok

            double ticket;
            double ticket_prize = 100;  // if ticket prize is not declared

            if (type == "monthly")
            {
                ticket_prize = 200;

            }
            else if (type == "bike")
            {
                ticket_prize = 5;

            }
            else if(relief_size == -1)  // family
            {
                if (!is_special)
                {
                    if (type == "T")
                    {
                        
                        ticket_prize = 47.20;
                    }
                    else if (type == "TP")
                    {
                        ticket_prize = 64.60;
                    }

                }
                else
                {
                    if (type == "T")
                    {
                        ticket_prize = 64.60;
                    }
                    else if (type == "TP")
                    {
                        ticket_prize = 77;
                    }
                }
            }   // end family
            else if(station_start == stations_1[0])
            {
                    if (type == "T" && !is_special)
                    {
                        ticket_prize = sroda_miasto_norm_t[station_destination];
                    }
                    else if (type == "T" && is_special)
                    {
                        ticket_prize = sroda_miasto_norm_t_ok[station_destination];
                    }
                    else if (type == "TP" && !is_special)
                    {
                        ticket_prize = sroda_miasto_norm_tp[station_destination];
                    }
                    else if (type == "TP" && is_special)
                    {
                        ticket_prize = sroda_miasto_norm_tp_ok[station_destination];
                    }
            }
            else if(station_start == stations_1[1]) // wask
            {
                   if (type == "T" && !is_special)
                   {
                       ticket_prize = sroda_wask_norm_t[station_destination];
                   }
                   else if (type == "T" && is_special)
                   {
                       ticket_prize = sroda_wask_norm_t_ok[station_destination];
                   }
                   else if (type == "TP" && !is_special)
                   {
                       ticket_prize = sroda_wask_norm_tp[station_destination];
                   }
                   else if (type == "TP" && is_special)
                   {
                       ticket_prize = sroda_wask_norm_tp_ok[station_destination];
                   }
            }
            else if(station_start == stations_1[2])  // slupia
            {
               if (type == "T" && !is_special)
               {
                   ticket_prize = slupia_wielka_norm_t[station_destination];
               }
               else if (type == "T" && is_special)
               {
                   ticket_prize = slupia_wielka_norm_t_ok[station_destination];
               }
               else if (type == "TP" && !is_special)
               {
                   ticket_prize = slupia_wielka_norm_tp[station_destination];
               }
               else if (type == "TP" && is_special)
               {
                   ticket_prize = slupia_wielka_norm_tp_ok[station_destination];
               }
            }
            else if(station_start == stations_1[3])  // annopole
            {
               if (type == "T" && !is_special)
               {
                   ticket_prize = annopole_norm_t[station_destination];
               }
               else if (type == "T" && is_special)
               {
                   ticket_prize = annopole_norm_t_ok[station_destination];
               }
               else if (type == "TP" && !is_special)
               {
                   ticket_prize = annopole_norm_tp[station_destination];
               }
               else if (type == "TP" && is_special)
               {
                   ticket_prize = annopole_norm_tp_ok[station_destination];
               }
            }
            else if(station_start == stations_1[4])  // placzki
            {
               if (type == "T" && !is_special)
               {
                   ticket_prize = placzki_norm_t[station_destination];
               }
               else if (type == "T" && is_special)
               {
                   ticket_prize = placzki_norm_t_ok[station_destination];
               }
               else if (type == "TP" && !is_special)
               {
                   ticket_prize = placzki_norm_tp[station_destination];
               }
               else if (type == "TP" && is_special)
               {
                   ticket_prize = placzki_norm_tp_ok[station_destination];
               }
            }
            else if (station_start == stations_1[5])  // œnieciska
            {
               if (type == "T" && !is_special)
               {
                   ticket_prize = snieciska_norm_t[station_destination];
               }
               else if (type == "T" && is_special)
               {
                   ticket_prize = snieciska_norm_t_ok[station_destination];
               }
               else if (type == "TP" && !is_special)
               {
                   ticket_prize = snieciska_norm_tp[station_destination];
               }
               else if (type == "TP" && is_special)
               {
                   ticket_prize = snieciska_norm_tp_ok[station_destination];
               }
            }
            else if(station_start == stations_1[6])  // polwica
            {
               if (type == "T" && !is_special)
               {
                   ticket_prize = polwica_norm_t[station_destination];
               }
               else if (type == "T" && is_special)
               {
                   ticket_prize = polwica_norm_t_ok[station_destination];
               }
               else if (type == "TP" && !is_special)
               {
                   ticket_prize = polwica_norm_tp[station_destination];
               }
               else if (type == "TP" && is_special)
               {
                   ticket_prize = polwica_norm_tp_ok[station_destination];
               }
            }
            else if(station_start == stations_1[7])  // zaniemyœl
            {
               if (type == "T" && !is_special)
               {
                   ticket_prize = zaniemysl_norm_t[station_destination];
               }
               else if (type == "T" && is_special)
               {
                   ticket_prize = zaniemysl_norm_t_ok[station_destination];
               }
               else if (type == "TP" && !is_special)
               {
                   ticket_prize = zaniemysl_norm_tp[station_destination];
               }
               else if (type == "TP" && is_special)
               {
                   ticket_prize = zaniemysl_norm_tp_ok[station_destination];
               }
            }

            if (relief_size > 0)
            {
                normal_ticket_prize = ticket_prize;
                ticket = ticket_prize - (ticket_prize * (relief_size * 0.01));
                relief_money = ticket_prize * (relief_size * 0.01);
                return ticket;
            }
            else if (relief_size == -1) // (-1) family ticket!
            {
                return ticket_prize;
            }
            else // normal 
            {
                normal_ticket_prize = ticket_prize;
                return ticket_prize;
            }

        }

        // credentials
        private void credentials_return_all_Click(object sender, EventArgs e)
        {
            this.panel9.Visible = true;
            this.panel9.Focus();
            this.panel5.Visible = false;
            this.textBox9.Visible = true;
            this.comboBox3.Visible = true;
            was_printed = false;
            credentials_type = 1;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "bilet calkowicie niewykorzystany");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void credentials_return_part_Click(object sender, EventArgs e)
        {
            this.panel9.Visible = true;
            this.panel9.Focus();
            this.panel5.Visible = false;
            this.comboBox3.Visible = false;
            this.credentials_first.Text = "niewykorzystany przez osób:";
            this.panel10.Visible = true;
            was_printed = false;
            credentials_type = 2;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "bilet czesciowo niewykorzystany");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void credentials_late_Click(object sender, EventArgs e)
        {
            this.panel9.Visible = true;
            was_printed = false;
            this.panel9.Focus();
            this.panel5.Visible = false;
            this.credentials_first.Visible = false;
            this.panel_credentials.Visible = true;
            this.label71.Visible = true;
            this.textBox10.Visible = true;
            this.comboBox3.Visible = false;
            this.textBox7.Visible = false;
            this.label64.Text = "Poci¹g nr:";
            credentials_type = 3;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "pociag opozniony");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) // to credentials 
        {
            if (comboBox3.SelectedItem.ToString() == "Wydano nowy bilet numer:")
            {
                this.panel_credentials.Visible = true;
            }
            else
            {
                this.panel_credentials.Visible = false;
            }
        }
        private void credentials_rebuild(object sender, EventArgs e) // activate on focus at credentials
        {
            this.credentials_data.Text = DateTime.Now.ToString("dd-MM-yyyy");
            this.textBox7.Text = TM_SERIES;
            this.textBox8.Text = "";
            this.textBox9.Text = TM_SERIES;
            this.textBox10.Text = "";
            this.panel_credentials.Visible = false;
            this.credentials_first.Visible = true;
            this.textBox9.Visible = false;
            this.label71.Visible = false;
            this.panel10.Visible = false;
            this.credentials_numeric_norm.Value = 0;
            this.credentials_numeric_relief.Value = 0;
            this.credentials_numeric_family.Value = 0;
            this.credentials_numeric_bike.Value = 0;
            this.panel_credentials.Focus();
        }

        private void opt_prices_Click(object sender, EventArgs e) // price list
        {
            //MessageBox.Show("Brak mo¿liwoœci wydrukowania cenników.", "Us³uga wy³¹czona.");
            was_printed = false;
            
            this.panel4.Visible = false;
            this.panel14.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cennik");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void opt_return_ticket_Click(object sender, EventArgs e) // get back ticket
        {
            //MessageBox.Show("Brak mo¿liwoœci anulowania biletu", "Us³uga wy³¹czona.");    // OK
            this.panel11.Visible = true;
            this.panel4.Visible = false;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "anulowanie biletu");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button48_Click(object sender, EventArgs e) // cancelled get back
        {
            this.panel11.Visible = false;
            this.panel4.Visible = true;
            this.ticket_to_cancel.Text = "";
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "anulowanie biletu - cofnij");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button50_Click(object sender, EventArgs e) // get back
        {
            this.panel12.Visible = false;
            this.panel4.Visible = true;
            this.ticket_to_cancel.Text = "";
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "anulowanie biletu - cofnij anulowanie potwierdzonego");
            saveToFile(CSV_filename_logs, temp_string, false);
        }        
        private void button47_Click(object sender, EventArgs e) //cancelling - submited nr ticket - get back
        {
            string info;
            string type;
            canceled_ticket_info_line = ticket_cancel_read_from_series(CSV_filename_tickets_series, CSV_delimiter, int.Parse(ticket_to_cancel.Text), ref cancel_ticket_date, ref cancel_value);
            try
            {
                DateTime earlier_datatime = DateTime.Now.AddMinutes(-5);
                if (earlier_datatime.CompareTo(cancel_ticket_date) == -1)
                {
                    if (!canceled_tickets.Contains(int.Parse(ticket_to_cancel.Text)))
                    {
                        this.panel11.Visible = false;
                        this.panel12.Visible = true;
                        this.cancel_ticket_nr.Text = ticket_to_cancel.Text;
                        this.cancel_ticket_cash.Text = String.Format("{0:0.00}", cancel_value)+ "PLN";
                        info = "potwierdzony";
                        type = "info";
                    }
                    else
                    {
                        info = "Ten bilet by³ ju¿ zwrócony";
                        type = "warning";
                        MessageBox.Show(info);
                    }
                }
                else if (earlier_datatime.CompareTo(cancel_ticket_date) == 1)
                {
                    type = "warning";
                    info = "Czas na zwrot tego biletu min¹³.";
                    MessageBox.Show(info , "B³¹d");
                }
                else
                {
                    type = "warning";
                    info = "B³¹d odczytu plików";
                    MessageBox.Show(info, "B³¹d");
                }
            }
            catch (Exception d)
            {
                MessageBox.Show(d.Message);
                type = "error";
                info = d.Message;
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "anulowanie biletu - " + info);
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button49_Click(object sender, EventArgs e) // cancel ticket and save to file
        {
            string type = "warning";
            string info;
            try
            {
                canceled_ticket_number = TM_SERIES + ticket_to_cancel.Text;
                canceled_tickets.Add(int.Parse(ticket_to_cancel.Text));
                if (!saveToFile(CSV_filename_canceled_SD, canceled_ticket_info_line, false))
                {
                    info = "B³¹d zapisu!";
                    MessageBox.Show(info +"\n"+ CSV_filename_canceled_SD);
                }
                if (!saveToFile(cancel_filename_flash, canceled_ticket_info_line, false))
                {
                    info = "B³¹d zapisu!";
                    MessageBox.Show(info, "B³¹d I/O");
                }
                else
                {
                    if (!debug)
                    {
                        print_canceled_ticket();
                        info = "Anulowano bilet!";
                        type = "info";
                    }
                    else
                    {
                        MessageBox.Show("Anulowano bilet: " + canceled_ticket_number + "\nkwota: " + cancel_value.ToString());
                    }
                    this.panel12.Visible = false;
                    this.panel3.Visible = true;
                    
                }
                string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "anulowanie biletu - cofnij");
                saveToFile(CSV_filename_logs, temp_string, false);
            }
            catch (Exception d)
            {
                MessageBox.Show(d.Message);
            }

        }

        private void close_selling(object sender, EventArgs e) // go to close selling
        {
            bool status;
            clear();
            status = look_for_tickets_in_series(CSV_filename_tickets_month, CSV_delimiter, counter_series); //if ceries was open
            if (status)
            {
                status = gen_selling_from_sold(CSV_filename_tickets_series, CSV_delimiter, ref ticket_cash_people, ref ticket_cash_bike, ref ticket_cash_extra);
                if ((File.Exists(CSV_filename_canceled_SD)) && status)  // jesli nie by³o zwrotów to nie ma pliku anulowane
                {
                    status = gen_selling_from_sold(CSV_filename_canceled_SD, CSV_delimiter, ref cancel_ticket_cash_people, ref cancel_ticket_cash_bike, ref cancel_ticket_cash_extra);
                }
            }
            
            earn_summary = ticket_cash_people + ticket_cash_bike + ticket_cash_extra;
            return_summary = cancel_ticket_cash_bike + cancel_ticket_cash_people + cancel_ticket_cash_extra;

            this.panel4.Visible = false;
            this.panel13.Visible = true;
            double summing = earn_summary - return_summary;
            this.summing_cash.Text = String.Format("{0:0.00}", summing) + " z³";
            this.summing_to_return.Text = String.Format("{0:0.00}", summing) + " z³";
            if (status)
            {
                this.button51.Enabled = true;
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "zamknij zmiane");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button51_Click(object sender, EventArgs e) //confirm cashless and close selling
        {
            get_ticket_numbers(CSV_filename_tickets_series, CSV_delimiter, ref ticket_first, ref ticket_last);
            double summing_calculated =0;

            try
            {
                if (summing_cash_input.Text != "")
                {
                    ticket_cashless_pay = double.Parse(summing_cash_input.Text, CultureInfo.InvariantCulture);//.Replace('.', ','));  // <<< this replace was the main issue
                }
                summing_calculated = earn_summary - return_summary - ticket_cashless_pay;
            }
            catch (Exception ej) // if entered value is wrong
            {
                MessageBox.Show("Wprowadzono znaki nie bêd¹ce liczb¹!\n\nkarta:" + ticket_cashless_pay + "obliczone: " + summing_calculated + "\n\n" + ej, "B£¥D");
                string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "warning", curr_user, counter_series.ToString(), "Zakoncz zamkniecie zmiany - znaki nie bêd¹ce liczba");
                saveToFile(CSV_filename_logs, tempstring, false);
                return;
            }
            /*
            if (summing_calculated < 0)
            {
                MessageBox.Show("Kwota p³atnoœci kart¹ nie mo¿e byæ wiêksza ni¿ zysk z biletów!", "Z³a kwota");
                return;
            }
            else connect it to else if*/ 
            if (ticket_last == 0 && summing_cash_input.Text != "")  // zmiana zamknieta bez sprzedanego biletu = brak wydruku
            {
                bool_open_cash = false;
                string temp_string;
                temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter_series.ToString(), curr_user, "close", ticket_cashless_pay.ToString());
                saveToFile(CSV_filename_series_logs, temp_string, false);
                counter_series += 1;
                temp_string = vars_ToCSV(CSV_delimiter, counter_tickets.ToString(), counter_series.ToString(), bool_open_cash.ToString());
                if (!saveToFile(CSV_filename_counters, temp_string, true))
                {
                    MessageBox.Show("B³¹d zapisu!", "B³¹d I/O");
                }
                this.panel4.Visible = false;
                this.panel13.Visible = false;
                this.panel1.Visible = true;
                user_name.Focus();
                this.curr_user = "";
                this.choosen_start = "";
                this.choosen_destination = "";
                summing_cash_input.Text = "";
                clear_var_after_close();
            }
            else if (ticket_last != 0 && summing_cash_input.Text != "")
            {
                bool_open_cash = false;
                string temp_string;
                temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter_series.ToString(), curr_user, "close", ticket_cashless_pay.ToString());
                saveToFile(CSV_filename_series_logs, temp_string, false);
                bool get_gate = dates_read_from_series(CSV_filename_series_logs, CSV_delimiter, counter_series, ref data_series_start, ref data_series_stop);
                counter_series += 1;
                temp_string = vars_ToCSV(CSV_delimiter, counter_tickets.ToString(), counter_series.ToString(), bool_open_cash.ToString());
                if (!saveToFile(CSV_filename_counters, temp_string, true))
                {
                    MessageBox.Show("B³¹d zapisu!\nZmiana nie moze zostaæ poprawnie zamkniêta.", "B³¹d I/O");
                    string tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "warning", curr_user, counter_series.ToString(), "Zakoncz zamkniecie zmiany - blad zapisu counters");
                    saveToFile(CSV_filename_logs, tempstring, false);
                    counter_series -= 1;    //if cannot save dont close
                    return;
                }
                this.panel4.Visible = false;
                this.panel13.Visible = false;
                this.panel1.Visible = true;
                user_name.Focus();
                print_close_selling();
                string tempstring1 = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "Zakoncz zamkniecie zmiany - zakonczono");
                saveToFile(CSV_filename_logs, tempstring1, false);
                clear_var_after_close();
            }
            else if (summing_cash_input.Text == "")
            { 
                MessageBox.Show("WprowadŸ kwotê p³atnoœci kart¹","Brak podanej kwoty");
                string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "warning", curr_user, counter_series.ToString(), "Zakoncz zamkniecie zmiany - nie wprowadzono kwoty");
                saveToFile(CSV_filename_logs, temp_string, false);
            }            
        }
        private void money_entered(object sender, EventArgs e)
        {
            double summing = 0;
            if (summing_cash_input.Text != "")
            {
                try
                {
                    double cash_entered = double.Parse(summing_cash_input.Text, CultureInfo.InvariantCulture);//.Replace('.', ','));
                    summing = earn_summary - return_summary - cash_entered;
                    this.summing_to_return.Text = String.Format("{0:0.00}", summing) + " z³";
                    //MessageBox.Show(summing+"\n"+earn_summary+"\n"+return_summary, "\n"+cash_entered);
                }
                catch (Exception) // if entered value is wrong
                {
                    MessageBox.Show("Wprowadzono znaki nie bêd¹ce liczb¹!", "B£¥D");
                }
            }
            else
            {
                summing = earn_summary - return_summary;
                this.summing_to_return.Text = summing_cash.Text;
            }
            if (summing < 0)
            {
                this.summing_to_return.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                this.summing_to_return.ForeColor = System.Drawing.Color.Black;
            }
        }
        private void button52_Click(object sender, EventArgs e) // back from ending current session
        {
            ticket_cashless_pay = 0;
            this.summing_cash_input.Text = "";

            this.panel13.Visible = false;
            this.panel4.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - zamkniecie zmiany");
            saveToFile(CSV_filename_logs, temp_string, false);

        }
        //price list
        private void button53_Click(object sender, EventArgs e) // price list type normal train
        {
            bool_is_special = false;
            this.panel14.Visible = false;
            this.panel15.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "poc O - cennik");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button54_Click(object sender, EventArgs e) // price list type special train
        {
            bool_is_special = true;
            this.panel14.Visible = false;
            this.panel15.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "poc OK - cennik");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button55_Click(object sender, EventArgs e) //price list monthly
        {
            int is_ok;
            string info;
            string tempstring;
            is_ok = print_from_bitmap(BMP_filename_ticket_monthly);            
            if (is_ok > 0)
            {
                this.panel14.Visible = false;
                this.panel4.Visible = true;
                tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "miesieczny - cennik wydrukowany");
            }
            else if (is_ok == -3)
            {
                info = "Nie znaleziono pliku BMP z cennikiem!";
                tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "error", curr_user, counter_series.ToString(), "miesieczny - cennik " + info);
                MessageBox.Show(info);
            }
            else
            {
                info = "Nie uda³o siê wydrukowaæ!";
                tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "error", curr_user, counter_series.ToString(), "miesieczny - cennik " + info);
                MessageBox.Show(info);
            }
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        private void button56_Click(object sender, EventArgs e) //price list bike
        {
            int is_ok;
            string info = "";
            string tempstring;
            string type = "warning";
            is_ok = print_from_bitmap(BMP_filename_ticket_bike);
            if (is_ok > 0)
            {
                this.panel14.Visible = false;
                this.panel4.Visible = true;
                type = "info";
            }
            else if (is_ok == -3)
            {
                info = "Nie znaleziono pliku BMP z cennikiem!";
                MessageBox.Show(info);
            }
            else
            {
                info = "Nie uda³o siê wydrukowaæ!";
                MessageBox.Show(info);
            }
            tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "rower - cennik " + info);
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        private void button57_Click(object sender, EventArgs e) //exit from price list
        {
            //clear();
            was_printed = true; // disable posibility of printing
            this.panel14.Visible = false;
            this.panel4.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - cenniki");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button62_Click(object sender, EventArgs e) // back from type train to prices list
        {
            //clear();
            this.panel15.Visible = false;
            this.panel14.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij wybor o/ok- cennik");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button58_Click(object sender, EventArgs e) // single O & OK
        {
            int is_ok;
            string info;
            string temp_string;
            if (bool_is_special)    // single OK
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_single_OK);
            }
            else
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_single_O);
            }
            temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "jednorazowy - cennik");
            if (is_ok > 0)
            {
                this.panel15.Visible = false;
                this.panel4.Visible = true;
            }
            else if (is_ok == -3)
            {
                info = "Nie znaleziono pliku BMP z cennikiem!";
                MessageBox.Show(info);
                temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "error", curr_user, counter_series.ToString(), "jednorazowy - cennik " + info);
            }
            else
            {
                info = "Nie uda³o siê wydrukowaæ!";
                MessageBox.Show(info);
                temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "error", curr_user, counter_series.ToString(), "jednorazowy - cennik " + info);
            }
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button59_Click(object sender, EventArgs e) // line OK
        {
            int is_ok;
            string info = "";
            string type = "info";
            string tempstring;
            if (bool_is_special)    // line OK
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_single_line_OK);
            }
            else
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_single_line_O);
            }

            tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "odcinkowy - cennik");
            if (is_ok > 0)
            {
                this.panel15.Visible = false;
                this.panel4.Visible = true;
            }
            else
            {
                info = "Nie uda³o siê wydrukowaæ!";
                type = "warning";
                MessageBox.Show(info);
                
            }
            tempstring = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), type, curr_user, counter_series.ToString(), "odcinkowy - cennik " + info);
            saveToFile(CSV_filename_logs, tempstring, false);
        }
        private void button60_Click(object sender, EventArgs e) // family
        {
            int is_ok;
            string info;
            if (bool_is_special)    // line OK
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_family_OK);
            }
            else
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_family_O);
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "rodzinny - cennik");
            if (is_ok > 0)
            {
                this.panel15.Visible = false;
                this.panel4.Visible = true;
            }
            else
            {
                info = "Nie uda³o siê wydrukowaæ!";
                MessageBox.Show(info);
            }
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button61_Click(object sender, EventArgs e) //taniogzubowy
        {
            int is_ok;
            string info;
            if (bool_is_special)    // line OK
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_special_OK);
            }
            else
            {
                is_ok = print_from_bitmap(BMP_filename_ticket_special_O);
            }
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "taniogzubowy - cennik");
            if (is_ok > 0)
            {
                this.panel15.Visible = false;
                this.panel4.Visible = true;
            }
            else
            {
                info = "Nie uda³o siê wydrukowaæ!";
                MessageBox.Show(info);
            }
            saveToFile(CSV_filename_logs, temp_string, false);
        }

        private void confirm_credentials_Click(object sender, EventArgs e)
        {
            print_credentials(credentials_type);
            this.panel9.Visible = false;
            this.panel3.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "wydruk poswiadczenia");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
        private void button63_Click(object sender, EventArgs e)
        {
            this.panel9.Visible = false;
            this.panel5.Visible = true;
            string temp_string = vars_ToCSV(CSV_delimiter, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "info", curr_user, counter_series.ToString(), "cofnij - poswiadczenie");
            saveToFile(CSV_filename_logs, temp_string, false);
        }
    }

    public sealed class BatteryInfo
    {
        public enum BatteryChemistry : byte
        {
            BATTERY_CHEMISTRY_ALKALINE = 0x01,  // Alkaline battery.
            BATTERY_CHEMISTRY_NICD = 0x02, // Nickel Cadmium battery.
            BATTERY_CHEMISTRY_NIMH = 0x03, // Nickel Metal Hydride battery.
            BATTERY_CHEMISTRY_LION = 0x04, // Lithium Ion battery.
            BATTERY_CHEMISTRY_LIPOLY = 0x05, // Lithium Polymer battery.
            BATTERY_CHEMISTRY_ZINCAIR = 0x06, // Zinc Air battery.
            BATTERY_CHEMISTRY_UNKNOWN = 0xFF // Battery chemistry is unknown.
        }

        public enum ACLineStatus : byte
        {
            AC_LINE_OFFLINE = 0, // Offline
            AC_LINE_ONLINE = 1, // Online
            AC_LINE_BACKUP_POWER = 2, // Backup Power
            AC_LINE_UNKNOWN = 0xFF, //
            Unknown = 0xFF, //status
        }

        private class SYSTEM_POWER_STATUS_EX2
        {
            //AC power status. 
            public ACLineStatus ACLineStatus;
            //Battery charge status
            public BatteryFlag BatteryFlag;
            // Percentage of full battery charge remaining. Must be in 
            // the range 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN if 
            // percentage of battery life remaining is unknown
            public byte BatteryLifePercent;
            byte Reserved1;
            //Percentage of full battery charge remaining. Must be 
            // in the range 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN 
            // if percentage of battery life remaining is unknown. 
            public int BatteryLifeTime;
            // Number of seconds of battery life when at full charge, 
            // or BATTERY_LIFE_UNKNOWN if full lifetime of battery is unknown
            public int BatteryFullLifeTime;
            byte Reserved2;
            // Backup battery charge status.
            public BatteryFlag BackupBatteryFlag;
            // Percentage of full backup battery charge remaining. Must be in 
            // the range 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN if percentage 
            // of backup battery life remaining is unknown. 

            public byte BackupBatteryLifePercent;
            byte Reserved3;
            // Number of seconds of backup battery life when at full charge, or 
            // BATTERY_LIFE_UNKNOWN if number of seconds of backup battery life 
            // remaining is unknown. 
            public int BackupBatteryLifeTime;
            // Number of seconds of backup battery life when at full charge, or 
            // BATTERY_LIFE_UNKNOWN if full lifetime of backup battery is unknown
            public int BackupBatteryFullLifeTime;
            // Number of millivolts (mV) of battery voltage. It can range from 0 
            // to 65535
            public int BatteryVoltage;
            // Number of milliamps (mA) of instantaneous current drain. It can 
            // range from 0 to 32767 for charge and 0 to –32768 for discharge. 
            public int BatteryCurrent;
            //Number of milliseconds (mS) that is the time constant interval 
            // used in reporting BatteryAverageCurrent. 
            public int BatteryAverageCurrent;
            // Number of milliseconds (mS) that is the time constant interval 
            // used in reporting BatteryAverageCurrent. 

            public int BatteryAverageInterval;
            // Average number of milliamp hours (mAh) of long-term cumulative 
            // average discharge. It can range from 0 to –32768. This value is 
            // reset when the batteries are charged or changed. 

            public int BatterymAHourConsumed;
            // Battery temperature reported in 0.1 degree Celsius increments. It 
            // can range from –3276.8 to 3276.7. 
            public int BatteryTemperature;
            // Number of millivolts (mV) of backup battery voltage. It can range 
            // from 0 to 65535.
            public int BackupBatteryVoltage;
            // Type of battery.
            public BatteryChemistry BatteryChemistry;
            //  Add any extra information after the BatteryChemistry member.
        }

        private enum BatteryFlag : byte
        {
            BATTERY_FLAG_HIGH = 0x01,
            BATTERY_FLAG_CRITICAL = 0x04,
            BATTERY_FLAG_CHARGING = 0x08,
            BATTERY_FLAG_NO_BATTERY = 0x80,
            BATTERY_FLAG_UNKNOWN = 0xFF,
            BATTERY_FLAG_LOW = 0x02
        }

        public enum BatteryLevel
        {
            Low,
            Medium,
            High,
            VeryHigh
        };

        [DllImport("CoreDLL")]
        private static extern int GetSystemPowerStatusEx2(SYSTEM_POWER_STATUS_EX2 statusInfo, int length, int getLatest);


        public static BatteryLevel GetSystemPowerStatus()
        {
            SYSTEM_POWER_STATUS_EX2 retVal = new SYSTEM_POWER_STATUS_EX2();
            int result = GetSystemPowerStatusEx2(retVal, Marshal.SizeOf(retVal), 0);
            switch (retVal.BatteryFlag)
            {
                case BatteryFlag.BATTERY_FLAG_CRITICAL:
                    return BatteryLevel.Medium;
                case BatteryFlag.BATTERY_FLAG_LOW:
                    return BatteryLevel.High;
                case BatteryFlag.BATTERY_FLAG_HIGH:
                    return BatteryLevel.VeryHigh;
                default:
                    return BatteryLevel.Low;
            }
        }

        public static Byte GetBatteryLifePercent()
        {
            SYSTEM_POWER_STATUS_EX2 retVal = new SYSTEM_POWER_STATUS_EX2();
            int result = GetSystemPowerStatusEx2(retVal, Marshal.SizeOf(retVal), 0);
            return retVal.BatteryLifePercent;
        }
    }
}