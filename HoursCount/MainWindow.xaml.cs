using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Word = Microsoft.Office.Interop.Word;
using System.Diagnostics;
using System.Globalization;

namespace HoursCount
{
    public partial class MainWindow : Window
    {
        DataSet result = new DataSet();
        
        string repotsFolderPath = @"C:\Users\" + Environment.UserName.ToString() + @"\Desktop\reports";
        bool login = false;        
        public MainWindow()
        {
            InitializeComponent();

            loginType(login);

            if (!Directory.Exists(repotsFolderPath))
            {
                Directory.CreateDirectory(repotsFolderPath);
            }

            dgReportList.ItemsSource = new DirectoryInfo(repotsFolderPath).GetFiles();

            if (cbSpecificPumpChoose.IsChecked == true)
            {
                cbN1_1.IsEnabled = true;
                cbN1_2.IsEnabled = true;
                cbN2_1.IsEnabled = true;
                cbN2_2.IsEnabled = true;
                cbN2_3.IsEnabled = true;
                cbN3_1.IsEnabled = true;
                cbN3_2.IsEnabled = true;
                cbN3_3.IsEnabled = true;
                cbN4_1.IsEnabled = true;
                cbN4_2.IsEnabled = true;
                cbN4_3.IsEnabled = true;
                cbN5_1.IsEnabled = true;
                cbN5_2.IsEnabled = true;
            }
            else
            {
                cbN1_1.IsEnabled = false;
                cbN1_2.IsEnabled = false;
                cbN2_1.IsEnabled = false;
                cbN2_2.IsEnabled = false;
                cbN2_3.IsEnabled = false;
                cbN3_1.IsEnabled = false;
                cbN3_2.IsEnabled = false;
                cbN3_3.IsEnabled = false;
                cbN4_1.IsEnabled = false;
                cbN4_2.IsEnabled = false;
                cbN4_3.IsEnabled = false;
                cbN5_1.IsEnabled = false;
                cbN5_2.IsEnabled = false;
            }

            dtpFrom.SelectedDate = DateTime.Today.AddDays(-1); 
            dtpTo.SelectedDate = DateTime.Today;
            cbAll.IsChecked = true;
            refreshDataGrid(0);
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {            
            Application.Current.Shutdown();
        }  
        private string workingHoursCount(string dateFrom, string dateTo, bool[]array)
        {            
            bool periodFrom = false, periodTo = false;
            string[] pumpArray = { "H1/1", "H1/2", "H2/1", "H2/2", "H2/3", "H3/1", "H3/2", "H3/3", "H4/1", "H4/2", "H4/3", "H5/1", "H5/2" };
            string totalHours = ""; 
            
            for (int i = 0; i < pumpArray.Length; i++)
            {
                if (array[i] == true)
                {
                    double resultHours = 0.0;
                    int hrsFrom = 0, minFrom = 0, secFrom = 0, hrsTo = 0, minTo = 0, secTo = 0;                   

                    result = SQL_Connection.timeSelect(pumpArray[i].ToString(), "ASC", "BETWEEN '" + dateFrom + "' AND '" + dateTo);

                    if (result.Tables[0].Rows.Count != 0)
                    {
                        hrsFrom = Convert.ToInt32(result.Tables[0].Rows[0]["Часы"].ToString());
                        minFrom = Convert.ToInt32(result.Tables[0].Rows[0]["Минуты"].ToString());
                        secFrom = Convert.ToInt32(result.Tables[0].Rows[0]["Секунды"].ToString());
                    }
                    else
                    {
                        periodFrom = true;                        
                    }

                    result = SQL_Connection.timeSelect(pumpArray[i].ToString(), "DESC", "BETWEEN '" + dateFrom + "' AND '" + dateTo);

                    if (result.Tables[0].Rows.Count != 0)
                    {
                        hrsTo = Convert.ToInt32(result.Tables[0].Rows[0]["Часы"].ToString());
                        minTo = Convert.ToInt32(result.Tables[0].Rows[0]["Минуты"].ToString());
                        secTo = Convert.ToInt32(result.Tables[0].Rows[0]["Секунды"].ToString());
                    }
                    else
                    {
                        periodTo = true;                        
                    }

                    if (periodFrom && periodTo)
                    {
                        result = SQL_Connection.timeSelect(pumpArray[i].ToString(), "DESC", "< '" + dateTo + "");

                        if (result.Tables[0].Rows.Count != 0)
                        {
                            hrsTo = Convert.ToInt32(result.Tables[0].Rows[0]["Часы"].ToString());
                            minTo = Convert.ToInt32(result.Tables[0].Rows[0]["Минуты"].ToString());
                            secTo = Convert.ToInt32(result.Tables[0].Rows[0]["Секунды"].ToString());
                        }
                        else { }

                        periodFrom = false;
                        periodTo = false;
                    }

                    TimeSpan tsFrom = new TimeSpan(hrsFrom, minFrom, secFrom);
                    TimeSpan tsTo = new TimeSpan(hrsTo, minTo, secTo);
                    TimeSpan tsResult = new TimeSpan();

                    tsResult = tsFrom - tsTo;
                    resultHours = Math.Round(Convert.ToDouble(tsResult.TotalHours.ToString()) / (-1), 2);

                    totalHours = totalHours + " " + pumpArray[i].ToString() + " - " + resultHours.ToString() + "\r\n";
                }
            }
            return totalHours;
        }
        private string dateFormating(string date)
        {
            if (Convert.ToInt32(date) < 10)
                date = "0" + date;

            return date;
        }
        private async void btnReport_Click(object sender, RoutedEventArgs e)
        {
        
        
            string templatePath = @"C:\Users\" + Environment.UserName.ToString() + @"\Desktop\pumpHours.docx", dateFrom = "", dateTo = "", timeFrom = "", timeTo = "";


            bool[] pumpArray = { true, true, true, true, true, true, true, true, true, true, true, true, true };
            string dateFromYear, dateFromMonth, dateFromDay, dateToYear, dateToMonth, dateToDay;
            int reportType = 1;

            if (cbSpecificPumpChoose.IsChecked == true)
            {
                if (cbN1_1.IsChecked == true) { pumpArray[0] = true; } else { pumpArray[0] = false; }
                if (cbN1_2.IsChecked == true) { pumpArray[1] = true; } else { pumpArray[1] = false; }
                if (cbN2_1.IsChecked == true) { pumpArray[2] = true; } else { pumpArray[2] = false; }
                if (cbN2_2.IsChecked == true) { pumpArray[3] = true; } else { pumpArray[3] = false; }
                if (cbN2_3.IsChecked == true) { pumpArray[4] = true; } else { pumpArray[4] = false; }
                if (cbN3_1.IsChecked == true) { pumpArray[5] = true; } else { pumpArray[5] = false; }
                if (cbN3_2.IsChecked == true) { pumpArray[6] = true; } else { pumpArray[6] = false; }
                if (cbN3_3.IsChecked == true) { pumpArray[7] = true; } else { pumpArray[7] = false; }
                if (cbN4_1.IsChecked == true) { pumpArray[8] = true; } else { pumpArray[8] = false; }
                if (cbN4_2.IsChecked == true) { pumpArray[9] = true; } else { pumpArray[9] = false; }
                if (cbN4_3.IsChecked == true) { pumpArray[10] = true; } else { pumpArray[10] = false; }
                if (cbN5_1.IsChecked == true) { pumpArray[11] = true; } else { pumpArray[11] = false; }
                if (cbN5_2.IsChecked == true) { pumpArray[12] = true; } else { pumpArray[12] = false; }
            }           

            if (dtpFrom.SelectedDate.Value < dtpTo.SelectedDate.Value)
            {
                if (chbReportShift.IsChecked == true) { reportType = 2; }
                if (chbReportPrevShift.IsChecked == true) { reportType = 3; } 
                if (chbReportDay.IsChecked == true) { reportType = 4; }              
                if (chbReportPrevDay.IsChecked == true) { reportType = 5; }

                switch (reportType)
                {
                    case 1:
                        dateFromYear = dtpFrom.SelectedDate.Value.Year.ToString();
                        dateFromMonth = dtpFrom.SelectedDate.Value.Month.ToString();
                        dateFromDay = dtpFrom.SelectedDate.Value.Day.ToString();
                        dateToYear = dtpTo.SelectedDate.Value.Year.ToString();
                        dateToMonth = dtpTo.SelectedDate.Value.Month.ToString();
                        dateToDay = dtpTo.SelectedDate.Value.Day.ToString();
                        
                        dateFromMonth = dateFormating(dateFromMonth);
                        dateFromDay = dateFormating(dateFromDay);
                        dateToMonth = dateFormating(dateToMonth);
                        dateToDay = dateFormating(dateToDay);

                        dateFrom = dateFromYear + dateFromMonth + dateFromDay;
                        dateTo = dateToYear + dateToMonth + dateToDay;

                        break;

                    case 2:
                        if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 20)
                        {                            
                            dateFrom = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 8:00:00";
                            dateTo = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 20:00:00";
                        }
                        else
                        {
                            dateFrom = DateTime.Today.AddDays(-1).Year.ToString() + dateFormating(DateTime.Today.AddDays(-1).Month.ToString()) + dateFormating(DateTime.Today.AddDays(-1).Day.ToString()) + " 20:00:00";
                            dateTo = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 8:00:00";
                        }

                        break;

                    case 3:
                        if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 20)
                        {
                            dateFrom = DateTime.Today.AddDays(-1).Year.ToString() + dateFormating(DateTime.Today.AddDays(-1).Month.ToString()) + dateFormating(DateTime.Today.AddDays(-1).Day.ToString()) + " 20:00:00";
                            dateTo = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 8:00:00";
                        }
                        else
                        {
                            if (DateTime.Now.Hour > 20 && DateTime.Now.Hour < 00)
                            {
                                dateFrom = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 8:00:00";
                                dateTo = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 20:00:00";
                            }
                            else
                            {
                                dateFrom = DateTime.Today.AddDays(-1).Year.ToString() + dateFormating(DateTime.Today.AddDays(-1).Month.ToString()) + dateFormating(DateTime.Today.AddDays(-1).Day.ToString()) + " 08:00:00";
                                dateTo = DateTime.Today.AddDays(-1).Year.ToString() + dateFormating(DateTime.Today.AddDays(-1).Month.ToString()) + dateFormating(DateTime.Today.AddDays(-1).Day.ToString()) + " 20:00:00";
                            }                            
                        }

                        break;

                    case 4:
                        dateFrom = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 00:00:00";
                        dateTo = DateTime.Today.Year.ToString() + dateFormating(DateTime.Today.Month.ToString()) + dateFormating(DateTime.Today.Day.ToString()) + " 23:59:59";

                        break;

                    case 5:
                        dateFrom = DateTime.Today.AddDays(-1).Year.ToString() + dateFormating(DateTime.Today.AddDays(-1).Month.ToString()) + dateFormating(DateTime.Today.AddDays(-1).Day.ToString()) + " 00:00:00";
                        dateTo = DateTime.Today.AddDays(-1).Year.ToString() + dateFormating(DateTime.Today.AddDays(-1).Month.ToString()) + dateFormating(DateTime.Today.AddDays(-1).Day.ToString()) + " 23:59:59";

                        break;
                }                        
            
                Task<string> task = new Task<string>(() => workingHoursCount(dateFrom, dateTo, pumpArray));
                task.Start();

                var wordApp = new Word.Application();
                wordApp.Visible = false;

                string str = await task;

                try
                {
                    var wordDocument = wordApp.Documents.Open(templatePath);
                    stringReport("{date}", dtpFrom.SelectedDate.Value.ToShortDateString() + " - " + dtpTo.SelectedDate.Value.ToShortDateString(), wordDocument);
                    stringReport("{pumpHoursList}", str, wordDocument);

                    wordDocument.SaveAs(repotsFolderPath + @"\" + DateTime.Today.ToShortDateString() + " " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ".docx");
                    wordApp.Visible = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка при формировании отчета" + " --- " + ex);
                }
            }
            else
            {
                MessageBox.Show("Дата ОТ должна быть меньше даты ДО");
            }
        } 
        private void stringReport(string varWordName, string text, Word.Document wordDoc)
        {
            var range = wordDoc.Content;
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: varWordName, ReplaceWith: text);
        }
        private void DgReportList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path = repotsFolderPath + @"\" + dgReportList.SelectedItem.ToString();
            Process.Start(path);
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            int errorType = 0;

