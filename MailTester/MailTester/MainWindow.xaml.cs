using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MailTester.ARSoft.Tools.Net.Dns;
using MailTester.ARSoft.Tools.Net.Dns.DnsRecord;

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

		private async void Button1_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(async () =>
			{
				try
				{
					await Dispatcher.InvokeAsync(() => ProgressBar1.Visibility = Visibility.Visible);
					var domainName = await Dispatcher.InvokeAsync(() => ToTextBox.Text.Split('@').Last());
					var response = DnsClient.Default.Resolve(domainName, RecordType.Mx);
					var records = response.AnswerRecords.OfType<MxRecord>();

					await Dispatcher.InvokeAsync(() =>
					{
						MxComboBox.Items.Clear();
					});

					foreach (var mxRecord in records)
					{
						await Dispatcher.InvokeAsync(() => MxComboBox.Items.Add(mxRecord));
					}

					await Dispatcher.InvokeAsync(() =>
					{
						MxComboBox.SelectedIndex = 0;
					});

					await Dispatcher.InvokeAsync(() => TestButton.IsEnabled = true);
					await Dispatcher.InvokeAsync(() => ProgressBar1.Visibility = Visibility.Collapsed);
				}
				catch (Exception ex)
				{
					Dispatcher.Invoke(() => ProgressBar1.Visibility = Visibility.Collapsed);
					MessageBox.Show(ex.Message);
				}

			});

		}

		private async void TestButton_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(async () =>
			{
				try
				{
					await Dispatcher.InvokeAsync(() => ProgressBar1.Visibility = Visibility.Visible);
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text = "");
					string response = "";
					var mxRecord = await Dispatcher.InvokeAsync(() => MxComboBox.SelectedItem as MxRecord);
					var helper = new SocketHelper(mxRecord.ExchangeDomainName, 25);

					var senderDomain = await Dispatcher.InvokeAsync(() => FromTextBox.Text.Split('@').Last());

					helper.SendCommand("EHLO " + senderDomain);
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += "EHLO " + senderDomain + "\r\n");
					response = await helper.GetFullResponse();
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += response);

					response = await helper.GetFullResponse();
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += response);

					helper.SendCommand("MAIL FROM:<" + await Dispatcher.InvokeAsync(() => FromTextBox.Text + ">"));
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += "MAIL FROM:<" + FromTextBox.Text + ">\r\n");
					response = await helper.GetFullResponse();
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += response);

					helper.SendCommand("RCPT TO:<" + await Dispatcher.InvokeAsync(() => ToTextBox.Text + ">"));
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += "RCPT TO:<" + ToTextBox.Text + ">\r\n");
					response = await helper.GetFullResponse();
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += response);

					helper.SendCommand("DATA");
					helper.SendCommand("Subject:Test Message");
					helper.SendCommand("\r\n");
					helper.SendCommand("Test Message");
					helper.SendCommand("\r\n");
					helper.SendCommand(".");
					helper.SendCommand("\r\n");
					response = await helper.GetFullResponse();
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += response);

					helper.SendCommand("QUIT");
					response = await helper.GetFullResponse();
					await Dispatcher.InvokeAsync(() => StatusTextBlock.Text += response);
					await Dispatcher.InvokeAsync(() => ProgressBar1.Visibility = Visibility.Collapsed);
				}
				catch (Exception ex)
				{
					Dispatcher.Invoke(() => ProgressBar1.Visibility = Visibility.Collapsed);
					MessageBox.Show(ex.Message);
				}

			});
		}
	}
}
