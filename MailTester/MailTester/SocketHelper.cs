using System;
using System.IO;
using System.Net.Sockets;

namespace MailTester
{
	public class SocketHelper
	{
		private readonly TcpClient _client;
		private NetworkStream stream = null;
		private StreamReader reader = null;
		private StreamWriter writer = null;
		private string resp = "";
		private int state = -1;

		public SocketHelper(string name, int port)
		{
			_client = new TcpClient(name, port);
			stream = _client.GetStream();
			reader = new StreamReader(stream);
			writer = new StreamWriter(stream);
		}

		public SocketHelper(TcpClient tc)
		{
			_client = tc;
			stream = _client.GetStream();
			reader = new StreamReader(stream);
			writer = new StreamWriter(stream);
		}

		public void SendData(byte[] bts)
		{
			if (GetResponseState() != 221)
			{
				stream.Write(bts, 0, bts.Length);
				stream.Flush();
			}
		}

		public void SendCommand(string cmd)
		{
			if (GetResponseState() != 221)
			{
				writer.WriteLine(cmd);
				writer.Flush();
			}
		}

		public string RecvResponse()
		{
			if (GetResponseState() != 221)
				resp = reader.ReadLine();
			else
				resp = "221 closed!";

			return resp;
		}

		public int GetResponseState()
		{
			if (resp.Length >= 3 && IsNumber(resp[0])
			    && IsNumber(resp[1]) && IsNumber(resp[2]))
				state = Convert.ToInt32(resp.Substring(0, 3));

			return state;
		}

		private bool IsNumber(char c)
		{
			return c >= '0' && c <= '9';
		}

		public string GetFullResponse()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(RecvResponse());
			sb.Append("\r\n");
			while (HaveNextResponse())
			{
				sb.Append(RecvResponse());
				sb.Append("\r\n");
			}
			return sb.ToString();
		}

		public bool HaveNextResponse()
		{
			if (GetResponseState() > -1)
			{
				if (resp.Length >= 4 && resp[3] != ' ')
					return true;
				else
					return false;
			}
			else
				return false;
		}
	}
}