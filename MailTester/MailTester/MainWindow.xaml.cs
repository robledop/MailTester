using System;
using System.Collections.Generic;
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
using ARSoft.Tools.Net.Dns;

namespace MailTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button1_Click(object sender, RoutedEventArgs e)
		{
			if ((string)Button1.Content == "Test")
			{
				string response = "";
				var mxRecord = MxComboBox.SelectedItem as MxRecord;
				var helper = new SocketHelper(mxRecord.ExchangeDomainName, 25);

				var senderDomain = FromTextBox.Text.Split('@').Last();

				helper.SendCommand("EHLO " + senderDomain);
				StatusTextBlock.Text += "EHLO " + senderDomain + "\r\n";
				response = helper.GetFullResponse();
				StatusTextBlock.Text += response;

				helper.SendCommand("MAIL FROM:<" + FromTextBox.Text + ">");
				StatusTextBlock.Text += "MAIL FROM:<" + FromTextBox.Text + ">\r\n";
				response = helper.GetFullResponse();
				StatusTextBlock.Text += response;

				helper.SendCommand("RCPT TO:<" + ToTextBox.Text + ">");
				StatusTextBlock.Text += "RCPT TO:<" + ToTextBox.Text + ">\r\n";
				StatusTextBlock.Text += helper.GetFullResponse();

				helper.SendCommand("Data");
				StatusTextBlock.Text += helper.GetFullResponse();

				helper.SendCommand("\r\n");
				helper.SendCommand(".");
				helper.SendCommand("\r\n");
				StatusTextBlock.Text += helper.GetFullResponse();

				helper.SendCommand("QUIT");
			}
			else
			{
				var domainName = ToTextBox.Text.Split('@').Last();
				var response = DnsClient.Default.Resolve(domainName, RecordType.Mx);
				var records = response.AnswerRecords.OfType<MxRecord>();

				foreach (var mxRecord in records)
				{
					MxComboBox.Items.Add(mxRecord);
				}

				MxComboBox.SelectedIndex = 0;
				Button1.Content = "Test";
			}

		}
	}
}