            if (txbUserName.Text != "ASU") errorType = 1;
            if (txbUserPassword.Text != "1") errorType = 2;
            if (txbUserPassword.Text != "1" && txbUserName.Text != "ASU") errorType = 3;

            switch (errorType)
            {
                case 0:
                    login = true;
                    loginType(login);
                    break;
                case 1:
                    MessageBox.Show("Имя пользователя введено не верно");
                    break;
                case 2:
                    MessageBox.Show("Пароль пользователя введен не верно");
                    break;
                case 3:
                    MessageBox.Show("Имя пользователя и пароль введены не верно");
                    break;
            }
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            login = false;
            loginType(login);
            TabItem item = tcMain.ItemContainerGenerator.ContainerFromIndex(4) as TabItem;
            item.IsEnabled = false;
        }
        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Пока не придумал :-( ");
        }
        private void loginType(bool login)
        {
            if (login)
            {
                txbFirstShift.IsEnabled = true;
                txbSecondShift.IsEnabled = true;
                txbThirdShift.IsEnabled = true;
                txbFourthShift.IsEnabled = true;
                txbDayTime.IsEnabled = true;
                txbPhone.IsEnabled = true;
                txbFolderReportPath.IsEnabled = true;
                txbReportTemplatePath.IsEnabled = true;

                lblFolderReportPath.Visibility = Visibility.Visible;
                lblReportTemplatePath.Visibility = Visibility.Visible;
                txbFolderReportPath.Visibility = Visibility.Visible;
                txbReportTemplatePath.Visibility = Visibility.Visible;
                lblActivityLog.Visibility = Visibility.Visible;
                dgActivityLog.Visibility = Visibility.Visible;

                TabItem item = tcMain.ItemContainerGenerator.ContainerFromIndex(4) as TabItem;
                item.IsEnabled = true;
            }
            else
            {
                txbFirstShift.IsEnabled = false;
                txbSecondShift.IsEnabled = false;
                txbThirdShift.IsEnabled = false;
                txbFourthShift.IsEnabled = false;
                txbDayTime.IsEnabled = false;
                txbPhone.IsEnabled = false;
                txbFolderReportPath.IsEnabled = false;
                txbReportTemplatePath.IsEnabled = false;

                lblFolderReportPath.Visibility = Visibility.Hidden;
                lblReportTemplatePath.Visibility = Visibility.Hidden;
                txbFolderReportPath.Visibility = Visibility.Hidden;
                txbReportTemplatePath.Visibility = Visibility.Hidden;
                lblActivityLog.Visibility = Visibility.Hidden;
                dgActivityLog.Visibility = Visibility.Hidden;

                txbUserName.Text = "";
                txbUserPassword.Text = "";                
            }
        }
        private void refreshDataGrid(int type)
        {
            string sqlString = "";

            switch (type)
            {
                case 0:
                    sqlString = "SELECT id, Hours as Часы, Minutes as Минуты, Secondes as Секунды, PumpNumber as 'Номер насоса', convert(varchar, AddDate, 120) as 'Дата добавления записи' FROM main ORDER BY id DESC";
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
                
            try
            { 
                result = SQL_Connection.MainConn(sqlString);

                if (result.Tables[0].Rows.Count != 0)
                {
                    dataGrid.ItemsSource = result.Tables[0].DefaultView;
                    dgAllRecords.ItemsSource = result.Tables[0].DefaultView;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void DgAllRecords_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView drv = dgAllRecords.SelectedValue as DataRowView;

            txbRecordID.Text = drv[0].ToString();
            txbRecordHours.Text = drv[1].ToString();
            txbRecordMinutes.Text = drv[2].ToString();
            txbRecordSeconds.Text = drv[3].ToString();
            txbRecordPumpNumber.Text = drv[4].ToString();            
            dtpRecordAddDate.SelectedDate = Convert.ToDateTime(drv[5]);
            txbRecordAddDate.Text = drv[5].ToString().Remove(0,11);
        }  
        private void BtnAddRecord_Click(object sender, RoutedEventArgs e)
        {
            string selectedDate;
            if (MessageBox.Show("Добавить запись в базу данных?", "Добавить запись", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if ((txbRecordHours.Text.Trim() == String.Empty) || (txbRecordMinutes.Text.Trim() == String.Empty) || (txbRecordSeconds.Text.Trim() == String.Empty) 
                    || (txbRecordPumpNumber.Text.Trim() == String.Empty) || (txbRecordAddDate.Text.Trim() == String.Empty) || (dtpRecordAddDate.SelectedDate == null))
                {
                    MessageBox.Show("Для добавления записи необходимо заполнить все поля, кроме id");
                }
                else
                {                    
                    try
                    {
                        selectedDate = dtpRecordAddDate.SelectedDate.Value.Year + "-" + dtpRecordAddDate.SelectedDate.Value.Month + "-"
                                        + dtpRecordAddDate.SelectedDate.Value.Day;
                        SQL_Connection.MainConn(@"INSERT INTO main (Hours, Minutes, Secondes, PumpNumber, AddDate) VALUES (" + txbRecordHours.Text +
                                                                                                                        "," + txbRecordMinutes.Text +
                                                                                                                        "," + txbRecordSeconds.Text +
                                                                                                                        ",'" + txbRecordPumpNumber.Text +
                                                                                                                        "',CONVERT(VARCHAR, '" + selectedDate + " " + txbRecordAddDate.Text +"',120))");
                        refreshDataGrid(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }
        private void BtnDeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Запись с ID = " + txbRecordID.Text  + " будет удалена", "Удалить запись?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (txbRecordID.Text.Trim() == String.Empty)
                {
                    MessageBox.Show("Необходимо выбрать запись для удаления");
                }
                else
                {
                    try
                    {
                        SQL_Connection.MainConn("DELETE FROM main WHERE id = " + txbRecordID.Text);
                        refreshDataGrid(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }
        private void BtnEditRecord_Click(object sender, RoutedEventArgs e)
        {
            string selectedDate;
            if (MessageBox.Show("Запись с ID = " + txbRecordID.Text + " будет изменена", "Изменить запись?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if ((txbRecordHours.Text.Trim() == String.Empty) || (txbRecordMinutes.Text.Trim() == String.Empty) || (txbRecordSeconds.Text.Trim() == String.Empty)
                    || (txbRecordPumpNumber.Text.Trim() == String.Empty) || (txbRecordAddDate.Text.Trim() == String.Empty) || (dtpRecordAddDate.SelectedDate == null))
                {
                    MessageBox.Show("Необходимо заполнить все поля, включая ID редактируемой записи");
                }
                else
                {
                    try
                    {
                        selectedDate = dtpRecordAddDate.SelectedDate.Value.Year + "-" + dtpRecordAddDate.SelectedDate.Value.Month + "-"
                                            + dtpRecordAddDate.SelectedDate.Value.Day;
                        SQL_Connection.MainConn(@"UPDATE main SET Hours = " + txbRecordHours.Text + ", Minutes = " + txbRecordMinutes.Text +
                                                                    ", Secondes = " + txbRecordSeconds.Text + ", PumpNumber = '" + txbRecordPumpNumber.Text +
                                                                    "', AddDate = CONVERT(VARCHAR, '" + selectedDate + " " + txbRecordAddDate.Text + "',120) " +
                                                                    "WHERE id = " + txbRecordID.Text);
                        refreshDataGrid(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }
        private void BtnRefreshDataGrid_Click(object sender, RoutedEventArgs e)
        {
            if(cbAll.IsChecked == true) refreshDataGrid(0);
            if(cbDay.IsChecked == true) refreshDataGrid(1);
            if(cbCurrentShift.IsChecked == true) refreshDataGrid(2);
            if(cbPrevShift.IsChecked == true) refreshDataGrid(3);
        }
        private void cbSpecificPumpChoose_Checked(object sender, RoutedEventArgs e)
        {
            cbN1_1.IsEnabled = true;
            cbN1_2.IsEnabled = true;
            cbN2_1.IsEnabled = true;
            cbN2_2.IsEnabled = true;
            cbN2_3.IsEnabled = true;
            cbN3_1.IsEnabled = true;
            cbN3_2.IsEnabled = true;
            cbN3_3.IsEnabled = true;
            cbN4_1.IsEnabled = true;
            cbN4_2.IsEnabled = true;
            cbN4_3.IsEnabled = true;
            cbN5_1.IsEnabled = true;
            cbN5_2.IsEnabled = true;
        }
        private void cbSpecificPumpChoose_Unchecked(object sender, RoutedEventArgs e)
        {
            cbN1_1.IsEnabled = false;
            cbN1_2.IsEnabled = false;
            cbN2_1.IsEnabled = false;
            cbN2_2.IsEnabled = false;
            cbN2_3.IsEnabled = false;
            cbN3_1.IsEnabled = false;
            cbN3_2.IsEnabled = false;
            cbN3_3.IsEnabled = false;
            cbN4_1.IsEnabled = false;
            cbN4_2.IsEnabled = false;
            cbN4_3.IsEnabled = false;
            cbN5_1.IsEnabled = false;
            cbN5_2.IsEnabled = false;
        }
        private void ChbReportShift_Checked(object sender, RoutedEventArgs e)
        {
            chbReportPrevShift.IsChecked = false;
            chbReportDay.IsChecked = false;
            chbReportPrevDay.IsChecked = false;
        }
        private void ChbReportPrevShift_Checked(object sender, RoutedEventArgs e)
        {
            chbReportShift.IsChecked = false;
            chbReportDay.IsChecked = false;
            chbReportPrevDay.IsChecked = false;
        }
        private void ChbReportDay_Checked(object sender, RoutedEventArgs e)
        {
            chbReportShift.IsChecked = false;
            chbReportPrevShift.IsChecked = false;
            chbReportPrevDay.IsChecked = false;
        }
        private void ChbReportPrevDay_Checked(object sender, RoutedEventArgs e)
        {
            chbReportShift.IsChecked = false;
            chbReportPrevShift.IsChecked = false;
            chbReportDay.IsChecked = false;
        }        
        private void CbDay_Checked(object sender, RoutedEventArgs e)
        {
            cbAll.IsChecked = false;
            cbCurrentShift.IsChecked = false;
            cbPrevShift.IsChecked = false;
        }
        private void CbPrevShift_Checked(object sender, RoutedEventArgs e)
        {
            cbAll.IsChecked = false;
            cbCurrentShift.IsChecked = false;
            cbDay.IsChecked = false;
        }
        private void CbCurrentShift_Checked(object sender, RoutedEventArgs e)
        {
            cbAll.IsChecked = false;
            cbDay.IsChecked = false;
            cbPrevShift.IsChecked = false;
        }
        private void CbAll_Checked(object sender, RoutedEventArgs e)
        {
            cbCurrentShift.IsChecked = false;
            cbDay.IsChecked = false;
            cbPrevShift.IsChecked = false;
        }  
    }
}
