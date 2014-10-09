using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HideSource
{
	static class TextHelper
	{
		const string Token = ".....";

		public static string EncryptOrDecryptText(string text, string lineComment, byte[] key, byte[] iv)
		{
			if (key == null) throw new ArgumentNullException("key");
			if (iv == null) throw new ArgumentNullException("iv");
			if (key.Length == 0) throw new InvalidOperationException("key must be a non zero-length array");
			if (iv.Length == 0) throw new InvalidOperationException("iv must be a non zero-length array");
			Contract.EndContractBlock();

			return IsEncrypted(text, lineComment)
				       ? DecryptText(text, lineComment, key, iv)
				       : EncryptText(text, lineComment, key, iv);
		}

		private static bool IsEncrypted(string text, string lineComment)
		{
			string token = lineComment + Token;
			return (text.StartsWith(token));
		}

		private static string EncryptText(string text, string lineComment, byte[] key, byte[] iv)
		{
			if (IsEncrypted(text, lineComment)) return text;

			byte[] encrypted;

			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cStream = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateEncryptor(key, iv), CryptoStreamMode.Write))
				using (StreamWriter sw = new StreamWriter(cStream))
				{
					sw.Write(text);
				}
				encrypted = ms.ToArray();
			}

			var sb = new StringBuilder(encrypted.Length * 2);
			sb.AppendLine(lineComment + Token);

			const int chunkSize = 140 / 2;
			for (int i = 0; i < (encrypted.Length / chunkSize) + 1; i++)
			{
				sb.Append(lineComment);
				for (int j = i * chunkSize; j < (i + 1) * chunkSize && j < encrypted.Length; j++)
				{
					sb.AppendFormat("{0:X2}", encrypted[j]);
				}
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		private static string DecryptText(string text, string lineComment, byte[] key, byte[] iv)
		{
			if (!IsEncrypted(text, lineComment)) return text;

			byte[] encrypted;

			using (MemoryStream ms = new MemoryStream(text.Length / 2))
			{
				using (StringReader sr = new StringReader(text))
				{
					sr.ReadLine();
					for (; ; )
					{
						string line = sr.ReadLine();
						if (line == null) break;

						for (int i = lineComment.Length; i < line.Length; i += 2)
						{
							byte b = byte.Parse(line.Substring(i, 2), NumberStyles.HexNumber);
							ms.WriteByte(b);
						}
					}
				}
				encrypted = ms.ToArray();
			}

			using (MemoryStream ms = new MemoryStream(encrypted))
			using (CryptoStream csDecrypt = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateDecryptor(key, iv), CryptoStreamMode.Read))
			{
				using (StreamReader sr = new StreamReader(csDecrypt))
				{
					return sr.ReadToEnd();
				}
			}
		}
	}
}
